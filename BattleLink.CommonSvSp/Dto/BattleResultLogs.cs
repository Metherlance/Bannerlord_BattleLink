using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common
{
    public class BattleResultLogs
    {
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

    public interface IBLLog { }// : ISerializable

    [Serializable]
    public class AgentCreatedLog : IBLLog
    {
        public short partyId = -1;
        public string characterStringId = "";
        public string playerUserName;

        public short agentId;
        public float health;// TODO remove no health on OnAgentCreated

        public short mountAgentId = -1;
    }

    [Serializable]
    public class ScoreHitLog : IBLLog
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
