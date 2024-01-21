using BattleLink.Common.Behavior;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using BattleLink.Common.Spawn.Warmup;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.Agent;
using BodyProperties = TaleWorlds.Core.BodyProperties;
using Equipment = TaleWorlds.Core.Equipment;
using Team = TaleWorlds.MountAndBlade.Team;

namespace BattleLink.Common.Spawn.Battle
{
    // TODO to delete?
    public class BLFlagDominationSpawningBehavior : SpawningBehaviorBase
    {
        // SpawningBehaviorBase private fields

        private const int MaxAgentCount = 2040;
        private static int AgentCountThreshold;
        private MissionTime _nextTimeToCleanUpMounts;

        private static FieldInfo fieldHasCalledSpawningEnded = typeof(SpawningBehaviorBase).GetField("_hasCalledSpawningEnded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo fieldOnSpawningEnded = typeof(SpawningBehaviorBase).GetField("OnSpawningEnded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static EventInfo eventOnSpawningEnded = typeof(SpawningBehaviorBase).GetEvent("OnSpawningEnded", BindingFlags.Public | BindingFlags.Instance);

        // ****

        private const int EnforcedSpawnTimeInSeconds = 15;
        private float _spawningTimer;
        private bool _spawningTimerTicking;
        private bool _haveBotsBeenSpawned;
        private bool _roundInitialSpawnOver;
        private MissionMultiplayerFlagDomination _flagDominationMissionController;
        private MultiplayerRoundController _roundController;//RB TODO
        private List<KeyValuePair<MissionPeer, Timer>> _enforcedSpawnTimers;

        private Dictionary<Team, int[]> dicFormationTroopMax = new Dictionary<Team, int[]>();
        private Dictionary<Team, int[]> dicFormationTroopIndex = new Dictionary<Team, int[]>();

        public BLFlagDominationSpawningBehavior() : base()
        {
            _enforcedSpawnTimers = new List<KeyValuePair<MissionPeer, Timer>>();
            AgentCountThreshold = (int)((double)MaxAgentCount * 0.899999976158142);
        }

        public override void Initialize(SpawnComponent spawnComponent)
        {
            base.Initialize(spawnComponent);

            _nextTimeToCleanUpMounts = MissionTime.Now;

            _flagDominationMissionController = Mission.GetMissionBehavior<MissionMultiplayerFlagDomination>();
            _roundController = Mission.GetMissionBehavior<MultiplayerRoundController>();//RB TODO
            _roundController.OnRoundStarted += new Action(RequestStartSpawnSession);
            _roundController.OnRoundEnding += new Action(RequestStopSpawnSession);
            _roundController.OnRoundEnding += new Action(SetRemainingAgentsInvulnerable);
            if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue() == 0)
                _roundController.EnableEquipmentUpdate();
            OnAllAgentsFromPeerSpawnedFromVisuals += new Action<MissionPeer>(OnAllAgentsFromPeerSpawnedFromVisualsAction);
            OnPeerSpawnedFromVisuals += new Action<MissionPeer>(OnPeerSpawnedFromVisualsAction);
        }

        public override void Clear()
        {
            base.Clear();
            _roundController.OnRoundStarted -= new Action(RequestStartSpawnSession);
            _roundController.OnRoundEnding -= new Action(SetRemainingAgentsInvulnerable);
            _roundController.OnRoundEnding -= new Action(RequestStopSpawnSession);
            OnAllAgentsFromPeerSpawnedFromVisuals -= new Action<MissionPeer>(OnAllAgentsFromPeerSpawnedFromVisualsAction);
            OnPeerSpawnedFromVisuals -= new Action<MissionPeer>(OnPeerSpawnedFromVisualsAction);
        }

        public override void OnTick(float dt)
        {
            if (_spawningTimerTicking)
                _spawningTimer += dt;
            if (IsSpawningEnabled)
            {
                if (!_roundInitialSpawnOver && IsRoundInProgress())
                {
                    foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                    {
                        MissionPeer component = networkPeer.GetComponent<MissionPeer>();
                        if (component?.Team != null && component.Team.Side != BattleSideEnum.None)
                            SpawnComponent.SetEarlyAgentVisualsDespawning(component);
                    }
                    _roundInitialSpawnOver = true;
                    Mission.AllowAiTicking = true;
                }
                //   if (!(_roundInitialSpawnOver && (_spawningTimer <= (float)MultiplayerOptions.OptionType.RoundPreparationTimeLimit.GetIntValue())))
                if (_roundInitialSpawnOver && _spawningTimer > (float)MultiplayerOptions.OptionType.RoundPreparationTimeLimit.GetIntValue())
                {
                    Mission.Current.AddMissionBehavior(new BLAgentLogsLogic());
                    //Mission.Current.AddMissionBehavior(new BattleEndLogic());
                    //Mission.Current.AddMissionBehavior(new BLBattleSummaryResult());

                    SpawnAgents();

                    IsSpawningEnabled = false;
                    _spawningTimer = 0.0f;
                    _spawningTimerTicking = false;
                }
            }

            // base.OnTick(dt);// no base call

            int countAllAgents = Mission.Current.AllAgents.Count;
            if (countAllAgents > AgentCountThreshold && this._nextTimeToCleanUpMounts.IsPast)
            {
                this._nextTimeToCleanUpMounts = MissionTime.SecondsFromNow(5f);
                for (int index = Mission.Current.MountsWithoutRiders.Count - 1; index >= 0; --index)
                {
                    KeyValuePair<Agent, MissionTime> mountsWithoutRider = Mission.Current.MountsWithoutRiders[index];
                    Agent key = mountsWithoutRider.Key;
                    if ((double)mountsWithoutRider.Value.ElapsedSeconds > 30.0)
                    {
                        key.FadeOut(false, false);
                    }
                }
            }

            if (this.IsSpawningEnabled || !this.IsRoundInProgress())
            {
                return;
            }

            bool _hasCalledSpawningEnded = (bool)fieldHasCalledSpawningEnded.GetValue(this);
            if (SpawningDelayTimer >= SpawningEndDelay && !_hasCalledSpawningEnded)
            {
                Mission.Current.AllowAiTicking = true;//p.Raise("SomethingHappening", EventArgs.Empty);

                if (fieldOnSpawningEnded.GetValue(this) != null)
                {
                    eventOnSpawningEnded.GetRaiseMethod().Invoke(this, new object[] { });//this.Raise
                }
                fieldHasCalledSpawningEnded.SetValue(this, true);
            }

            SpawningDelayTimer += dt;

        }

        public override void RequestStartSpawnSession()
        {
            if (IsSpawningEnabled)
                return;
            Mission.Current.SetBattleAgentCount(-1);
            IsSpawningEnabled = true;
            _haveBotsBeenSpawned = false;
            _spawningTimerTicking = true;
            ResetSpawnCounts();
            ResetSpawnTimers();
        }

        // this not overide...
        protected override void SpawnAgents()
        {

            var blTeams = BLReferentialHolder.listTeam;

            //  count all agents by formation
            foreach (Team team in Mission.Teams)
            {
                var formationClass0 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                dicFormationTroopMax.Add(team, formationClass0);
                formationClass0 = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                dicFormationTroopIndex.Add(team, formationClass0);
            }

            // ****
            // set initial position to the teams

            var missionDeployementSide = Mission.Current.DeploymentPlan;
            // get by reflexion _battleSideDeploymentPlans in missionDeployementSide
            var field = typeof(MissionDeploymentPlan).GetField("_battleSideDeploymentPlans", BindingFlags.NonPublic | BindingFlags.Instance);
            BattleSideDeploymentPlan[] _battleSideDeploymentPlans = (BattleSideDeploymentPlan[])field.GetValue(missionDeployementSide);

            int[] battleSizeForActivePhase = new int[2];
            foreach (var teamDto in blTeams)
            {
                int sideNumberTroop = 0;
                BattleSideEnum side = (BattleSideEnum)Enum.Parse(typeof(BattleSideEnum), teamDto.BattleSide);
                Team teamSide = BattleSideEnum.Attacker.ToString().Equals(teamDto.BattleSide) ? Mission.AttackerTeam : Mission.DefenderTeam;

                Mission.Current.DeploymentPlan.GetFormationPlan(side, 0, DeploymentPlanType.Initial);

                var sideDeploymentPlan = _battleSideDeploymentPlans[(int)side];
                var fieldDeploymentPlanInitial = typeof(BattleSideDeploymentPlan).GetField("_initialPlan", BindingFlags.NonPublic | BindingFlags.Instance);
                DeploymentPlan deploymentPlanInitial = (DeploymentPlan)fieldDeploymentPlanInitial.GetValue(sideDeploymentPlan);
                deploymentPlanInitial.ClearAddedTroops();
                deploymentPlanInitial.ClearPlan();

                dicFormationTroopMax.TryGetValue(teamSide, out int[] dicFormationMaxCpt);

                foreach (var party in teamDto.Parties)
                {
                    foreach (var troop in party.Troops)
                    {
                        BasicCharacterObject character = MBObjectManager.Instance.GetObject<BasicCharacterObject>(troop.Id);
                        var formationClass = Mission.Current.GetAgentTroopClass(side, character);

                        sideNumberTroop += troop.Number;
                        dicFormationMaxCpt[(int)formationClass] += troop.Number;

                        if (character.HasMount())
                        {
                            deploymentPlanInitial.AddTroops(formationClass, 0, troop.Number);
                        }
                        else
                        {
                            deploymentPlanInitial.AddTroops(formationClass, troop.Number, 0);
                        }

                    }
                }
                battleSizeForActivePhase[(int)side] = sideNumberTroop;
            }

            float spawnPathOffset = 0.0f;
            if (Mission.HasSpawnPath)
            {
                spawnPathOffset = Mission.GetBattleSizeOffset(MathF.Max(battleSizeForActivePhase[0], battleSizeForActivePhase[1]), Mission.GetInitialSpawnPath());//this.GetBattleSizeForActivePhase()
            }
            Mission.MakeDeploymentPlanForSide(BattleSideEnum.Attacker, DeploymentPlanType.Initial, spawnPathOffset);
            Mission.MakeDeploymentPlanForSide(BattleSideEnum.Defender, DeploymentPlanType.Initial, spawnPathOffset);

            //****



            var playersConfig = BLReferentialHolder.listPlayer;

            var dicPlayersConfig = playersConfig.ToDictionary(p => p.UserName, p => p);
            var attSide = blTeams.FindAll(x => BattleSideEnum.Attacker.ToString().Equals(x.BattleSide)).First();
            //var dicAttTroops = attSide.Parties[0].Troops.ToDictionary(t => t.Id, t => t);
            var defSide = blTeams.FindAll(x => BattleSideEnum.Defender.ToString().Equals(x.BattleSide)).First();
            //var dicDefTroops = defSide.Parties[0].Troops.ToDictionary(t => t.Id, t => t);

            // spawn player configured
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (!networkPeer.IsSynchronized)
                {
                    continue;
                }

                dicPlayersConfig.TryGetValue(networkPeer.UserName, out var playerBlConfig);
                if (playerBlConfig == null)
                {
                    continue;
                }

                MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
                if (missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None && missionPeer.ControlledAgent == null && !CheckIfEnforcedSpawnTimerExpiredForPeer(missionPeer))
                // (component.ControlledAgent == null && !component.HasSpawnedAgentVisuals && component.Team != null && component.Team != Mission.SpectatorTeam && component.TeamInitialPerkInfoReady && component.SpawnTimer.Check(Mission.CurrentTime))
                {
                    Team team = missionPeer.Team;
                    //var dicTeamTroops = team == Mission.AttackerTeam ? dicAttTroops : dicDefTroops;
                    var teamSide = team == Mission.AttackerTeam ? attSide : defSide;

                    Party partySelected = null;
                    Troop troopSelected = null;
                    foreach (var party in teamSide.Parties)
                    {
                        if (troopSelected == null && (playerBlConfig.PartyIndex < 0 || playerBlConfig.PartyIndex.Equals(party.Index)))
                        {
                            foreach (var troop in party.Troops)
                            {
                                if (troop.Id.Equals(playerBlConfig.TroopId))
                                {
                                    troopSelected = troop;
                                    partySelected = party;
                                    break;
                                }
                            }
                        }
                    }

                    // dicTeamTroops.TryGetValue(playerBlConfig.TroopId, out var troop);
                    if (troopSelected == null)
                    {
                        continue;
                    }
                    var character = MBObjectManager.Instance.GetObject<Model.BLCharacterObject>(troopSelected.Id);
                    if (!teamSide.IsOpen && !character.IsHero)
                    {
                        continue;
                    }
                    troopSelected.Number -= 1;
                    if (troopSelected.Number == 0)
                    {
                        partySelected.Troops.Remove(troopSelected);
                    }

                    spawnTroopId(team, teamSide, partySelected.Index, character, troopSelected.HitPoints, missionPeer);
                }
            }

            // spawn general
            spawnRandomGeneral(attSide, Mission.AttackerTeam);
            spawnRandomGeneral(defSide, Mission.DefenderTeam);

            // spawn player random troop
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (!networkPeer.IsSynchronized)
                {
                    continue;
                }

                MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
                if (missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None && missionPeer.ControlledAgent == null && !CheckIfEnforcedSpawnTimerExpiredForPeer(missionPeer))
                // (component.ControlledAgent == null && !component.HasSpawnedAgentVisuals && component.Team != null && component.Team != Mission.SpectatorTeam && component.TeamInitialPerkInfoReady && component.SpawnTimer.Check(Mission.CurrentTime))
                {
                    Team team = missionPeer.Team;
                    //var listTeamTroops = team == Mission.AttackerTeam ? listAttTroops : listDefTroops;
                    var teamSide = team == Mission.AttackerTeam ? attSide : defSide;
                    if (!teamSide.IsOpen)
                    {
                        continue;
                    }
                    var party = teamSide.Parties.ElementAt(MBRandom.RandomInt(teamSide.Parties.Count));
                    var troop = party.Troops.ElementAt(MBRandom.RandomInt(party.Troops.Count));

                    troop.Number -= 1;
                    if (troop.Number == 0)
                    {
                        party.Troops.Remove(troop);
                    }
                    var character = MBObjectManager.Instance.GetObject<Model.BLCharacterObject>(troop.Id);
                    spawnTroopId(team, teamSide, party.Index, character, troop.HitPoints, missionPeer);
                }
            }

            // spawn bots
            foreach (var side in blTeams)
            {
                Team teamSide = BattleSideEnum.Attacker.ToString().Equals(side.BattleSide) ? Mission.AttackerTeam : Mission.DefenderTeam;
                foreach (var party in side.Parties)
                {
                    foreach (Troop troop in party.Troops)
                    {
                        int nbTroopLeftToSpawn = troop.Number;
                        for (int index = 0; index < nbTroopLeftToSpawn; index += 1)
                        {
                            troop.Number -= 1;
                            var character = MBObjectManager.Instance.GetObject<Model.BLCharacterObject>(troop.Id);
                            spawnTroopId(teamSide, side, party.Index, character, troop.HitPoints, null);
                        }
                    }
                }

                if (teamSide.TeamAI == null && teamSide.GeneralAgent == null)
                {
                    BLTeamAIGeneral teamAI = new BLTeamAIGeneral(Mission.Current, teamSide);
                    teamAI.AddTacticOption(new TacticDefensiveEngagement(teamSide));
                    teamAI.AddTacticOption(new TacticCharge(teamSide));
                    TeamQuerySystemUtils.setPowerFix(Mission.Current);
                    foreach (Formation formation in teamSide.FormationsIncludingSpecialAndEmpty)
                    {
                        teamAI.OnUnitAddedToFormationForTheFirstTime(formation);
                    }
                    teamSide.AddTeamAI(teamAI);
                    teamSide.SetPlayerRole(false, false);
                }

            }

        }

        private void spawnRandomGeneral(DtoSpSv.Team attSide, Team team)
        {
            if (team.GeneralAgent == null && attSide.IsOpen)
            {
                Party partyGeneral = null;
                Troop troopGeneral = null;
                foreach (var party in attSide.Parties)
                {
                    foreach (var troop in party.Troops)
                    {
                        if (troopGeneral == null && troop.Id.Equals(party.GeneralId))
                        {
                            troopGeneral = troop;
                            partyGeneral = party;
                            break;
                        }
                    }
                }
                if (troopGeneral == null)
                {
                    MBDebug.Print("No general not normal " + team.Side, 0, DebugColor.Red);
                    return;
                }

                foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                {
                    if (!networkPeer.IsSynchronized)
                    {
                        continue;
                    }

                    MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
                    if (missionPeer.Team != null && missionPeer.Team.Side == team.Side && missionPeer.ControlledAgent == null && !CheckIfEnforcedSpawnTimerExpiredForPeer(missionPeer))
                    // (component.ControlledAgent == null && !component.HasSpawnedAgentVisuals && component.Team != null && component.Team != Mission.SpectatorTeam && component.TeamInitialPerkInfoReady && component.SpawnTimer.Check(Mission.CurrentTime))
                    {

                        troopGeneral.Number -= 1;
                        if (troopGeneral.Number == 0)
                        {
                            partyGeneral.Troops.Remove(troopGeneral);
                        }
                        var character = MBObjectManager.Instance.GetObject<Model.BLCharacterObject>(troopGeneral.Id);
                        spawnTroopId(team, attSide, partyGeneral.Index, character, troopGeneral.HitPoints, missionPeer);
                    }
                }
            }
        }

        private void spawnTroopId(Team team, DtoSpSv.Team teamSide, int partyIndex, BLCharacterObject character, int hitpoint, MissionPeer? missionPeer)
        {
            AgentBuildData agentBuildData = BLWarmupSpawningBehavior.buidAgentData(team, character);
            agentBuildData.TroopOrigin(new BLBattleAgentOrigin(character, partyIndex));

            if (missionPeer != null)
            {
                agentBuildData.MissionPeer(missionPeer).MakeUnitStandOutOfFormationDistance(7f);
            }

            //bool firstSpawn = missionPeer.SpawnCountThisRound == 0;
            //MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(missionPeer.Team, agentBuildData.hasMount(), firstSpawn);
            //Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();
            //// Randomize direction so players don't go all straight.
            //initialDirection.RotateCCW(MBRandom.RandomFloatRanged(-MathF.PI / 3f, MathF.PI / 3f));
            //agentBuildData.InitialPosition(in spawnFrame.origin).InitialDirection(in initialDirection);

            var formationClass = GetAgentTroopClass(team.Side, character);
            Formation formation = team.GetFormation(formationClass);
            dicFormationTroopMax.TryGetValue(team, out int[] dicFormationMaxCpt);
            dicFormationTroopIndex.TryGetValue(team, out int[] dicFormationIndexCpt);
            if (!formation.HasBeenPositioned)
            {
                formation.BeginSpawn(dicFormationMaxCpt[(int)formationClass], agentBuildData.hasMount());
                Mission.Current.SetFormationPositioningFromDeploymentPlan(formation);
            }

            int formationTroopCount = dicFormationMaxCpt[(int)formationClass];
            int formationTroopIndex = dicFormationIndexCpt[(int)formationClass];
            agentBuildData.Formation(formation).FormationTroopSpawnCount(formationTroopCount).FormationTroopSpawnIndex(formationTroopIndex);
            dicFormationIndexCpt[(int)formationClass] += 1;

            //if (GameMode.ShouldSpawnVisualsForServer(networkPeer)) { 
            //    this.AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData, missionPeer.SelectedTroopIndex);
            //}
            // GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData);

            Agent agent = Mission.SpawnAgent(agentBuildData);

            agent.WieldInitialWeapons();
            //if (!agent.HasMount || agentBuildData.hasExtraSlotEquipped())
            //{
            //    agent.WieldInitialWeapons();
            //}

            agent.Health = hitpoint;

            if (missionPeer != null)
            {
                missionPeer.HasSpawnedAgentVisuals = true;
            }
            else
            {
                agent.AIStateFlags |= AIStateFlag.Alarmed;
            }

            //component.ControlledFormation = formation;
            //if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue() > 0)
            //{
            //    int troopCount = MPPerkObject.GetTroopCount(heroClassForPeer, intValue, spawnPerkHandler);
            //    IEnumerable<(EquipmentIndex, EquipmentElement)> alternativeEquipments2 = spawnPerkHandler?.GetAlternativeEquipments(false);
            //    for (int index = 0; index < troopCount; ++index)
            //    {
            //        if (index + 1 >= agentVisualsForPeer)
            //            updateExistingAgentVisuals = false;
            //        SpawnBotVisualsInPlayerFormation(component, index + 1, team, basicCultureObject3, heroClassForPeer.TroopCharacter.StringId, formation, updateExistingAgentVisuals, troopCount, alternativeEquipments2);
            //    }
            //}

            // Set General
            if (team.GeneralAgent == null && teamSide.Parties[0].GeneralId == character.StringId && missionPeer != null)
            {
                team.SetPlayerRole(true, false);
                team.GeneralAgent = agent;
                formation.PlayerOwner = agent;
                for (int i = 0; i < (int)FormationClass.NumberOfRegularFormations; i += 1)
                {
                    agent.Team.AssignPlayerAsSergeantOfFormation(missionPeer, (FormationClass)i);
                }

                int countUnitTotal = 0;
                foreach (var form in team.FormationsIncludingSpecialAndEmpty)
                {
                    countUnitTotal += form.CountOfUnits;
                }
                Mission.GetMissionBehavior<MissionMultiplayerGameModeFlagDominationClient>().OnBotsControlledChanged(missionPeer, countUnitTotal, countUnitTotal);
            }
        }

        private void OnPeerSpawnedFromVisualsAction(MissionPeer peer)
        {
            if (peer.ControlledFormation == null)
                return;
            peer.ControlledAgent.Team.AssignPlayerAsSergeantOfFormation(peer, peer.ControlledFormation.FormationIndex);
        }

        private void OnAllAgentsFromPeerSpawnedFromVisualsAction(MissionPeer peer)
        {
            if (peer.ControlledFormation != null)
            {
                peer.ControlledFormation.OnFormationDispersed();
                peer.ControlledFormation.SetMovementOrder(MovementOrder.MovementOrderFollow(peer.ControlledAgent));
                NetworkCommunicator networkPeer = peer.GetNetworkPeer();
                if (peer.BotsUnderControlAlive != 0 || peer.BotsUnderControlTotal != 0)
                {
                    GameNetwork.BeginBroadcastModuleEvent();
                    GameNetwork.WriteMessage(new BotsControlledChange(networkPeer, peer.BotsUnderControlAlive, peer.BotsUnderControlTotal));
                    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
                    Mission.GetMissionBehavior<MissionMultiplayerGameModeFlagDominationClient>().OnBotsControlledChanged(peer, peer.BotsUnderControlAlive, peer.BotsUnderControlTotal);
                }
                if (peer.Team == Mission.AttackerTeam)
                {
                    ++Mission.NumOfFormationsSpawnedTeamOne;
                }
                else
                {
                    ++Mission.NumOfFormationsSpawnedTeamTwo;
                }
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SetSpawnedFormationCount(Mission.NumOfFormationsSpawnedTeamOne, Mission.NumOfFormationsSpawnedTeamTwo));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }


            if (_flagDominationMissionController.UseGold())
            {
                bool flag = peer.Team == Mission.AttackerTeam;
                Team defenderTeam = Mission.DefenderTeam;
                MultiplayerClassDivisions.MPHeroClass mpHeroClass = MultiplayerClassDivisions.GetMPHeroClasses(MBObjectManager.Instance.GetObject<BasicCultureObject>(
                    flag ? MultiplayerOptions.OptionType.CultureTeam1.GetStrValue() : MultiplayerOptions.OptionType.CultureTeam2.GetStrValue()))
                    .ElementAt(peer.SelectedTroopIndex);

                int num = _flagDominationMissionController.GetMissionType() == MultiplayerGameType.Battle ? mpHeroClass.TroopBattleCost : mpHeroClass.TroopCost;
                _flagDominationMissionController.ChangeCurrentGoldForPeer(peer, _flagDominationMissionController.GetCurrentGoldForPeer(peer) - num);
            }
        }

        private void BotFormationSpawned(Team team)
        {
            if (team == Mission.AttackerTeam)
            {
                Mission.NumOfFormationsSpawnedTeamOne++;
            }
            else if (team == Mission.DefenderTeam)
            {
                Mission.NumOfFormationsSpawnedTeamTwo++;
            }
        }

        private void AllBotFormationsSpawned()
        {
            if (Mission.NumOfFormationsSpawnedTeamOne != 0 || Mission.NumOfFormationsSpawnedTeamTwo != 0)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(new SetSpawnedFormationCount(Mission.NumOfFormationsSpawnedTeamOne, Mission.NumOfFormationsSpawnedTeamTwo));
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.None);
            }
        }

        public override bool AllowEarlyAgentVisualsDespawning(MissionPeer lobbyPeer)
        {
            if (MultiplayerOptions.OptionType.NumberOfBotsPerFormation.GetIntValue() == 0)
            {
                if (!_roundController.IsRoundInProgress)
                {
                    return false;
                }

                if (!lobbyPeer.HasSpawnTimerExpired && lobbyPeer.SpawnTimer.Check(Mission.Current.CurrentTime))
                {
                    lobbyPeer.HasSpawnTimerExpired = true;
                }

                return lobbyPeer.HasSpawnTimerExpired;
            }

            return false;
        }

        protected override bool IsRoundInProgress()
        {
            return _roundController.IsRoundInProgress;
        }

        private void CreateEnforcedSpawnTimerForPeer(MissionPeer peer, int durationInSeconds)
        {
            if (!_enforcedSpawnTimers.Any((pair) => pair.Key == peer))
            {
                _enforcedSpawnTimers.Add(new KeyValuePair<MissionPeer, Timer>(peer, new Timer(Mission.CurrentTime, durationInSeconds)));
                MBDebug.Print("EST for " + peer.Name + " set to " + durationInSeconds + " seconds.", 0, DebugColor.Yellow);
            }
        }

        private bool CheckIfEnforcedSpawnTimerExpiredForPeer(MissionPeer peer)
        {
            KeyValuePair<MissionPeer, Timer> keyValuePair = _enforcedSpawnTimers.FirstOrDefault((pr) => pr.Key == peer);
            if (keyValuePair.Key == null)
            {
                return false;
            }

            if (peer.ControlledAgent != null)
            {
                _enforcedSpawnTimers.RemoveAll((p) => p.Key == peer);
                MBDebug.Print("EST for " + peer.Name + " is no longer valid (spawned already).", 0, DebugColor.Yellow, 64uL);
                return false;
            }

            Timer value = keyValuePair.Value;
            if (peer.HasSpawnedAgentVisuals && value.Check(Mission.Current.CurrentTime))
            {
                SpawnComponent.SetEarlyAgentVisualsDespawning(peer);
                _enforcedSpawnTimers.RemoveAll((p) => p.Key == peer);
                MBDebug.Print("EST for " + peer.Name + " has expired.", 0, DebugColor.Yellow, 64uL);
                return true;
            }

            return false;
        }

        public override void OnClearScene()
        {
            base.OnClearScene();
            _enforcedSpawnTimers.Clear();
            _roundInitialSpawnOver = false;
        }

        // to remove/ modify?
        protected void SpawnBotInBotFormation(int visualsIndex, Team agentTeam, BasicCultureObject cultureLimit, BasicCharacterObject character, Formation formation)
        {
            AgentBuildData agentBuildData = new AgentBuildData(character).Team(agentTeam).TroopOrigin(new BasicBattleAgentOrigin(character)).VisualsIndex(visualsIndex)
                .EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(character, visualsIndex))
                .Formation(formation)
                .IsFemale(character.IsFemale)
                .ClothingColor1(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color : cultureLimit.ClothAlternativeColor)
                .ClothingColor2(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2);
            agentBuildData.Equipment(Equipment.GetRandomEquipmentElements(character, !GameNetwork.IsMultiplayer, isCivilianEquipment: false, agentBuildData.AgentEquipmentSeed));
            agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentRace, agentBuildData.AgentIsFemale, character.GetBodyPropertiesMin(), character.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, character.HairTags, character.BeardTags, character.TattooTags));
            Mission.SpawnAgent(agentBuildData).AIStateFlags |= Agent.AIStateFlag.Alarmed;
        }

        // to remove/ modify?
        protected void SpawnBotVisualsInPlayerFormation(MissionPeer missionPeer, int visualsIndex, Team agentTeam, BasicCultureObject cultureLimit, string troopName, Formation formation, bool updateExistingAgentVisuals, int totalCount, IEnumerable<(EquipmentIndex, EquipmentElement)> alternativeEquipments)
        {
            BasicCharacterObject @object = MBObjectManager.Instance.GetObject<BasicCharacterObject>(troopName);
            AgentBuildData agentBuildData = new AgentBuildData(@object).Team(agentTeam).OwningMissionPeer(missionPeer).VisualsIndex(visualsIndex)
                .TroopOrigin(new BasicBattleAgentOrigin(@object))
                .EquipmentSeed(MissionLobbyComponent.GetRandomFaceSeedForCharacter(@object, visualsIndex))
                .Formation(formation)
                .IsFemale(@object.IsFemale)
                .ClothingColor1(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color : cultureLimit.ClothAlternativeColor)
                .ClothingColor2(agentTeam.Side == BattleSideEnum.Attacker ? cultureLimit.Color2 : cultureLimit.ClothAlternativeColor2);
            Equipment randomEquipmentElements = Equipment.GetRandomEquipmentElements(@object, !GameNetwork.IsMultiplayer, isCivilianEquipment: false, MBRandom.RandomInt());
            if (alternativeEquipments != null)
            {
                foreach (var alternativeEquipment in alternativeEquipments)
                {
                    randomEquipmentElements[alternativeEquipment.Item1] = alternativeEquipment.Item2;
                }
            }

            agentBuildData.Equipment(randomEquipmentElements);
            agentBuildData.BodyProperties(BodyProperties.GetRandomBodyProperties(agentBuildData.AgentRace, agentBuildData.AgentIsFemale, @object.GetBodyPropertiesMin(), @object.GetBodyPropertiesMax(), (int)agentBuildData.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData.AgentEquipmentSeed, @object.HairTags, @object.BeardTags, @object.TattooTags));
            NetworkCommunicator networkPeer = missionPeer.GetNetworkPeer();
            if (GameMode.ShouldSpawnVisualsForServer(networkPeer))
            {
                AgentVisualSpawnComponent.SpawnAgentVisualsForPeer(missionPeer, agentBuildData, -1, isBot: true, totalCount);
                if (agentBuildData.AgentVisualsIndex == 0)
                {
                    missionPeer.HasSpawnedAgentVisuals = true;
                    missionPeer.EquipmentUpdatingExpired = false;
                }
            }

            GameMode.HandleAgentVisualSpawning(networkPeer, agentBuildData, totalCount, useCosmetics: false);
        }
        public FormationClass GetAgentTroopClass(BattleSideEnum battleSide, BasicCharacterObject agentCharacter)
        {
            FormationClass troopClass = agentCharacter.GetFormationClass();

            if (Mission.Current.IsSiegeBattle || Mission.Current.IsSallyOutBattle && battleSide == BattleSideEnum.Attacker)
            {
                troopClass = troopClass.DismountedClass();
            }

            return troopClass;
        }

    }
}

