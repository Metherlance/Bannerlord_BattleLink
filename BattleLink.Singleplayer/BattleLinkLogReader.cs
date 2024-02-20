using BattleLink.Common;
using BattleLink.Singleplayer.Patch;
using HarmonyLib;
using SandBox;
using SandBox.Missions.MissionLogics;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using Team = TaleWorlds.MountAndBlade.Team;

namespace BattleLink.Singleplayer
{
    public class BattleLinkLogReader
    {
        private static FieldInfo fieldAgentTeam = typeof(Agent).GetField("<Team>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo fieldTeamSide = typeof(Team).GetField("<Side>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo fieldTeamActiveAgent = typeof(Team).GetField("_activeAgents", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo fieldAgentComponents = typeof(Agent).GetField("_components", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo filedWeapon = typeof(MissionWeapon).GetField("_weapons", BindingFlags.NonPublic | BindingFlags.Instance);
        private static PropertyInfo propertyCBResultPlayerVictory = typeof(CampaignBattleResult).GetProperty("PlayerVictory", BindingFlags.Public | BindingFlags.Instance);
        private static PropertyInfo propertyCBResultPlayerDefeat = typeof(CampaignBattleResult).GetProperty("PlayerDefeat", BindingFlags.Public | BindingFlags.Instance);
        private static MethodInfo methodMissionBehaviorOnGetAgentState = typeof(MissionBehavior).GetMethod("OnGetAgentState", BindingFlags.NonPublic | BindingFlags.Instance);


        private Agent[] agents;
        private BattleAgentLogic _battleAgentLogic;
        public List<MissionBehavior> MissionBehaviors { get; }

        private BattleLinkLogReader()
        {
            agents = new Agent[2048];
            MissionBehaviors = new List<MissionBehavior>()
            {
               new BattleAgentLogic(),
               new BattleSurgeonLogic(),
            };
        }

        private void execute(List<IBLLog> mpBattleLogs)
        {
            MapEvent battle = PlayerEncounter.Battle;

            //battle.AttackerSide.

            // copy of EncounterAttackConsequence
            PlayerEncounter.StartAttackMission();
            MapEvent.PlayerMapEvent.BeginWait();
            // _battleAgentLogic = Mission.Current.GetMissionBehavior<BattleAgentLogic>();

            // Dictionary<int, PartyBase> agentToPartyIndex = new Dictionary<int, PartyBase>();

            //battle.AttackerSide.Parties.ElementAt(0).CommitXpGain();
            //SideDto playerSide;


            Mission mission = (Mission)FormatterServices.GetUninitializedObject(typeof(Mission));
            // CampaignMission.OpenBattleMission("", false);
            MBTeam mbT = (MBTeam)FormatterServices.GetUninitializedObject(typeof(MBTeam));
            //Team tAtt = new TaleWorlds.MountAndBlade.Team(mbT, BattleSideEnum.Attacker, null,0,0,null);
            //Team tDef = new TaleWorlds.MountAndBlade.Team(mbT, BattleSideEnum.Attacker, null, 0, 0, null);
            Team tAtt = (Team)FormatterServices.GetUninitializedObject(typeof(Team));
            Team tDef = (Team)FormatterServices.GetUninitializedObject(typeof(Team));
            fieldTeamSide.SetValue(tAtt, BattleSideEnum.Attacker);
            fieldTeamSide.SetValue(tDef, BattleSideEnum.Defender);

            fieldTeamActiveAgent.SetValue(tAtt, new MBList<Agent>());
            fieldTeamActiveAgent.SetValue(tDef, new MBList<Agent>());

            //this._teamAgents = new MBList<Agent>();
            //this._cachedEnemyDataForFleeing = new MBList<(float, WorldPosition, int, Vec2, Vec2, bool)>();
            //if (GameNetwork.IsReplay)
            //    return;
            //this.FormationsIncludingSpecialAndEmpty = new MBList<Formation>(10);
            //this.FormationsIncludingEmpty = new MBList<Formation>(8);
            //for (int index = 0; index < 10; ++index)
            //{
            //    Formation formation = new Formation(this, index);
            //    this.FormationsIncludingSpecialAndEmpty.Add(formation);
            //    if (index < 8)
            //        this.FormationsIncludingEmpty.Add(formation);
            //    formation.AI.OnActiveBehaviorChanged += new Action<Formation>(this.FormationAI_OnActiveBehaviorChanged);
            //}




            Dictionary<int, (MapEventParty, Team, List<FlattenedTroopRosterElement>)> dicParties = new Dictionary<int, (MapEventParty, Team, List<FlattenedTroopRosterElement>)>();
            //Dictionary<(int,string), int> dicTroopIndex = new Dictionary<(int, string), int>();
            foreach (var party in battle.AttackerSide.Parties)
            {
                dicParties.Add(party.Party.Index, (party, tAtt, party.Troops.ToList()));
            }
            foreach (var party in battle.DefenderSide.Parties)
            {
                dicParties.Add(party.Party.Index, (party, tDef, party.Troops.ToList()));
            }

            var harmony = new Harmony("BattleLink.Singleplayer.Patch.Temp");
            var GetAgentFlags = AccessTools.Method(typeof(Agent), "GetAgentFlags");
            var GetAgentFlagsPrefix = AccessTools.Method(typeof(AgentPatch), "GetAgentFlags");
            harmony.Patch(GetAgentFlags, prefix: new HarmonyMethod(GetAgentFlagsPrefix));
            var GetStateFlags = AccessTools.Method(typeof(Agent), "get_State");
            var GetStatePrefix = AccessTools.Method(typeof(AgentPatch), "get_State");
            harmony.Patch(GetStateFlags, prefix: new HarmonyMethod(GetStatePrefix));

            //WinnerLog sideWinner = null;
            foreach (IBLLog log in mpBattleLogs)
            {
                if (log is AgentCreatedLog agentBuildLog)
                {
                    //ConstructorInfo ctor = typeof(Agent).GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic)[0];
                    //Agent agent = (Agent) ctor.Invoke(new object[] { null, null, Agent.CreationType.FromRoster, null });

                    Agent agent = (Agent)FormatterServices.GetUninitializedObject(typeof(Agent));
                    agents[agentBuildLog.agentId] = agent;

                    //BasicCharacterObject character = MBObjectManager.Instance.GetObject<BasicCharacterObject>(agentBuildLog.characterStringId);
                    //agent.Character = character;

                    BLAgentOriginSp bLAgentOriginSp = new BLAgentOriginSp();
                    agent.Origin = bLAgentOriginSp;
                    bLAgentOriginSp.agentState = AgentState.Active;
                    agent.Health = agentBuildLog.health;

                    if (agentBuildLog.partyId >= 0)
                    {
                        bLAgentOriginSp.agentFlag = AgentFlag.IsHumanoid;
                        bLAgentOriginSp.partyIndex = agentBuildLog.partyId;

                        var partyTeamElement = dicParties[agentBuildLog.partyId];
                        TaleWorlds.MountAndBlade.Team team = partyTeamElement.Item2;
                        List<FlattenedTroopRosterElement> troopElements = partyTeamElement.Item3;

                        fieldAgentTeam.SetValue(agent, team);

                        var itemToRemove = troopElements.Where(r => r.Troop.StringId.Equals(agentBuildLog.characterStringId)).First();
                        troopElements.Remove(itemToRemove);
                        bLAgentOriginSp.troopElement = itemToRemove;

                        // CharacterObject not BasicCharacterObject
                        agent.Character = itemToRemove.Troop;


                        fieldAgentComponents.SetValue(agent, new MBList<AgentComponent>()
                        {
                            new CampaignAgentComponent(agent),
                        });

                        bLAgentOriginSp.mapEventParty = partyTeamElement.Item1;

                        if (team.GeneralAgent == null && partyTeamElement.Item1.Party.General!=null && partyTeamElement.Item1.Party.General.StringId == agentBuildLog.characterStringId)
                        {
                            team.GeneralAgent = agent;
                        }
                        if ("main_hero".Equals(agentBuildLog.characterStringId))
                        {
                            //Agent.Main = agent;
                        }

                        // set hp
                        if (itemToRemove.Troop.IsHero)
                        {
                            agent.Health = itemToRemove.Troop.HeroObject.HitPoints;
                        }


                    }
                    else
                    {
                        // Charater null for horses
                        bLAgentOriginSp.agentFlag = AgentFlag.Mountable;
                    }

                    if (agentBuildLog.mountAgentId >= 0)
                    {
                        var agentMount = agents[agentBuildLog.mountAgentId];
                        mount(agent, agentMount);
                    }

                }
                else if (log is ScoreHitLog scoreHitLog)
                {
                    var affectorAgent = scoreHitLog.affectorAgentId >= 0 ? agents[scoreHitLog.affectorAgentId] : null;
                    var affectedAgent = scoreHitLog.affectedAgentId >= 0 ? agents[scoreHitLog.affectedAgentId] : null;
                    WeaponComponentData weaponComponentData = new WeaponComponentData(null, scoreHitLog.weaponClass, scoreHitLog.weaponFlags);
                    Blow blow = new Blow();
                    blow.InflictedDamage = (int)scoreHitLog.damagedHp;
                    blow.DamageType = scoreHitLog.damageType;
                    blow.VictimBodyPart = scoreHitLog.victimBodyPart;
                    AttackCollisionData attackCollisionData = new AttackCollisionData();

                    var affectorWeapon = new MissionWeapon();
                    filedWeapon.SetValue(affectorWeapon, new List<WeaponComponentData>()
                    {
                        weaponComponentData,
                    });

                    // SP

                    foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
                    {
                        missionBehavior.OnAgentHit(affectedAgent, affectorAgent, affectorWeapon, blow, attackCollisionData);
                        missionBehavior.OnScoreHit(affectedAgent, affectorAgent, weaponComponentData, scoreHitLog.isBlocked, scoreHitLog.isSiegeEngineHit, blow, attackCollisionData, scoreHitLog.damagedHp, scoreHitLog.hitDistance, scoreHitLog.shotDifficulty);
                    }

                    //before the health reduce
                    bool isFatal = affectedAgent.Health <= scoreHitLog.damagedHp;
                    affectedAgent.Health -= scoreHitLog.damagedHp;


                    if (isFatal)
                    {
                        BLAgentOriginSp blOrigin = (BLAgentOriginSp)affectorAgent.Origin;
                        BLAgentOriginSp blOrignAffected = (BLAgentOriginSp)affectedAgent.Origin;

                        if (affectedAgent.Character == null)
                        {
                            // dont count died horses
                            blOrignAffected.agentState = AgentState.Killed;
                            continue;
                        }
                        //for debug
                        if (affectedAgent.State != AgentState.Active)
                        {
                            // ??? error in damage calcul  FIXME
                            continue;
                        }

                        var agentNewState = GetAgentState(affectorAgent, affectedAgent, scoreHitLog.damageType, scoreHitLog.weaponFlags);
                        blOrignAffected.agentState = agentNewState;

                        KillingBlow killingBlow = new KillingBlow();
                        killingBlow.DamageType = scoreHitLog.damageType;
                        killingBlow.VictimBodyPart = scoreHitLog.victimBodyPart;
                        killingBlow.InflictedDamage = (int)scoreHitLog.damagedHp;

                        foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
                        {
                            missionBehavior.OnEarlyAgentRemoved(affectedAgent, affectorAgent, agentNewState, killingBlow);
                        }
                        foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
                        {
                            missionBehavior.OnAgentRemoved(affectedAgent, affectorAgent, agentNewState, killingBlow);
                        }

                    }

                }
                else if (log is AgentRemovedLog agentRemovedLog)
                {
                    //var affectedAgent = agentRemovedLog.affectedAgentId >= 0 ? agents[agentRemovedLog.affectedAgentId] : null;
                    //if (agentRemovedLog.agentState == AgentState.Killed)
                    //{
                    //    AgentState stateNew = MissionGetAgentState(affectedAgent);
                    //}

                    // TODO fleeing
                    //
                    //BLAgentOriginSp blOrigin = (BLAgentOriginSp) affectedAgent.Origin;
                    //var party = dicParties[blOrigin.partyIndex].Item1;

                    if (AgentState.Routed == agentRemovedLog.agentState)
                    {
                        //party.OnTroopRouted(blOrigin.troopElement.Descriptor);
                        var affectedAgent = agents[agentRemovedLog.affectedAgentId];
                        // no mount
                        if (affectedAgent.Character != null)
                        {
                            KillingBlow kBlow = new KillingBlow();
                            foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
                            {
                                missionBehavior.OnEarlyAgentRemoved(affectedAgent, null, AgentState.Routed, kBlow);
                            }
                            foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
                            {
                                missionBehavior.OnAgentRemoved(affectedAgent, null, AgentState.Routed, kBlow);
                            }
                        }

                    }
                    //else if (AgentState.Killed == agentRemovedLog.agentState)
                    //{

                    //}

                    //


                }
                else if (log is AgentMountLog agentMountLog)
                {
                    var agentRider = agents[agentMountLog.agentId];
                    var agentMount = agents[agentMountLog.mountAgentId];
                    mount(agentRider, agentMount);

                }
                else if (log is AgentDismountLog agentDismountLog)
                {
                    var agentRider = agents[agentDismountLog.agentId];
                    if (agentRider.Origin is BLAgentOriginSp blOrigin)
                    {
                        if (blOrigin.mount.Origin is BLAgentOriginSp mountBlOrigin)
                        {
                            mountBlOrigin.rider = null;
                        }
                        blOrigin.mount = null;
                    }
                }
                else if (log is WinnerLog winnerLog)
                {
                    //sideWinner = winnerLog;
                    CampaignBattleResult campaignBattleResult = PlayerEncounter.CampaignBattleResult;
                    bool playerVictory = winnerLog.winner == battle.PlayerSide;
                    propertyCBResultPlayerVictory.SetValue(campaignBattleResult, playerVictory);
                    propertyCBResultPlayerDefeat.SetValue(campaignBattleResult, !playerVictory);
                    // EnemyPulledBack one day TODO
                }
            }

            //for sur agent pour les pv
            foreach (Agent agent in agents)
            {
                if (agent != null && agent.Character != null && agent.Character.IsHero && agent.Origin is BLAgentOriginSp blOrigin)
                {
                    if (AgentState.Active == agent.State)
                    {
                        blOrigin.troopElement.Troop.HeroObject.HitPoints = (int)agent.Health;
                    }
                    // hp set to 1 in Hero MakeWounded
                    //else if (AgentState.Unconscious == agent.State && agent.Health<1)
                    //{
                    //    blOrigin.troopElement.Troop.HeroObject.HitPoints = 1;
                    //}
                }
            }

            //for debug
            //var nbAtt = PlayerEncounter.Battle.AttackerSide.GetTotalHealthyTroopCountOfSide();
            //var nbDef = PlayerEncounter.Battle.DefenderSide.GetTotalHealthyTroopCountOfSide();
            //foreach (Agent agent in agents)
            //{
            //    if (agent != null && agent.Character != null && AgentState.Active == agent.State && agent.Team.Side != sideWinner.winner)
            //    {
            //        //
            //        var a = nbAtt+ nbDef;
            //    }
            //}




            harmony.UnpatchAll("BattleLink.Singleplayer.Patch.Temp");

            //applyKilledWonded(battle.AttackerSide, battleResult.attacker);
            //applyKilledWonded(battle.DefenderSide, battleResult.defender);

            //CampaignBattleResult campaignBattleResult = PlayerEncounter.CampaignBattleResult;
            //Type t = typeof(CampaignBattleResult);
            //var p = t.GetProperty("PlayerVictory", BindingFlags.Public | BindingFlags.Instance);
            //p.SetValue(campaignBattleResult, true);

            PlayerEncounter.Update();

            //PlayerEncounter.Finish();
        }

        private static void mount(Agent agentRider, Agent agentMount)
        {
            if (agentRider.Origin is BLAgentOriginSp blOrigin && agentMount.Origin is BLAgentOriginSp mountBlOrigin)
            {
                blOrigin.mount = agentMount;
                mountBlOrigin.rider = agentRider;
            }
        }

        public static void ExecBattle(List<IBLLog> logs)
        {
            BattleLinkLogReader blbr = new BattleLinkLogReader();
            blbr.execute(logs);
        }

        public AgentState GetAgentState(
          Agent affectorAgent,
          Agent agent,
          DamageTypes damageType,
          WeaponFlags weaponFlags)
        {
            float useSurgeryProbability;
            float stateProbability = MissionGameModels.Current.AgentDecideKilledOrUnconsciousModel.GetAgentStateProbability(affectorAgent, agent, damageType, weaponFlags, out useSurgeryProbability);
            AgentState agentState = AgentState.None;
            bool usedSurgery = false;
            foreach (MissionBehavior missionBehavior in this.MissionBehaviors)
            {
                if (missionBehavior is IAgentStateDecider)
                {
                    agentState = (missionBehavior as IAgentStateDecider).GetAgentState(agent, stateProbability, out usedSurgery);
                    break;
                }
            }
            if (agentState == AgentState.None)
            {
                float randomFloat = MBRandom.RandomFloat;
                if ((double)randomFloat < (double)stateProbability)
                {
                    agentState = AgentState.Killed;
                    usedSurgery = true;
                }
                else
                {
                    agentState = AgentState.Unconscious;
                    if ((double)randomFloat > 1.0 - (double)useSurgeryProbability)
                        usedSurgery = true;
                }
            }
            if (usedSurgery && affectorAgent != null && affectorAgent.Team != null && agent.Team != null && affectorAgent.Team == agent.Team)
            {
                usedSurgery = false;
            }
            for (int index = 0; index < this.MissionBehaviors.Count; ++index)
            {
                methodMissionBehaviorOnGetAgentState.Invoke(this.MissionBehaviors[index], new object[] { agent, usedSurgery });
                //this.MissionBehaviors[index].OnGetAgentState(agent, usedSurgery);
            }
            return agentState;
        }

    }
}
