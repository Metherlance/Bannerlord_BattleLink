using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using static TaleWorlds.Library.Debug;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{
    /// <summary>
    /// NetworkMessage to synchronize AgentsInfoModel between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLAgentLabelRefreshMessage : GameNetworkMessage
    {
        private static CompressionInfo.Integer AgentCompressionInfo = new CompressionInfo.Integer(0, 11);
        
        public int id;
        
        public BLAgentLabelRefreshMessage()
        {

        }


        protected override void OnWrite()
        {
            WriteIntToPacket(id, CompressionMission.AgentCompressionInfo);   
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;
                     
            id = ReadIntFromPacket(CompressionMission.AgentCompressionInfo, ref bufferReadValid);
          
            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync Agent Label informations";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionDetailed;
        }

    }
}
