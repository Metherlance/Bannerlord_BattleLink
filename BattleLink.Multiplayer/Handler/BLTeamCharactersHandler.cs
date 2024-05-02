using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.Handler
{
    public class BLTeamCharactersHandler
    {
        public static BLTeamCharactersMessage[] teamCharacters = new BLTeamCharactersMessage[16];

        public static void HandleServerTeamCharactersMessage(GameNetworkMessage mes)
        {
            BLTeamCharactersMessage message = (BLTeamCharactersMessage)mes;

            teamCharacters[message.teamId] = message;
        }

    }
}