using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;

namespace BattleLink.TeamSelect
{
    public class BLTeamSelectVM : ViewModel
    {
        private readonly Action _onClose;

        private readonly Action _onAutoAssign;

        private readonly MissionMultiplayerGameModeBaseClient _gameMode;

        private readonly MissionPeer _missionPeer;

        private readonly string _gamemodeStr;

        private string _teamSelectTitle;

        private bool _isRoundCountdownAvailable;

        private string _remainingRoundTime;

        private string _gamemodeLbl;

        private string _autoassignLbl;

        private bool _isCancelDisabled;

        private BLTeamSelectTeamVM _team1;

        private BLTeamSelectTeamVM _team2;

        private BLTeamSelectTeamVM _teamSpectators;


        private MissionRepresentativeBase missionRep
        {
            get
            {
                NetworkCommunicator myPeer = GameNetwork.MyPeer;
                if (myPeer == null)
                {
                    return null;
                }
                VirtualPlayer virtualPlayer = myPeer.VirtualPlayer;
                if (virtualPlayer == null)
                {
                    return null;
                }
                return virtualPlayer.GetComponent<MissionRepresentativeBase>();
            }
        }

        public BLTeamSelectVM(Mission mission, Action<Team> onChangeTeamTo, Action onAutoAssign, Action onClose, IEnumerable<Team> teams, string gamemode)
        {
            _onClose = onClose;
            _onAutoAssign = onAutoAssign;
            _gamemodeStr = gamemode;
            _gameMode = mission.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>();
            MissionScoreboardComponent missionBehavior = mission.GetMissionBehavior<MissionScoreboardComponent>();
            IsRoundCountdownAvailable = _gameMode.IsGameModeUsingRoundCountdown;
            Team team = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.None);
            TeamSpectators = new BLTeamSelectTeamVM(missionBehavior, team, null, null, onChangeTeamTo);
            Team team2 = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.Attacker);
            BasicCultureObject basicCultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
            Team1 = new BLTeamSelectTeamVM(missionBehavior, team2, basicCultureObject, BannerCode.CreateFrom(team2.Banner), onChangeTeamTo);
            Team team3 = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.Defender);
            basicCultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
            Team2 = new BLTeamSelectTeamVM(missionBehavior, team3, basicCultureObject, BannerCode.CreateFrom(team3.Banner), onChangeTeamTo);
            if (GameNetwork.IsMyPeerReady)
            {
                _missionPeer = GameNetwork.MyPeer.GetComponent<MissionPeer>();
                IsCancelDisabled = _missionPeer.Team == null;
            }
            RefreshValues();
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            AutoassignLbl = new TextObject("{=bON4Kn6B}Auto Assign", null).ToString();
            TeamSelectTitle = new TextObject("{=aVixswW5}Team Selection", null).ToString();
            GamemodeLbl = GameTexts.FindText("str_multiplayer_official_game_type_name", _gamemodeStr).ToString();
            Team1.RefreshValues();
            _team2.RefreshValues();
            _teamSpectators.RefreshValues();
        }

        public void Tick(float dt)
        {
            RemainingRoundTime = TimeSpan.FromSeconds(MathF.Ceiling(_gameMode.RemainingTime)).ToString("mm':'ss");
        }

        public void RefreshDisabledTeams(List<Team> disabledTeams)
        {
            if (disabledTeams == null)
            {
                BLTeamSelectTeamVM teamSpectators = TeamSpectators;
                if (teamSpectators != null)
                {
                    teamSpectators.SetIsDisabled(false, false);
                }
                BLTeamSelectTeamVM team = Team1;
                if (team != null)
                {
                    team.SetIsDisabled(false, false);
                }
                BLTeamSelectTeamVM team2 = Team2;
                if (team2 == null)
                {
                    return;
                }
                team2.SetIsDisabled(false, false);
                return;
            }
            else
            {
                BLTeamSelectTeamVM teamSpectators2 = TeamSpectators;
                if (teamSpectators2 != null)
                {
                    bool flag = false;
                    bool flag2;
                    if (disabledTeams == null)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        BLTeamSelectTeamVM teamSpectators3 = TeamSpectators;
                        flag2 = disabledTeams.Contains(teamSpectators3 != null ? teamSpectators3.Team : null);
                    }
                    teamSpectators2.SetIsDisabled(flag, flag2);
                }
                BLTeamSelectTeamVM team3 = Team1;
                if (team3 != null)
                {
                    BLTeamSelectTeamVM team4 = Team1;
                    Team team5 = team4 != null ? team4.Team : null;
                    MissionPeer missionPeer = _missionPeer;
                    bool flag3 = team5 == (missionPeer != null ? missionPeer.Team : null);
                    bool flag4;
                    if (disabledTeams == null)
                    {
                        flag4 = false;
                    }
                    else
                    {
                        BLTeamSelectTeamVM team6 = Team1;
                        flag4 = disabledTeams.Contains(team6 != null ? team6.Team : null);
                    }
                    team3.SetIsDisabled(flag3, flag4);
                }
                BLTeamSelectTeamVM team7 = Team2;
                if (team7 == null)
                {
                    return;
                }
                BLTeamSelectTeamVM team8 = Team2;
                Team team9 = team8 != null ? team8.Team : null;
                MissionPeer missionPeer2 = _missionPeer;
                bool flag5 = team9 == (missionPeer2 != null ? missionPeer2.Team : null);
                bool flag6;
                if (disabledTeams == null)
                {
                    flag6 = false;
                }
                else
                {
                    BLTeamSelectTeamVM team10 = Team2;
                    flag6 = disabledTeams.Contains(team10 != null ? team10.Team : null);
                }
                team7.SetIsDisabled(flag5, flag6);
                return;
            }
        }

        public void RefreshPlayerAndBotCount(int playersCountOne, int playersCountTwo, int botsCountOne, int botsCountTwo)
        {
            MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountOne.ToString(), false);
            Team1.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", null).ToString();
            MBTextManager.SetTextVariable("BOT_COUNT", botsCountOne.ToString(), false);
            Team1.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", null).ToString();
            MBTextManager.SetTextVariable("PLAYER_COUNT", playersCountTwo.ToString(), false);
            Team2.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", null).ToString();
            MBTextManager.SetTextVariable("BOT_COUNT", botsCountTwo.ToString(), false);
            Team2.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", null).ToString();
        }

        public void RefreshFriendsPerTeam(IEnumerable<MissionPeer> friendsTeamOne, IEnumerable<MissionPeer> friendsTeamTwo)
        {
            Team1.RefreshFriends(friendsTeamOne);
            Team2.RefreshFriends(friendsTeamTwo);
        }

        [UsedImplicitly]
        public void ExecuteCancel()
        {
            _onClose();
        }

        [UsedImplicitly]
        public void ExecuteAutoAssign()
        {
            _onAutoAssign();
        }

        [DataSourceProperty]
        public BLTeamSelectTeamVM Team1
        {
            get
            {
                return _team1;
            }
            set
            {
                if (value != _team1)
                {
                    _team1 = value;
                    OnPropertyChangedWithValue(value, "Team1");
                }
            }
        }

        [DataSourceProperty]
        public BLTeamSelectTeamVM Team2
        {
            get
            {
                return _team2;
            }
            set
            {
                if (value != _team2)
                {
                    _team2 = value;
                    OnPropertyChangedWithValue(value, "Team2");
                }
            }
        }

        [DataSourceProperty]
        public BLTeamSelectTeamVM TeamSpectators
        {
            get
            {
                return _teamSpectators;
            }
            set
            {
                if (value != _teamSpectators)
                {
                    _teamSpectators = value;
                    OnPropertyChangedWithValue(value, "TeamSpectators");
                }
            }
        }

        [DataSourceProperty]
        public string TeamSelectTitle
        {
            get
            {
                return _teamSelectTitle;
            }
            set
            {
                _teamSelectTitle = value;
                OnPropertyChangedWithValue(value, "TeamSelectTitle");
            }
        }

        [DataSourceProperty]
        public bool IsRoundCountdownAvailable
        {
            get
            {
                return _isRoundCountdownAvailable;
            }
            set
            {
                if (value != _isRoundCountdownAvailable)
                {
                    _isRoundCountdownAvailable = value;
                    OnPropertyChangedWithValue(value, "IsRoundCountdownAvailable");
                }
            }
        }

        [DataSourceProperty]
        public string RemainingRoundTime
        {
            get
            {
                return _remainingRoundTime;
            }
            set
            {
                if (value != _remainingRoundTime)
                {
                    _remainingRoundTime = value;
                    OnPropertyChangedWithValue(value, "RemainingRoundTime");
                }
            }
        }

        [DataSourceProperty]
        public string GamemodeLbl
        {
            get
            {
                return _gamemodeLbl;
            }
            set
            {
                _gamemodeLbl = value;
                OnPropertyChangedWithValue(value, "GamemodeLbl");
            }
        }

        [DataSourceProperty]
        public string AutoassignLbl
        {
            get
            {
                return _autoassignLbl;
            }
            set
            {
                _autoassignLbl = value;
                OnPropertyChangedWithValue(value, "AutoassignLbl");
            }
        }

        [DataSourceProperty]
        public bool IsCancelDisabled
        {
            get
            {
                return _isCancelDisabled;
            }
            set
            {
                _isCancelDisabled = value;
                OnPropertyChangedWithValue(value, "IsCancelDisabled");
            }
        }
    }
}
