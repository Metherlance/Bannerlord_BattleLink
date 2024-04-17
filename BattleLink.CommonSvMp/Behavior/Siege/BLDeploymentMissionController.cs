using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade;
using TaleWorlds.Library;
using BattleLink.Server.Behavior;

namespace BattleLink.CommonSvMp.Behavior.Siege
{
    public class BLDeploymentMissionController : MissionLogic
    {
        private BattleDeploymentHandler _battleDeploymentHandler;
        protected BLMissionAgentSpawnLogic MissionAgentSpawnLogic;
        //private readonly bool _isPlayerAttacker;
        protected bool TeamSetupOver;
        private bool _isPlayerControllerSetToAI;

        public BLDeploymentMissionController()
        {

        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            _battleDeploymentHandler = Mission.GetMissionBehavior<BattleDeploymentHandler>();
            MissionAgentSpawnLogic = Mission.GetMissionBehavior<BLMissionAgentSpawnLogic>();
        }

        public override void AfterStart()
        {
            base.AfterStart();
            Mission.AllowAiTicking = false;
            for (int side = 0; side < 2; ++side)
                this.MissionAgentSpawnLogic.SetSpawnTroops((BattleSideEnum)side, false);
            this.MissionAgentSpawnLogic.SetReinforcementsSpawnEnabled(false);
        }

        private void SetupTeams()
        {
            //Utilities.SetLoadingScreenPercentage(0.92f);
            Mission.DisableDying = true;
            //bool _isPlayerAttacker = false;
            //BattleSideEnum sideAI = _isPlayerAttacker ? BattleSideEnum.Defender : BattleSideEnum.Attacker;
            //BattleSideEnum sidePlayer = _isPlayerAttacker ? BattleSideEnum.Attacker : BattleSideEnum.Defender;
            SetupTeamsOfSide(BattleSideEnum.Attacker);
            OnSideDeploymentFinished(BattleSideEnum.Attacker);
            //if (_isPlayerAttacker)
            //{
            //    foreach (Agent agent in (List<Agent>)Mission.Agents)
            //    {
            //        if (agent.IsHuman && agent.Team != null && agent.Team.Side == sideAI)
            //        {
            //            agent.SetRenderCheckEnabled(false);
            //            agent.AgentVisuals.SetVisible(false);
            //            agent.MountAgent?.SetRenderCheckEnabled(false);
            //            agent.MountAgent?.AgentVisuals.SetVisible(false);
            //        }
            //    }
            //}
            SetupTeamsOfSide(BattleSideEnum.Defender);
            OnSideDeploymentFinished(BattleSideEnum.Defender);
            Mission.IsTeleportingAgents = true;
            //Utilities.SetLoadingScreenPercentage(0.96f);
            //if (MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
            //    return;
            FinishDeployment();
        }

        public override void OnAgentControllerSetToPlayer(Agent agent)
        {
            if (_isPlayerControllerSetToAI)
                return;
            agent.Controller = Agent.ControllerType.AI;
            agent.SetIsAIPaused(true);
            agent.SetDetachableFromFormation(false);
            _isPlayerControllerSetToAI = true;
        }

        public override void OnMissionTick(float dt)
        {
            //base.OnMissionTick(dt);
            //if (TeamSetupOver || !(Mission.Scene != null))
            //    return;
            //SetupTeams();
            //TeamSetupOver = true;
        }

        public void BLOnMissionTickSetupTeams()
        {
            SetupTeams();
            TeamSetupOver = true;
        }


        //[Conditional("DEBUG")]
        //private void DebugTick()
        //{
        //    if (!Input.DebugInput.IsHotKeyPressed("SwapToEnemy"))
        //        return;
        //    this.Mission.MainAgent.Controller = Agent.ControllerType.AI;
        //    this.Mission.PlayerEnemyTeam.Leader.Controller = Agent.ControllerType.Player;
        //    this.SwapTeams();
        //}

        //private void SwapTeams() => Mission.PlayerTeam = Mission.PlayerEnemyTeam;

        protected void SetupTeamsOfSideAux(BattleSideEnum side)
        {
            Team team1 = side == BattleSideEnum.Attacker ? Mission.AttackerTeam : Mission.DefenderTeam;
            foreach (Formation formation in (List<Formation>)team1.FormationsIncludingSpecialAndEmpty)
            {
                if (formation.CountOfUnits > 0)
                    formation.ApplyActionOnEachUnit(agent =>
                    {
                        if (!agent.IsAIControlled)
                            return;
                        agent.AIStateFlags &= ~Agent.AIStateFlag.Alarmed;
                        agent.SetIsAIPaused(true);
                    });
            }
            Team team2 = side == BattleSideEnum.Attacker ? Mission.AttackerAllyTeam : Mission.DefenderAllyTeam;
            if (team2 != null)
            {
                foreach (Formation formation in (List<Formation>)team2.FormationsIncludingSpecialAndEmpty)
                {
                    if (formation.CountOfUnits > 0)
                        formation.ApplyActionOnEachUnit(agent =>
                        {
                            if (!agent.IsAIControlled)
                                return;
                            agent.AIStateFlags &= ~Agent.AIStateFlag.Alarmed;
                            agent.SetIsAIPaused(true);
                        });
                }
            }
            // null here for now
            this.MissionAgentSpawnLogic.OnBattleSideDeployed(team1.Side);
        }

        protected virtual void SetupTeamsOfSide(BattleSideEnum side)
        {
            this.MissionAgentSpawnLogic.SetSpawnTroops(side, true, true);
            SetupTeamsOfSideAux(side);
        }

        protected void OnSideDeploymentFinished(BattleSideEnum side)
        {
            if (side == BattleSideEnum.Attacker)
            {
                return;
            }
            Mission.IsTeleportingAgents = true;
            DeployFormationsOfTeam(Mission.DefenderTeam);
            if (null != Mission.DefenderAllyTeam)
            {
                DeployFormationsOfTeam(Mission.DefenderAllyTeam);
            }
            Mission.IsTeleportingAgents = false;
        }

        protected void DeployFormationsOfTeam(Team team)
        {
            foreach (Formation formation in (List<Formation>)team.FormationsIncludingEmpty)
            {
                if (formation.CountOfUnits > 0)
                    formation.SetControlledByAI(true);
            }
            team.QuerySystem.Expire();
            Mission.AllowAiTicking = true;
            Mission.ForceTickOccasionally = true;
            team.ResetTactic();
            bool teleportingAgents = Mission.Current.IsTeleportingAgents;
            Mission.IsTeleportingAgents = true;
            team.Tick(0.0f);
            Mission.IsTeleportingAgents = teleportingAgents;
            Mission.AllowAiTicking = false;
            Mission.ForceTickOccasionally = false;
        }

        public void FinishDeployment()
        {
            OnBeforeDeploymentFinished();
            //if (_isPlayerAttacker)
            //{
                //foreach (Agent agent in (List<Agent>)Mission.Agents)
                //{
                //    if (agent.IsHuman && agent.Team != null && agent.Team.Side == BattleSideEnum.Defender)
                //    {
                //        agent.SetRenderCheckEnabled(true);
                //        agent.AgentVisuals.SetVisible(true);
                //        agent.MountAgent?.SetRenderCheckEnabled(true);
                //        agent.MountAgent?.AgentVisuals.SetVisible(true);
                //    }
                //}
            //}
            Mission.IsTeleportingAgents = false;
            Mission.Current.OnDeploymentFinished();
            foreach (Agent agent in (List<Agent>)Mission.Agents)
            {
                if (agent.IsAIControlled)
                {
                    agent.AIStateFlags |= Agent.AIStateFlag.Alarmed;
                    agent.SetIsAIPaused(false);
                    if (agent.GetAgentFlags().HasAnyFlag(AgentFlag.CanWieldWeapon))
                        agent.ResetEnemyCaches();
                    agent.HumanAIComponent?.SyncBehaviorParamsIfNecessary();
                }
            }
            Agent mainAgent = Mission.MainAgent;
            if (mainAgent != null)
            {
                mainAgent.SetDetachableFromFormation(true);
                mainAgent.Controller = Agent.ControllerType.Player;
            }
            Mission.AllowAiTicking = true;
            Mission.DisableDying = false;
            this.MissionAgentSpawnLogic.SetReinforcementsSpawnEnabled(true);
            OnAfterDeploymentFinished();
            Mission.RemoveMissionBehavior(this);
        }

        public virtual void OnBeforeDeploymentFinished() => OnSideDeploymentFinished(Mission.AttackerTeam.Side);

        public virtual void OnAfterDeploymentFinished()
        {
            // null here for now
            //Mission.RemoveMissionBehavior(_battleDeploymentHandler);
        }
    }
}
