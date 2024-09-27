using BattleLink.Common;
using BattleLink.Common.Behavior;
using System;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Core.SkillObject;

namespace BattleLink.CommonSvMp.Behavior
{
    // copy of MissionCombatantsLogic
    public class BLTeamAiUtils
    {
        // copy of EarlyStart
        // need 0 bot or Reset will break
        public static void addAIGeneral(Mission mission)
        {

            // do it in 2 time because of Lane static init 2 times in TeamAISiegeComponent and reuse lane in tactics...
            foreach (Team team in mission.Teams)
            {
                if (team.Side==BattleSideEnum.None)
                {
                    continue;
                }
                if (team.TeamAI != null)
                {
                    // already has AI
                    return;
                }

                TeamAIComponent teamAI = null;
                switch (Mission.Current.MissionTeamAIType)
                {
                    case Mission.MissionTeamAITypeEnum.NoTeamAI:
                    case Mission.MissionTeamAITypeEnum.FieldBattle:
                        //team.AddTeamAI(new TeamAIGeneral(mission, team))
                        teamAI = new BLTeamAIGeneral(mission, team);
                        break;

                    case Mission.MissionTeamAITypeEnum.Siege:
                        if (team.Side == BattleSideEnum.Attacker)
                        {
                            teamAI = new TeamAISiegeAttacker(mission, team, 5f, 1f);
                        }
                        else if (team.Side == BattleSideEnum.Defender)
                        {
                            teamAI = new TeamAISiegeDefender(mission, team, 5f, 1f);
                        }
                        break;

                    case Mission.MissionTeamAITypeEnum.SallyOut:
                        if (team.Side == BattleSideEnum.Attacker)
                        {
                            teamAI = new TeamAISallyOutDefender(mission, team, 5f, 1f);
                        }
                        else
                        {
                            teamAI = new TeamAISallyOutAttacker(mission, team, 5f, 1f);
                        }
                        break;
                }

                // **
                // no tactics here change curent state for avoid exec of ResetTactic()
                var missionState = Mission.Current.CurrentState;
                FieldInfo fMissionState = typeof(Mission).GetField("\u003CCurrentState\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                fMissionState.SetValue(Mission.Current, Mission.State.Initializing);

                // need 0 bot or Reset will break
                team.AddTeamAI(teamAI); 

                fMissionState.SetValue(Mission.Current, missionState);

            }

            foreach (Team team in mission.Teams)
            {
                if (team.Side == BattleSideEnum.None)
                {
                    continue;
                }

                var teamAI = team.TeamAI;

                // **

                // elseif siege lane without querysystem , Lane copied in Tactics...
                //teamAI.OnDeploymentFinished();

                // **

                switch (Mission.Current.MissionTeamAIType)
                {
                    case Mission.MissionTeamAITypeEnum.NoTeamAI:

                        teamAI.AddTacticOption(new TacticCharge(team));
                        break;

                    case Mission.MissionTeamAITypeEnum.FieldBattle:
                        // no team.generalAgent here
                        int tacticsSkillMax = getTacticsSkillGeneral(team);
                        //_battleCombatants.Where((IBattleCombatant bc) => bc.Side == team.Side).Max((IBattleCombatant bcs) => bcs.GetTacticsSkillAmount());
                        teamAI.AddTacticOption(new TacticCharge(team));
                        if (tacticsSkillMax >= 20)
                        {
                            teamAI.AddTacticOption(new TacticFullScaleAttack(team));
                            if (team.Side == BattleSideEnum.Defender)
                            {
                                teamAI.AddTacticOption(new TacticDefensiveEngagement(team));
                                teamAI.AddTacticOption(new TacticDefensiveLine(team));
                            }
                            else if (team.Side == BattleSideEnum.Attacker)
                            {
                                teamAI.AddTacticOption(new TacticRangedHarrassmentOffensive(team));
                            }
                        }

                        if (tacticsSkillMax >= 50)
                        {
                            teamAI.AddTacticOption(new TacticFrontalCavalryCharge(team));
                            if (team.Side == BattleSideEnum.Defender)
                            {
                                teamAI.AddTacticOption(new TacticDefensiveRing(team));
                                teamAI.AddTacticOption(new TacticHoldChokePoint(team));
                            }
                            else if (team.Side == BattleSideEnum.Attacker)
                            {
                                teamAI.AddTacticOption(new TacticCoordinatedRetreat(team));
                            }
                        }
                        break;

                    case Mission.MissionTeamAITypeEnum.Siege:

                        if (team.Side == BattleSideEnum.Attacker)
                        {
                            teamAI.AddTacticOption(new TacticBreachWalls(team));
                        }
                        else if (team.Side == BattleSideEnum.Defender)
                        {
                            teamAI.AddTacticOption(new TacticDefendCastle(team));
                        }
                        break;

                    case Mission.MissionTeamAITypeEnum.SallyOut:

                        if (team.Side == BattleSideEnum.Defender)
                        {
                            teamAI.AddTacticOption(new TacticSallyOutHitAndRun(team));
                        }
                        else if (team.Side == BattleSideEnum.Attacker)
                        {
                            teamAI.AddTacticOption(new TacticSallyOutDefense(team));
                        }
                        teamAI.AddTacticOption(new TacticCharge(team));
                        break;
                }

                //team.AddTeamAI(teamAI); not here because => team.TeamAI as TeamAISiegeDefender; in ctor TacticDefendCastle

                //team.QuerySystem.Expire();
                //team.ResetTactic(); //did it in AddTeamAI and no bot here...

                // **

            }
        }

        private static int getTacticsSkillGeneral(Team team)
        {
            (_, var teamDto) = BLReferentialHolder.getTeamDtoBy(team);
            String generalId = teamDto.generalId;
            if (string.IsNullOrEmpty(generalId))
            {
                return 0;
            }
            int tactics = BLReferentialHolder.basicCharacterObjects.Find(c=> generalId.Equals(c.StringId)).GetSkillValue(DefaultSkills.Tactics);
            return tactics;
        }
    }
}
