using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{

    /// <summary>
    /// NetworkMessage to synchronize MissionInitializerRecord from server to clients.
    /// Use DataType to specify which field is being send.
    /// </summary>
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
    public sealed class BLMissionInitializerRecordMessage : GameNetworkMessage
    {
        private static readonly CompressionInfo.Float timeOfDayCompression = new CompressionInfo.Float(0f, 24f, 8);
        private static readonly CompressionInfo.Float float0To1Compression = new CompressionInfo.Float(0f, 1f, 8);
        private static readonly CompressionInfo.Integer int0To7Compression = new CompressionInfo.Integer(0, 3);
        private static readonly CompressionInfo.Integer int0To16383Compression = new CompressionInfo.Integer(0, 14);
        private static readonly CompressionInfo.Float floatCompression = new CompressionInfo.Float(-1000f, 1000f, 16);
        private static readonly CompressionInfo.LongInteger longCompression = new CompressionInfo.LongInteger(0, 48);


        public string SceneName;
        public string SceneLevels;
        public float TimeOfDay;

        public bool DisableDynamicPointlightShadows;
        public int DecalAtlasGroup;

        public float RainInfoDensity;
        public float SnowInfoDensity;

        public float SkyInfoBrightness;

        public float TimeInfoTimeOfDay;
        public float TimeInfoNightTimeFactor;
        public float TimeInfoDrynessFactor;
        public float TimeInfoWinterTimeFactor;
        public int TimeInfoSeason;

        public int TerrainType;
        public bool NeedsRandomTerrain;
        public int RandomTerrainSeed;
        public string AtmosphereName;

        public long campaignTimeTick;
        public float partyPosLogicalX;
        public float partyPosLogicalY;
        public float partyPosLogicalZ;
        public string gameId;

        public float SunInfoAltitude;
        public float SunInfoAngle;
        public Vec3 SunInfoColor;
        public float SunInfoBrightness;
        public float SunInfoMaxBrightness;
        public float SunInfoSize;
        public float SunInfoRayStrength;

        public float AmbientInfoRayleighConstant;
        public float AmbientInfoMieScatterStrength;
        public Vec3 AmbientInfoAmbientColor;
        public float AmbientInfoEnvironmentMultiplier;

        public float PostProInfoMinExposure;
        public float PostProInfoBrightpassThreshold;
        public float PostProInfoMaxExposure;
        public float PostProInfoMiddleGray;

        public float FogInfoDensity;
        public Vec3 FogInfoColor;
        public float FogInfoFalloff;

        public string InterpolatedAtmosphereName;

        public BLMissionInitializerRecordMessage()
        {

        }


        protected override void OnWrite()
        {
            //MBDebug.Print("BL BLInitCultureMessage " + id, 0, DebugColor.Green);


            WriteStringToPacket(SceneName);
            WriteStringToPacket(SceneLevels);

            WriteFloatToPacket(TimeOfDay, timeOfDayCompression);

            WriteBoolToPacket(DisableDynamicPointlightShadows);

            WriteIntToPacket(DecalAtlasGroup, int0To7Compression);

            WriteFloatToPacket(RainInfoDensity, float0To1Compression);
            WriteFloatToPacket(SnowInfoDensity, float0To1Compression);
            WriteFloatToPacket(SkyInfoBrightness, floatCompression);
            WriteFloatToPacket(TimeInfoTimeOfDay, floatCompression);
            WriteFloatToPacket(TimeInfoNightTimeFactor, float0To1Compression);
            WriteFloatToPacket(TimeInfoDrynessFactor, float0To1Compression);
            WriteFloatToPacket(TimeInfoWinterTimeFactor, float0To1Compression);
            WriteIntToPacket(TimeInfoSeason, int0To7Compression);

            WriteIntToPacket(TerrainType, int0To7Compression);
            WriteBoolToPacket(NeedsRandomTerrain);
            WriteIntToPacket(RandomTerrainSeed, int0To16383Compression);
            WriteStringToPacket(AtmosphereName);

            WriteLongToPacket(campaignTimeTick, longCompression);
            //WriteFloatToPacket(partyPosLogicalX, floatCompression);
            //WriteFloatToPacket(partyPosLogicalY, floatCompression);
            //WriteFloatToPacket(partyPosLogicalZ, floatCompression);
            //WriteStringToPacket(gameId);

            WriteFloatToPacket(SunInfoAltitude, floatCompression);
            WriteFloatToPacket(SunInfoAngle, floatCompression);
            WriteVec3ToPacket(SunInfoColor, floatCompression);
            WriteFloatToPacket(SunInfoBrightness, floatCompression);
            WriteFloatToPacket(SunInfoMaxBrightness, floatCompression);
            WriteFloatToPacket(SunInfoSize, floatCompression);
            WriteFloatToPacket(SunInfoRayStrength, floatCompression);

            WriteFloatToPacket(AmbientInfoRayleighConstant, floatCompression);
            WriteFloatToPacket(AmbientInfoMieScatterStrength, floatCompression);
            WriteVec3ToPacket(AmbientInfoAmbientColor, floatCompression);
            WriteFloatToPacket(AmbientInfoEnvironmentMultiplier, floatCompression);

            WriteFloatToPacket(PostProInfoMinExposure, floatCompression);
            WriteFloatToPacket(PostProInfoBrightpassThreshold, floatCompression);
            WriteFloatToPacket(PostProInfoMaxExposure, floatCompression);
            WriteFloatToPacket(PostProInfoMiddleGray, floatCompression);


            WriteFloatToPacket(FogInfoDensity, floatCompression);
            WriteVec3ToPacket(FogInfoColor, floatCompression);
            WriteFloatToPacket(FogInfoFalloff, floatCompression);

            WriteStringToPacket(InterpolatedAtmosphereName);

        }

        protected override bool OnRead()
        {
            bool bufferReadValid = true;

            SceneName = ReadStringFromPacket(ref bufferReadValid);
            SceneLevels = ReadStringFromPacket(ref bufferReadValid);

            TimeOfDay = ReadFloatFromPacket(timeOfDayCompression, ref bufferReadValid);

            DisableDynamicPointlightShadows = ReadBoolFromPacket(ref bufferReadValid);
            DecalAtlasGroup = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);

            RainInfoDensity = ReadFloatFromPacket(float0To1Compression, ref bufferReadValid);
            SnowInfoDensity = ReadFloatFromPacket(float0To1Compression, ref bufferReadValid);
            SkyInfoBrightness = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            TimeInfoTimeOfDay = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            TimeInfoNightTimeFactor = ReadFloatFromPacket(float0To1Compression, ref bufferReadValid);
            TimeInfoDrynessFactor = ReadFloatFromPacket(float0To1Compression, ref bufferReadValid);
            TimeInfoWinterTimeFactor = ReadFloatFromPacket(float0To1Compression, ref bufferReadValid);
            TimeInfoSeason = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);

            TerrainType = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);
            NeedsRandomTerrain = ReadBoolFromPacket(ref bufferReadValid);
            RandomTerrainSeed = ReadIntFromPacket(int0To16383Compression, ref bufferReadValid);
            AtmosphereName = ReadStringFromPacket(ref bufferReadValid);

            campaignTimeTick = ReadLongFromPacket(longCompression, ref bufferReadValid);
            //partyPosLogicalX = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            //partyPosLogicalY = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            //partyPosLogicalZ = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            //gameId = ReadStringFromPacket(ref bufferReadValid);

            SunInfoAltitude = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            SunInfoAngle = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            SunInfoColor = ReadVec3FromPacket(floatCompression, ref bufferReadValid);
            SunInfoBrightness = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            SunInfoMaxBrightness = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            SunInfoSize = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            SunInfoRayStrength = ReadFloatFromPacket(floatCompression, ref bufferReadValid);


            AmbientInfoRayleighConstant = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            AmbientInfoMieScatterStrength = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            AmbientInfoAmbientColor = ReadVec3FromPacket(floatCompression, ref bufferReadValid);
            AmbientInfoEnvironmentMultiplier = ReadFloatFromPacket(floatCompression, ref bufferReadValid);

            PostProInfoMinExposure = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            PostProInfoBrightpassThreshold = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            PostProInfoMaxExposure = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            PostProInfoMiddleGray = ReadFloatFromPacket(floatCompression, ref bufferReadValid);

            FogInfoDensity = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
            FogInfoColor = ReadVec3FromPacket(floatCompression, ref bufferReadValid);
            FogInfoFalloff = ReadFloatFromPacket(floatCompression, ref bufferReadValid);

            InterpolatedAtmosphereName = ReadStringFromPacket(ref bufferReadValid);

            return bufferReadValid;
        }

        protected override string OnGetLogFormat()
        {
            return "Sync MissionInitializerRecord informations";
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.MissionDetailed;
        }

    }
}
