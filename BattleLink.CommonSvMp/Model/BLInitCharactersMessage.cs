using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Common.Model
{
    /// <summary>
    /// NetworkMessage to synchronize AgentsInfoModel between server and clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLInitCharactersMessage : GameNetworkMessage
    {
        private static CompressionInfo.Integer SkillValueCompressionInfo = new CompressionInfo.Integer(0, 9);
        private static CompressionInfo.Integer ByteCompressionInfo = new CompressionInfo.Integer(0, 8);

        public uint mbguid;

        public string id;
        public string name;
        public bool isFemale;
        public int defaultGroup;
        public int occupation;

        public TaleWorlds.Core.BodyProperties bodyPropertiesValue;
        public TaleWorlds.Core.BodyProperties bodyPropertiesValueMax;

        public int indexDic;

        public int skillRiding;
        public int skillOneHanded;
        public int skillTwoHanded;
        public int skillPolearm;
        public int skillCrossbow;
        public int skillBow;
        public int skillThrowing;
        public int skillAthletics;

        public string culture;


        public BLInitCharactersMessage()
        {

        }


        protected override void OnWrite()
        {
            //MBDebug.Print("BL BLInitCharactersMessage " + id, 0, DebugColor.Green);

            WriteUintToPacket(mbguid, CompressionBasic.ColorCompressionInfo);

            WriteStringToPacket(id);
            WriteStringToPacket(name);
            WriteBoolToPacket(isFemale);
            WriteIntToPacket(defaultGroup, CompressionMission.FactionCompressionInfo);//0,4
            WriteBodyPropertiesToPacket(bodyPropertiesValue);
            WriteBodyPropertiesToPacket(bodyPropertiesValueMax);

            WriteIntToPacket(indexDic, ByteCompressionInfo);

            WriteIntToPacket(skillRiding, SkillValueCompressionInfo);
            WriteIntToPacket(skillOneHanded, SkillValueCompressionInfo);
            WriteIntToPacket(skillTwoHanded, SkillValueCompressionInfo);
            WriteIntToPacket(skillPolearm, SkillValueCompressionInfo);
            WriteIntToPacket(skillCrossbow, SkillValueCompressionInfo);
            WriteIntToPacket(skillBow, SkillValueCompressionInfo);
            WriteIntToPacket(skillThrowing, SkillValueCompressionInfo);
            WriteIntToPacket(skillAthletics, SkillValueCompressionInfo);

            WriteStringToPacket(culture);
        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;

            mbguid = ReadUintFromPacket(CompressionBasic.ColorCompressionInfo, ref bufferReadValid);

            id = ReadStringFromPacket(ref bufferReadValid);
            name = ReadStringFromPacket(ref bufferReadValid);
            isFemale = ReadBoolFromPacket(ref bufferReadValid);
            defaultGroup = ReadIntFromPacket(CompressionMission.FactionCompressionInfo, ref bufferReadValid);//0,4
            bodyPropertiesValue = ReadBodyPropertiesFromPacket(ref bufferReadValid);
            bodyPropertiesValueMax = ReadBodyPropertiesFromPacket(ref bufferReadValid);

            indexDic = ReadIntFromPacket(ByteCompressionInfo, ref bufferReadValid);

            skillRiding = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillOneHanded = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillTwoHanded = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillPolearm = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillCrossbow = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillBow = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillThrowing = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);
            skillAthletics = ReadIntFromPacket(SkillValueCompressionInfo, ref bufferReadValid);

            culture = ReadStringFromPacket(ref bufferReadValid);

            //InformationManager.DisplayMessage(new InformationMessage("BattleLink - BLInitCharactersMessage id: " + id, TaleWorlds.Library.Color.FromUint(0x008000)));

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
