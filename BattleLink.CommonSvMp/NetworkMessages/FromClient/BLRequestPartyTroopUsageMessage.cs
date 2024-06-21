using BattleLink.Common.Model;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.MountAndBlade.MultiplayerClassDivisions;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{
    /// <summary>
    /// Request to use a party troop.
    /// Copy of https://github.com/Byak0/Alliance/ ... /Alliance.Common/Extensions/ClassLimiter/NetworkMessages/FromClient/RequestCharacterUsage.cs
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class BLRequestPartyTroopUsageMessage : GameNetworkMessage
    {
        public int teamIndex;
        public int partyIndex; // not used yet
        public BasicCharacterObject characterObject;

        //public MultiplayerClassDivisions.MPHeroClass MpHeroClass { get; private set; }

        public BLRequestPartyTroopUsageMessage() { }

        public BLRequestPartyTroopUsageMessage(int _teamIndex, MultiplayerClassDivisions.MPHeroClass mpHeroClass)
        {
            //MpHeroClass = mpHeroClass;
            teamIndex = _teamIndex;
            characterObject = mpHeroClass.TroopCharacter;
        }

        protected override void OnWrite()
        {
            WriteIntToPacket(teamIndex, CompressionMission.AgentPrefabComponentIndexCompressionInfo);//0,16
            WriteIntToPacket(partyIndex, CompressionBasic.GUIDIntCompressionInfo);
            WriteObjectReferenceToPacket(characterObject, CompressionBasic.GUIDCompressionInfo);
            //WriteObjectReferenceToPacket(MpHeroClass, CompressionBasic.GUIDCompressionInfo);
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
            teamIndex = ReadIntFromPacket(CompressionMission.AgentPrefabComponentIndexCompressionInfo, ref bufferReadValid);
            partyIndex = ReadIntFromPacket(CompressionBasic.GUIDIntCompressionInfo, ref bufferReadValid);
            characterObject = (BasicCharacterObject)ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref bufferReadValid);
            //MpHeroClass = (MultiplayerClassDivisions.MPHeroClass)ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref bufferReadValid);
            return bufferReadValid;
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

        protected override string OnGetLogFormat()
        {
            return "Request usage of a Party Troop.";
        }
    }
}
