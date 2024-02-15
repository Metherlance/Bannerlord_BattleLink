using BattleLink.Common.Utils;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common
{
    public class BLTeamAIGeneral : TeamAIGeneral
    {
        public BLTeamAIGeneral(Mission currentMission, Team currentTeam, float thinkTimerTime = 10f, float applyTimerTime = 1f) : base(currentMission, currentTeam, thinkTimerTime, applyTimerTime)
        {

        }

        protected override void Tick(float dt)
        {
            TeamQuerySystemUtils.setPowerFix(this.Mission, dt);
            base.Tick(dt);
        }

        public override void OnUnitAddedToFormationForTheFirstTime(Formation formation)
        {
            if (GameNetwork.IsServer && formation.AI.GetBehavior<BehaviorCharge>() == null
                // else bug on position
                && formation.CountOfUnits > 0)
            {
                if (formation.FormationIndex == FormationClass.NumberOfRegularFormations)
                {
                    formation.AI.AddAiBehavior(new BehaviorGeneral(formation));
                }
                else if (formation.FormationIndex == FormationClass.Bodyguard)
                {
                    formation.AI.AddAiBehavior(new BehaviorProtectGeneral(formation));
                }

                formation.AI.AddAiBehavior(new BehaviorCharge(formation));
                formation.AI.AddAiBehavior(new BehaviorPullBack(formation));
                formation.AI.AddAiBehavior(new BehaviorRegroup(formation));
                formation.AI.AddAiBehavior(new BehaviorReserve(formation));
                formation.AI.AddAiBehavior(new BehaviorRetreat(formation));
                formation.AI.AddAiBehavior(new BehaviorStop(formation));
                formation.AI.AddAiBehavior(new BehaviorTacticalCharge(formation));
                formation.AI.AddAiBehavior(new BehaviorAdvance(formation));
                formation.AI.AddAiBehavior(new BehaviorCautiousAdvance(formation));
                formation.AI.AddAiBehavior(new BehaviorCavalryScreen(formation));
                formation.AI.AddAiBehavior(new BehaviorDefend(formation));
                formation.AI.AddAiBehavior(new BehaviorDefensiveRing(formation));
                formation.AI.AddAiBehavior(new BehaviorFireFromInfantryCover(formation));
                formation.AI.AddAiBehavior(new BehaviorFlank(formation));
                formation.AI.AddAiBehavior(new BehaviorHoldHighGround(formation));
                formation.AI.AddAiBehavior(new BehaviorHorseArcherSkirmish(formation));
                formation.AI.AddAiBehavior(new BehaviorMountedSkirmish(formation));
                formation.AI.AddAiBehavior(new BehaviorProtectFlank(formation));
                formation.AI.AddAiBehavior(new BehaviorScreenedSkirmish(formation));
                formation.AI.AddAiBehavior(new BehaviorSkirmish(formation));
                formation.AI.AddAiBehavior(new BehaviorSkirmishBehindFormation(formation));
                formation.AI.AddAiBehavior(new BehaviorSkirmishLine(formation));
                formation.AI.AddAiBehavior(new BehaviorVanguard(formation));
            }
        }
    }
}
