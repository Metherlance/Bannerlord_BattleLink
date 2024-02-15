using BattleLink.Common;
using BattleLink.Common.Behavior;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Utils;
using NetworkMessages.FromClient;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.MissionRepresentatives;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;
using Team = TaleWorlds.MountAndBlade.Team;

namespace BattleLink.Server
{
    // copy of MissionMultiplayerFlagDomination
    public class BLMissionMpDomination : MissionMultiplayerGameModeBase //, IMissionBehavior, IAnalyticsFlagInfo,
    {

        public BLMissionMpDomination()
        {

            MBDebug.Print("RBMissionMpGameMode - RBMissionMpGameMode", 0, DebugColor.Green);

            this._gameType = MultiplayerGameType.Captain;
            this._moraleMultiplierForEachFlag = 1f;
            this._pointRemovalTimeInSeconds = 180f;
            this._moraleMultiplierOnLastFlag = 2f;

        }

        public override void AfterStart()
        {
            // base.AfterStart();
            //base.Mission.Teams.RemoveAt(2);
            //base.Mission.Teams.RemoveAt(1);

            RoundController.OnRoundStarted += OnPreparationStart;
            MissionPeer.OnPreTeamChanged += OnPreTeamChanged;
            RoundController.OnPreparationEnded += OnPreparationEnded;
            if (WarmupComponent != null)
            {
                WarmupComponent.OnWarmupEnding += OnWarmupEnding;
            }

            RoundController.OnPreRoundEnding += OnRoundEnd;
            RoundController.OnPostRoundEnded += OnPostRoundEnd;


            {
                var team = BLReferentialHolder.listTeam.Where(x => BattleSideEnum.Attacker.ToString().Equals(x.BattleSide)).First();
                var Color = Convert.ToUInt32(team.Color, 16);
                var Color2 = Convert.ToUInt32(team.Color2, 16);
                Banner banner = new Banner(team.FactionBannerKey, Color, Color2);
                base.Mission.Teams.Add(BattleSideEnum.Attacker, Color, Color2, banner, isPlayerGeneral: false, isPlayerSergeant: true);
            }

            {
                var team = BLReferentialHolder.listTeam.Where(x => BattleSideEnum.Defender.ToString().Equals(x.BattleSide)).First();
                var Color = Convert.ToUInt32(team.Color, 16);
                var Color2 = Convert.ToUInt32(team.Color2, 16);
                Banner banner = new Banner(team.FactionBannerKey, Color, Color2);
                base.Mission.Teams.Add(BattleSideEnum.Defender, Color, Color2, banner, isPlayerGeneral: false, isPlayerSergeant: true);
            }


            MBDebug.Print("RBMissionMpGameMode - AfterStart", 0, DebugColor.Green);
            // TeamQuerySystemUtils.setPowerFix(this.Mission);

            //Clear spectator formation
            // note : init only on server MissionLobbyComponent EarlyStart  
            Mission.SpectatorTeam.FormationsIncludingSpecialAndEmpty.Clear();
            Mission.SpectatorTeam.FormationsIncludingEmpty.Clear();



        }

        //public override void OnAfterMissionCreated()//mission == null ...
        //{
        //    base.OnAfterMissionCreated();
        //}

        public override void EarlyStart()
        {
            base.EarlyStart();
            TeamQuerySystemUtils.setMission(Mission);
            MBDebug.Print("RBMissionMpGameMode - EarlyStart", 0, DebugColor.Cyan);
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();

            if (BLReferentialHolder.currentBattleInitializerPending)
            {
                //move init file
                string filenameInit = BLReferentialHolder.initializerFilename;
                string pathServerFileInitializerFinished = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished", filenameInit);
                if (File.Exists(pathServerFileInitializerFinished))
                {
                    File.Delete(pathServerFileInitializerFinished);
                }
                //
                File.Move(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Pending", filenameInit), pathServerFileInitializerFinished);
            }            

            // load next init file
            setNextBLMap();

            MBDebug.Print("BLMissionMDomination - OnEndMission", 0, DebugColor.Green);
        }


        public static void setNextBLMap()
        {
            // Check pending state first
            FileInfo[] fileEntries = new DirectoryInfo(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Pending"))
                        .GetFiles("BL_MPBattle_*_Initializer.xml")
                        .OrderBy(f => f.CreationTime)
                        .ToArray();
            if (fileEntries.Length > 0)
            {
                FileInfo fileEntrie = fileEntries[0];

                MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
                MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;

                XmlSerializer serializer = new XmlSerializer(typeof(MPBattleInitializer));
                MPBattleInitializer battleInitializerNext = null;
                using (Stream reader = new FileStream(fileEntrie.FullName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    battleInitializerNext = (MPBattleInitializer)serializer.Deserialize(reader);
                }

                BLReferentialHolder.nextBattleInitializerFilePath = fileEntrie.FullName;
                BLReferentialHolder.nextBattleInitializerPending = true;

                MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.Map,      MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.MissionInitializerRecord.SceneName);
                MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.GameType, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue("BattleLink");

                //if (BattleSideEnum.Attacker.ToString().Equals(battleInitializerNext.Teams[0].BattleSide))
                //{
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam1, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[0].Culture);
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam2, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[1].Culture);
                //}
                //else
                //{
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam1, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[1].Culture);
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam2, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[0].Culture);
                //}

                MBDebug.Print("BLMissionMDomination - setNextBLMap pending scene:" + battleInitializerNext.MissionInitializerRecord.SceneName, 0, DebugColor.Green);
            
                return;
            }

            // pick random initializer from finished
            fileEntries = new DirectoryInfo(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished"))
                .GetFiles("BL_MPBattle_*_Initializer.xml")
                .ToArray();
            if (fileEntries.Length > 0)
            {
                FileInfo fileEntrie = fileEntries[new Random().Next(fileEntries.Length)];

                MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
                MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;

                XmlSerializer serializer = new XmlSerializer(typeof(MPBattleInitializer));
                MPBattleInitializer battleInitializerNext = null;
                using (Stream reader = new FileStream(fileEntrie.FullName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    battleInitializerNext = (MPBattleInitializer)serializer.Deserialize(reader);
                }

                BLReferentialHolder.nextBattleInitializerFilePath = fileEntrie.FullName;
                BLReferentialHolder.nextBattleInitializerPending = false;

                MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.Map,      MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.MissionInitializerRecord.SceneName);
                MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.GameType, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue("BattleLink");

                //if (BattleSideEnum.Attacker.ToString().Equals(battleInitializerNext.Teams[0].BattleSide))
                //{
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam1, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[0].Culture);
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam2, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[1].Culture);
                //}
                //else
                //{
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam1, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[1].Culture);
                //    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.CultureTeam2, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(battleInitializerNext.Teams[0].Culture);
                //}

                MBDebug.Print("BLMissionMDomination - setNextBLMap finished scene:" + battleInitializerNext.MissionInitializerRecord.SceneName, 0, DebugColor.Green);

                return;
            }

            // no initializer, go back to default
            {
                BLReferentialHolder.nextBattleInitializerFilePath = null;
                BLReferentialHolder.nextBattleInitializerPending = false;

                MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = true;
                MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = true;
                string gamemodeDefault = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "config.properties")).Get("gamemode.default");
                MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.GameType, MultiplayerOptionsAccessMode.NextMapOptions).UpdateValue(gamemodeDefault);

                MBDebug.Print($"BLMissionMDomination - setNextBLMap no initializer, go back to {gamemodeDefault}", 0, DebugColor.DarkYellow);
            }
        }

        // copy of MissionMultiplayerFlagDomination

        public const int NumberOfFlagsInGame = 3;
        public const float MoraleRoundPrecision = 0.01f;
        public const int DefaultGoldAmountForTroopSelectionForSkirmish = 300;
        public const int MaxGoldAmountToCarryOverForSkirmish = 80;
        private const int MaxGoldAmountToCarryOverForSurvivalForSkirmish = 30;
        public const int InitialGoldAmountForTroopSelectionForBattle = 200;
        public const int DefaultGoldAmountForTroopSelectionForBattle = 120;
        public const int MaxGoldAmountToCarryOverForBattle = 110;
        private const int MaxGoldAmountToCarryOverForSurvivalForBattle = 20;
        private const float MoraleGainOnTick = 0.000625f;
        private const float MoralePenaltyPercentageIfNoPointsCaptured = 0.1f;
        private const float MoraleTickTimeInSeconds = 0.25f;
        public const float TimeTillFlagRemovalForPriorInfoInSeconds = 30f;
        public const float PointRemovalTimeInSecondsForBattle = 210f;
        public const float PointRemovalTimeInSecondsForCaptain = 180f;
        public const float PointRemovalTimeInSecondsForSkirmish = 120f;
        public const float MoraleMultiplierForEachFlagForBattle = 0.75f;
        public const float MoraleMultiplierForEachFlagForCaptain = 1f;
        private const float MoraleMultiplierOnLastFlagForBattle = 3.5f;
        private static int _defaultGoldAmountForTroopSelection = -1;
        private static int _maxGoldAmountToCarryOver = -1;
        private static int _maxGoldAmountToCarryOverForSurvival = -1;
        private const float MoraleMultiplierOnLastFlagForCaptainSkirmish = 2f;
        public const float MoraleMultiplierForEachFlagForSkirmish = 2f;
        private readonly float _pointRemovalTimeInSeconds = -1f;
        private readonly float _moraleMultiplierForEachFlag = -1f;
        private readonly float _moraleMultiplierOnLastFlag = -1f;
        private Team[] _capturePointOwners;
        private bool _flagRemovalOccured;
        private float _nextTimeToCheckForPointRemoval = float.MinValue;
        private MissionMultiplayerGameModeFlagDominationClient _gameModeFlagDominationClient;
        private float _morale;
        private readonly MultiplayerGameType _gameType;
        private int[] _agentCountsOnSide = new int[2];
        private (int, int)[] _defenderAttackerCountsInFlagArea = new (int, int)[3];

        public override bool IsGameModeHidingAllAgentVisuals => this._gameType == MultiplayerGameType.Captain || this._gameType == MultiplayerGameType.Battle;

        public override bool IsGameModeUsingOpposingTeams => true;

        //public MBReadOnlyList<FlagCapturePoint> AllCapturePoints { get; private set; }

        public float MoraleRounded => (float)(int)((double)this._morale / 0.00999999977648258) * 0.01f;

        public bool GameModeUsesSingleSpawning => this.GetMissionType() == MultiplayerGameType.Captain || this.GetMissionType() == MultiplayerGameType.Battle;

        public bool UseGold() => this._gameModeFlagDominationClient.IsGameModeUsingGold;

        public override bool AllowCustomPlayerBanners() => false;

        public override bool UseRoundController() => true;


        public override MultiplayerGameType GetMissionType() => this._gameType;

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            this._gameModeFlagDominationClient = Mission.Current.GetMissionBehavior<MissionMultiplayerGameModeFlagDominationClient>();
            this._morale = 0.0f;
            this._capturePointOwners = new Team[3];
            //this.AllCapturePoints = (MBReadOnlyList<FlagCapturePoint>)Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToMBList<FlagCapturePoint>();
            //foreach (FlagCapturePoint allCapturePoint in (List<FlagCapturePoint>)this.AllCapturePoints)
            //{
            //    allCapturePoint.SetTeamColorsWithAllSynched(4284111450U, uint.MaxValue);
            //    this._capturePointOwners[allCapturePoint.FlagIndex] = (Team)null;
            //}
        }

        //public override void AfterStart()
        //{
        //    base.AfterStart();
        //    this.RoundController.OnRoundStarted += new Action(this.OnPreparationStart);
        //    MissionPeer.OnPreTeamChanged += new MissionPeer.OnTeamChangedDelegate(this.OnPreTeamChanged);
        //    this.RoundController.OnPreparationEnded += new Action(this.OnPreparationEnded);
        //    if (this.WarmupComponent != null)
        //        this.WarmupComponent.OnWarmupEnding += new Action(this.OnWarmupEnding);
        //    this.RoundController.OnPreRoundEnding += new Action(this.OnRoundEnd);
        //    this.RoundController.OnPostRoundEnded += new Action(this.OnPostRoundEnd);
        //    BasicCultureObject firstObject1 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam1.GetStrValue());
        //    BasicCultureObject firstObject2 = MBObjectManager.Instance.GetObject<BasicCultureObject>(MultiplayerOptions.OptionType.CultureTeam2.GetStrValue());
        //    if (firstObject1 == null)
        //        firstObject1 = MBObjectManager.Instance.GetFirstObject<BasicCultureObject>();
        //    if (firstObject2 == null)
        //        firstObject2 = MBObjectManager.Instance.GetFirstObject<BasicCultureObject>();
        //    Banner banner1 = new Banner(firstObject1.BannerKey, firstObject1.BackgroundColor1, firstObject1.ForegroundColor1);
        //    Banner banner2 = new Banner(firstObject2.BannerKey, firstObject2.BackgroundColor2, firstObject2.ForegroundColor2);
        //    this.Mission.Teams.Add(BattleSideEnum.Attacker, firstObject1.BackgroundColor1, firstObject1.ForegroundColor1, banner1, false, true);
        //    this.Mission.Teams.Add(BattleSideEnum.Defender, firstObject2.BackgroundColor2, firstObject2.ForegroundColor2, banner2, false, true);
        //}

        protected override void AddRemoveMessageHandlers(
          GameNetwork.NetworkMessageHandlerRegistererContainer registerer)
        {
            registerer.RegisterBaseHandler<RequestForfeitSpawn>(new GameNetworkMessage.ClientMessageHandlerDelegate<GameNetworkMessage>(this.HandleClientEventRequestForfeitSpawn));
        }

        public override void OnRemoveBehavior()
        {
            this.RoundController.OnRoundStarted -= new Action(this.OnPreparationStart);
            MissionPeer.OnPreTeamChanged -= new MissionPeer.OnTeamChangedDelegate(this.OnPreTeamChanged);
            this.RoundController.OnPreparationEnded -= new Action(this.OnPreparationEnded);
            if (this.WarmupComponent != null)
                this.WarmupComponent.OnWarmupEnding -= new Action(this.OnWarmupEnding);
            this.RoundController.OnPreRoundEnding -= new Action(this.OnRoundEnd);
            this.RoundController.OnPostRoundEnded -= new Action(this.OnPostRoundEnd);
            base.OnRemoveBehavior();
        }

        public override void OnPeerChangedTeam(NetworkCommunicator peer, Team oldTeam, Team newTeam)
        {
            if (oldTeam == null || oldTeam == newTeam || !this.UseGold() || this.WarmupComponent != null && this.WarmupComponent.IsInWarmup)
                return;
            this.ChangeCurrentGoldForPeer(peer.GetComponent<MissionPeer>(), 0);
        }

        private void OnPreparationStart() => this.NotificationsComponent.PreparationStarted();

        public override void OnMissionTick(float dt)
        {
            base.OnMissionTick(dt);
            if (this.MissionLobbyComponent.CurrentMultiplayerState != MissionLobbyComponent.MultiplayerGameState.Playing)
                return;
            if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue() > 0)
            {
                this.CheckForPlayersSpawningAsBots();
                this.CheckPlayerBeingDetached();
            }
            //if (!this.RoundController.IsRoundInProgress || !this.CanGameModeSystemsTickThisFrame)
            //    return;
            //if (!this._flagRemovalOccured)
            //    this.CheckRemovingOfPoints();
            //this.CheckMorales();
            //this.TickFlags();
        }

        private void CheckForPlayersSpawningAsBots()
        {
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer.IsSynchronized)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (component != null && component.ControlledAgent == null && component.Team != null && component.ControlledFormation != null && component.SpawnCountThisRound > 0)
                    {
                        if (!component.HasSpawnTimerExpired && component.SpawnTimer.Check(this.Mission.CurrentTime))
                            component.HasSpawnTimerExpired = true;
                        if (component.HasSpawnTimerExpired && component.WantsToSpawnAsBot && component.ControlledFormation.HasUnitsWithCondition((Func<Agent, bool>)(agent => agent.IsActive() && agent.IsAIControlled)))
                        {
                            Agent newAgent = (Agent)null;
                            Agent followingAgent = component.FollowedAgent;
                            if (followingAgent != null && followingAgent.IsActive() && followingAgent.IsAIControlled && component.ControlledFormation.HasUnitsWithCondition((Func<Agent, bool>)(agent => agent == followingAgent)))
                            {
                                newAgent = followingAgent;
                            }
                            else
                            {
                                float maxHealth = 0.0f;
                                component.ControlledFormation.ApplyActionOnEachUnit((Action<Agent>)(agent =>
                                {
                                    if ((double)agent.Health <= (double)maxHealth)
                                        return;
                                    maxHealth = agent.Health;
                                    newAgent = agent;
                                }));
                            }
                            Mission.Current.ReplaceBotWithPlayer(newAgent, component);
                            component.WantsToSpawnAsBot = false;
                            component.HasSpawnTimerExpired = false;
                        }
                    }
                }
            }
        }

        private bool GetMoraleGain(out float moraleGain)
        {
            //List<FlagCapturePoint> list = this.AllCapturePoints.Where<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(flag => !flag.IsDeactivated && this.GetFlagOwnerTeam(flag) != null && flag.IsFullyRaised)).ToList<FlagCapturePoint>();
            //int f = list.Count<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(flag => this.GetFlagOwnerTeam(flag).Side == BattleSideEnum.Attacker)) - list.Count<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(flag => this.GetFlagOwnerTeam(flag).Side == BattleSideEnum.Defender));
            //int num1 = MathF.Sign(f);
            moraleGain = 0.0f;
            //if (num1 == 0)
            //    return false;
            //float num2 = 0.000625f * this._moraleMultiplierForEachFlag * (float)MathF.Abs(f);
            //moraleGain = num1 <= 0 ? MBMath.ClampFloat((float)num1 - this._morale, -2f, -1f) * num2 : MBMath.ClampFloat((float)num1 - this._morale, 1f, 2f) * num2;
            //if (this._flagRemovalOccured)
            //    moraleGain *= this._moraleMultiplierOnLastFlag;
            return true;
        }

        public float GetTimeUntilBattleSideVictory(BattleSideEnum side)
        {
            float a = float.MaxValue;
            if (side == BattleSideEnum.Attacker && (double)this._morale > 0.0 || side == BattleSideEnum.Defender && (double)this._morale < 0.0)
                a = this.RoundController.RemainingRoundTime;
            float b = float.MaxValue;
            float moraleGain;
            this.GetMoraleGain(out moraleGain);
            if (side == BattleSideEnum.Attacker && (double)moraleGain > 0.0)
                b = (1f - this._morale) / moraleGain;
            else if (side == BattleSideEnum.Defender && (double)moraleGain < 0.0)
                b = (float)((-1.0 - (double)this._morale) / ((double)moraleGain / 0.25));
            return MathF.Min(a, b);
        }

        private void CheckMorales()
        {
            float moraleGain;
            if (!this.GetMoraleGain(out moraleGain))
                return;
            this._morale += moraleGain;
            this._morale = MBMath.ClampFloat(this._morale, -1f, 1f);
            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationMoraleChangeMessage(this.MoraleRounded));
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            this._gameModeFlagDominationClient?.OnMoraleChanged(this.MoraleRounded);
            MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.MoraleChange);
        }

        //private void CheckRemovingOfPoints()
        //{
        //if ((double)this._nextTimeToCheckForPointRemoval < 0.0)
        //    this._nextTimeToCheckForPointRemoval = this.Mission.CurrentTime + this._pointRemovalTimeInSeconds;
        //if ((double)this.Mission.CurrentTime < (double)this._nextTimeToCheckForPointRemoval)
        //    return;
        //this._nextTimeToCheckForPointRemoval += this._pointRemovalTimeInSeconds;
        //List<BattleSideEnum> battleSideEnumList = new List<BattleSideEnum>();
        //foreach (Team team1 in (List<Team>)this.Mission.Teams)
        //{
        //    Team team = team1;
        //    if (team.Side != BattleSideEnum.None)
        //    {
        //        int num = (int)team.Side * 2 - 1;
        //        //if (this.AllCapturePoints.All<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => this.GetFlagOwnerTeam(cp) != team)))
        //        //{
        //        //    if (this.AllCapturePoints.FirstOrDefault<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => this.GetFlagOwnerTeam(cp) == null)) != null)
        //        //    {
        //        //        this._morale -= 0.1f * (float)num;
        //        //        battleSideEnumList.Add(BattleSideEnum.None);
        //        //    }
        //        //    else
        //        //    {
        //        //        this._morale -= (float)(0.100000001490116 * (double)num * 2.0);
        //        //        battleSideEnumList.Add(team.Side.GetOppositeSide());
        //        //    }
        //        //    this._morale = MBMath.ClampFloat(this._morale, -1f, 1f);
        //        //}
        //        //else
        //        //    battleSideEnumList.Add(team.Side);
        //    }
        //}
        //List<int> removedCapIndexList = new List<int>();
        ////MBList<FlagCapturePoint> mbList1 = this.AllCapturePoints.ToMBList<FlagCapturePoint>();
        //foreach (BattleSideEnum battleSideEnum in battleSideEnumList)
        //{
        //    BattleSideEnum side = battleSideEnum;
        //    if (side == BattleSideEnum.None)
        //    {
        //        //removedCapIndexList.Add(this.RemoveCapturePoint(mbList1.GetRandomElementWithPredicate<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => this.GetFlagOwnerTeam(cp) == null))));
        //    }
        //    else
        //    {
        //        //List<FlagCapturePoint> list = mbList1.Where<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => this.GetFlagOwnerTeam(cp) != null && this.GetFlagOwnerTeam(cp).Side == side)).ToList<FlagCapturePoint>();
        //        //MBList<FlagCapturePoint> mbList2 = list.Where<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => this.GetNumberOfAttackersAroundFlag(cp) == 0)).ToMBList<FlagCapturePoint>();
        //        if (mbList2.Count > 0)
        //        {
        //            //removedCapIndexList.Add(this.RemoveCapturePoint(mbList2.GetRandomElement<FlagCapturePoint>()));
        //        }
        //        else
        //        {
        //            MBList<KeyValuePair<FlagCapturePoint, int>> mbList3 = new MBList<KeyValuePair<FlagCapturePoint, int>>();
        //            foreach (FlagCapturePoint flagCapturePoint in list)
        //            {
        //                if (mbList3.Count == 0)
        //                {
        //                    mbList3.Add(new KeyValuePair<FlagCapturePoint, int>(flagCapturePoint, this.GetNumberOfAttackersAroundFlag(flagCapturePoint)));
        //                }
        //                else
        //                {
        //                    int count = this.GetNumberOfAttackersAroundFlag(flagCapturePoint);
        //                    if (mbList3.Any<KeyValuePair<FlagCapturePoint, int>>((Func<KeyValuePair<FlagCapturePoint, int>, bool>)(cc => cc.Value > count)))
        //                    {
        //                        mbList3.Clear();
        //                        mbList3.Add(new KeyValuePair<FlagCapturePoint, int>(flagCapturePoint, count));
        //                    }
        //                    else if (mbList3.Any<KeyValuePair<FlagCapturePoint, int>>((Func<KeyValuePair<FlagCapturePoint, int>, bool>)(cc => cc.Value == count)))
        //                        mbList3.Add(new KeyValuePair<FlagCapturePoint, int>(flagCapturePoint, count));
        //                }
        //            }
        //           // removedCapIndexList.Add(this.RemoveCapturePoint(mbList3.GetRandomElement<KeyValuePair<FlagCapturePoint, int>>().Key));
        //        }
        //    }
        //    //FlagCapturePoint flagCapturePoint1 = mbList1.First<FlagCapturePoint>(closure_0 ?? (closure_0 = (Func<FlagCapturePoint, bool>)(fl => fl.FlagIndex == removedCapIndexList[removedCapIndexList.Count - 1])));
        //    //mbList1.Remove(flagCapturePoint1);
        //}
        //removedCapIndexList.Sort();
        //int first = removedCapIndexList[0];
        //int second = removedCapIndexList[1];
        //FlagCapturePoint remainingFlag = this.AllCapturePoints.First<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => cp.FlagIndex != first && cp.FlagIndex != second));
        //this.NotificationsComponent.FlagXRemaining(remainingFlag);
        //GameNetwork.BeginBroadcastModuleEvent();
        //GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationMoraleChangeMessage(this.MoraleRounded));
        //GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        //GameNetwork.BeginBroadcastModuleEvent();
        //GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationFlagsRemovedMessage());
        //GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        //this._flagRemovalOccured = true;
        //this._gameModeFlagDominationClient?.OnNumberOfFlagsChanged();
        //foreach (MissionBehavior missionBehavior in this.Mission.MissionBehaviors)
        //{
        //    if (missionBehavior is IFlagRemoved flagRemoved)
        //        flagRemoved.OnFlagsRemoved(remainingFlag.FlagIndex);
        //}
        //MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.FlagRemoval);
        //}

        //private int RemoveCapturePoint(FlagCapturePoint capToRemove)
        //{
        //    int flagIndex = capToRemove.FlagIndex;
        //    capToRemove.RemovePointAsServer();
        //    GameNetwork.BeginBroadcastModuleEvent();
        //    GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationCapturePointMessage(flagIndex, -1));
        //    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        //    return flagIndex;
        //}

        public override void OnClearScene()
        {
            base.OnClearScene();
            //this.AllCapturePoints = (MBReadOnlyList<FlagCapturePoint>)Mission.Current.MissionObjects.FindAllWithType<FlagCapturePoint>().ToMBList<FlagCapturePoint>();
            //foreach (FlagCapturePoint allCapturePoint in (List<FlagCapturePoint>)this.AllCapturePoints)
            //{
            //    allCapturePoint.ResetPointAsServer(4284111450U, uint.MaxValue);
            //    this._capturePointOwners[allCapturePoint.FlagIndex] = (Team)null;
            //}
            this._morale = 0.0f;
            this._nextTimeToCheckForPointRemoval = float.MinValue;
            this._flagRemovalOccured = false;

            //
            //TeamQuerySystemUtils.setPowerFix(Mission);
        }

        public override bool CheckIfOvertime()
        {
            return false;
            //if (!this._flagRemovalOccured)
            //    return false;
            //FlagCapturePoint flagCapturePoint = this.AllCapturePoints.FirstOrDefault<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(flag => !flag.IsDeactivated));
            //Team flagOwnerTeam = this.GetFlagOwnerTeam(flagCapturePoint);
            //if (flagOwnerTeam == null)
            //    return false;
            //return (double)((int)flagOwnerTeam.Side * 2 - 1) * (double)this._morale < 0.0 || this.GetNumberOfAttackersAroundFlag(flagCapturePoint) > 0;
        }

        public override bool CheckForWarmupEnd()
        {
            int[] source = new int[2];
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                if (networkPeer.IsSynchronized && component?.Team != null && component.Team.Side != BattleSideEnum.None)
                    ++source[(int)component.Team.Side];
            }
            return ((IEnumerable<int>)source).Sum() >= MultiplayerOptions.OptionType.MaxNumberOfPlayers.GetIntValue();
        }

        public override bool CheckForRoundEnd()
        {
            if (this.CanGameModeSystemsTickThisFrame)
            {
                //if ((double)MathF.Abs(this._morale) >= 1.0)
                //{
                //    if (!this._flagRemovalOccured)
                //        return true;
                //    FlagCapturePoint flagCapturePoint = this.AllCapturePoints.FirstOrDefault<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(flag => !flag.IsDeactivated));
                //    Team flagOwnerTeam = this.GetFlagOwnerTeam(flagCapturePoint);
                //    if (flagOwnerTeam == null)
                //        return true;
                //    BattleSideEnum battleSideEnum = (double)this._morale > 0.0 ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
                //    return flagOwnerTeam.Side == battleSideEnum && flagCapturePoint.IsFullyRaised && this.GetNumberOfAttackersAroundFlag(flagCapturePoint) == 0;
                //}
                bool flag1 = this.Mission.AttackerTeam.ActiveAgents.Count > 0;
                bool flag2 = this.Mission.DefenderTeam.ActiveAgents.Count > 0;
                if (flag1 & flag2)
                    return false;
                if (!this.SpawnComponent.AreAgentsSpawning())
                    return true;
                bool[] flagArray = new bool[2];
                if (this.UseGold())
                {
                    foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                    {
                        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                        if (component?.Team != null && component.Team.Side != BattleSideEnum.None && !flagArray[(int)component.Team.Side])
                        {
                            string strValue = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue();
                            if (component.Team.Side != BattleSideEnum.Attacker)
                                strValue = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue();
                            if (this.GetCurrentGoldForPeer(component) >= MultiplayerClassDivisions.GetMinimumTroopCost(MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue)))
                                flagArray[(int)component.Team.Side] = true;
                        }
                    }
                }
                if (!flag1 && !flagArray[1] || !flag2 && !flagArray[0])
                    return true;
            }
            return false;
        }

        public override bool UseCultureSelection() => false;

        private void OnWarmupEnding() => this.NotificationsComponent.WarmupEnding();

        private void OnRoundEnd()
        {
            //foreach (FlagCapturePoint allCapturePoint in (List<FlagCapturePoint>)this.AllCapturePoints)
            //{
            //    if (!allCapturePoint.IsDeactivated)
            //        allCapturePoint.SetMoveNone();
            //}
            RoundEndReason roundEndReason = RoundEndReason.Invalid;
            bool isRoundFinished = (double)this.RoundController.RemainingRoundTime <= 0.0 && !this.CheckIfOvertime();
            int indexSideWinner = -1;
            // moral check
            //for (int indexSide = 0; indexSide < 2; indexSide++)
            //{
            //    int oneOrMinusOne = indexSide * 2 - 1;
            //    if ((isRoundFinished && (float)oneOrMinusOne * _morale > 0f) || (!isRoundFinished && (float)oneOrMinusOne * _morale >= 1f))
            //    {
            //        indexSideWinner = indexSide;
            //        break;
            //    }
            //}
            CaptureTheFlagCaptureResultEnum roundResult = CaptureTheFlagCaptureResultEnum.NotCaptured;
            if (indexSideWinner >= 0)
            {
                roundResult = indexSideWinner == 0 ? CaptureTheFlagCaptureResultEnum.DefendersWin : CaptureTheFlagCaptureResultEnum.AttackersWin;
                this.RoundController.RoundWinner = indexSideWinner == 0 ? BattleSideEnum.Defender : BattleSideEnum.Attacker;
                roundEndReason = isRoundFinished ? RoundEndReason.RoundTimeEnded : RoundEndReason.GameModeSpecificEnded;
            }
            else
            {
                bool attHasActiveAgent = this.Mission.AttackerTeam.ActiveAgents.Count > 0;
                bool defHasActiveAgent = this.Mission.DefenderTeam.ActiveAgents.Count > 0;
                if (attHasActiveAgent & defHasActiveAgent)
                {
                    //if ((double)this._morale > 0.0)
                    //{
                    //    roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                    //    this.RoundController.RoundWinner = BattleSideEnum.Attacker;
                    //}
                    //else if ((double)this._morale < 0.0)
                    //{
                    roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                    this.RoundController.RoundWinner = BattleSideEnum.Defender;
                    //}
                    //else
                    //{
                    //    roundResult = CaptureTheFlagCaptureResultEnum.Draw;
                    //    this.RoundController.RoundWinner = BattleSideEnum.None;
                    //}
                    roundEndReason = RoundEndReason.RoundTimeEnded;
                }
                else if (attHasActiveAgent)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.AttackersWin;
                    this.RoundController.RoundWinner = BattleSideEnum.Attacker;
                    roundEndReason = RoundEndReason.SideDepleted;
                }
                else if (defHasActiveAgent)
                {
                    roundResult = CaptureTheFlagCaptureResultEnum.DefendersWin;
                    this.RoundController.RoundWinner = BattleSideEnum.Defender;
                    roundEndReason = RoundEndReason.SideDepleted;
                }
                else
                {
                    foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                    {
                        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                        if (component?.Team != null && component.Team.Side != BattleSideEnum.None)
                        {
                            string strValue = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue();
                            if (component.Team.Side != BattleSideEnum.Attacker)
                                strValue = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue();
                            if (this.GetCurrentGoldForPeer(component) >= MultiplayerClassDivisions.GetMinimumTroopCost(MBObjectManager.Instance.GetObject<BasicCultureObject>(strValue)))
                            {
                                this.RoundController.RoundWinner = component.Team.Side;
                                roundEndReason = RoundEndReason.SideDepleted;
                                roundResult = component.Team.Side == BattleSideEnum.Attacker ? CaptureTheFlagCaptureResultEnum.AttackersWin : CaptureTheFlagCaptureResultEnum.DefendersWin;
                                break;
                            }
                        }
                    }
                }
            }
            if (roundResult == CaptureTheFlagCaptureResultEnum.NotCaptured)
                return;
            this.RoundController.RoundEndReason = roundEndReason;
            this.HandleRoundEnd(roundResult);
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            agent.UpdateSyncHealthToAllClients(true);
            if (!agent.IsPlayerControlled)
                return;
            agent.MissionPeer.GetComponent<FlagDominationMissionRepresentative>().UpdateSelectedClassServer(agent);
        }

        private void HandleRoundEnd(CaptureTheFlagCaptureResultEnum roundResult)
        {
            AgentVictoryLogic missionBehavior = this.Mission.GetMissionBehavior<AgentVictoryLogic>();
            if (missionBehavior == null)
                Debug.FailedAssert("Agent victory logic should not be null after someone just won/lost!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\Multiplayer\\MissionNetworkLogics\\MultiplayerGameModeLogics\\ServerGameModeLogics\\MissionMultiplayerFlagDomination.cs", nameof(HandleRoundEnd), 780);
            else if (roundResult != CaptureTheFlagCaptureResultEnum.AttackersWin)
            {
                if (roundResult != CaptureTheFlagCaptureResultEnum.DefendersWin)
                    return;
                missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Defender);
            }
            else
                missionBehavior.SetTimersOfVictoryReactionsOnBattleEnd(BattleSideEnum.Attacker);
        }

        private void OnPostRoundEnd()
        {
            if (!this.UseGold() || this.RoundController.IsMatchEnding)
                return;
            return;
            //foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            //{
            //    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
            //    if (component != null && this.RoundController.RoundCount > 0)
            //    {
            //        int forTroopSelection = MissionMultiplayerFlagDomination._defaultGoldAmountForTroopSelection;
            //        int num = this.GetCurrentGoldForPeer(component);
            //        if (num < 0)
            //            num = MissionMultiplayerFlagDomination._maxGoldAmountToCarryOver;
            //        else if (component.Team != null && component.Team.Side != BattleSideEnum.None && this.RoundController.RoundWinner == component.Team.Side && component.GetComponent<FlagDominationMissionRepresentative>().CheckIfSurvivedLastRoundAndReset())
            //            num += MissionMultiplayerFlagDomination._maxGoldAmountToCarryOverForSurvival;
            //        int newAmount = forTroopSelection + MBMath.ClampInt(num, 0, MissionMultiplayerFlagDomination._maxGoldAmountToCarryOver);
            //        if (newAmount > MissionMultiplayerFlagDomination._defaultGoldAmountForTroopSelection)
            //            this.NotificationsComponent.GoldCarriedFromPreviousRound(newAmount - MissionMultiplayerFlagDomination._defaultGoldAmountForTroopSelection, component.GetNetworkPeer());
            //        this.ChangeCurrentGoldForPeer(component, newAmount);
            //    }
            //}
        }

        protected override void HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
        {
            if (!this.RoundController.IsRoundInProgress || MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue() <= 0)
                return;
            this.MakePlayerFormationCharge(networkPeer);
        }

        private void OnPreTeamChanged(NetworkCommunicator peer, Team currentTeam, Team newTeam)
        {
            if (!peer.IsSynchronized || peer.GetComponent<MissionPeer>().ControlledAgent == null)
                return;
            this.MakePlayerFormationCharge(peer);
        }

        private void OnPreparationEnded()
        {
            if (!this.UseGold())
                return;
            List<MissionPeer>[] missionPeerListArray = new List<MissionPeer>[2];
            for (int index = 0; index < missionPeerListArray.Length; ++index)
                missionPeerListArray[index] = new List<MissionPeer>();
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                if (component != null && component.Team != null && component.Team.Side != BattleSideEnum.None)
                    missionPeerListArray[(int)component.Team.Side].Add(component);
            }
            int f = missionPeerListArray[1].Count - missionPeerListArray[0].Count;
            BattleSideEnum index1 = f == 0 ? BattleSideEnum.None : (f < 0 ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
            if (index1 == BattleSideEnum.None)
                return;
            int num1 = MathF.Abs(f);
            int count = missionPeerListArray[(int)index1].Count;
            if (count <= 0)
                return;
            int num2 = _defaultGoldAmountForTroopSelection * num1 / 10 / count * 10;
            foreach (MissionPeer peer in missionPeerListArray[(int)index1])
                this.ChangeCurrentGoldForPeer(peer, this.GetCurrentGoldForPeer(peer) + num2);
        }

        private void CheckPlayerBeingDetached()
        {
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (networkPeer.IsSynchronized)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (this.PlayerDistanceToFormation(component) >= component.CaptainBeingDetachedThreshold)
                        this.MakePlayerFormationFollowPlayer(component.GetNetworkPeer());
                }
            }
        }

        private int PlayerDistanceToFormation(MissionPeer missionPeer)
        {
            float formation = 0.0f;
            if (missionPeer != null && missionPeer.ControlledAgent != null && missionPeer.ControlledFormation != null)
            {
                Vec2 vec2 = missionPeer.ControlledFormation.GetAveragePositionOfUnits(true, true);
                float num1 = vec2.Distance(missionPeer.ControlledAgent.Position.AsVec2);
                vec2 = missionPeer.ControlledFormation.OrderPosition;
                float num2 = vec2.Distance(missionPeer.ControlledAgent.Position.AsVec2);
                formation += num1 + num2;
                if (missionPeer.ControlledFormation.PhysicalClass.IsMounted())
                    formation *= 0.8f;
            }
            return (int)formation;
        }

        private void MakePlayerFormationFollowPlayer(NetworkCommunicator peer)
        {
            if (!peer.IsSynchronized)
                return;
            MissionPeer component = peer.GetComponent<MissionPeer>();
            if (component.ControlledFormation == null)
                return;
            component.ControlledFormation.SetMovementOrder(MovementOrder.MovementOrderFollow(component.ControlledAgent));
            this.NotificationsComponent.FormationAutoFollowEnforced(peer);
        }

        private void MakePlayerFormationCharge(NetworkCommunicator peer)
        {
            if (!peer.IsSynchronized)
                return;
            MissionPeer component = peer.GetComponent<MissionPeer>();
            if (component.ControlledFormation == null)
                return;
            component.ControlledFormation.SetMovementOrder(MovementOrder.MovementOrderCharge);
        }

        protected override void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer) => networkPeer.AddComponent<FlagDominationMissionRepresentative>();

        protected override void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
            if (this.UseGold())
            {
                int num1 = this._gameType == MultiplayerGameType.Battle ? 200 : _defaultGoldAmountForTroopSelection;
                int num2 = !this.RoundController.IsRoundInProgress ? num1 : 0;
                this.ChangeCurrentGoldForPeer(networkPeer.GetComponent<MissionPeer>(), num2);
                this._gameModeFlagDominationClient?.OnGoldAmountChangedForRepresentative((MissionRepresentativeBase)networkPeer.GetComponent<FlagDominationMissionRepresentative>(), num2);
            }
            //if (this.AllCapturePoints == null || networkPeer.IsServerPeer)
            //    return;
            //foreach (FlagCapturePoint flagCapturePoint in this.AllCapturePoints.Where<FlagCapturePoint>((Func<FlagCapturePoint, bool>)(cp => !cp.IsDeactivated)))
            //{
            //    GameNetwork.BeginModuleEventAsServer(networkPeer);
            //    int flagIndex = flagCapturePoint.FlagIndex;
            //    Team capturePointOwner = this._capturePointOwners[flagCapturePoint.FlagIndex];
            //    int ownerTeamIndex = capturePointOwner != null ? capturePointOwner.TeamIndex : -1;
            //    GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationCapturePointMessage(flagIndex, ownerTeamIndex));
            //    GameNetwork.EndModuleEventAsServer();
            //}
        }

        private bool HandleClientEventRequestForfeitSpawn(
          NetworkCommunicator peer,
          GameNetworkMessage baseMessage)
        {
            this.ForfeitSpawning(peer);
            return true;
        }

        public void ForfeitSpawning(NetworkCommunicator peer)
        {
            MissionPeer component = peer.GetComponent<MissionPeer>();
            if (component == null || !component.HasSpawnedAgentVisuals || !this.UseGold() || !this.RoundController.IsRoundInProgress)
                return;
            if (GameNetwork.IsServerOrRecorder)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage((GameNetworkMessage)new RemoveAgentVisualsForPeer(component.GetNetworkPeer()));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
            component.HasSpawnedAgentVisuals = false;
            this.ChangeCurrentGoldForPeer(component, -1);
        }

        public static void SetWinnerTeam(int winnerTeamNo)
        {
            Mission current = Mission.Current;
            MissionMultiplayerFlagDomination missionBehavior = current.GetMissionBehavior<MissionMultiplayerFlagDomination>();
            if (missionBehavior == null)
                return;
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                missionBehavior.ChangeCurrentGoldForPeer(component, 0);
            }
            for (int index = current.Agents.Count - 1; index >= 0; --index)
            {
                Agent agent = current.Agents[index];
                if (agent.IsHuman && agent.Team.MBTeam.Index != winnerTeamNo + 1)
                    Mission.Current.KillAgentCheat(agent);
            }
        }

        //private void TickFlags()
        //{
        //    foreach (FlagCapturePoint allCapturePoint in (List<FlagCapturePoint>)this.AllCapturePoints)
        //    {
        //        if (!allCapturePoint.IsDeactivated)
        //        {
        //            for (int index = 0; index < 2; ++index)
        //                this._agentCountsOnSide[index] = 0;
        //            Team capturePointOwner = this._capturePointOwners[allCapturePoint.FlagIndex];
        //            Agent agent = (Agent)null;
        //            float num1 = 16f;
        //            AgentProximityMap.ProximityMapSearchStruct searchStruct = AgentProximityMap.BeginSearch(Mission.Current, allCapturePoint.Position.AsVec2, 6f);
        //            while (searchStruct.LastFoundAgent != null)
        //            {
        //                Agent lastFoundAgent = searchStruct.LastFoundAgent;
        //                if (lastFoundAgent.IsHuman && lastFoundAgent.IsActive())
        //                {
        //                    ++this._agentCountsOnSide[(int)lastFoundAgent.Team.Side];
        //                    float num2 = lastFoundAgent.Position.DistanceSquared(allCapturePoint.Position);
        //                    if ((double)num2 <= (double)num1)
        //                    {
        //                        agent = lastFoundAgent;
        //                        num1 = num2;
        //                    }
        //                }
        //                AgentProximityMap.FindNext(Mission.Current, ref searchStruct);
        //            }
        //            (int, int) tuple = ValueTuple.Create<int, int>(this._agentCountsOnSide[0], this._agentCountsOnSide[1]);
        //            bool flag = tuple.Item1 != this._defenderAttackerCountsInFlagArea[allCapturePoint.FlagIndex].Item1 || tuple.Item2 != this._defenderAttackerCountsInFlagArea[allCapturePoint.FlagIndex].Item2;
        //            this._defenderAttackerCountsInFlagArea[allCapturePoint.FlagIndex] = tuple;
        //            bool isContested = allCapturePoint.IsContested;
        //            float speedMultiplier = 1f;
        //            if (agent != null)
        //            {
        //                BattleSideEnum side = agent.Team.Side;
        //                BattleSideEnum oppositeSide = side.GetOppositeSide();
        //                if (this._agentCountsOnSide[(int)oppositeSide] != 0)
        //                {
        //                    int val1 = Math.Min(this._agentCountsOnSide[(int)side], 200);
        //                    int val2 = Math.Min(this._agentCountsOnSide[(int)oppositeSide], 200);
        //                    speedMultiplier = Math.Min(1f, (float)(((double)MathF.Log10((float)val1) + 1.0) / (2.0 * ((double)MathF.Log10((float)val2) + 1.0)) - 0.0900000035762787));
        //                }
        //            }
        //            if (capturePointOwner == null)
        //            {
        //                if (!isContested && agent != null)
        //                    allCapturePoint.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
        //                else if (agent == null & isContested)
        //                    allCapturePoint.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
        //                else if (flag)
        //                    allCapturePoint.ChangeMovementSpeed(speedMultiplier);
        //            }
        //            else if (agent != null)
        //            {
        //                if (agent.Team != capturePointOwner && !isContested)
        //                    allCapturePoint.SetMoveFlag(CaptureTheFlagFlagDirection.Down, speedMultiplier);
        //                else if (agent.Team == capturePointOwner & isContested)
        //                    allCapturePoint.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
        //                else if (flag)
        //                    allCapturePoint.ChangeMovementSpeed(speedMultiplier);
        //            }
        //            else if (isContested)
        //                allCapturePoint.SetMoveFlag(CaptureTheFlagFlagDirection.Up, speedMultiplier);
        //            else if (flag)
        //                allCapturePoint.ChangeMovementSpeed(speedMultiplier);
        //            bool ownerTeamChanged;
        //            allCapturePoint.OnAfterTick(agent != null, out ownerTeamChanged);
        //            if (ownerTeamChanged)
        //            {
        //                Team team = agent.Team;
        //                uint color = team != null ? team.Color : 4284111450U;
        //                uint color2 = team != null ? team.Color2 : uint.MaxValue;
        //                allCapturePoint.SetTeamColorsWithAllSynched(color, color2);
        //                this._capturePointOwners[allCapturePoint.FlagIndex] = team;
        //                GameNetwork.BeginBroadcastModuleEvent();
        //                GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationCapturePointMessage(allCapturePoint.FlagIndex, team != null ? team.TeamIndex : -1));
        //                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
        //                this._gameModeFlagDominationClient?.OnCapturePointOwnerChanged(allCapturePoint, team);
        //                this.NotificationsComponent.FlagXCapturedByTeamX((SynchedMissionObject)allCapturePoint, agent.Team);
        //                MPPerkObject.RaiseEventForAllPeers(MPPerkCondition.PerkEventFlags.FlagCapture);
        //            }
        //        }
        //    }
        //}

        //public int GetNumberOfAttackersAroundFlag(FlagCapturePoint capturePoint)
        //{
        //    Team flagOwnerTeam = this.GetFlagOwnerTeam(capturePoint);
        //    if (flagOwnerTeam == null)
        //        return 0;
        //    int attackersAroundFlag = 0;
        //    AgentProximityMap.ProximityMapSearchStruct searchStruct = AgentProximityMap.BeginSearch(Mission.Current, capturePoint.Position.AsVec2, 6f);
        //    while (searchStruct.LastFoundAgent != null)
        //    {
        //        Agent lastFoundAgent = searchStruct.LastFoundAgent;
        //        if (lastFoundAgent.IsHuman && lastFoundAgent.IsActive() && (double)lastFoundAgent.Position.DistanceSquared(capturePoint.Position) <= 36.0 && lastFoundAgent.Team.Side != flagOwnerTeam.Side)
        //            ++attackersAroundFlag;
        //        AgentProximityMap.FindNext(Mission.Current, ref searchStruct);
        //    }
        //    return attackersAroundFlag;
        //}

        //public Team GetFlagOwnerTeam(FlagCapturePoint flag) => flag == null ? (Team)null : this._capturePointOwners[flag.FlagIndex];

        public override void OnAgentRemoved(
          Agent affectedAgent,
          Agent affectorAgent,
          AgentState agentState,
          KillingBlow blow)
        {
            base.OnAgentRemoved(affectedAgent, affectorAgent, agentState, blow);

            replaceGeneralIfNeeded(affectedAgent);

            if (this.UseGold() && affectorAgent != null && affectedAgent != null && affectedAgent.IsHuman && blow.DamageType != DamageTypes.Invalid && (agentState == AgentState.Unconscious || agentState == AgentState.Killed))
            {
                bool isFriendly = affectorAgent.Team != null && affectedAgent.Team != null && affectorAgent.Team.Side == affectedAgent.Team.Side;
                Agent.Hitter assistingHitter = affectedAgent.GetAssistingHitter(affectorAgent.MissionPeer);
                MultiplayerClassDivisions.MPHeroClass classForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(affectedAgent.Character);
                if (affectorAgent.MissionPeer != null && affectorAgent.MissionPeer.Representative is FlagDominationMissionRepresentative representative1)
                {
                    int gainsFromKillData = representative1.GetGoldGainsFromKillData(MPPerkObject.GetPerkHandler(affectorAgent.MissionPeer), MPPerkObject.GetPerkHandler(assistingHitter?.HitterPeer), classForCharacter, false, isFriendly);
                    if (gainsFromKillData > 0)
                        this.ChangeCurrentGoldForPeer(affectorAgent.MissionPeer, representative1.Gold + gainsFromKillData);
                }
                if (assistingHitter?.HitterPeer != null && assistingHitter.HitterPeer.Peer.Communicator.IsConnectionActive && !assistingHitter.IsFriendlyHit && assistingHitter.HitterPeer.Representative is FlagDominationMissionRepresentative representative2)
                {
                    int gainsFromKillData = representative2.GetGoldGainsFromKillData(MPPerkObject.GetPerkHandler(affectorAgent.MissionPeer), MPPerkObject.GetPerkHandler(assistingHitter.HitterPeer), classForCharacter, true, isFriendly);
                    if (gainsFromKillData > 0)
                        this.ChangeCurrentGoldForPeer(assistingHitter.HitterPeer, representative2.Gold + gainsFromKillData);
                }
                if (affectedAgent.MissionPeer?.Team != null && !isFriendly)
                {
                    IEnumerable<(MissionPeer, int)> goldRewardsOnDeath = MPPerkObject.GetPerkHandler(affectedAgent.MissionPeer)?.GetTeamGoldRewardsOnDeath();
                    if (goldRewardsOnDeath != null)
                    {
                        foreach ((MissionPeer peer, int baseAmount) in goldRewardsOnDeath)
                        {
                            if (baseAmount > 0 && peer?.Representative is FlagDominationMissionRepresentative representative3)
                            {
                                int fromAllyDeathReward = representative3.GetGoldGainsFromAllyDeathReward(baseAmount);
                                if (fromAllyDeathReward > 0)
                                    this.ChangeCurrentGoldForPeer(peer, representative3.Gold + fromAllyDeathReward);
                            }
                        }
                    }
                }
            }
            if (affectedAgent.IsPlayerControlled)
                affectedAgent.MissionPeer.GetComponent<FlagDominationMissionRepresentative>().UpdateSelectedClassServer((Agent)null);
            else if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue() > 0 && (this.WarmupComponent == null || !this.WarmupComponent.IsInWarmup) && !affectedAgent.IsMount && affectedAgent.OwningAgentMissionPeer != null && affectedAgent.Formation != null && affectedAgent.Formation.CountOfUnits == 1)
            {
                if (!GameNetwork.IsDedicatedServer)
                {
                    MatrixFrame cameraFrame = Mission.Current.GetCameraFrame();
                    Vec3 position = cameraFrame.origin + cameraFrame.rotation.u;
                    MBSoundEvent.PlaySound(SoundEvent.GetEventIdFromString("event:/alerts/report/squad_wiped"), position);
                }
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage((GameNetworkMessage)new FormationWipedMessage());
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.ExcludeOtherTeamPlayers, affectedAgent.OwningAgentMissionPeer.GetNetworkPeer());
            }
            if (this._gameType != MultiplayerGameType.Battle || !affectedAgent.IsHuman || !this.RoundController.IsRoundInProgress || blow.DamageType == DamageTypes.Invalid || agentState != AgentState.Unconscious && agentState != AgentState.Killed)
                return;
            MultiplayerClassDivisions.MPHeroClass classForCharacter1 = MultiplayerClassDivisions.GetMPHeroClassForCharacter(affectedAgent.Character);
            if (affectorAgent?.MissionPeer != null && affectorAgent.Team != affectedAgent.Team)
            {
                FlagDominationMissionRepresentative representative = affectorAgent.MissionPeer.Representative as FlagDominationMissionRepresentative;
                int dataAndUpdateFlags = representative.GetGoldGainFromKillDataAndUpdateFlags(classForCharacter1, false);
                this.ChangeCurrentGoldForPeer(affectorAgent.MissionPeer, representative.Gold + dataAndUpdateFlags);
            }
            Agent.Hitter assistingHitter1 = affectedAgent.GetAssistingHitter(affectorAgent?.MissionPeer);
            if (assistingHitter1?.HitterPeer == null || assistingHitter1.IsFriendlyHit)
                return;
            FlagDominationMissionRepresentative representative4 = assistingHitter1.HitterPeer.Representative as FlagDominationMissionRepresentative;
            int dataAndUpdateFlags1 = representative4.GetGoldGainFromKillDataAndUpdateFlags(classForCharacter1, true);
            this.ChangeCurrentGoldForPeer(assistingHitter1.HitterPeer, representative4.Gold + dataAndUpdateFlags1);

        }

        private static void replaceGeneralIfNeeded(Agent affectedAgent)
        {
            Team teamAffected = affectedAgent.Team;
            if (teamAffected != null && teamAffected.GeneralAgent == affectedAgent)
            {
                MBDebug.Print("BLMissionMDomination - OnAgentDeleted - General Dead", 0, DebugColor.Green);

                teamAffected.GeneralAgent = null;
                // set ai now TODO choose peer
                if (teamAffected.TeamAI == null && teamAffected.GeneralAgent == null)
                {
                    BLTeamAIGeneral teamAI = new BLTeamAIGeneral(Mission.Current, teamAffected);
                    teamAI.AddTacticOption(new TacticDefensiveEngagement(teamAffected));
                    teamAI.AddTacticOption(new TacticCharge(teamAffected));
                    TeamQuerySystemUtils.setPowerFix(Mission.Current);
                    foreach (Formation formation in teamAffected.FormationsIncludingSpecialAndEmpty)
                    {
                        teamAI.OnUnitAddedToFormationForTheFirstTime(formation);
                    }
                    teamAffected.AddTeamAI(teamAI);
                    teamAffected.SetPlayerRole(false, false);
                }
            }
        }

        public override float GetTroopNumberMultiplierForMissingPlayer(MissionPeer spawningPeer)
        {
            if (this._gameType == MultiplayerGameType.Captain)
            {
                List<MissionPeer>[] missionPeerListArray = new List<MissionPeer>[2];
                for (int index = 0; index < missionPeerListArray.Length; ++index)
                    missionPeerListArray[index] = new List<MissionPeer>();
                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                    if (component != null && component.Team != null && component.Team.Side != BattleSideEnum.None)
                        missionPeerListArray[(int)component.Team.Side].Add(component);
                }
                int[] numArray = new int[2]
                {
          0,
          MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue()
                };
                numArray[0] = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();
                int f = missionPeerListArray[1].Count + numArray[1] - (missionPeerListArray[0].Count + numArray[0]);
                BattleSideEnum index1 = f == 0 ? BattleSideEnum.None : (f < 0 ? BattleSideEnum.Attacker : BattleSideEnum.Defender);
                if (index1 == spawningPeer.Team.Side)
                    return (float)(1.0 + (double)MathF.Abs(f) / (double)(missionPeerListArray[(int)index1].Count + numArray[(int)index1]));
            }
            return 1f;
        }

        protected override void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            if (networkPeer.IsServerPeer)
                return;
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage((GameNetworkMessage)new FlagDominationMoraleChangeMessage(this.MoraleRounded));
            GameNetwork.EndModuleEventAsServer();


            // networkHandler.HandleNewClientAfterLoadingFinished(networkPeer); is before
            // networkHandler.HandleLateNewClientAfterLoadingFinished(networkPeer); where are send agent data
            //foreach (var characterMessage in listCharacterMessage)
            //{
            //    GameNetwork.BeginModuleEventAsServer(networkPeer);
            //    GameNetwork.WriteMessage(characterMessage);
            //    GameNetwork.EndModuleEventAsServer();
            //}

            MBDebug.Print("RBMissionMpGameMode - HandleNewClientAfterLoadingFinished", 0, DebugColor.Green);
        }

    }



}