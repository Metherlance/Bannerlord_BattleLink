using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.CommonSvMp.Behavior.Siege
{
    public class BLDeploymentHandler : MissionLogic
    {
        protected MissionMode previousMissionMode;
        //protected readonly bool isPlayerAttacker;
        private bool areDeploymentPointsInitialized;

        public BLDeploymentHandler()
        {

        }

        public override void EarlyStart()
        {
        }

        public override void AfterStart()
        {
            base.AfterStart();
            previousMissionMode = Mission.Mode;
            Mission.SetMissionMode(MissionMode.Deployment, true);
            //this.team.OnOrderIssued += new OnOrderIssuedDelegate(this.OrderController_OnOrderIssued);
            //Mission.AttackerTeam.OnOrderIssued += new OnOrderIssuedDelegate(this.OrderController_OnOrderIssued);
            //Mission.DefenderTeam.OnOrderIssued += new OnOrderIssuedDelegate(this.OrderController_OnOrderIssued);
        }

        public override void AfterAddTeam(Team team)
        {
            base.AfterAddTeam(team);
            team.OnOrderIssued += new OnOrderIssuedDelegate(this.OrderController_OnOrderIssued);
        }

        private void OrderController_OnOrderIssued(
          OrderType orderType,
          MBReadOnlyList<Formation> appliedFormations,
          OrderController orderController,
          params object[] delegateParams)
        {
            OrderController_OnOrderIssued_Aux(orderType, appliedFormations, orderController, delegateParams);
        }

        internal static void OrderController_OnOrderIssued_Aux(
          OrderType orderType,
          MBReadOnlyList<Formation> appliedFormations,
          OrderController orderController = null,
          params object[] delegateParams)
        {
            bool flag = false;
            foreach (Formation appliedFormation in (List<Formation>)appliedFormations)
            {
                if (appliedFormation.CountOfUnits > 0)
                {
                    flag = true;
                    break;
                }
            }
            if (!flag)
                return;
            switch (orderType)
            {
                case OrderType.None:
                    Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\DeploymentHandler.cs", nameof(OrderController_OnOrderIssued_Aux), 107);
                    break;
                case OrderType.Move:
                case OrderType.MoveToLineSegment:
                case OrderType.MoveToLineSegmentWithHorizontalLayout:
                case OrderType.FollowMe:
                case OrderType.FollowEntity:
                case OrderType.Advance:
                case OrderType.FallBack:
                    ForcePositioning();
                    ForceUpdateFormationParams();
                    break;
                case OrderType.Charge:
                case OrderType.ChargeWithTarget:
                case OrderType.GuardMe:
                    ForcePositioning();
                    ForceUpdateFormationParams();
                    break;
                case OrderType.StandYourGround:
                    ForcePositioning();
                    ForceUpdateFormationParams();
                    break;
                case OrderType.Retreat:
                    ForcePositioning();
                    ForceUpdateFormationParams();
                    break;
                case OrderType.LookAtEnemy:
                case OrderType.LookAtDirection:
                    ForcePositioning();
                    ForceUpdateFormationParams();
                    break;
                case OrderType.ArrangementLine:
                case OrderType.ArrangementCloseOrder:
                case OrderType.ArrangementLoose:
                case OrderType.ArrangementCircular:
                case OrderType.ArrangementSchiltron:
                case OrderType.ArrangementVee:
                case OrderType.ArrangementColumn:
                case OrderType.ArrangementScatter:
                    ForceUpdateFormationParams();
                    break;
                case OrderType.FormCustom:
                case OrderType.FormDeep:
                case OrderType.FormWide:
                case OrderType.FormWider:
                    ForceUpdateFormationParams();
                    break;
                case OrderType.CohesionHigh:
                    break;
                case OrderType.CohesionMedium:
                    break;
                case OrderType.CohesionLow:
                    break;
                case OrderType.HoldFire:
                    break;
                case OrderType.FireAtWill:
                    break;
                case OrderType.Mount:
                case OrderType.Dismount:
                    ForceUpdateFormationParams();
                    break;
                case OrderType.AIControlOn:
                case OrderType.AIControlOff:
                    ForcePositioning();
                    ForceUpdateFormationParams();
                    break;
                case OrderType.Transfer:
                case OrderType.Use:
                case OrderType.AttackEntity:
                    ForceUpdateFormationParams();
                    break;
                case OrderType.PointDefence:
                    Debug.FailedAssert("will be removed", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\DeploymentHandler.cs", nameof(OrderController_OnOrderIssued_Aux), 180);
                    break;
                default:
                    Debug.FailedAssert("false", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade\\Missions\\MissionLogics\\DeploymentHandler.cs", nameof(OrderController_OnOrderIssued_Aux), 183);
                    break;
            }

            void ForceUpdateFormationParams()
            {
                foreach (Formation appliedFormation in (List<Formation>)appliedFormations)
                {
                    if (appliedFormation.CountOfUnits > 0 && (orderController == null || orderController.FormationUpdateEnabledAfterSetOrder))
                    {
                        bool flag = false;
                        if (appliedFormation.IsPlayerTroopInFormation)
                            flag = appliedFormation.GetReadonlyMovementOrderReference().OrderEnum == MovementOrder.MovementOrderEnum.Follow;
                        appliedFormation.ApplyActionOnEachUnit(agent => agent.UpdateCachedAndFormationValues(true, false), flag ? Mission.Current.MainAgent : null);
                    }
                }
            }

            void ForcePositioning()
            {
                foreach (Formation appliedFormation in (List<Formation>)appliedFormations)
                {
                    if (appliedFormation.CountOfUnits > 0)
                    {
                        Vec2 direction = appliedFormation.FacingOrder.GetDirection(appliedFormation);
                        appliedFormation.SetPositioning(new WorldPosition?(appliedFormation.GetReadonlyMovementOrderReference().CreateNewOrderWorldPosition(appliedFormation, WorldPosition.WorldPositionEnforcedCache.None)), new Vec2?(direction));
                    }
                }
            }
        }

        public void ForceUpdateAllUnits()
        {
            //OrderController_OnOrderIssued_Aux(OrderType.Move, (MBReadOnlyList<Formation>)this.team.FormationsIncludingSpecialAndEmpty, null);
            OrderController_OnOrderIssued_Aux(OrderType.Move, (MBReadOnlyList<Formation>)Mission.AttackerTeam.FormationsIncludingSpecialAndEmpty, null);
            OrderController_OnOrderIssued_Aux(OrderType.Move, (MBReadOnlyList<Formation>)Mission.DefenderTeam.FormationsIncludingSpecialAndEmpty, null);
        }

        public virtual void FinishDeployment()
        {
        }

        public override void OnRemoveBehavior()
        {
            //if (this.team != null)
            //    this.team.OnOrderIssued -= new OnOrderIssuedDelegate(OrderController_OnOrderIssued);

            if (Mission.AttackerTeam!=null)
            {
                Mission.AttackerTeam.OnOrderIssued -= new OnOrderIssuedDelegate(OrderController_OnOrderIssued);
                Mission.DefenderTeam.OnOrderIssued -= new OnOrderIssuedDelegate(OrderController_OnOrderIssued);
            }

            Mission.SetMissionMode(previousMissionMode, false);
            base.OnRemoveBehavior();
        }

        public void InitializeDeploymentPoints()
        {
            if (areDeploymentPointsInitialized)
                return;
            foreach (DeploymentPoint deploymentPoint in Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>())
                deploymentPoint.Hide();
            areDeploymentPointsInitialized = true;
        }
    }
}
