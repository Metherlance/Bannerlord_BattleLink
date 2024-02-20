using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.GauntletUI.BaseTypes;
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

        private BLTeamSelectTeamVM _teamAttVM;

        private BLTeamSelectTeamVM _teamDefVM;

        private BLTeamSelectTeamVM _team3;

        private BLTeamSelectTeamVM _team4;

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

            Team teamSpec = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.None);
            TeamSpectators = new BLTeamSelectTeamVM(missionBehavior, teamSpec, null, null, onChangeTeamTo);
            
            Team teamAtt = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.Attacker);
            BasicCultureObject basicCultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
            Team1 = new BLTeamSelectTeamVM(missionBehavior, teamAtt, basicCultureObject, BannerCode.CreateFrom(teamAtt.Banner), onChangeTeamTo);

            Team teamAtt2 = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.Attacker && !t.Equals(teamAtt));
            if (teamAtt2 != null)
            {
                var bannerCode = BannerCode.CreateFrom(teamAtt2.Banner);
                //var bannerCode = BannerCode.CreateFrom("34.154.154.1536.1536.764.764.1.0.0.411.155.155.454.454.764.764.0.0.0");
                Team3 = new BLTeamSelectTeamVM(missionBehavior, teamAtt2, basicCultureObject, bannerCode, onChangeTeamTo);
                IsTeam3Hidden = false;
                Width+= 400;
            }
            else
            {
                Team3 = Team1;
                IsTeam3Hidden = true;
            }

            Team teamDef = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.Defender);
            basicCultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue(MultiplayerOptions.MultiplayerOptionsAccessMode.CurrentMapOptions));
            Team2 = new BLTeamSelectTeamVM(missionBehavior, teamDef, basicCultureObject, BannerCode.CreateFrom(teamDef.Banner), onChangeTeamTo);

            Team teamDef2 = teams.FirstOrDefault((t) => t.Side == BattleSideEnum.Defender && !t.Equals(teamDef));
            if (teamDef2 != null)
            {
                Team4 = new BLTeamSelectTeamVM(missionBehavior, teamDef2, basicCultureObject, BannerCode.CreateFrom(teamDef2.Banner), onChangeTeamTo);
                IsTeam4Hidden = false;
                Width += 400;
            }
            else
            {
                Team4 = Team2;
                IsTeam4Hidden = true;
            }


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
            _teamAttVM.RefreshValues();
            _teamDefVM.RefreshValues();
            _team3.RefreshValues();
            _team4.RefreshValues();
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

                if (Team3!= null)
                {
                    Team3.SetIsDisabled(false, false);
                }

                if (Team4 != null)
                {
                    Team4.SetIsDisabled(false, false);
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
                BLTeamSelectTeamVM teamSpectatorsDup = TeamSpectators;
                if (teamSpectatorsDup != null)
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
                    teamSpectatorsDup.SetIsDisabled(flag, flag2);
                }
                
                BLTeamSelectTeamVM team1VM = Team1;
                if (team1VM != null)
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
                    team1VM.SetIsDisabled(flag3, flag4);
                }

                if (Team3 != null)
                {
                    bool isInTeam = Team3.Team == (_missionPeer != null ? _missionPeer.Team : null);
                    bool isDisableTeam = disabledTeams!=null && disabledTeams.Contains(Team3.Team);
                    Team3.SetIsDisabled(isInTeam, isDisableTeam);
                }

                if (Team4 != null)
                {
                    bool isInTeam = Team4.Team == (_missionPeer != null ? _missionPeer.Team : null);
                    bool isDisableTeam = disabledTeams != null && disabledTeams.Contains(Team4.Team);
                    Team4.SetIsDisabled(isInTeam, isDisableTeam);
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


        public void RefreshPlayerAndBotCount(Team team, int playerCountForTeam, int nbBot)
        {
            if (team == Team1.Team)
            {
                RefreshPlayerAndBotCount(Team1, playerCountForTeam, nbBot);
            }
            else if (team == Team2.Team)
            {
                RefreshPlayerAndBotCount(Team2, playerCountForTeam, nbBot);
            }
            else if (team == Team3.Team)
            {
                RefreshPlayerAndBotCount(Team3, playerCountForTeam, nbBot);
            }
            else if (team == Team4.Team)
            {
                RefreshPlayerAndBotCount(Team4, playerCountForTeam, nbBot);
            }
        }

        private static void RefreshPlayerAndBotCount(BLTeamSelectTeamVM team, int playerCountForTeam, int nbBot)
        {
            MBTextManager.SetTextVariable("PLAYER_COUNT", playerCountForTeam.ToString(), false);
            team.DisplayedSecondary = new TextObject("{=Etjqamlh}{PLAYER_COUNT} Players", null).ToString();
            MBTextManager.SetTextVariable("BOT_COUNT", nbBot.ToString(), false);
            team.DisplayedSecondarySub = new TextObject("{=eCOJSSUH}({BOT_COUNT} Bots)", null).ToString();
        }


        public void RefreshFriendsForTeam(Team team, IEnumerable<MissionPeer> eFriends)
        {
            if (team==Team1.Team)
            {
                Team1.RefreshFriends(eFriends);
            }
            else if(team == Team2.Team)
            {
                Team2.RefreshFriends(eFriends);
            }
            else if (team == Team3.Team)
            {
                Team3.RefreshFriends(eFriends);
            }
            else if (team == Team4.Team)
            {
                Team4.RefreshFriends(eFriends);
            }
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
                return _teamAttVM;
            }
            set
            {
                if (value != _teamAttVM)
                {
                    _teamAttVM = value;
                    OnPropertyChangedWithValue(value, "Team1");
                }
            }
        }

        [DataSourceProperty]
        public BLTeamSelectTeamVM Team2
        {
            get
            {
                return _teamDefVM;
            }
            set
            {
                if (value != _teamDefVM)
                {
                    _teamDefVM = value;
                    OnPropertyChangedWithValue(value, "Team2");
                }
            }
        }

        [DataSourceProperty]
        public BLTeamSelectTeamVM Team3
        {
            get
            {
                return _team3;
            }
            set
            {
                if (value != _team3)
                {
                    _team3 = value;
                    OnPropertyChangedWithValue(value, "Team3");
                }
            }
        }

        [DataSourceProperty]
        public BLTeamSelectTeamVM Team4
        {
            get
            {
                return _team4;
            }
            set
            {
                if (value != _team4)
                {
                    _team4= value;
                    OnPropertyChangedWithValue(value, "Team4");
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

        private bool _isTeam3Hidden;
        [DataSourceProperty]
        public bool IsTeam3Hidden
        {
            get
            {
                return _isTeam3Hidden;
            }
            set
            {
                _isTeam3Hidden = value;
                OnPropertyChangedWithValue(value, "IsTeam3Hidden");
            }
        }

        private bool _isTeam4Hidden;
        [DataSourceProperty]
        public bool IsTeam4Hidden 
        { 
            get
            {
                 return _isTeam4Hidden;
            }
            set
            {
                _isTeam4Hidden = value;
                OnPropertyChangedWithValue(value, "IsTeam4Hidden");
            } 
        }


        [DataSourceProperty]
        public float Width = 800;
    }
}
