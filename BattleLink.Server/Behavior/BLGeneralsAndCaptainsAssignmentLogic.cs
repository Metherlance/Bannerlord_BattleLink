using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.MountAndBlade;
using TaleWorlds.LinQuick;
using BattleLink.Common.Behavior;

namespace BattleLink.Server.Behavior
{
    internal class BLGeneralsAndCaptainsAssignmentLogic : MissionLogic
    {
        public int MinimumAgentCountToLeadGeneralFormation = 3;
        private BannerBearerLogic _bannerLogic;
        private readonly bool _createBodyguard;
        private bool _isPlayerTeamGeneralFormationSet;

        public BLGeneralsAndCaptainsAssignmentLogic(bool createBodyguard = true)
        {
            this._createBodyguard = createBodyguard;
            this._isPlayerTeamGeneralFormationSet = false;
        }

        public override void AfterStart() => this._bannerLogic = this.Mission.GetMissionBehavior<BannerBearerLogic>();

        public override void OnTeamDeployed(Team team)
        {
            this.SetGeneralAgentOfTeam(team);
            if (team.IsPlayerTeam)
            {
                if (MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
                    return;
                if (this.CanTeamHaveGeneralsFormation(team))
                {
                    this.CreateGeneralFormationForTeam(team);
                    this._isPlayerTeamGeneralFormationSet = true;
                }
                this.AssignBestCaptainsForTeam(team);
            }
            else
            {
                if (this.CanTeamHaveGeneralsFormation(team))
                    this.CreateGeneralFormationForTeam(team);
                this.AssignBestCaptainsForTeam(team);
            }
        }

        public override void OnDeploymentFinished()
        {
            //if (!this._isPlayerTeamGeneralFormationSet && this.CanTeamHaveGeneralsFormation(playerTeam))
            //{
            //    this.CreateGeneralFormationForTeam(playerTeam);
            //    this._isPlayerTeamGeneralFormationSet = true;
            //}
            //Agent mainAgent;
            //if (!this._isPlayerTeamGeneralFormationSet || (mainAgent = this.Mission.MainAgent) == null || playerTeam.GeneralAgent == mainAgent)
            //    return;
            //mainAgent.SetCanLeadFormationsRemotely(true);
            //Formation formation = playerTeam.GetFormation(FormationClass.NumberOfRegularFormations);
            //mainAgent.Formation = formation;
            //mainAgent.Team.TriggerOnFormationsChanged(formation);
            //formation.QuerySystem.Expire();
        }

        protected virtual void SortCaptainsByPriority(Team team, ref List<Agent> captains) => captains = captains.OrderByDescending<Agent, float>((Func<Agent, float>)(captain => team.GeneralAgent != captain ? captain.Character.GetPower() : float.MaxValue)).ToList<Agent>();

        protected virtual Formation PickBestRegularFormationToLead(
          Agent agent,
          List<Formation> candidateFormations)
        {
            Formation lead = (Formation)null;
            int num = 0;
            foreach (Formation candidateFormation in candidateFormations)
            {
                if (!(agent.HasMount ^ candidateFormation.CalculateHasSignificantNumberOfMounted))
                {
                    int countOfUnits = candidateFormation.CountOfUnits;
                    if (countOfUnits > num)
                    {
                        num = countOfUnits;
                        lead = candidateFormation;
                    }
                }
            }
            return lead;
        }

        private bool CanTeamHaveGeneralsFormation(Team team)
        {
            Agent generalAgent = team.GeneralAgent;
            if (generalAgent == null)
                return false;
            return generalAgent == this.Mission.MainAgent || team.QuerySystem.MemberCount >= 50;
        }

        private void AssignBestCaptainsForTeam(Team team)
        {
            List<Agent> list1 = team.ActiveAgents.Where<Agent>((Func<Agent, bool>)(agent => agent.IsHero)).ToList<Agent>();
            this.SortCaptainsByPriority(team, ref list1);
            int numRegularFormations = 8;
            List<Formation> list2 = team.FormationsIncludingEmpty.WhereQ<Formation>((Func<Formation, bool>)(f => f.CountOfUnits > 0 && f.FormationIndex < (FormationClass)numRegularFormations)).ToList<Formation>();
            List<Agent> agentList = new List<Agent>();
            foreach (Agent agent in list1)
            {
                Formation formation = (Formation)null;
                BattleBannerBearersModel bannerBearersModel = MissionGameModels.Current.BattleBannerBearersModel;
                if (agent == team.GeneralAgent && team.BodyGuardFormation != null && team.BodyGuardFormation.CountOfUnits > 0)
                    formation = team.BodyGuardFormation;
                if (formation == null)
                {
                    formation = this.PickBestRegularFormationToLead(agent, list2);
                    if (formation != null)
                        list2.Remove(formation);
                }
                if (formation != null)
                {
                    agentList.Add(agent);
                    this.OnCaptainAssignedToFormation(agent, formation);
                }
            }
            foreach (Agent agent in agentList)
                list1.Remove(agent);
            foreach (Agent agent in list1)
            {
                Agent candidate = agent;
                if (list2.IsEmpty<Formation>())
                    break;
                Formation formation = list2.FirstOrDefault<Formation>((Func<Formation, bool>)(f => f.CalculateHasSignificantNumberOfMounted == candidate.HasMount));
                if (formation != null)
                {
                    this.OnCaptainAssignedToFormation(candidate, formation);
                    list2.Remove(formation);
                }
            }
        }

        private void SetGeneralAgentOfTeam(Team team)
        {
            Agent agent = (Agent)null;

            List<IFormationUnit> list = team.FormationsIncludingEmpty.SelectMany<Formation, IFormationUnit>((Func<Formation, IEnumerable<IFormationUnit>>)(f => (IEnumerable<IFormationUnit>)f.UnitsWithoutLooseDetachedOnes)).ToList<IFormationUnit>();
            
            (_, var teamDto) = BLReferentialHolder.getTeamDtoBy(team);
            string generalId = teamDto.generalId;

            if (generalId != null && list.Count<IFormationUnit>((Func<IFormationUnit, bool>)(ta => ((Agent)ta).Character != null && ((Agent)ta).Character.GetName().Equals(generalId))) >= 1)
            {
                agent = (Agent)list.First<IFormationUnit>((Func<IFormationUnit, bool>)(ta => ((Agent)ta).Character != null && ((Agent)ta)
                .Character.StringId.Equals(generalId)));
            }
            else if (list.Any<IFormationUnit>((Func<IFormationUnit, bool>)(u => !((Agent)u).IsMainAgent && ((Agent)u).IsHero)))
            {
                agent = (Agent)list.Where<IFormationUnit>((Func<IFormationUnit, bool>)(u => !((Agent)u).IsMainAgent && ((Agent)u).IsHero))
                    .MaxBy<IFormationUnit, float>((Func<IFormationUnit, float>)(u => ((Agent)u).CharacterPowerCached));
            }

            agent?.SetCanLeadFormationsRemotely(true);
            team.GeneralAgent = agent;
        }

        private void CreateGeneralFormationForTeam(Team team)
        {
            Agent generalAgent = team.GeneralAgent;
            Formation formation1 = team.GetFormation(FormationClass.NumberOfRegularFormations);
            this.Mission.SetFormationPositioningFromDeploymentPlan(formation1);
            WorldPosition orderWorldPosition = formation1.CreateNewOrderWorldPosition(WorldPosition.WorldPositionEnforcedCache.GroundVec3);
            formation1.SetMovementOrder(MovementOrder.MovementOrderMove(orderWorldPosition));
            formation1.SetControlledByAI(true);
            team.GeneralsFormation = formation1;
            generalAgent.Formation = formation1;
            generalAgent.Team.TriggerOnFormationsChanged(formation1);
            formation1.QuerySystem.Expire();
            TacticComponent.SetDefaultBehaviorWeights(formation1);
            formation1.AI.SetBehaviorWeight<BehaviorGeneral>(1f);
            formation1.PlayerOwner = (Agent)null;
            if (!this._createBodyguard || generalAgent == this.Mission.MainAgent)
                return;
            List<IFormationUnit> list1 = team.FormationsIncludingEmpty.SelectMany<Formation, IFormationUnit>((Func<Formation, IEnumerable<IFormationUnit>>)(f => (IEnumerable<IFormationUnit>)f.UnitsWithoutLooseDetachedOnes)).ToList<IFormationUnit>();
            list1.Remove((IFormationUnit)generalAgent);
            List<IFormationUnit> list2 = list1.Where<IFormationUnit>((Func<IFormationUnit, bool>)(u =>
            {
                if (!(u is Agent agent2) || agent2.Character != null && agent2.Character.IsHero || agent2.Banner != null)
                    return false;
                return generalAgent.MountAgent == null ? !agent2.HasMount : agent2.HasMount;
            })).ToList<IFormationUnit>();
            int count = MathF.Min((int)((double)list2.Count / 10.0), 20);
            if (count == 0)
                return;
            Formation formation2 = team.GetFormation(FormationClass.Bodyguard);
            formation2.SetMovementOrder(MovementOrder.MovementOrderMove(orderWorldPosition));
            formation2.SetControlledByAI(true);
            List<IFormationUnit> list3 = list2.OrderByDescending<IFormationUnit, float>((Func<IFormationUnit, float>)(u => ((Agent)u).CharacterPowerCached)).Take<IFormationUnit>(count).ToList<IFormationUnit>();
            IEnumerable<Formation> formations = list3.Select<IFormationUnit, Formation>((Func<IFormationUnit, Formation>)(bu => ((Agent)bu).Formation)).Distinct<Formation>();
            foreach (Agent agent3 in list3)
                agent3.Formation = formation2;
            foreach (Formation formation3 in formations)
            {
                team.TriggerOnFormationsChanged(formation3);
                formation3.QuerySystem.Expire();
            }
            TacticComponent.SetDefaultBehaviorWeights(formation2);
            formation2.AI.SetBehaviorWeight<BehaviorProtectGeneral>(1f);
            formation2.PlayerOwner = (Agent)null;
            formation2.QuerySystem.Expire();
            team.BodyGuardFormation = formation2;
            team.TriggerOnFormationsChanged(formation2);
        }

        private void OnCaptainAssignedToFormation(Agent captain, Formation formation)
        {
            if (captain.Formation != formation && captain != formation.Team.GeneralAgent)
            {
                captain.Formation = formation;
                formation.Team.TriggerOnFormationsChanged(formation);
                formation.QuerySystem.Expire();
            }
            formation.Captain = captain;
            if (this._bannerLogic == null || captain.FormationBanner == null)
                return;
            this._bannerLogic.SetFormationBanner(formation, captain.FormationBanner);
        }
    }
}
