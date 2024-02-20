using BattleLink.Common.Behavior;
using BattleLink.Common.Utils;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;
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


        private static TimerDeltaTime timerPower = new TimerDeltaTime(10f);

        public static void setMission(Mission mission)
        {
            battlePowerLogic.setMission(mission);
        }

        /// <summary>
        /// do it only once
        /// </summary>
        /// <param name="mission"></param>
        public static void setPowerFix(Mission mission)
        {
            MBDebug.Print("TeamQuerySystemUtils - setPowerFix", 0, DebugColor.Green);
            // battlePowerLogic.Mission = mission;
            foreach (var team in mission.Teams)
            {
                if (BattleSideEnum.None != team.Side)
                {
                    setPower(team, mission);
                }
            }
        }

        public static void setPowerFix(Mission mission, float dt)
        {
            if(timerPower.ResetIfCheck(dt))
            {
                MBDebug.Print("TeamQuerySystemUtils - setPowerFix", 0, DebugColor.Green);
                // battlePowerLogic.Mission = mission;
                foreach (var team in mission.Teams)
                {
                    if (BattleSideEnum.None != team.Side)
                    {
                        setPower(team, mission);
                    }
                }
            }
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


        private static void setPower(Team team, Mission mission)
        {
            QueryData<float> qdRemainingPowerRatio = new QueryData<float>((Func<float>)(() =>
            {
                float powerAlly = 0f;
                float powerEn = 0f;
                foreach (Team teamInMission in mission.Teams)
                {
                    if (BattleSideEnum.None==teamInMission.Side)
                    {
                        //spec team
                    }
                    else if (teamInMission.IsEnemyOf(team))
                    {
                        powerEn += battlePowerLogic.GetTotalTeamPower(teamInMission);
                        foreach (Formation item3 in teamInMission.FormationsIncludingSpecialAndEmpty)
                        {
                            powerEn -= team.QuerySystem.CasualtyHandler.GetCasualtyPowerLossOfFormation(item3);
                        }
                    }
                    else
                    {
                        powerAlly += battlePowerLogic.GetTotalTeamPower(teamInMission);
                        foreach (Formation item4 in teamInMission.FormationsIncludingSpecialAndEmpty)
                        {
                            powerAlly -= team.QuerySystem.CasualtyHandler.GetCasualtyPowerLossOfFormation(item4);
                        }
                    }
                }

                powerAlly = MathF.Max(0f, powerAlly);
                powerEn = MathF.Max(0f, powerEn);
                float res = (powerAlly + 1f) / (powerEn + 1f);
                return res;

            }), 5f);
            fTeamRemainingPowerRatio.SetValue(team.QuerySystem, qdRemainingPowerRatio);

            QueryData<float> qdTotalPowerRatio = new QueryData<float>((Func<float>)(() =>
            {
                float powerAlly = 0f;
                float powerEn = 0f;
                foreach (Team teamInMission in mission.Teams)
                {
                    if (BattleSideEnum.None == teamInMission.Side)
                    {
                        //spec team
                    }
                    else if (teamInMission.IsEnemyOf(team))
                    {
                        powerEn += battlePowerLogic.GetTotalTeamPower(teamInMission);
                    }
                    else
                    {
                        powerAlly += battlePowerLogic.GetTotalTeamPower(teamInMission);
                    }
                }

                float res = (powerAlly + 1f) / (powerEn + 1f);
                return res;

            }), 10f);
            fTeamTotalPowerRatio.SetValue(team.QuerySystem, qdTotalPowerRatio);
        }
    }
}
