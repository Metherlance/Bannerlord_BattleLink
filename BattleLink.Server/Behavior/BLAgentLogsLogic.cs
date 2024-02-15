using BattleLink.Common.Model;
using BattleLink.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Common.Behavior
{
    public class BLAgentLogsLogic : MissionBehavior
    {
        List<IBLLog> logs = new List<IBLLog>();


        public BLAgentLogsLogic()
        {
           
        }


        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            if (agent.Origin is BLBattleAgentOrigin origin)
            {
                logs.Add(new AgentCreatedLog()
                {
                    partyId = (short)origin.partyIndex,
                    characterStringId = agent.Character.StringId,
                    agentId = (short)agent.Index,
                    health = agent.Health,
                    mountAgentId = (short)(agent.MountAgent != null ? agent.MountAgent.Index : -1),

                });
            }
            //horse
            else
            {
                logs.Add(new AgentCreatedLog()
                {
                    partyId = -1,
                    characterStringId = "",
                    agentId = (short)agent.Index,
                    health = agent.Health,
                    mountAgentId = -1,
                });
            }


            //MBDebug.Print("RBDebugMissionLogic - OnAgentCreated", 0, DebugColor.Cyan);
        }

       
        public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
        {
            short affectorAgentIndex = (short)(affectorAgent != null ? affectorAgent.Index:-1);
            short affectedAgentIndex = (short)(affectedAgent != null ? affectedAgent.Index : -1);

            logs.Add(new ScoreHitLog()
            {
                affectorAgentId = affectorAgentIndex,
                affectedAgentId = affectedAgentIndex,
               // attackerWeapon = attackerWeapon,
                weaponClass = attackerWeapon!=null? attackerWeapon.WeaponClass : WeaponClass.Undefined,
                weaponFlags = attackerWeapon!=null? attackerWeapon.WeaponFlags : 0,
                isBlocked = isBlocked,
                isSiegeEngineHit = isSiegeEngineHit,
               // blow = blow,
                damageType = blow.DamageType,
                victimBodyPart = blow.VictimBodyPart,
               // collisionData = collisionData,
                damagedHp = damagedHp,
                hitDistance = hitDistance,
                shotDifficulty = shotDifficulty,
            });


        //MBDebug.Print("RBDebugMissionLogic - OnScoreHit", 0, DebugColor.Cyan);//3
    }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
           logs.Add(new AgentRemovedLog()
           {
                affectedAgentId = (short)affectedAgent.Index,
                agentState = agentState,
            });
        }

        public override void OnAgentMount(Agent agent)
        {
            logs.Add(new AgentMountLog()
            {
                agentId = (short)agent.Index,
                mountAgentId = (short)agent.MountAgent.Index,
            });
        }
        public override void OnAgentDismount(Agent agent)
        {
            logs.Add(new AgentDismountLog()
            {
                agentId = (short)agent.Index,
            });
        }


        protected override void OnEndMission()
        {
            var RoundController = base.Mission.GetMissionBehavior<MultiplayerRoundController>();

            logs.Add(new WinnerLog()
            {
                winner = RoundController.RoundWinner,
            });

            //BL_MPBattle_2185888_Initializer.xml
            //BL_MPBattle_2185888_Result_.xml

            string filenameInit = BLReferentialHolder.initializerFilename;
            string filenameResult = filenameInit.Replace("_Initializer.xml", "_Result.csbin");


            string pathServerFileResultFinished = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished", filenameResult);
            if (File.Exists(pathServerFileResultFinished))
            {
                File.Delete(pathServerFileResultFinished);
            }
            BattleResultLogs.Serialize(logs, pathServerFileResultFinished);

            //cross local
            //Serialize(logs, System.IO.Path.Combine(BasePath.Name, "..", "Mount & Blade II Bannerlord", "Modules", "BattleLink.Singleplayer", "Battles", filenameResult));


            // Note:  move file initilizer to finished and Set next map are done in BLMissionMpDomination


            MBDebug.Print("BLAgentLogsLogic - OnEndMission", 0, DebugColor.Cyan);
        }
    }

}