using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Behavior
{
    public class BLReferentialHolder
    {
        public static List<BLCharacterObject> basicCharacterObjects;
        public static List<SideDto> listTeam;
        public static Battle battle;
        public static List<Player> listPlayer;
        public static string initializerFilename;

       // public static List<BLInitCharactersMessage> listCharacterMessage;
       // public static List<BLInitCultureMessage> listCultureMessage;

        public static string? nextBattleInitializerFilePath = null;
        internal static bool nextBattleInitializerPending = false;
        internal static bool currentBattleInitializerPending = false;

        public static (SideDto sideDto, TeamDto teamDto) getTeamDtoBy(Team team)
        {
            foreach (var side in listTeam)
            {
                foreach (var teamDto in side.Teams)
                {
                    var teamMis = Mission.Current.Teams[teamDto.missionTeamsIndex];
                    if (teamMis.Equals(team))
                    {
                        return (side, teamDto);
                    }
                }
            }
            throw new MBNotFoundException("not found");
        }

        public static Team getTeamBy(TeamDto teamDto)
        {
            var teamMis = Mission.Current.Teams[teamDto.missionTeamsIndex];
            return teamMis;
        }

    }
}
