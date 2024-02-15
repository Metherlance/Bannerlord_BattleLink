using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace BattleLink.Common.Behavior
{
    public class BLDeploymentMissionView : MissionView
    {
        private DeploymentMissionController _deploymentMissionController;
        private OrderTroopPlacer _orderTroopPlacer;
        private MissionDeploymentBoundaryMarker _deploymentBoundaryMarkerHandler;
        private MissionEntitySelectionUIHandler _entitySelectionHandler;
        public OnPlayerDeploymentFinishDelegate OnDeploymentFinish;

        public override void AfterStart()
        {
            _orderTroopPlacer = base.Mission.GetMissionBehavior<OrderTroopPlacer>();
            _entitySelectionHandler = base.Mission.GetMissionBehavior<MissionEntitySelectionUIHandler>();
            _deploymentBoundaryMarkerHandler = base.Mission.GetMissionBehavior<MissionDeploymentBoundaryMarker>();
            _deploymentMissionController = base.Mission.GetMissionBehavior<DeploymentMissionController>();
            if (_deploymentMissionController != null)
            {
                // _deploymentMissionController.PlayerDeploymentFinish += OnPlayerDeploymentFinish;
            }
            else
            {
                TaleWorlds.Library.Debug.FailedAssert("Deployment mission controller is null is null!", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.MountAndBlade.View\\MissionViews\\Singleplayer\\DeploymentMissionView.cs", "AfterStart", 33);
            }
        }

        public override void OnInitialDeploymentPlanMadeForSide(BattleSideEnum side, bool isFirstPlan)
        {
            if (base.Mission.PlayerTeam != null && side == base.Mission.PlayerTeam.Side && base.Mission.DeploymentPlan.HasDeploymentBoundaries(base.Mission.PlayerTeam.Side))
            {
                _orderTroopPlacer?.RestrictOrdersToDeploymentBoundaries(enabled: true);
            }
        }

        public void OnPlayerDeploymentFinish()
        {
            OnDeploymentFinish();
            if (_entitySelectionHandler != null)
            {
                base.Mission.RemoveMissionBehavior(_entitySelectionHandler);
            }

            if (_deploymentBoundaryMarkerHandler != null)
            {
                if (base.Mission.DeploymentPlan.HasDeploymentBoundaries(base.Mission.PlayerTeam.Side))
                {
                    _orderTroopPlacer?.RestrictOrdersToDeploymentBoundaries(enabled: false);
                }

                base.Mission.RemoveMissionBehavior(_deploymentBoundaryMarkerHandler);
            }

            if (!base.Mission.HasMissionBehavior<MissionBoundaryWallView>())
            {
                MissionBoundaryWallView missionView = new MissionBoundaryWallView();
                base.MissionScreen.AddMissionView(missionView);
            }
        }
    }
}
