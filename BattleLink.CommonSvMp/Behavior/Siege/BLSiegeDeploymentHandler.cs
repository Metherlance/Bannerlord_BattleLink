using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.AI;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Missions;
using TaleWorlds.MountAndBlade;

namespace BattleLink.CommonSvMp.Behavior.Siege
{
    public class BLSiegeDeploymentHandler : BLDeploymentHandler
    {
        private IMissionSiegeWeaponsController _defenderSiegeWeaponsController;
        private IMissionSiegeWeaponsController _attackerSiegeWeaponsController;

        //public IEnumerable<DeploymentPoint> PlayerDeploymentPoints { get; private set; }

        public IEnumerable<DeploymentPoint> AllDeploymentPoints { get; private set; }

        public BLSiegeDeploymentHandler()
          : base()
        {
        }

        public override void OnBehaviorInitialize()
        {
            MissionSiegeEnginesLogic missionBehavior = Mission.GetMissionBehavior<MissionSiegeEnginesLogic>();
            _defenderSiegeWeaponsController = missionBehavior.GetSiegeWeaponsController(BattleSideEnum.Defender);
            _attackerSiegeWeaponsController = missionBehavior.GetSiegeWeaponsController(BattleSideEnum.Attacker);
        }

        public override void AfterStart()
        {
            base.AfterStart();
            AllDeploymentPoints = Mission.Current.ActiveMissionObjects.FindAllWithType<DeploymentPoint>();
            //PlayerDeploymentPoints = AllDeploymentPoints.Where((Func<DeploymentPoint, bool>)(dp => dp.Side == this.team.Side));
            foreach (DeploymentPoint allDeploymentPoint in AllDeploymentPoints)
                allDeploymentPoint.OnDeploymentStateChanged += new Action<DeploymentPoint, SynchedMissionObject>(OnDeploymentStateChange);
            Mission.IsFormationUnitPositionAvailable_AdditionalCondition += new Func<WorldPosition, Team, bool>(Mission_IsFormationUnitPositionAvailable_AdditionalCondition);
        }

        public override void OnRemoveBehavior()
        {
            base.OnRemoveBehavior();
            Mission.IsFormationUnitPositionAvailable_AdditionalCondition -= new Func<WorldPosition, Team, bool>(Mission_IsFormationUnitPositionAvailable_AdditionalCondition);
        }

        public override void FinishDeployment()
        {
            foreach (DeploymentPoint allDeploymentPoint in AllDeploymentPoints)
                allDeploymentPoint.OnDeploymentStateChanged -= new Action<DeploymentPoint, SynchedMissionObject>(OnDeploymentStateChange);
            base.FinishDeployment();
        }

        public void DeployAllSiegeWeaponsOfPlayer(BattleSideEnum side)
        {
            //BattleSideEnum side = this.isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
            var listDeploymentPoint = Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where(dp => dp.Side == side).ToList();
            var siegeWeaponController = GetWeaponsControllerOfSide(side);
            new SiegeWeaponAutoDeployer(listDeploymentPoint, siegeWeaponController).DeployAll(side);
        }

        public int GetMaxDeployableWeaponCountOfPlayer(BattleSideEnum side, Type weapon)
        {
            return GetWeaponsControllerOfSide(side).GetMaxDeployableWeaponCount(weapon);
        }

        public void DeployAllSiegeWeaponsOfAi(BattleSideEnum side)
        {
            //BattleSideEnum side = this.isPlayerAttacker ? BattleSideEnum.Defender : BattleSideEnum.Attacker;
            new SiegeWeaponAutoDeployer(Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where(dp => dp.Side == side).ToList(), GetWeaponsControllerOfSide(side)).DeployAll(side);
            RemoveDeploymentPoints(side);
        }

        public void RemoveDeploymentPoints(BattleSideEnum side)
        {
            foreach (DeploymentPoint deploymentPoint in Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where(dp => dp.Side == side).ToArray())
            {
                foreach (SynchedMissionObject synchedMissionObject in deploymentPoint.DeployableWeapons.ToArray())
                {
                    if ((deploymentPoint.DeployedWeapon == null || !synchedMissionObject.GameEntity.IsVisibleIncludeParents()) && synchedMissionObject is SiegeWeapon siegeWeapon)
                        siegeWeapon.SetDisabledSynched();
                }
                deploymentPoint.SetDisabledSynched();
            }
        }

        public void RemoveUnavailableDeploymentPoints(BattleSideEnum side)
        {
            IMissionSiegeWeaponsController weapons = side == BattleSideEnum.Defender ? _defenderSiegeWeaponsController : _attackerSiegeWeaponsController;
            foreach (DeploymentPoint deploymentPoint in Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where(dp => dp.Side == side).ToArray())
            {
                if (!deploymentPoint.DeployableWeaponTypes.Any(wt => weapons.GetMaxDeployableWeaponCount(wt) > 0))
                {
                    foreach (SynchedMissionObject synchedMissionObject in deploymentPoint.DeployableWeapons.Select(sw => sw as SiegeWeapon))
                        synchedMissionObject.SetDisabledSynched();
                    deploymentPoint.SetDisabledSynched();
                }
            }
        }

        public void UnHideDeploymentPoints(BattleSideEnum side)
        {
            foreach (DeploymentPoint deploymentPoint in Mission.ActiveMissionObjects.FindAllWithType<DeploymentPoint>().Where(dp => !dp.IsDisabled && dp.Side == side))
                deploymentPoint.Show();
        }

        //public int GetDeployableWeaponCountOfPlayer(BattleSideEnum side, Type weapon){
        //    return GetWeaponsControllerOfSide(side).GetMaxDeployableWeaponCount(weapon) - PlayerDeploymentPoints.Count(dp => dp.IsDeployed && MissionSiegeWeaponsController.GetWeaponType(dp.DeployedWeapon) == weapon);
        //} 

        protected bool Mission_IsFormationUnitPositionAvailable_AdditionalCondition(
          WorldPosition position,
          Team team)
        {
            if (team == null || team.Side != BattleSideEnum.Defender)
                return true;
            Scene scene = Mission.Scene;
            Vec3 globalPosition = scene.FindEntityWithTag("defender_infantry").GlobalPosition;
            WorldPosition position1 = new WorldPosition(scene, UIntPtr.Zero, globalPosition, false);
            return scene.DoesPathExistBetweenPositions(position1, position);
        }

        private void OnDeploymentStateChange(DeploymentPoint deploymentPoint, SynchedMissionObject targetObject)
        {
            //if (!deploymentPoint.IsDeployed && this.team.DetachmentManager.ContainsDetachment(deploymentPoint.DisbandedWeapon as IDetachment))
            //    this.team.DetachmentManager.DestroyDetachment(deploymentPoint.DisbandedWeapon as IDetachment);
            if (!(targetObject is SiegeWeapon missionWeapon))
                return;
            IMissionSiegeWeaponsController controllerOfSide = GetWeaponsControllerOfSide(deploymentPoint.Side);
            if (deploymentPoint.IsDeployed)
                controllerOfSide.OnWeaponDeployed(missionWeapon);
            else
                controllerOfSide.OnWeaponUndeployed(missionWeapon);
        }

        private IMissionSiegeWeaponsController GetWeaponsControllerOfSide(BattleSideEnum side)
        {
            return side == BattleSideEnum.Attacker ? _attackerSiegeWeaponsController : _defenderSiegeWeaponsController;
        }

        [Conditional("DEBUG")]
        private void AssertSiegeWeapons(IEnumerable<DeploymentPoint> allDeploymentPoints)
        {
            HashSet<SynchedMissionObject> synchedMissionObjectSet = new HashSet<SynchedMissionObject>();
            foreach (SynchedMissionObject synchedMissionObject in allDeploymentPoints.SelectMany(amo => amo.DeployableWeapons))
            {
                if (!synchedMissionObjectSet.Add(synchedMissionObject))
                    break;
            }
        }
    }
}
