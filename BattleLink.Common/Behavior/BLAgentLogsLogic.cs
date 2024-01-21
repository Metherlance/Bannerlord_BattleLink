using BattleLink.Common.Model;
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
            Serialize(logs, pathServerFileResultFinished);

            //cross local
            //Serialize(logs, System.IO.Path.Combine(BasePath.Name, "..", "Mount & Blade II Bannerlord", "Modules", "BattleLink.Singleplayer", "Battles", filenameResult));

            // move file initilizer to finished
            string pathServerFileInitializerFinished = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished", filenameInit);
            if (File.Exists(pathServerFileInitializerFinished))
            {
                File.Delete(pathServerFileInitializerFinished);
            }
            File.Move(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Pending", filenameInit), pathServerFileInitializerFinished);


            MBDebug.Print("BLAgentLogsLogic - OnEndMission", 0, DebugColor.Cyan);
        }


        public static void Serialize(List<IBLLog> logs, String filename)
        {
            //Create the stream to add object into it.  
            System.IO.Stream ms = File.OpenWrite(filename);

            //Format the object as Binary  
            BinaryFormatter formatter = new BinaryFormatter();

            //It serialize the employee object  
            formatter.Serialize(ms, logs);
            ms.Flush();
            ms.Close();
            ms.Dispose();
        }
        //Deserializing the List  
        public static T Deserialize<T>(String filename)
        {
            //Format the object as Binary  
            BinaryFormatter formatter = new BinaryFormatter();

            //Reading the file from the server  
            FileStream fs = File.Open(filename, FileMode.Open);
            object obj = formatter.Deserialize(fs);
            T logs = (T)obj;
            fs.Flush();
            fs.Close();
            fs.Dispose();
            
            return logs;
        }

    }


    public interface IBLLog{ }// : ISerializable

    [Serializable]
    public class AgentCreatedLog : IBLLog
    {
        public short partyId = -1;
        public string characterStringId = "";

        public short agentId;
        public float health;// TODO remove no health on OnAgentCreated

        public short mountAgentId = -1;
    }

    [Serializable]
    public class ScoreHitLog: IBLLog
    {
        public short affectorAgentId;
        public short affectedAgentId;

        //public WeaponComponentData attackerWeapon;
        public WeaponClass weaponClass;
        public WeaponFlags weaponFlags;

        public bool isBlocked;
        public bool isSiegeEngineHit;

        // public Blow blow;
        public DamageTypes damageType;
        public BoneBodyPartType victimBodyPart;

        //public AttackCollisionData collisionData;

        public float damagedHp;// max (damage blow , health left)
        //todo self incted damage
        public float hitDistance;
        public float shotDifficulty;
    }
    [Serializable]
    public class AgentRemovedLog : IBLLog
    {
        public short affectedAgentId;
        public AgentState agentState;
    }
    [Serializable]
    public class WinnerLog : IBLLog
    {
        public BattleSideEnum winner;
    }
    [Serializable]
    public class AgentMountLog : IBLLog
    {
        public short agentId;
        public short mountAgentId;
    }
    [Serializable]
    public class AgentDismountLog : IBLLog
    {
        public short agentId;
    }
}