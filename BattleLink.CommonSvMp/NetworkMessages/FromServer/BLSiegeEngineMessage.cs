//using System.Collections.Generic;
//using TaleWorlds.Core;
//using TaleWorlds.Library;
//using TaleWorlds.MountAndBlade;
//using TaleWorlds.MountAndBlade.Network.Messages;

//namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
//{

//    /// <summary>
//    /// NetworkMessage to synchronize SiegeEngine from server to clients.
//    /// </summary>
//    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromServer)]
//    public sealed class BLSiegeEngineMessage : GameNetworkMessage
//    {
//        private static readonly CompressionInfo.Float timeOfDayCompression = new CompressionInfo.Float(0f, 24f, 8);
//        private static readonly CompressionInfo.Float float0To1Compression = new CompressionInfo.Float(0f, 1f, 8);
//        private static readonly CompressionInfo.Integer int0To7Compression = new CompressionInfo.Integer(0, 3);
//        private static readonly CompressionInfo.Integer int0To16383Compression = new CompressionInfo.Integer(0, 14);
//        private static readonly CompressionInfo.Float floatCompression = new CompressionInfo.Float(-10000f, 10000f, 16);
//        private static readonly CompressionInfo.LongInteger longCompression = new CompressionInfo.LongInteger(0, 48);

//        public float[] wallHitPointsPercentages;
//        public List<SiegeEngine> siegeEngineAtt;
//        public List<SiegeEngine> siegeEngineDef;



//        public BLSiegeEngineMessage()
//        {

//        }


//        protected override void OnWrite()
//        {
//            //MBDebug.Print("BL BLInitCultureMessage " + id, 0, DebugColor.Green);

//            WriteIntToPacket(wallHitPointsPercentages.Length, int0To7Compression);
//            foreach (float val in wallHitPointsPercentages)
//            {
//                WriteFloatToPacket(val, float0To1Compression);
//            }
//            WriteIntToPacket(siegeEngineAtt.Count, int0To7Compression);
//            foreach (SiegeEngine val in siegeEngineAtt)
//            {
//                WriteStringToPacket(val.type);
//                WriteIntToPacket(val.index, int0To7Compression);
//                WriteFloatToPacket(val.health, floatCompression);
//            }
//            WriteIntToPacket(siegeEngineDef.Count, int0To7Compression);
//            foreach (SiegeEngine val in siegeEngineDef)
//            {
//                WriteStringToPacket(val.type);
//                WriteIntToPacket(val.index, int0To7Compression);
//                WriteFloatToPacket(val.health, floatCompression);
//            }

//        }

//        protected override bool OnRead()
//        {
//            bool bufferReadValid = true;

//            int nbWall = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);
//            wallHitPointsPercentages = new float[nbWall];
//            for (int index = 0; index < nbWall; index += 1)
//            {
//                wallHitPointsPercentages[index] = ReadFloatFromPacket(float0To1Compression, ref bufferReadValid);
//            }

//            int nbSiegeEngineAtt = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);
//            siegeEngineAtt = new List<SiegeEngine>(nbSiegeEngineAtt);
//            for (int index = 0; index < nbSiegeEngineAtt; index += 1)
//            {
//                var siegeEngine = new SiegeEngine();
//                siegeEngineAtt.Add(siegeEngine);
//                siegeEngine.type = ReadStringFromPacket(ref bufferReadValid);
//                siegeEngine.index = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);
//                siegeEngine.health = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
//            }

//            int nbSiegeEngineDef = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);
//            siegeEngineDef = new List<SiegeEngine>(nbSiegeEngineDef);
//            for (int index = 0; index < nbSiegeEngineDef; index += 1)
//            {
//                var siegeEngine = new SiegeEngine();
//                siegeEngineDef.Add(siegeEngine);
//                siegeEngine.type = ReadStringFromPacket(ref bufferReadValid);
//                siegeEngine.index = ReadIntFromPacket(int0To7Compression, ref bufferReadValid);
//                siegeEngine.health = ReadFloatFromPacket(floatCompression, ref bufferReadValid);
//            }


//            return bufferReadValid;
//        }

//        protected override string OnGetLogFormat()
//        {
//            return "Sync BLSiegeEngineMessage informations";
//        }

//        protected override MultiplayerMessageFilter OnGetLogFilter()
//        {
//            return MultiplayerMessageFilter.MissionDetailed;
//        }

//    }

//    public class SiegeEngine
//    {
//        public string type;
//        public int index;
//        public float health;
//    }
//}
