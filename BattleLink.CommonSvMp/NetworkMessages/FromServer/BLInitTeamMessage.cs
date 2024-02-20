using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{
    /// <summary>
    /// NetworkMessage to synchronize TeamInfoModel between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLInitTeamMessage : GameNetworkMessage
    {
        private static CompressionInfo.Integer IndexCompressionInfo = new CompressionInfo.Integer(0, 3);

        public int id;
        public string name;

        //public uint Color;
        //public uint Color2;

        //public uint BannerBackgroundColor;
        //public uint BannerForegroundColor;

        // reason : bs HandleServerEventAddTeam
        public string FactionBannerKey;

        public BLInitTeamMessage()
        {

        }


        protected override void OnWrite()
        {
            //MBDebug.Print("BL BLInitCultureMessage " + id, 0, DebugColor.Green);

            WriteIntToPacket(id, IndexCompressionInfo);

            //WriteUintToPacket(Color, CompressionBasic.ColorCompressionInfo);
            //WriteUintToPacket(Color2, CompressionBasic.ColorCompressionInfo);

            ////WriteUintToPacket(BannerBackgroundColor, CompressionBasic.ColorCompressionInfo);
            ////WriteUintToPacket(BannerForegroundColor, CompressionBasic.ColorCompressionInfo);

            WriteStringToPacket(name);
            WriteStringToPacket(FactionBannerKey);

            //WriteUintToPacket(debugIdCpt++, CompressionBasic.ColorCompressionInfo);

        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;

            id = ReadIntFromPacket(IndexCompressionInfo, ref bufferReadValid);

            //Color = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            //Color2 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            //BannerBackgroundColor = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            //BannerForegroundColor = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            name = ReadStringFromPacket(ref bufferReadValid);
            FactionBannerKey = ReadStringFromPacket(ref bufferReadValid);


            // uint debug = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            // InformationManager.DisplayMessage(new InformationMessage("BattleLink - BLInitCultureMessage num: " + debug, TaleWorlds.Library.Color.FromUint(0x008000)));
            //InformationManager.DisplayMessage(new InformationMessage("BattleLink - BLInitCultureMessage id: " + id, TaleWorlds.Library.Color.FromUint(0x008000)));

            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync team informations";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionDetailed;
        }

    }
}
