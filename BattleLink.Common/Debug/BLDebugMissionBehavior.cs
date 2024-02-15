using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Common.Debug
{
    public class BLDebugMissionBehavior : MissionBehavior
    {
        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

        public override void OnAfterMissionCreated()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAfterMissionCreated", 0, DebugColor.Blue);
        }

        public override void OnBehaviorInitialize()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnBehaviorInitialize", 0, DebugColor.Blue);
        }

        public override void OnCreated()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnCreated", 0, DebugColor.Blue);
        }

        public override void EarlyStart()
        {
            MBDebug.Print("RBDebugMissionBehavior - EarlyStart", 0, DebugColor.Blue);
        }

        public override void AfterStart()
        {
            MBDebug.Print("RBDebugMissionBehavior - AfterStart", 0, DebugColor.Blue);
        }

        //public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnMissileHit", 0, DebugColor.Blue);
        //}

        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent, sbyte attachedBoneIndex)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnMissileCollisionReaction", 0, DebugColor.Blue);
        }

        public override void OnMissionScreenPreLoad()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnMissionScreenPreLoad", 0, DebugColor.Blue);
        }

        public override void OnAgentCreated(Agent agent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentCreated", 0, DebugColor.Blue);
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentBuild", 0, DebugColor.Blue);
        }

        public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentTeamChanged", 0, DebugColor.Blue);
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentHit", 0, DebugColor.Blue);
        }

        public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnScoreHit", 0, DebugColor.Blue);
        }

        public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnEarlyAgentRemoved", 0, DebugColor.Blue);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentRemoved", 0, DebugColor.Blue);
        }

        public override void OnAgentDeleted(Agent affectedAgent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentDeleted", 0, DebugColor.Blue);
        }

        public override void OnAgentFleeing(Agent affectedAgent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentFleeing", 0, DebugColor.Blue);
        }

        public override void OnAgentPanicked(Agent affectedAgent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentPanicked", 0, DebugColor.Blue);
        }

        public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnFocusGained", 0, DebugColor.Blue);
        }

        public override void OnFocusLost(Agent agent, IFocusable focusableObject)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnFocusLost", 0, DebugColor.Blue);
        }

        public override void OnAddTeam(Team team)
        {
            //if (team.Side==BattleSideEnum.Attacker)
            //{

            //}

            //team.Color= new Color(0, 0, 255);
            MBDebug.Print("RBDebugMissionBehavior - OnAddTeam", 0, DebugColor.Blue);
        }

        public override void AfterAddTeam(Team team)
        {
            MBDebug.Print("RBDebugMissionBehavior - AfterAddTeam", 0, DebugColor.Blue);
        }

        public override void OnAgentInteraction(Agent userAgent, Agent agent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentInteraction", 0, DebugColor.Blue);
        }

        public override void OnClearScene()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnClearScene", 0, DebugColor.Blue);
        }

        //public override void HandleOnCloseMission()
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - HandleOnCloseMission", 0, DebugColor.Blue);
        //    OnEndMission();
        //}

        protected override void OnEndMission()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnEndMission", 0, DebugColor.Blue);
        }

        public override void OnRemoveBehavior()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnRemoveBehavior", 0, DebugColor.Blue);
        }

        //public override void OnPreMissionTick(float dt)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnPreMissionTick", 0, DebugColor.Blue);
        //}

        //public override void OnPreDisplayMissionTick(float dt)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnPreDisplayMissionTick", 0, DebugColor.Blue);
        //}

        //public override void OnMissionTick(float dt)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnMissionTick", 0, DebugColor.Blue);
        //}

        public override void OnAgentMount(Agent agent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentMount", 0, DebugColor.Blue);
        }

        public override void OnAgentDismount(Agent agent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentDismount", 0, DebugColor.Blue);
        }

        public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
        {
            MBDebug.Print("RBDebugMissionBehavior - IsThereAgentAction", 0, DebugColor.Blue);
            return false;
        }

        public override void OnEntityRemoved(GameEntity entity)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnEntityRemoved", 0, DebugColor.Blue);
        }

        public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnObjectUsed", 0, DebugColor.Blue);
        }

        public override void OnObjectStoppedBeingUsed(Agent userAgent, UsableMissionObject usedObject)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnObjectStoppedBeingUsed", 0, DebugColor.Blue);
        }

        public override void OnRenderingStarted()
        {
            MBDebug.Print("RBDebugMissionBehavior - OnRenderingStarted", 0, DebugColor.Blue);
        }

        //public override void OnMissionActivate()
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnMissionActivate", 0, DebugColor.Blue);
        //}

        //public override void OnMissionDeactivate()
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnMissionDeactivate", 0, DebugColor.Blue);
        //}

        //public override void OnMissionRestart()
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnMissionRestart", 0, DebugColor.Blue);
        //}

        public override List<CompassItemUpdateParams> GetCompassTargets()
        {
            MBDebug.Print("RBDebugMissionBehavior - GetCompassTargets", 0, DebugColor.Blue);
            return null;
        }

        public override void OnAssignPlayerAsSergeantOfFormation(Agent agent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAssignPlayerAsSergeantOfFormation", 0, DebugColor.Blue);
        }

        //public override void OnFormationUnitsSpawned(Team team)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnFormationUnitsSpawned", 0, DebugColor.Blue);
        //}

        protected override void OnGetAgentState(Agent agent, bool usedSurgery)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnGetAgentState", 0, DebugColor.Blue);
        }

        public override void OnAgentAlarmedStateChanged(Agent agent, Agent.AIStateFlag flag)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentAlarmedStateChanged", 0, DebugColor.Blue);
        }

        protected override void OnObjectDisabled(DestructableComponent destructionComponent)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnObjectDisabled", 0, DebugColor.Blue);
        }

        public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnMissionModeChange", 0, DebugColor.Blue);
        }

        protected override void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentControllerChanged", 0, DebugColor.Blue);
        }

        //public override void OnItemPickup(Agent agent, SpawnedItemEntity item)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnItemPickup", 0, DebugColor.Blue);
        //}

        //public override void OnItemDrop(Agent agent, SpawnedItemEntity item)
        //{
        //    MBDebug.Print("RBDebugMissionBehavior - OnItemDrop", 0, DebugColor.Blue);
        //}

        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnRegisterBlow", 0, DebugColor.Blue);
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            MBDebug.Print("RBDebugMissionBehavior - OnAgentShootMissile", 0, DebugColor.Blue);
        }

    }
}