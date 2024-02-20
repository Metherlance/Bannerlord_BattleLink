using BattleLink.Common.DtoSpSv;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using BattleLink.Server;
using System.IO;
using System.Xml.Serialization;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Behavior
{
    public class BLCommonMissionEndLogic : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public BLCommonMissionEndLogic()
        {
           
        }

        public override void OnEndMissionInternal()
        {
            base.OnEndMissionInternal();


            // I didnt found solution for send mission initializer record to client before MissionLoad (thread block) when we change map
            if(string.IsNullOrEmpty(BLReferentialHolder.nextBattleInitializerFilePath))
            {
                return;
            }

            XmlSerializer serializer = new XmlSerializer(typeof(MPBattleInitializer));
            MPBattleInitializer battleInitializer = null;
            using (Stream reader = new FileStream(BLReferentialHolder.nextBattleInitializerFilePath, FileMode.Open))
            {
                // Call the Deserialize method to restore the object's state.
                battleInitializer = (MPBattleInitializer)serializer.Deserialize(reader);
            }

            BLSendReferential2Client.setInitRecord(battleInitializer.MissionInitializerRecord, battleInitializer.battle);


            //BLSendReferential2Client.initRecordMessage.TimeOfDay = 2;

            GameNetwork.BeginBroadcastModuleEvent();
            GameNetwork.WriteMessage(BLSendReferential2Client.initRecordMessage);
            GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients, null);


            //BLSendReferential2Client.initRecordMessage.TimeOfDay = 3;

            //foreach (var networkPeer in GameNetwork.NetworkPeers)
            //{
            //    GameNetwork.BeginBroadcastModuleEvent();
            //    GameNetwork.WriteMessage(BLSendReferential2Client.initRecordMessage);
            //    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients, networkPeer);
            //}

            //BLSendReferential2Client.initRecordMessage.TimeOfDay = 4;

            //foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            //{
            //    GameNetwork.BeginModuleEventAsServer(networkPeer);
            //    GameNetwork.WriteMessage(BLSendReferential2Client.initRecordMessage);
            //    GameNetwork.EndModuleEventAsServer();
            //}

        }
    }

}