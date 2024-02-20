using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;
using TaleWorlds.Engine;
using BattleLink.Common;

namespace BattleLink.CommonSvMp.Behavior
{
    public class BLMultiplayerRoundController : MissionNetwork, IRoundComponent, IMissionBehavior
    {
        private MissionMultiplayerGameModeBase _gameModeServer;
        private int _roundCount;
        private BattleSideEnum _roundWinner;
        private RoundEndReason _roundEndReason;
        private MissionLobbyComponent _missionLobbyComponent;
        private bool _roundTimeOver;
        private MissionTime _currentRoundStateStartTime;
        private bool _equipmentUpdateDisabled = true;

        public event Action OnRoundStarted;

        public event Action OnPreparationEnded;

        public event Action OnPreRoundEnding;

        public event Action OnRoundEnding;

        public event Action OnPostRoundEnded;

        public event Action OnCurrentRoundStateChanged;

        public int RoundCount
        {
            get => _roundCount;
            set
            {
                if (_roundCount == value)
                    return;
                _roundCount = value;
                if (!GameNetwork.IsServer)
                    return;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new RoundCountChange(_roundCount));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        public BattleSideEnum RoundWinner
        {
            get => _roundWinner;
            set
            {
                if (_roundWinner == value)
                    return;
                _roundWinner = value;
                if (!GameNetwork.IsServer)
                    return;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new RoundWinnerChange(value));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        public RoundEndReason RoundEndReason
        {
            get => _roundEndReason;
            set
            {
                if (_roundEndReason == value)
                    return;
                _roundEndReason = value;
                if (!GameNetwork.IsServer)
                    return;
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new RoundEndReasonChange(value));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        public bool IsMatchEnding { get; private set; }

        public float LastRoundEndRemainingTime { get; private set; }

        public float RemainingRoundTime => _gameModeServer.TimerComponent.GetRemainingTime(false);

        public MultiplayerRoundState CurrentRoundState { get; private set; }

        public bool IsRoundInProgress => CurrentRoundState == MultiplayerRoundState.InProgress;

        public void EnableEquipmentUpdate() => _equipmentUpdateDisabled = false;

        public override void AfterStart()
        {
            AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Add);
            if (GameNetwork.IsServerOrRecorder)
                _gameModeServer = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBase>();
            _missionLobbyComponent = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
            _roundCount = 0;
            _gameModeServer.TimerComponent.StartTimerAsServer(8f);
        }

        private void EndRound()
        {
            if (OnPreRoundEnding != null)
                OnPreRoundEnding();
            ChangeRoundState(MultiplayerRoundState.Ending);
            _gameModeServer.TimerComponent.StartTimerAsServer(3f);
            _roundTimeOver = false;
            if (OnRoundEnding == null)
                return;
            OnRoundEnding();
        }

        private bool CheckPostEndRound() => _gameModeServer.TimerComponent.CheckIfTimerPassed();

        private bool CheckPostMatchEnd() => _gameModeServer.TimerComponent.CheckIfTimerPassed();

        private void PostRoundEnd()
        {
            _gameModeServer.TimerComponent.StartTimerAsServer(5f);
            ChangeRoundState(MultiplayerRoundState.Ended);
            if (_roundCount == MultiplayerOptions.OptionType.RoundTotal.GetIntValue() || CheckForMatchEndEarly() || !HasEnoughCharactersOnBothSides())
                IsMatchEnding = true;
            if (OnPostRoundEnded == null)
                return;
            OnPostRoundEnded();
        }

        private void PostMatchEnd()
        {
            _gameModeServer.TimerComponent.StartTimerAsServer(5f);
            ChangeRoundState(MultiplayerRoundState.MatchEnded);
            _missionLobbyComponent.SetStateEndingAsServer();
        }

        public override void OnRemoveBehavior()
        {
            GameNetwork.RemoveNetworkHandler(this);
            base.OnRemoveBehavior();
        }

        private void AddRemoveMessageHandlers(
          GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer handlerRegisterer = new GameNetwork.NetworkMessageHandlerRegisterer(mode);
            if (GameNetwork.IsClient || !GameNetwork.IsServer)
                return;
            handlerRegisterer.Register(new GameNetworkMessage.ClientMessageHandlerDelegate<CultureVoteClient>(HandleClientEventCultureSelect));
        }

        protected override void OnUdpNetworkHandlerClose() => AddRemoveMessageHandlers(GameNetwork.NetworkMessageHandlerRegisterer.RegisterMode.Remove);

        public override void OnPreDisplayMissionTick(float dt)
        {
            if (GameNetwork.IsServer)
            {
                if (_missionLobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.WaitingFirstPlayers)
                    return;
                if (!IsMatchEnding && _missionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Ending && (CurrentRoundState == MultiplayerRoundState.WaitingForPlayers || CurrentRoundState == MultiplayerRoundState.Ended))
                {
                    if (CheckForNewRound())
                    {
                        BeginNewRound();
                    }
                    else
                    {
                        if (!IsMatchEnding)
                            return;
                        PostMatchEnd();
                    }
                }
                else if (CurrentRoundState == MultiplayerRoundState.Preparation)
                {
                    if (!CheckForPreparationEnd())
                        return;
                    EndPreparation();
                    StartSpawning(_equipmentUpdateDisabled);
                }
                else if (CurrentRoundState == MultiplayerRoundState.InProgress)
                {
                    if (!CheckForRoundEnd())
                        return;
                    EndRound();
                }
                else if (CurrentRoundState == MultiplayerRoundState.Ending)
                {
                    if (!CheckPostEndRound())
                        return;
                    PostRoundEnd();
                }
                else
                {
                    if (CurrentRoundState != MultiplayerRoundState.Ended || !IsMatchEnding || !CheckPostMatchEnd())
                        return;
                    PostMatchEnd();
                }
            }
            else
                _gameModeServer.TimerComponent.CheckIfTimerPassed();
        }

        private void ChangeRoundState(MultiplayerRoundState newRoundState)
        {
            if (CurrentRoundState == newRoundState)
                return;
            if (CurrentRoundState == MultiplayerRoundState.InProgress)
                LastRoundEndRemainingTime = RemainingRoundTime;
            CurrentRoundState = newRoundState;
            _currentRoundStateStartTime = MissionTime.Now;
            Action roundStateChanged = OnCurrentRoundStateChanged;
            if (roundStateChanged != null)
                roundStateChanged();
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(new RoundStateChange(newRoundState, _currentRoundStateStartTime.NumberOfTicks, MathF.Ceiling(LastRoundEndRemainingTime)));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        }

        protected override void HandleLateNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
        }

        public bool HandleClientEventCultureSelect(NetworkCommunicator peer, CultureVoteClient message)
        {
            peer.GetComponent<MissionPeer>().HandleVoteChange(message.VotedType, message.VotedCulture);
            return true;
        }

        private bool CheckForRoundEnd()
        {
            if (!_roundTimeOver)
                _roundTimeOver = _gameModeServer.TimerComponent.CheckIfTimerPassed();
            return !_gameModeServer.CheckIfOvertime() && _roundTimeOver || _gameModeServer.CheckForRoundEnd();
        }

        private bool CheckForNewRound()
        {
            if (CurrentRoundState != MultiplayerRoundState.WaitingForPlayers && !_gameModeServer.TimerComponent.CheckIfTimerPassed())
                return false;
            int[] source = new int[2];
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
                    ++source[(int)component.Team.Side];
            }
            if (source.Sum() >= MultiplayerOptions.OptionType.MinNumberOfPlayersForMatchStart.GetIntValue() || RoundCount != 0)
                return true;
            IsMatchEnding = true;
            return false;
        }

        private bool HasEnoughCharactersOnBothSides() => ((MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue() > 0 ? 1 : GameNetwork.NetworkPeers.Count(q => q.GetComponent<MissionPeer>() != null && q.GetComponent<MissionPeer>().Team == Mission.Current.AttackerTeam) > 0 ? 1 : 0) & (MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue() > 0 ? true ? 1 : 0 : GameNetwork.NetworkPeers.Count(q => q.GetComponent<MissionPeer>() != null && q.GetComponent<MissionPeer>().Team == Mission.Current.DefenderTeam) > 0 ? 1 : 0)) != 0;

        private void BeginNewRound()
        {
            if (CurrentRoundState == MultiplayerRoundState.WaitingForPlayers)
                _gameModeServer.ClearPeerCounts();
            ChangeRoundState(MultiplayerRoundState.Preparation);
            ++RoundCount;
            Mission.Current.ResetMission();
            _gameModeServer.MultiplayerTeamSelectComponent.BalanceTeams();
            _gameModeServer.TimerComponent.StartTimerAsServer(MultiplayerOptions.OptionType.RoundPreparationTimeLimit.GetIntValue());
            Action onRoundStarted = OnRoundStarted;
            if (onRoundStarted != null)
                onRoundStarted();
            _gameModeServer.SpawnComponent.ToggleUpdatingSpawnEquipment(true);

            MBDebug.Print("RBMultiplayerRoundController - BeginNewRound", 0, DebugColor.Green);
            TeamQuerySystemUtils.setPowerFix(Mission.Current);
        }

        private bool CheckForPreparationEnd() => CurrentRoundState == MultiplayerRoundState.Preparation && _gameModeServer.TimerComponent.CheckIfTimerPassed();

        private void EndPreparation()
        {
            if (OnPreparationEnded == null)
                return;
            OnPreparationEnded();
        }

        private void StartSpawning(bool disableEquipmentUpdate = true)
        {
            _gameModeServer.TimerComponent.StartTimerAsServer(MultiplayerOptions.OptionType.RoundTimeLimit.GetIntValue());
            if (disableEquipmentUpdate)
                _gameModeServer.SpawnComponent.ToggleUpdatingSpawnEquipment(false);
            ChangeRoundState(MultiplayerRoundState.InProgress);
        }

        private bool CheckForMatchEndEarly()
        {
            bool flag = false;
            MissionScoreboardComponent missionBehavior = Mission.Current.GetMissionBehavior<MissionScoreboardComponent>();
            if (missionBehavior != null)
            {
                for (int side = 0; side < 2; ++side)
                {
                    if (missionBehavior.GetRoundScore((BattleSideEnum)side) > MultiplayerOptions.OptionType.RoundTotal.GetIntValue() / 2)
                    {
                        flag = true;
                        break;
                    }
                }
            }
            return flag;
        }

        protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            if (networkPeer.IsServerPeer)
                return;
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new RoundStateChange(CurrentRoundState, _currentRoundStateStartTime.NumberOfTicks, MathF.Ceiling(LastRoundEndRemainingTime)));
            GameNetwork.EndModuleEventAsServer();
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new RoundWinnerChange(RoundWinner));
            GameNetwork.EndModuleEventAsServer();
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(new RoundCountChange(RoundCount));
            GameNetwork.EndModuleEventAsServer();
        }
    }
}
