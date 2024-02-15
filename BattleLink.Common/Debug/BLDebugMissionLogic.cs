using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Common.Debug
{
    //Order of methods called:
    //OnAfterMissionCreated
    //OnCreated
    //OnMissionScreenPreLoad
    //OnBehaviorInitialize
    //EarlyStart
    //OnAddTeam
    //AfterAddTeam
    //AfterStart
    //OnAddTeam
    //AfterAddTeam
    //OnAddTeam
    //AfterAddTeam
    //OnMissionModeChange
    //OnClearScene
    //OnAgentCreated
    //OnAgentTeamChanged
    //OnAgentBuild
    //OnAgentCreated
    //OnAgentTeamChanged
    //OnAgentBuild
    public class BLDebugMissionLogic : MissionLogic
   {
        public override InquiryData OnEndMissionRequest(out bool canLeave)
        {
            MBDebug.Print("RBDebugMissionLogic - OnEndMissionRequest", 0, DebugColor.Cyan);
            canLeave = true;
            return null;
        }

        //public override bool MissionEnded(ref MissionResult missionResult)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - MissionEnded", 0, DebugColor.Cyan);
        //    return false;
        //}

        public override void OnBattleEnded()
        {
            MBDebug.Print("RBDebugMissionLogic - OnBattleEnded", 0, DebugColor.Cyan);
        }

        public override void ShowBattleResults()
        {
            MBDebug.Print("RBDebugMissionLogic - ShowBattleResults", 0, DebugColor.Cyan);
        }

        //public override void AccelerateHorseKeyPressAnswer()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - AccelerateHorseKeyPressAnswer", 0, DebugColor.Cyan);
        //}

        public override void OnRetreatMission()
        {
            MBDebug.Print("RBDebugMissionLogic - OnRetreatMission", 0, DebugColor.Cyan);
        }

        public override void OnSurrenderMission()
        {
            MBDebug.Print("RBDebugMissionLogic - OnSurrenderMission", 0, DebugColor.Cyan);
        }

        public override void OnAutoDeployTeam(Team team)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAutoDeployTeam", 0, DebugColor.Cyan);
        }

        //public override bool IsAgentInteractionAllowed()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - IsAgentInteractionAllowed", 0, DebugColor.Cyan);
        //    return true;
        //}

        //public override bool IsOrderShoutingAllowed()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - IsOrderShoutingAllowed", 0, DebugColor.Cyan);
        //    return true;
        //}

        public override List<EquipmentElement> GetExtraEquipmentElementsForCharacter(BasicCharacterObject character, bool getAllEquipments = false)
        {
            MBDebug.Print("RBDebugMissionLogic - GetExtraEquipmentElementsForCharacter", 0, DebugColor.Cyan);
            return null;
        }

        public override void OnMissionResultReady(MissionResult missionResult)
        {
            MBDebug.Print("RBDebugMissionLogic - OnMissionResultReady", 0, DebugColor.Cyan);
        }


        // ****

        public override void OnAfterMissionCreated()
        {
            MBDebug.Print("RBDebugMissionLogic - OnAfterMissionCreated", 0, DebugColor.Cyan);
        }

        public override void OnBehaviorInitialize()
        {
            MBDebug.Print("RBDebugMissionLogic - OnBehaviorInitialize", 0, DebugColor.Cyan);
        }

        public override void OnCreated()
        {
            MBDebug.Print("RBDebugMissionLogic - OnCreated", 0, DebugColor.Cyan);
        }

        public override void EarlyStart()
        {
            MBDebug.Print("RBDebugMissionLogic - EarlyStart", 0, DebugColor.Cyan);
        }

        public override void AfterStart()
        {
            //BasicCharacterObject character21 = MBObjectManager.Instance.GetObject<BasicCharacterObject>("bl_character");
            //var car = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("bl_character_class_division");


            MBDebug.Print("RBDebugMissionLogic - AfterStart", 0, DebugColor.Cyan);
        }

        //public override void OnMissileHit(Agent attacker, Agent victim, bool isCanceled)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnMissileHit", 0, DebugColor.Cyan);
        //}

        public override void OnMissileCollisionReaction(Mission.MissileCollisionReaction collisionReaction, Agent attackerAgent, Agent attachedAgent, sbyte attachedBoneIndex)
        {
            MBDebug.Print("RBDebugMissionLogic - OnMissileCollisionReaction", 0, DebugColor.Cyan);
        }

        public override void OnMissionScreenPreLoad()
        {
            MBDebug.Print("RBDebugMissionLogic - OnMissionScreenPreLoad", 0, DebugColor.Cyan);
        }

        public override void OnAgentCreated(Agent agent)
        {
            //BasicCharacterObject character21 = MBObjectManager.Instance.GetObject<BasicCharacterObject>("bl_character");
            //var car = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("bl_character_class_division");


            MBDebug.Print("RBDebugMissionLogic - OnAgentCreated", 0, DebugColor.Cyan);
        }

        public override void OnAgentBuild(Agent agent, Banner banner)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentBuild", 0, DebugColor.Cyan);
        }

        public override void OnAgentTeamChanged(Team prevTeam, Team newTeam, Agent agent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentTeamChanged", 0, DebugColor.Cyan);
        }

        public override void OnAgentHit(Agent affectedAgent, Agent affectorAgent, in MissionWeapon affectorWeapon, in Blow blow, in AttackCollisionData attackCollisionData)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentHit", 0, DebugColor.Cyan);//2
        }

        public override void OnScoreHit(Agent affectedAgent, Agent affectorAgent, WeaponComponentData attackerWeapon, bool isBlocked, bool isSiegeEngineHit, in Blow blow, in AttackCollisionData collisionData, float damagedHp, float hitDistance, float shotDifficulty)
        {
            MBDebug.Print("RBDebugMissionLogic - OnScoreHit", 0, DebugColor.Cyan);//3
        }

        public override void OnEarlyAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            MBDebug.Print("RBDebugMissionLogic - OnEarlyAgentRemoved", 0, DebugColor.Cyan);
        }

        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentRemoved", 0, DebugColor.Cyan);///la
        }

        public override void OnAgentDeleted(Agent affectedAgent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentDeleted", 0, DebugColor.Cyan);
        }

        public override void OnAgentFleeing(Agent affectedAgent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentFleeing", 0, DebugColor.Cyan);
        }

        public override void OnAgentPanicked(Agent affectedAgent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentPanicked", 0, DebugColor.Cyan);
        }

        public override void OnFocusGained(Agent agent, IFocusable focusableObject, bool isInteractable)
        {
            MBDebug.Print("RBDebugMissionLogic - OnFocusGained", 0, DebugColor.Cyan);
        }

        public override void OnFocusLost(Agent agent, IFocusable focusableObject)
        {
            MBDebug.Print("RBDebugMissionLogic - OnFocusLost", 0, DebugColor.Cyan);
        }

        public override void OnAddTeam(Team team)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAddTeam", 0, DebugColor.Cyan);
        }

        public override void AfterAddTeam(Team team)
        {
            MBDebug.Print("RBDebugMissionLogic - AfterAddTeam", 0, DebugColor.Cyan);
        }

        public override void OnAgentInteraction(Agent userAgent, Agent agent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentInteraction", 0, DebugColor.Cyan);
        }

        public override void OnClearScene()
        {
            MBDebug.Print("RBDebugMissionLogic - OnClearScene", 0, DebugColor.Cyan);
        }

        //public override void HandleOnCloseMission()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - HandleOnCloseMission", 0, DebugColor.Cyan);
        //    OnEndMission();
        //}

        protected override void OnEndMission()
        {
            MBDebug.Print("RBDebugMissionLogic - OnEndMission", 0, DebugColor.Cyan);
        }

        public override void OnRemoveBehavior()
        {
            MBDebug.Print("RBDebugMissionLogic - OnRemoveBehavior", 0, DebugColor.Cyan);
        }

        //public override void OnPreMissionTick(float dt)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnPreMissionTick", 0, DebugColor.Cyan);
        //}

        //public override void OnPreDisplayMissionTick(float dt)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnPreDisplayMissionTick", 0, DebugColor.Cyan);
        //}

        //public override void OnMissionTick(float dt)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnMissionTick", 0, DebugColor.Cyan);
        //}

        public override void OnAgentMount(Agent agent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentMount", 0, DebugColor.Cyan);
        }

        public override void OnAgentDismount(Agent agent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentDismount", 0, DebugColor.Cyan);
        }

        public override bool IsThereAgentAction(Agent userAgent, Agent otherAgent)
        {
            MBDebug.Print("RBDebugMissionLogic - IsThereAgentAction", 0, DebugColor.Cyan);
            return false;
        }

        public override void OnEntityRemoved(GameEntity entity)
        {
            MBDebug.Print("RBDebugMissionLogic - OnEntityRemoved", 0, DebugColor.Cyan);
        }

        public override void OnObjectUsed(Agent userAgent, UsableMissionObject usedObject)
        {
            MBDebug.Print("RBDebugMissionLogic - OnObjectUsed", 0, DebugColor.Cyan);
        }

        public override void OnObjectStoppedBeingUsed(Agent userAgent, UsableMissionObject usedObject)
        {
            MBDebug.Print("RBDebugMissionLogic - OnObjectStoppedBeingUsed", 0, DebugColor.Cyan);
        }

        public override void OnRenderingStarted()
        {
            MBDebug.Print("RBDebugMissionLogic - OnRenderingStarted", 0, DebugColor.Cyan);
        }

        //public override void OnMissionActivate()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnMissionActivate", 0, DebugColor.Cyan);
        //}

        //public override void OnMissionDeactivate()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnMissionDeactivate", 0, DebugColor.Cyan);
        //}

        //public override void OnMissionRestart()
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnMissionRestart", 0, DebugColor.Cyan);
        //}

        public override List<CompassItemUpdateParams> GetCompassTargets()
        {
            MBDebug.Print("RBDebugMissionLogic - GetCompassTargets", 0, DebugColor.Cyan);
            return null;
        }

        public override void OnAssignPlayerAsSergeantOfFormation(Agent agent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAssignPlayerAsSergeantOfFormation", 0, DebugColor.Cyan);
        }

        //public override void OnFormationUnitsSpawned(Team team)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnFormationUnitsSpawned", 0, DebugColor.Cyan);
        //}

        protected override void OnGetAgentState(Agent agent, bool usedSurgery)
        {
            MBDebug.Print("RBDebugMissionLogic - OnGetAgentState", 0, DebugColor.Cyan);//killed state  agent.State
        }

        public override void OnAgentAlarmedStateChanged(Agent agent, Agent.AIStateFlag flag)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentAlarmedStateChanged", 0, DebugColor.Cyan);
        }

        protected override void OnObjectDisabled(DestructableComponent destructionComponent)
        {
            MBDebug.Print("RBDebugMissionLogic - OnObjectDisabled", 0, DebugColor.Cyan);
        }

        public override void OnMissionModeChange(MissionMode oldMissionMode, bool atStart)
        {
            MBDebug.Print("RBDebugMissionLogic - OnMissionModeChange", 0, DebugColor.Cyan);
        }

        protected override void OnAgentControllerChanged(Agent agent, Agent.ControllerType oldController)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentControllerChanged", 0, DebugColor.Cyan);
        }

        //public override void OnItemPickup(Agent agent, SpawnedItemEntity item)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnItemPickup", 0, DebugColor.Cyan);
        //}

        //public override void OnItemDrop(Agent agent, SpawnedItemEntity item)
        //{
        //    MBDebug.Print("RBDebugMissionLogic - OnItemDrop", 0, DebugColor.Cyan);
        //}

        public override void OnRegisterBlow(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData, in MissionWeapon attackerWeapon)
        {
            MBDebug.Print("RBDebugMissionLogic - OnRegisterBlow", 0, DebugColor.Cyan);// mele hit 1
        }

        public override void OnAgentShootMissile(Agent shooterAgent, EquipmentIndex weaponIndex, Vec3 position, Vec3 velocity, Mat3 orientation, bool hasRigidBody, int forcedMissileIndex)
        {
            MBDebug.Print("RBDebugMissionLogic - OnAgentShootMissile", 0, DebugColor.Cyan);
        }
    }
}