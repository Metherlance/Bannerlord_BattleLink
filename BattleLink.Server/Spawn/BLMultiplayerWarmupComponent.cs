using BattleLink.Common.Model;
using BattleLink.Common.Spawn.Battle;
using BattleLink.Common.Spawn.Warmup;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using BattleLink.Server.Handler;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.GameNetwork;
using static TaleWorlds.MountAndBlade.GameNetwork.NetworkMessageHandlerRegisterer;

namespace BattleLink.Common.Spawn
{
    // TODO to delete ?
    public class BLMultiplayerWarmupComponent : MultiplayerWarmupComponent
    {
        private static readonly FieldInfo fMpWarmupComponentState = typeof(MultiplayerWarmupComponent).GetField("_warmupState", BindingFlags.NonPublic | BindingFlags.Instance);


        private MissionMultiplayerGameModeBase _gameMode;
        private MultiplayerTimerComponent _timerComponent;
        private MissionLobbyComponent _lobbyComponent;
        private MissionTime _currentStateStartTime;
        private WarmupStates _warmupState;
        public new static float TotalWarmupDuration => MultiplayerOptions.OptionType.WarmupTimeLimit.GetIntValue() * 60;//10;//

        public new event Action OnWarmupEnding;

        public new event Action OnWarmupEnded;
        public new bool IsInWarmup => _warmupState != WarmupStates.Ended;

        private BattleSpawnPathSelector _battleSpawnPathSelector;

        //private string charactersXml;

        public WarmupStates WarmupState
        {
            get => _warmupState;
            set
            {
                _warmupState = value;
                if (!IsServer)
                    return;
                _currentStateStartTime = MissionTime.Now;
                BeginBroadcastModuleEvent();
                WriteMessage(new WarmupStateChange(_warmupState, _currentStateStartTime.NumberOfTicks));
                EndBroadcastModuleEvent(EventBroadcastFlags.None);

                MBDebug.Print("RBMultiplayerWarmupComponent - _warmupState " + _warmupState);
                                
                fMpWarmupComponentState.SetValue(this, value);

                MBDebug.Print("RBMultiplayerWarmupComponent - _warmupState " + _warmupState);
            }
        }

        //public override void OnAfterMissionCreated()
        //{
        //    base.OnAfterMissionCreated();
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnAfterMissionCreated ");           
        //}

        //public override void EarlyStart()
        //{
        //    base.EarlyStart();
        //    MBDebug.Print("RBMultiplayerWarmupComponent - EarlyStart", 0, DebugColor.Green);
        //}

        //public override void OnMissionTick(float dt)
        //{
        //    base.OnMissionTick(dt);
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnMissionTick ");
        //}


        //protected override void HandleNewClientConnect(PlayerConnectionInfo clientConnectionInfo)
        //{
        //    base.HandleNewClientConnect(clientConnectionInfo);//1
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnPlayerConnectedToServer ");
        //}

        //public override void OnPlayerConnectedToServer(NetworkCommunicator networkPeer)
        //{
        //    base.OnPlayerConnectedToServer(networkPeer);//2
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnPlayerConnectedToServer ");
        //}

        //protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        //{
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnPlayerConnectedToServer ");//3
        //}

        //protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        //{
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnPlayerConnectedToServer ");//4
        //}

        //protected override void HandleLateNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        //{
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnPlayerConnectedToServer ");//5
        //}

        protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            if (!IsInWarmup || networkPeer.IsServerPeer)//6
                return;
            BeginModuleEventAsServer(networkPeer);
            WriteMessage(new WarmupStateChange(_warmupState, _currentStateStartTime.NumberOfTicks));
            EndModuleEventAsServer();

            MBDebug.Print("RBMultiplayerWarmupComponent - HandleNewClientAfterSynchronized ");
        }

        //protected override void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        //{
        //    MBDebug.Print("RBMultiplayerWarmupComponent - OnPlayerConnectedToServer ");//7

        //}


        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _gameMode = Mission.GetMissionBehavior<MissionMultiplayerGameModeBase>();
            _timerComponent = Mission.GetMissionBehavior<MultiplayerTimerComponent>();
            _lobbyComponent = Mission.GetMissionBehavior<MissionLobbyComponent>();

            MBDebug.Print("RBMultiplayerWarmupComponent - OnBehaviorInitialize ", 0, DebugColor.Green);


        }
        public override void AfterStart()
        {
            base.AfterStart();
            AddRemoveMessageHandlers(RegisterMode.Add);
           
            MBDebug.Print("RBMultiplayerWarmupComponent - AfterStart", 0, DebugColor.Green);
        }
        protected override void OnUdpNetworkHandlerClose() => AddRemoveMessageHandlers(RegisterMode.Remove);

        

        private void AddRemoveMessageHandlers(RegisterMode mode)
        {
            GameNetwork.NetworkMessageHandlerRegisterer reg = new NetworkMessageHandlerRegisterer(mode);
            reg.Register<BLMainHeroRequestEndWarmupMessage>(BLMainHeroRequestEndWarmupHandler.HandleClientEventMainHeroRequestEndWarmupMessage);
           // Mission.MissionNetworkHelper
        }

        public new bool CheckForWarmupProgressEnd() => _gameMode.CheckForWarmupEnd() || (double)_timerComponent.GetRemainingTime(false) <= 30.0;

        public override void OnPreDisplayMissionTick(float dt)
        {
            if (!IsServer)
                return;
            switch (WarmupState)
            {
                case WarmupStates.WaitingForPlayers:
                    BeginWarmup();
                    break;
                case WarmupStates.InProgress:
                    if (!CheckForWarmupProgressEnd())
                        break;
                    EndWarmupProgress();
                    break;
                case WarmupStates.Ending:
                    if (!_timerComponent.CheckIfTimerPassed())
                        break;
                    EndWarmup();
                    break;
                case WarmupStates.Ended:
                    if (!_timerComponent.CheckIfTimerPassed())
                        break;

                    MBDebug.Print("RBMultiplayerWarmupComponent - OnPreDisplayMissionTick", 0, DebugColor.Green);
                    TeamQuerySystemUtils.setPowerFix(Mission.Current);

                    Mission.RemoveMissionBehavior(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void BeginWarmup()
        {
            WarmupState = WarmupStates.InProgress;
            Mission.Current.ResetMission();
            _gameMode.MultiplayerTeamSelectComponent.BalanceTeams();
            _timerComponent.StartTimerAsServer(TotalWarmupDuration);
            _gameMode.SpawnComponent.SpawningBehavior.Clear();
            //SpawnComponent.SetWarmupSpawningBehavior();
            Mission.Current.GetMissionBehavior<SpawnComponent>().SetNewSpawnFrameBehavior(new BLWarmupSpawnFrameBehavior());
            Mission.Current.GetMissionBehavior<SpawnComponent>().SetNewSpawningBehavior(new BLWarmupSpawningBehavior());
            MBDebug.Print("RBMultiplayerWarmupComponent - BeginWarmup", 0, DebugColor.Green);
        }

        private new void EndWarmupProgress()
        {
            WarmupState = WarmupStates.Ending;
            _timerComponent.StartTimerAsServer(30f);
            //Action onWarmupEndin5g = OnWarmupEnded;
            Action onWarmupEnding = OnWarmupEnding;
            if (onWarmupEnding == null)
                return;
            onWarmupEnding();
        }

        private void EndWarmup()
        {
            WarmupState = WarmupStates.Ended;
            _timerComponent.StartTimerAsServer(3f);
            Action onWarmupEnded = OnWarmupEnded;
            if (onWarmupEnded != null)
                onWarmupEnded();
            if (!IsDedicatedServer)
                PlayBattleStartingSound();
            Mission.Current.ResetMission();
            _gameMode.MultiplayerTeamSelectComponent.BalanceTeams();
            _gameMode.SpawnComponent.SpawningBehavior.Clear();

            MBDebug.Print("RBMultiplayerWarmupComponent - EndWarmup", 0, DebugColor.Green);

            TeamQuerySystemUtils.setPowerFix(Mission.Current);
            //SpawnComponent.SetSpawningBehaviorForCurrentGameType(this._gameMode.GetMissionType());
            //Mission.Current.GetMissionBehavior<SpawnComponent>().SetNewSpawnFrameBehavior(new FlagDominationSpawnFrameBehavior());
            Mission.Current.GetMissionBehavior<SpawnComponent>().SetNewSpawnFrameBehavior(new BLFlagDominationSpawnFrameBehavior());
            //Mission.Current.GetMissionBehavior<SpawnComponent>().SetNewSpawningBehavior(new FlagDominationSpawningBehavior());
            Mission.Current.GetMissionBehavior<SpawnComponent>().SetNewSpawningBehavior(new BLFlagDominationSpawningBehavior());

            MBDebug.Print("RBMultiplayerWarmupComponent - EndWarmup", 0, DebugColor.Green);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            OnWarmupEnding = null;
            OnWarmupEnded = null;
            if (!IsServer || _gameMode.UseRoundController() || _lobbyComponent.CurrentMultiplayerState == MissionLobbyComponent.MultiplayerGameState.Ending)
                return;
            _gameMode.SpawnComponent.SpawningBehavior.RequestStartSpawnSession();
        }


        private void HandleServerEventWarmupStateChange(WarmupStateChange message)
        {
            WarmupState = message.WarmupState;
            switch (WarmupState)
            {
                case WarmupStates.InProgress:
                    _timerComponent.StartTimerAsClient(message.StateStartTimeInSeconds, TotalWarmupDuration);
                    break;
                case WarmupStates.Ending:
                    _timerComponent.StartTimerAsClient(message.StateStartTimeInSeconds, 30f);
                    Action onWarmupEnding = OnWarmupEnding;
                    if (onWarmupEnding == null)
                        break;
                    onWarmupEnding();
                    break;
                case WarmupStates.Ended:
                    _timerComponent.StartTimerAsClient(message.StateStartTimeInSeconds, 3f);
                    Action onWarmupEnded = OnWarmupEnded;
                    if (onWarmupEnded != null)
                        onWarmupEnded();
                    PlayBattleStartingSound();
                    break;
            }
        }

        private void PlayBattleStartingSound()
        {
            MBDebug.Print("RBMultiplayerWarmupComponent - PlayBattleStartingSound", 0, DebugColor.Green);
            MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
            Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
            NetworkCommunicator myPeer = MyPeer;
            MissionPeer missionPeer = myPeer != null ? myPeer.GetComponent<MissionPeer>() : null;
            if (missionPeer?.Team != null)
                MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/alerts/rally/" + (missionPeer.Team.Side == BattleSideEnum.Attacker ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue() : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue()).ToLower()), position);
            else
                MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/alerts/rally/generic"), position);
        }

        public void EndingWarmupByMainHero()
        {
            WarmupState = WarmupStates.Ending;
            _timerComponent.StartTimerAsServer(5f);
            this.OnWarmupEnding?.Invoke();
            _timerComponent.StartTimerAsServer(5f);

            //WarmupState = WarmupStates.Ended;
            //_timerComponent.StartTimerAsServer(3f);
            //this.OnWarmupEnded?.Invoke();
        }

    }
}
