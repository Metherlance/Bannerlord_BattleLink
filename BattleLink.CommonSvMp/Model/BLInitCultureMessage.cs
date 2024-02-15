using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.Common.Model
{

    /// <summary>
    /// NetworkMessage to synchronize AgentsInfoModel between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLInitCultureMessage : GameNetworkMessage
    {
        //private static uint debugIdCpt = 0;

        public uint mbguid;

        public string id;
        public string name;

        public uint Color;
        public uint Color2;

        public uint ClothAlternativeColor1;
        public uint ClothAlternativeColor2;

        public uint BannerBackgroundColor1;
        public uint BannerBackgroundColor2;

        public uint BannerForegroundColor1;
        public uint BannerForegroundColor2;

        public string FactionBannerKey;

        public BLInitCultureMessage()
        {

        }


        protected override void OnWrite()
        {
            //MBDebug.Print("BL BLInitCultureMessage " + id, 0, DebugColor.Green);

            WriteUintToPacket(mbguid, CompressionBasic.ColorCompressionInfo);

            WriteStringToPacket(id);
            WriteStringToPacket(name);

            WriteUintToPacket(Color, CompressionBasic.ColorCompressionInfo);
            WriteUintToPacket(Color2, CompressionBasic.ColorCompressionInfo);

            WriteUintToPacket(ClothAlternativeColor1, CompressionBasic.ColorCompressionInfo);
            WriteUintToPacket(ClothAlternativeColor2, CompressionBasic.ColorCompressionInfo);

            WriteUintToPacket(BannerBackgroundColor1, CompressionBasic.ColorCompressionInfo);
            WriteUintToPacket(BannerBackgroundColor2, CompressionBasic.ColorCompressionInfo);

            WriteUintToPacket(BannerForegroundColor1, CompressionBasic.ColorCompressionInfo);
            WriteUintToPacket(BannerForegroundColor2, CompressionBasic.ColorCompressionInfo);

            WriteStringToPacket(FactionBannerKey);

            //WriteUintToPacket(debugIdCpt++, CompressionBasic.ColorCompressionInfo);

        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;

            mbguid = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            id = ReadStringFromPacket(ref bufferReadValid);
            name = ReadStringFromPacket(ref bufferReadValid);

            Color = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            Color2 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            ClothAlternativeColor1 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            ClothAlternativeColor2 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            BannerBackgroundColor1 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            BannerBackgroundColor2 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            BannerForegroundColor1 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            BannerForegroundColor2 = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            FactionBannerKey = ReadStringFromPacket(ref bufferReadValid);


            // uint debug = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);
            // InformationManager.DisplayMessage(new InformationMessage("BattleLink - BLInitCultureMessage num: " + debug, TaleWorlds.Library.Color.FromUint(0x008000)));
            //InformationManager.DisplayMessage(new InformationMessage("BattleLink - BLInitCultureMessage id: " + id, TaleWorlds.Library.Color.FromUint(0x008000)));

            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync character informations";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionDetailed;
        }

    }
}
