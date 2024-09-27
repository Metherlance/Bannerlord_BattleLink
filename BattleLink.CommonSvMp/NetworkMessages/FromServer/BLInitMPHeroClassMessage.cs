using BattleLink.Common.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Core.ItemObject;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.MultiplayerClassDivisions;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{
    /// <summary>
    /// NetworkMessage to synchronize class between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLInitMPHeroClassMessage : GameNetworkMessage
    {
        //private static CompressionInfo.Integer SkillValueCompressionInfo = new CompressionInfo.Integer(0, 9);
        //private static CompressionInfo.Integer ByteCompressionInfo = new CompressionInfo.Integer(0, 8);

        public uint mbguid;

        public string id;
        public string name;
 
        public BLCharacterObject characterObject;

        public int partyIndex;
        public int cost;
        public int nbReserved;
        public int nbTotal;


        public BLInitMPHeroClassMessage()
        {

        }

        protected override void OnWrite()
        {
            //MBDebug.Print("BL BLInitCharactersMessage " + id, 0, DebugColor.Green);

            WriteUintToPacket(mbguid, CompressionBasic.ColorCompressionInfo);

            WriteStringToPacket(id);
            WriteStringToPacket(name);

            WriteObjectReferenceToPacket(characterObject, CompressionBasic.GUIDCompressionInfo);


            WriteIntToPacket(partyIndex, CompressionBasic.GUIDIntCompressionInfo);
            WriteIntToPacket(cost, CompressionBasic.GUIDIntCompressionInfo);
            WriteIntToPacket(nbReserved, CompressionBasic.GUIDIntCompressionInfo);
            WriteIntToPacket(nbTotal, CompressionBasic.GUIDIntCompressionInfo);

        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;

            mbguid = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            id = ReadStringFromPacket(ref bufferReadValid);
            name = ReadStringFromPacket(ref bufferReadValid);
            
            characterObject = (BLCharacterObject) ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref bufferReadValid);

            partyIndex = ReadIntFromPacket(CompressionBasic.GUIDIntCompressionInfo, ref bufferReadValid);
            cost = ReadIntFromPacket(CompressionBasic.GUIDIntCompressionInfo, ref bufferReadValid);
            nbReserved = ReadIntFromPacket(CompressionBasic.GUIDIntCompressionInfo, ref bufferReadValid);
            nbTotal = ReadIntFromPacket(CompressionBasic.GUIDIntCompressionInfo, ref bufferReadValid);

            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync class informations";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionDetailed;
        }

    }
}
