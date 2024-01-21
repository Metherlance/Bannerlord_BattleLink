using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using System.Collections.Generic;

namespace BattleLink.Common.Behavior
{
    public class BLReferentialHolder
    {
        public static List<BLCharacterObject> basicCharacterObjects;
        public static List<Team> listTeam;
        public static List<Player> listPlayer;
        public static string initializerFilename;

        public static List<BLInitCharactersMessage> listCharacterMessage;
        public static List<BLInitCultureMessage> listCultureMessage;
    }
}
