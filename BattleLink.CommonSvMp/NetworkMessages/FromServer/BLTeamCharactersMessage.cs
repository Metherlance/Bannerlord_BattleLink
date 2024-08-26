using BattleLink.Common.Model;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem;
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
    /// NetworkMessage to synchronize AgentsInfoModel between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLTeamCharactersMessage : GameNetworkMessage
    {
        public int teamId;
        public List<MPHeroClass> characterObjects;

        public BLTeamCharactersMessage()
        {

        }

        protected override void OnWrite()
        {
            WriteIntToPacket(teamId, CompressionMission.AgentPrefabComponentIndexCompressionInfo);//0,16
            WriteIntToPacket(characterObjects.Count, CompressionMission.BoneIndexCompressionInfo);//0,64
            foreach(MPHeroClass characterObject in characterObjects)
            {
                GameNetworkMessage.WriteObjectReferenceToPacket(characterObject, CompressionBasic.GUIDCompressionInfo);
            }
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;

            teamId = ReadIntFromPacket(CompressionMission.AgentPrefabComponentIndexCompressionInfo, ref bufferReadValid);

            int nbCharacter = ReadIntFromPacket(CompressionMission.BoneIndexCompressionInfo, ref bufferReadValid);
            characterObjects = new List<MPHeroClass>(nbCharacter);
            for (int indexCharacter =0 ; indexCharacter< nbCharacter ; indexCharacter+=1)
            {
                var character = (MPHeroClass)GameNetworkMessage.ReadObjectReferenceFromPacket(MBObjectManager.Instance, CompressionBasic.GUIDCompressionInfo, ref bufferReadValid);
                characterObjects.Add(character);
            }

            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync team characters informations";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionDetailed;
        }

    }
}
