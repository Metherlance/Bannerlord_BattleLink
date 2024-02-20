using BattleLink.Common.Utils;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace BattleLink.Handler
{
    public class BLMissionInitializerRecordHandler
    {
        public static BLMissionInitializerRecordMessage message;

        public static void HandleServerEventMissionInitializerRecordMessage(GameNetworkMessage mes)
        {
            message = (BLMissionInitializerRecordMessage)mes;
        }

    }
}