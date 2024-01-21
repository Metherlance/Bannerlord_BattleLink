using BattleLink.Common.Behavior;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Common
{
    public class TeamQuerySystemUtils
    {
        private static readonly BLBattlePowerCalculationLogic battlePowerLogic = new BLBattlePowerCalculationLogic();
        private static readonly FieldInfo fTeamRemainingPowerRatio = typeof(TeamQuerySystem).GetField("_remainingPowerRatio", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo fTeamTotalPowerRatio = typeof(TeamQuerySystem).GetField("_totalPowerRatio", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void setMission(Mission mission)
        {
            battlePowerLogic.setMission(mission);
        }

        public static void setPowerFix(Mission mission)
        {
            MBDebug.Print("TeamQuerySystemUtils - setPowerFix", 0, DebugColor.Green);
            // battlePowerLogic.Mission = mission;
            setPower(mission.AttackerTeam, mission.DefenderTeam);
            setPower(mission.DefenderTeam, mission.AttackerTeam);
        }

        //int CalculateDaysBetweenDates(DateTime date, DateTime date2)
        //{
        //    int days = (date2 - date).Days;
        //    return days;
        //}

        // public static void setPowerFix(Team team, Team teamEn)
        //{
        // Change RBBattlePowerCalculationLogic in Team TeamQuerySystem QueryData
        //setPower(team, teamEn);
        //setPower(teamEn, team);
        // }

        private static void setPower(Team team, Team teamEn)
        {
            QueryData<float> qdRemainingPowerRatio = new QueryData<float>((Func<float>)(() =>
            {
                float res = (float)(((double)MathF.Max(0.0f, battlePowerLogic.GetTotalTeamPower(team) - team.FormationsIncludingSpecialAndEmpty.Sum<Formation>((Func<Formation, float>)(f => team.QuerySystem.CasualtyHandler.GetCasualtyPowerLossOfFormation(f)))) + 1.0)
                                / ((double)MathF.Max(0.0f, battlePowerLogic.GetTotalTeamPower(teamEn) - teamEn.FormationsIncludingSpecialAndEmpty.Sum<Formation>((Func<Formation, float>)(f => team.QuerySystem.CasualtyHandler.GetCasualtyPowerLossOfFormation(f)))) + 1.0));
                return res;
            }), 5f);
            fTeamRemainingPowerRatio.SetValue(team.QuerySystem, qdRemainingPowerRatio);

            QueryData<float> qdTotalPowerRatio = new QueryData<float>((Func<float>)(() =>
            {
                float res = (float)(((double)battlePowerLogic.GetTotalTeamPower(team) + 1.0) / ((double)battlePowerLogic.GetTotalTeamPower(teamEn) + 1.0));
                return res;
            }), 10f);
            fTeamTotalPowerRatio.SetValue(team.QuerySystem, qdTotalPowerRatio);
        }
    }
}
