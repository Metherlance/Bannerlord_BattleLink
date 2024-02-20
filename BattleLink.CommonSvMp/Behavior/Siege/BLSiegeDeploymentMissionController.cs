using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using System.Linq;

namespace BattleLink.CommonSvMp.Behavior.Siege
{
    public class BLSiegeDeploymentMissionController : BLDeploymentMissionController
    {
        private BLSiegeDeploymentHandler _siegeDeploymentHandler;

        public BLSiegeDeploymentMissionController() : base()
        {
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _siegeDeploymentHandler = Mission.GetMissionBehavior<BLSiegeDeploymentHandler>();
        }

        public override void AfterStart()
        {
            Mission.GetMissionBehavior<BLDeploymentHandler>().InitializeDeploymentPoints();
            base.AfterStart();
        }

        protected override void SetupTeamsOfSide(BattleSideEnum side)
        {
            Team team = side == BattleSideEnum.Attacker ? Mission.AttackerTeam : Mission.DefenderTeam;
            //if (team == this.Mission.AttackerTeam)
            //{
            _siegeDeploymentHandler.RemoveUnavailableDeploymentPoints(side);
            _siegeDeploymentHandler.UnHideDeploymentPoints(side);
            _siegeDeploymentHandler.DeployAllSiegeWeaponsOfPlayer(side);
            //}
            //else
            //    this._siegeDeploymentHandler.DeployAllSiegeWeaponsOfAi(side);

            //this.MissionAgentSpawnLogic.SetSpawnTroops(side, true, true);
            foreach (GameEntity gameEntity in Mission.GetActiveEntitiesWithScriptComponentOfType<SiegeWeapon>())
            {
                SiegeWeapon siegeWeapon = gameEntity.GetScriptComponents<SiegeWeapon>().FirstOrDefault();
                if (siegeWeapon != null && siegeWeapon.GetSide() == side)
                    siegeWeapon.TickAuxForInit();
            }
            SetupTeamsOfSideAux(side);
            //if (team != this.Mission.AttackerTeam)
            //    return;
            foreach (Formation formation in (List<Formation>)team.FormationsIncludingEmpty)
            {
                if (formation.CountOfUnits > 0)
                    formation.SetControlledByAI(true);
            }
        }

        public override void OnBeforeDeploymentFinished()
        {
            // for ladder
            BattleSideEnum side = Mission.AttackerTeam.Side;
            _siegeDeploymentHandler.RemoveDeploymentPoints(side);
            foreach (SynchedMissionObject synchedMissionObject in Mission.Current.ActiveMissionObjects.FindAllWithType<SiegeLadder>().Where(sl => !sl.GameEntity.IsVisibleIncludeParents()).ToList())
                synchedMissionObject.SetDisabledSynched();
            OnSideDeploymentFinished(side);

        }

        public override void OnAfterDeploymentFinished() => Mission.RemoveMissionBehavior(_siegeDeploymentHandler);

        public List<ItemObject> GetSiegeMissiles()
        {
            List<ItemObject> siegeMissiles = new List<ItemObject>();
            siegeMissiles.Add(MBObjectManager.Instance.GetObject<ItemObject>("grapeshot_fire_projectile"));
            foreach (GameEntity gameEntity in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<RangedSiegeWeapon>())
            {
                RangedSiegeWeapon firstScriptOfType = gameEntity.GetFirstScriptOfType<RangedSiegeWeapon>();
                if (!string.IsNullOrEmpty(firstScriptOfType.MissileItemID))
                {
                    ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>(firstScriptOfType.MissileItemID);
                    if (!siegeMissiles.Contains(itemObject))
                        siegeMissiles.Add(itemObject);
                }
            }
            foreach (GameEntity gameEntity in Mission.Current.GetActiveEntitiesWithScriptComponentOfType<StonePile>())
            {
                StonePile firstScriptOfType = gameEntity.GetFirstScriptOfType<StonePile>();
                if (!string.IsNullOrEmpty(firstScriptOfType.GivenItemID))
                {
                    ItemObject itemObject = MBObjectManager.Instance.GetObject<ItemObject>(firstScriptOfType.GivenItemID);
                    if (!siegeMissiles.Contains(itemObject))
                        siegeMissiles.Add(itemObject);
                }
            }
            return siegeMissiles;
        }
    }
}