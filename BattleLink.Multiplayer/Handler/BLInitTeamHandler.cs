using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.Handler
{
    public class BLInitTeamHandler
    {
        public static BLInitTeamMessage[] teamInfos = new BLInitTeamMessage[8];

        public static void HandleServerEventInitTeamMessage(GameNetworkMessage mes)
        {
            BLInitTeamMessage message = (BLInitTeamMessage)mes;

            teamInfos[message.id] = message;
        }

    }
}