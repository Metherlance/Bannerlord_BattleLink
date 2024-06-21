using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{
    /// <summary>
    /// NetworkMessage to synchronize BLPartyTroopAvaibilityMessage between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLPartyTroopAvaibilityMessage : GameNetworkMessage
    {
        private static readonly CompressionInfo.Integer int0To2048Compression = new CompressionInfo.Integer(0, 11);

        public int teamIndex;
        public int partyIndex; // not used yet
        public BasicCharacterObject characterObject;

        public int nbTroopReserved;
        public int nbTroopAvailabled;

        public int nbTroopReservedUsed;
        public int nbTroopAvailabledUsed;

        public bool isPeerTroop = false;

        public BLPartyTroopAvaibilityMessage()
        {

        }

        protected override void OnWrite()
        {
            WriteIntToPacket(teamIndex, CompressionMission.AgentPrefabComponentIndexCompressionInfo);//0,16
            WriteIntToPacket(partyIndex, CompressionBasic.GUIDIntCompressionInfo);
            WriteObjectReferenceToPacket(characterObject, CompressionBasic.GUIDCompressionInfo);

            WriteIntToPacket(nbTroopReserved, int0To2048Compression);
            WriteIntToPacket(nbTroopAvailabled, int0To2048Compression);
            WriteIntToPacket(nbTroopReservedUsed, int0To2048Compression);
            WriteIntToPacket(nbTroopAvailabledUsed, int0To2048Compression);
            WriteBoolToPacket(isPeerTroop);
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
                     
            teamIndex = ReadIntFromPacket(CompressionMission.AgentPrefabComponentIndexCompressionInfo, ref bufferReadValid);
            partyIndex = ReadIntFromPacket(CompressionBasic.GUIDIntCompressionInfo, ref bufferReadValid);
            characterObject = (BasicCharacterObject)ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref bufferReadValid);

            nbTroopReserved = ReadIntFromPacket(int0To2048Compression, ref bufferReadValid);
            nbTroopAvailabled = ReadIntFromPacket(int0To2048Compression, ref bufferReadValid);
            nbTroopReservedUsed = ReadIntFromPacket(int0To2048Compression, ref bufferReadValid);
            nbTroopAvailabledUsed = ReadIntFromPacket(int0To2048Compression, ref bufferReadValid);

            isPeerTroop = ReadBoolFromPacket(ref bufferReadValid);

            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync BLPartyTroopAvaibilityMessage";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Equipment;
        }

    }
}
