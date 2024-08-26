using BattleLink.Common.Model;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using BattleLink.Handler;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Diamond;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection;
using TaleWorlds.MountAndBlade.Multiplayer.ViewModelCollection.ClassLoadout;
using TaleWorlds.ObjectSystem;
using TaleWorlds.SaveSystem;
using static TaleWorlds.MountAndBlade.MultiplayerClassDivisions;

namespace BattleLink.Views.Class
{
    public class BLMultiplayerClassLoadoutVM : ViewModel
    {
        public const float UPDATE_INTERVAL = 1f;
        private float _updateTimeElapsed;
        private readonly Action<MultiplayerClassDivisions.MPHeroClass> _onRefreshSelection;
        private readonly MissionMultiplayerGameModeBaseClient _missionMultiplayerGameMode;
        private Dictionary<MissionPeer, MPPlayerVM> _enemyDictionary;
        private readonly Mission _mission;
        private bool _isTeammateAndEnemiesRelevant;
        private const float REMAINING_TIME_WARNING_THRESHOLD = 5f;
        private MissionLobbyEquipmentNetworkComponent _missionLobbyEquipmentNetworkComponent;
        private bool _isInitializing;
        private Dictionary<MissionPeer, MPPlayerVM> _teammateDictionary;
        private int _gold;
        private string _culture;
        private string _cultureId;
        private string _spawnLabelText;
        private string _spawnForfeitLabelText;
        private string _remainingTimeText;
        private bool _warnRemainingTime;
        private bool _isSpawnTimerVisible;
        private bool _isSpawnLabelVisible;
        private bool _isSpawnForfeitLabelVisible;
        private bool _isGoldEnabled;
        private bool _isInWarmup;
        private bool _showAttackerOrDefenderIcons;
        private bool _isAttacker;
        private string _warmupInfoText;
        private Color _cultureColor1;
        private Color _cultureColor2;
        private MBBindingList<HeroClassGroupVM> _classes;
        private HeroInformationVM _heroInformation;
        private HeroClassVM _currentSelectedClass;
        private MBBindingList<MPPlayerVM> _teammates;
        private MBBindingList<MPPlayerVM> _enemies;

        private MissionRepresentativeBase missionRep => GameNetwork.MyPeer?.VirtualPlayer?.GetComponent<MissionRepresentativeBase>();

        private Team _playerTeam
        {
            get
            {
                if (!GameNetwork.IsMyPeerReady)
                    return (Team)null;
                MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();
                return component.Team == null || component.Team.Side == BattleSideEnum.None ? (Team)null : component.Team;
            }
        }

        public BLMultiplayerClassLoadoutVM(
          MissionMultiplayerGameModeBaseClient gameMode,
          Action<MultiplayerClassDivisions.MPHeroClass> onRefreshSelection,
          MultiplayerClassDivisions.MPHeroClass initialHeroSelection)
        {
            MBTextManager.SetTextVariable("newline", "\n", false);
            this._isInitializing = true;
            this._onRefreshSelection = onRefreshSelection;
            this._missionMultiplayerGameMode = gameMode;
            this._mission = gameMode.Mission;
            Team team = GameNetwork.MyPeer.GetComponent<MissionPeer>().Team;
            this.Classes = new MBBindingList<HeroClassGroupVM>();
            this.HeroInformation = new HeroInformationVM();
            this._enemyDictionary = new Dictionary<MissionPeer, MPPlayerVM>();
            this._missionLobbyEquipmentNetworkComponent = Mission.Current.GetMissionBehavior<MissionLobbyEquipmentNetworkComponent>();
            this.IsGoldEnabled = this._missionMultiplayerGameMode.IsGameModeUsingGold;
            if (this.IsGoldEnabled)
                this.Gold = this._missionMultiplayerGameMode.GetGoldAmount();
            HeroClassVM heroClass = (HeroClassVM)null;


            //foreach (MultiplayerClassDivisions.MPHeroClassGroup multiplayerHeroClassGroup in listClassDiv)// MultiplayerClassDivisions.MultiplayerHeroClassGroups
            //{
            //    HeroClassGroupVM heroClassGroupVm = new HeroClassGroupVM(new Action<HeroClassVM>(this.RefreshCharacter), new Action<HeroPerkVM, MPPerkVM>(this.OnSelectPerk), multiplayerHeroClassGroup, this.UseSecondary);
            //    if (heroClassGroupVm.IsValid)
            //        this.Classes.Add(heroClassGroupVm);
            //}

            foreach (var HeroClassGroupVM in getHeroClassDivisionsVMBy(team))
            {
                if (HeroClassGroupVM.IsValid)
                    this.Classes.Add(HeroClassGroupVM);
            }

            int goldCost = initialHeroSelection != null ? (!gameMode.IsGameModeUsingCasualGold ? (gameMode.GameType == MultiplayerGameType.Battle ? initialHeroSelection.TroopBattleCost : initialHeroSelection.TroopCost) : initialHeroSelection.TroopCasualCost) : 0;
            if (initialHeroSelection == null || this.IsGoldEnabled && goldCost > this.Gold)
            {
                HeroClassGroupVM heroClassGroupVm = this.Classes.FirstOrDefault<HeroClassGroupVM>();
                heroClass = heroClassGroupVm != null ? heroClassGroupVm.SubClasses.FirstOrDefault<HeroClassVM>() : (HeroClassVM)null;
            }
            else
            {
                foreach (HeroClassGroupVM heroClassGroupVm in (Collection<HeroClassGroupVM>)this.Classes)
                {
                    foreach (HeroClassVM subClass in (Collection<HeroClassVM>)heroClassGroupVm.SubClasses)
                    {
                        if (subClass.HeroClass == initialHeroSelection)
                        {
                            heroClass = subClass;
                            break;
                        }
                    }
                    if (heroClass != null)
                        break;
                }
                if (heroClass == null)
                {
                    HeroClassGroupVM heroClassGroupVm = this.Classes.FirstOrDefault<HeroClassGroupVM>();
                    heroClass = heroClassGroupVm != null ? heroClassGroupVm.SubClasses.FirstOrDefault<HeroClassVM>() : (HeroClassVM)null;
                }
            }

            this._isInitializing = false;
            this.RefreshCharacter(heroClass);
            this._teammateDictionary = new Dictionary<MissionPeer, MPPlayerVM>();
            this.Teammates = new MBBindingList<MPPlayerVM>();
            this.Enemies = new MBBindingList<MPPlayerVM>();
            MissionPeer.OnEquipmentIndexRefreshed += new MissionPeer.OnUpdateEquipmentSetIndexEventDelegate(this.RefreshPeerDivision);
            MissionPeer.OnPerkSelectionUpdated += new MissionPeer.OnPerkUpdateEventDelegate(this.RefreshPeerPerkSelection);
            NetworkCommunicator.OnPeerComponentAdded += new Action<PeerComponent>(this.OnPeerComponentAdded);
            BasicCultureObject culture = GameNetwork.MyPeer.GetComponent<MissionPeer>().Culture;
            this.CultureId = culture.StringId;
            this.CultureColor1 = Color.FromUint(culture.Color);
            this.CultureColor2 = Color.FromUint(culture.Color2);
            if (Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>())
            {
                this.ShowAttackerOrDefenderIcons = true;
                this.IsAttacker = team.Side == BattleSideEnum.Attacker;
            }
            this.RefreshValues();
            this._isTeammateAndEnemiesRelevant = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().IsGameModeTactical && !Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>() && Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().GameType != MultiplayerGameType.Battle;
            if (!this._isTeammateAndEnemiesRelevant)
                return;
            this.OnRefreshTeamMembers();
            this.OnRefreshEnemyMembers();
        }

        private List<HeroClassGroupVM> getHeroClassDivisionsVMBy(Team team)
        {
            List<string> listType = new List<string>(){ "Infantry" , "Ranged" , "Cavalry" , "HorseArcher" };
            List<HeroClassGroupVM> heroClassGroupsVM = new List<HeroClassGroupVM>();
            foreach (var type in listType)
            {
                var heroClassGroup = new MultiplayerClassDivisions.MPHeroClassGroup("Infantry");
                HeroClassGroupVM heroClassGroupVm = new HeroClassGroupVM(new Action<HeroClassVM>(this.RefreshCharacter), new Action<HeroPerkVM, MPPerkVM>(this.OnSelectPerk), heroClassGroup, this.UseSecondary);
                heroClassGroupsVM.Add(heroClassGroupVm);
                heroClassGroupVm.SubClasses.Clear();
            }

            var teamCharacters = BLTeamCharactersHandler.teamCharacters[team.TeamIndex];
            if (teamCharacters!=null)
            {
                foreach (var charater in teamCharacters.characterObjects)
                {                    
                    MPHeroClass heroClass = MBObjectManager.Instance.GetObject<MPHeroClass>(charater.StringId + "_class_division");
                    HeroClassVM heroClassVM = new HeroClassVM(new Action<HeroClassVM>(this.RefreshCharacter), new Action<HeroPerkVM, MPPerkVM>(this.OnSelectPerk), heroClass, this.UseSecondary);
                    switch (charater.ClassGroup.ToString()) // check that old : DefaultFormationClass
                    {
                        case "Infantry":
                            heroClassGroupsVM[0].SubClasses.Add(heroClassVM);
                            break;
                        case "Ranged":
                            heroClassGroupsVM[1].SubClasses.Add(heroClassVM);
                            break;
                        case "Cavalry":
                            heroClassGroupsVM[2].SubClasses.Add(heroClassVM);
                            break;
                        case "HorseArcher":
                            heroClassGroupsVM[3].SubClasses.Add(heroClassVM);
                            break;
                    }
                }
            }           

            foreach (var heroClassGroupVm in heroClassGroupsVM)
            {
                heroClassGroupVm.RefreshValues();
            }

            return heroClassGroupsVM;
        }

        public override void RefreshValues()
        {
            base.RefreshValues();
            this.UpdateSpawnAndTimerLabels();
            string strValue = MultiplayerOptions.OptionType.GameType.GetStrValue();
            TextObject textObject = new TextObject("{=XJTX8w8M}Warmup Phase - {GAME_MODE}\nWaiting for players to join");
            textObject.SetTextVariable("GAME_MODE", GameTexts.FindText("str_multiplayer_official_game_type_name", strValue));
            this.WarmupInfoText = textObject.ToString();
            this.Culture = GameNetwork.MyPeer.GetComponent<MissionPeer>().Culture.Name.ToString();
            this.Classes.ApplyActionOnAllItems((Action<HeroClassGroupVM>)(x => x.RefreshValues()));
            this.CurrentSelectedClass.RefreshValues();
            this.HeroInformation.RefreshValues();
        }

        private void UpdateSpawnAndTimerLabels()
        {
            GameTexts.SetVariable("USE_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", 13)));
            this.SpawnLabelText = GameTexts.FindText("str_skirmish_battle_press_action_to_spawn").ToString();
            if (this._missionMultiplayerGameMode.RoundComponent != null)
            {
                if (this._missionMultiplayerGameMode.IsInWarmup || this._missionMultiplayerGameMode.IsRoundInProgress)
                {
                    this.IsSpawnTimerVisible = false;
                    this.IsSpawnLabelVisible = true;
                    if (!this._missionMultiplayerGameMode.IsRoundInProgress || (!(this._missionMultiplayerGameMode is MissionMultiplayerGameModeFlagDominationClient) ? 0 : (this._missionMultiplayerGameMode.GameType == MultiplayerGameType.Skirmish ? 1 : 0)) == 0 || GameNetwork.MyPeer.GetComponent<MissionPeer>() == null)
                        return;
                    this.IsSpawnForfeitLabelVisible = true;
                    GameTexts.SetVariable("ALT_WEAP_KEY", HyperlinkTexts.GetKeyHyperlinkText(HotKeyManager.GetHotKeyId("CombatHotKeyCategory", "ForfeitSpawn")));
                    this.SpawnForfeitLabelText = GameTexts.FindText("str_skirmish_battle_press_alternative_to_forfeit_spawning").ToString();
                }
                else
                    this.IsSpawnTimerVisible = true;
            }
            else
            {
                this.IsSpawnTimerVisible = false;
                this.IsSpawnLabelVisible = true;
            }
        }

        public override void OnFinalize()
        {
            base.OnFinalize();
            MissionPeer.OnEquipmentIndexRefreshed -= new MissionPeer.OnUpdateEquipmentSetIndexEventDelegate(this.RefreshPeerDivision);
            MissionPeer.OnPerkSelectionUpdated -= new MissionPeer.OnPerkUpdateEventDelegate(this.RefreshPeerPerkSelection);
            NetworkCommunicator.OnPeerComponentAdded -= new Action<PeerComponent>(this.OnPeerComponentAdded);
        }

        private void RefreshCharacter(HeroClassVM heroClass)
        {
            if (this._isInitializing)
            {
                return;
            }
            foreach (HeroClassGroupVM heroClassGroupVm in (Collection<HeroClassGroupVM>)this.Classes)
            {
                foreach (HeroClassVM subClass in (Collection<HeroClassVM>)heroClassGroupVm.SubClasses)
                {
                    subClass.IsSelected = false;
                }
            }

            heroClass.IsSelected = true;
            this.CurrentSelectedClass = heroClass;
            
            RequestUsage(heroClass.HeroClass);

            if (GameNetwork.IsMyPeerReady)
                GameNetwork.MyPeer.GetComponent<MissionPeer>().NextSelectedTroopIndex = MultiplayerClassDivisions.GetMPHeroClasses(heroClass.HeroClass.Culture).ToList<MultiplayerClassDivisions.MPHeroClass>().IndexOf(heroClass.HeroClass);
            this.HeroInformation.RefreshWith(heroClass.HeroClass, heroClass.SelectedPerks);
            this._missionLobbyEquipmentNetworkComponent.EquipmentUpdated();
            if (this._missionMultiplayerGameMode.IsGameModeUsingGold)
                this.Gold = this._missionMultiplayerGameMode.GetGoldAmount();
            this.HeroInformation.RefreshWith(this.HeroInformation.HeroClass, heroClass.Perks.Select<HeroPerkVM, IReadOnlyPerkObject>((Func<HeroPerkVM, IReadOnlyPerkObject>)(x => x.SelectedPerk)).ToList<IReadOnlyPerkObject>());
            List<Tuple<HeroPerkVM, MPPerkVM>> tupleList = new List<Tuple<HeroPerkVM, MPPerkVM>>();
            foreach (HeroPerkVM perk in (Collection<HeroPerkVM>)heroClass.Perks)
                tupleList.Add(new Tuple<HeroPerkVM, MPPerkVM>(perk, perk.SelectedPerkItem));
            tupleList.ForEach((Action<Tuple<HeroPerkVM, MPPerkVM>>)(p => this.OnSelectPerk(p.Item1, p.Item2)));
            Action<MultiplayerClassDivisions.MPHeroClass> refreshSelection = this._onRefreshSelection;
            if (refreshSelection == null)
                return;
            refreshSelection(heroClass.HeroClass);
        }


        public void RequestUsage(MPHeroClass heroClass)
        {
            InformationManager.DisplayMessage(new InformationMessage("click on "+ heroClass.Id+" "+ heroClass.TroopName.ToString(), Colors.Cyan));
           // Print(message, 0, GetConsoleColorForLogLevel(level));
            //Console.WriteLine("test2");
            Debug.Print("test3");//C:\ProgramData\Mount and Blade II Bannerlord\logs
            MBDebug.Print("test4");
            //Debug.PrintError("test4");
            // Debug.WriteDebugLineOnScreen("test5");
            Team team = GameNetwork.MyPeer.GetComponent<MissionPeer>().Team;

            //Log($"Requesting usage of {character.Name}", LogLevel.Debug);
            GameNetwork.BeginModuleEventAsClient();
            GameNetwork.WriteMessage(new BLRequestPartyTroopUsageMessage(team.TeamIndex, heroClass));
            GameNetwork.EndModuleEventAsClient();
        }


        private void OnSelectPerk(HeroPerkVM heroPerk, MPPerkVM candidate)
        {
            if (!GameNetwork.IsMyPeerReady || this.HeroInformation.HeroClass == null || this.CurrentSelectedClass == null)
                return;
            MissionPeer component = GameNetwork.MyPeer.GetComponent<MissionPeer>();
            if ((!GameNetwork.IsServer ? 1 : (component.SelectPerk(heroPerk.PerkIndex, candidate.PerkIndex) ? 1 : 0)) != 0)
                this._missionLobbyEquipmentNetworkComponent.PerkUpdated(heroPerk.PerkIndex, candidate.PerkIndex);
            List<IReadOnlyPerkObject> list = this.CurrentSelectedClass.Perks.Select<HeroPerkVM, IReadOnlyPerkObject>((Func<HeroPerkVM, IReadOnlyPerkObject>)(x => x.SelectedPerk)).ToList<IReadOnlyPerkObject>();
            if (list.Count <= 0)
                return;
            this.HeroInformation.RefreshWith(this.HeroInformation.HeroClass, list);
        }

        public void RefreshPeerDivision(MissionPeer peer, int divisionType) => this.Teammates.FirstOrDefault<MPPlayerVM>((Func<MPPlayerVM, bool>)(t => t.Peer == peer))?.RefreshDivision();

        private void RefreshPeerPerkSelection(MissionPeer peer) => this.Teammates.FirstOrDefault<MPPlayerVM>((Func<MPPlayerVM, bool>)(t => t.Peer == peer))?.RefreshActivePerks();

        public void Tick(float dt)
        {
            if (this._missionMultiplayerGameMode != null)
            {
                this.IsInWarmup = this._missionMultiplayerGameMode.IsInWarmup;
                this.IsGoldEnabled = !this.IsInWarmup && this._missionMultiplayerGameMode.IsGameModeUsingGold;
                if (this.IsGoldEnabled)
                    this.Gold = this._missionMultiplayerGameMode.GetGoldAmount();
                foreach (HeroClassGroupVM heroClassGroupVm in (Collection<HeroClassGroupVM>)this.Classes)
                {
                    foreach (HeroClassVM subClass in (Collection<HeroClassVM>)heroClassGroupVm.SubClasses)
                        subClass.IsGoldEnabled = this.IsGoldEnabled;
                }
            }
            this.RefreshRemainingTime();
            this._updateTimeElapsed += dt;
            if ((double)this._updateTimeElapsed < 1.0)
                return;
            this._updateTimeElapsed = 0.0f;
            if (!this._isTeammateAndEnemiesRelevant)
                return;
            this.OnRefreshTeamMembers();
            this.OnRefreshEnemyMembers();
        }

        private void OnPeerComponentAdded(PeerComponent component)
        {
            if (!component.IsMine || !(component is MissionRepresentativeBase))
                return;
            this._isTeammateAndEnemiesRelevant = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeBaseClient>().IsGameModeTactical && !Mission.Current.HasMissionBehavior<MissionMultiplayerSiegeClient>();
            if (!this._isTeammateAndEnemiesRelevant)
                return;
            this.OnRefreshTeamMembers();
            this.OnRefreshEnemyMembers();
        }

        private void OnRefreshTeamMembers()
        {
            List<MPPlayerVM> list = this.Teammates.ToList<MPPlayerVM>();
            foreach (MissionPeer peer in VirtualPlayer.Peers<MissionPeer>())
            {
                if (peer.GetNetworkPeer().GetComponent<MissionPeer>() != null && this._playerTeam != null && peer.Team == this._playerTeam)
                {
                    if (!this._teammateDictionary.ContainsKey(peer))
                    {
                        MPPlayerVM mpPlayerVm = new MPPlayerVM(peer);
                        this.Teammates.Add(mpPlayerVm);
                        this._teammateDictionary.Add(peer, mpPlayerVm);
                    }
                    else
                        list.Remove(this._teammateDictionary[peer]);
                }
            }
            foreach (MPPlayerVM mpPlayerVm in list)
            {
                this.Teammates.Remove(mpPlayerVm);
                this._teammateDictionary.Remove(mpPlayerVm.Peer);
            }
            foreach (MPPlayerVM teammate in (Collection<MPPlayerVM>)this.Teammates)
            {
                if (teammate.CompassElement == null)
                    teammate.RefreshDivision();
            }
        }

        private void OnRefreshEnemyMembers()
        {
            List<MPPlayerVM> list = this.Enemies.ToList<MPPlayerVM>();
            foreach (MissionPeer peer in VirtualPlayer.Peers<MissionPeer>())
            {
                if (peer.GetNetworkPeer().GetComponent<MissionPeer>() != null && this._playerTeam != null && peer.Team != null && peer.Team != this._playerTeam && peer.Team != Mission.Current.SpectatorTeam)
                {
                    if (!this._enemyDictionary.ContainsKey(peer))
                    {
                        MPPlayerVM mpPlayerVm = new MPPlayerVM(peer);
                        this.Enemies.Add(mpPlayerVm);
                        this._enemyDictionary.Add(peer, mpPlayerVm);
                    }
                    else
                        list.Remove(this._enemyDictionary[peer]);
                }
            }
            foreach (MPPlayerVM mpPlayerVm in list)
            {
                this.Enemies.Remove(mpPlayerVm);
                this._enemyDictionary.Remove(mpPlayerVm.Peer);
            }
            foreach (MPPlayerVM enemy in (Collection<MPPlayerVM>)this.Enemies)
            {
                enemy.RefreshDivision();
                enemy.UpdateDisabled();
            }
        }

        public void OnPeerEquipmentRefreshed(MissionPeer peer)
        {
            if (this._teammateDictionary.ContainsKey(peer))
            {
                this._teammateDictionary[peer].RefreshActivePerks();
            }
            else
            {
                if (!this._enemyDictionary.ContainsKey(peer))
                    return;
                this._enemyDictionary[peer].RefreshActivePerks();
            }
        }

        public void OnGoldUpdated()
        {
            foreach (HeroClassGroupVM heroClassGroupVm in (Collection<HeroClassGroupVM>)this.Classes)
                heroClassGroupVm.SubClasses.ApplyActionOnAllItems((Action<HeroClassVM>)(sc => sc.UpdateEnabled()));
        }

        public void RefreshRemainingTime()
        {
            int num = MathF.Ceiling(this._missionMultiplayerGameMode.RemainingTime);
            this.RemainingTimeText = TimeSpan.FromSeconds((double)num).ToString("mm':'ss");
            this.WarnRemainingTime = (double)num < 5.0;
        }

        [DataSourceProperty]
        public string Culture
        {
            get => this._culture;
            set
            {
                if (!(value != this._culture))
                    return;
                this._culture = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(Culture));
            }
        }

        [DataSourceProperty]
        public Color CultureColor1
        {
            get => this._cultureColor1;
            set
            {
                if (!(value != this._cultureColor1))
                    return;
                this._cultureColor1 = value;
                this.OnPropertyChangedWithValue(value, nameof(CultureColor1));
            }
        }

        [DataSourceProperty]
        public Color CultureColor2
        {
            get => this._cultureColor2;
            set
            {
                if (!(value != this._cultureColor2))
                    return;
                this._cultureColor2 = value;
                this.OnPropertyChangedWithValue(value, nameof(CultureColor2));
            }
        }

        [DataSourceProperty]
        public string CultureId
        {
            get => this._cultureId;
            set
            {
                if (!(value != this._cultureId))
                    return;
                this._cultureId = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(CultureId));
            }
        }

        [DataSourceProperty]
        public bool IsSpawnTimerVisible
        {
            get => this._isSpawnTimerVisible;
            set
            {
                if (value == this._isSpawnTimerVisible)
                    return;
                this._isSpawnTimerVisible = value;
                this.OnPropertyChangedWithValue(value, nameof(IsSpawnTimerVisible));
            }
        }

        [DataSourceProperty]
        public string SpawnLabelText
        {
            get => this._spawnLabelText;
            set
            {
                if (!(value != this._spawnLabelText))
                    return;
                this._spawnLabelText = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(SpawnLabelText));
            }
        }

        [DataSourceProperty]
        public bool IsSpawnLabelVisible
        {
            get => this._isSpawnLabelVisible;
            set
            {
                if (value == this._isSpawnLabelVisible)
                    return;
                this._isSpawnLabelVisible = value;
                this.OnPropertyChangedWithValue(value, nameof(IsSpawnLabelVisible));
            }
        }

        [DataSourceProperty]
        public bool UseSecondary
        {
            get => false;//color
        }

        [DataSourceProperty]
        public bool ShowAttackerOrDefenderIcons
        {
            get => this._showAttackerOrDefenderIcons;
            set
            {
                if (value == this._showAttackerOrDefenderIcons)
                    return;
                this._showAttackerOrDefenderIcons = value;
                this.OnPropertyChangedWithValue(value, nameof(ShowAttackerOrDefenderIcons));
            }
        }

        [DataSourceProperty]
        public bool IsAttacker
        {
            get => this._isAttacker;
            set
            {
                if (value == this._isAttacker)
                    return;
                this._isAttacker = value;
                this.OnPropertyChangedWithValue(value, nameof(IsAttacker));
            }
        }

        [DataSourceProperty]
        public string SpawnForfeitLabelText
        {
            get => this._spawnForfeitLabelText;
            set
            {
                if (!(value != this._spawnForfeitLabelText))
                    return;
                this._spawnForfeitLabelText = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(SpawnForfeitLabelText));
            }
        }

        [DataSourceProperty]
        public bool IsSpawnForfeitLabelVisible
        {
            get => this._isSpawnForfeitLabelVisible;
            set
            {
                if (value == this._isSpawnForfeitLabelVisible)
                    return;
                this._isSpawnForfeitLabelVisible = value;
                this.OnPropertyChangedWithValue(value, nameof(IsSpawnForfeitLabelVisible));
            }
        }

        [DataSourceProperty]
        public int Gold
        {
            get => this._gold;
            set
            {
                if (value == this._gold)
                    return;
                this._gold = value;
                this.OnPropertyChangedWithValue(value, nameof(Gold));
            }
        }

        [DataSourceProperty]
        public MBBindingList<MPPlayerVM> Teammates
        {
            get => this._teammates;
            set
            {
                if (value == this._teammates)
                    return;
                this._teammates = value;
                this.OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, nameof(Teammates));
            }
        }

        [DataSourceProperty]
        public MBBindingList<MPPlayerVM> Enemies
        {
            get => this._enemies;
            set
            {
                if (value == this._enemies)
                    return;
                this._enemies = value;
                this.OnPropertyChangedWithValue<MBBindingList<MPPlayerVM>>(value, nameof(Enemies));
            }
        }

        [DataSourceProperty]
        public HeroInformationVM HeroInformation
        {
            get => this._heroInformation;
            set
            {
                if (value == this._heroInformation)
                    return;
                this._heroInformation = value;
                this.OnPropertyChangedWithValue<HeroInformationVM>(value, nameof(HeroInformation));
            }
        }

        [DataSourceProperty]
        public HeroClassVM CurrentSelectedClass
        {
            get => this._currentSelectedClass;
            set
            {
                if (value == this._currentSelectedClass)
                    return;
                this._currentSelectedClass = value;
                this.OnPropertyChangedWithValue<HeroClassVM>(value, nameof(CurrentSelectedClass));
            }
        }

        [DataSourceProperty]
        public string RemainingTimeText
        {
            get => this._remainingTimeText;
            set
            {
                if (!(value != this._remainingTimeText))
                    return;
                this._remainingTimeText = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(RemainingTimeText));
            }
        }

        [DataSourceProperty]
        public bool WarnRemainingTime
        {
            get => this._warnRemainingTime;
            set
            {
                if (value == this._warnRemainingTime)
                    return;
                this._warnRemainingTime = value;
                this.OnPropertyChangedWithValue(value, nameof(WarnRemainingTime));
            }
        }

        [DataSourceProperty]
        public MBBindingList<HeroClassGroupVM> Classes
        {
            get => this._classes;
            set
            {
                if (value == this._classes)
                    return;
                this._classes = value;
                this.OnPropertyChangedWithValue<MBBindingList<HeroClassGroupVM>>(value, nameof(Classes));
            }
        }

        [DataSourceProperty]
        public bool IsGoldEnabled
        {
            get => this._isGoldEnabled;
            set
            {
                if (value == this._isGoldEnabled)
                    return;
                this._isGoldEnabled = value;
                this.OnPropertyChangedWithValue(value, nameof(IsGoldEnabled));
            }
        }

        [DataSourceProperty]
        public bool IsInWarmup
        {
            get => this._isInWarmup;
            set
            {
                if (value == this._isInWarmup)
                    return;
                this._isInWarmup = value;
                this.OnPropertyChangedWithValue(value, nameof(IsInWarmup));
            }
        }

        [DataSourceProperty]
        public string WarmupInfoText
        {
            get => this._warmupInfoText;
            set
            {
                if (!(value != this._warmupInfoText))
                    return;
                this._warmupInfoText = value;
                this.OnPropertyChangedWithValue<string>(value, nameof(WarmupInfoText));
            }
        }
    }
}
