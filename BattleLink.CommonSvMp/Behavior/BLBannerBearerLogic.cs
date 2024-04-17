using System;
using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.MountAndBlade;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;

namespace BattleLink.Server.Behavior
{
    public class BLBannerBearerLogic : BannerBearerLogic
    {
        public new const float DefaultBannerBearerAgentDefensiveness = 1f;
        public new const float BannerSearcherUpdatePeriod = 3f;
        private readonly Dictionary<UIntPtr, BLBannerBearerLogic.FormationBannerController> _bannerToFormationMap = new Dictionary<UIntPtr, BLBannerBearerLogic.FormationBannerController>();
        private readonly Dictionary<Formation, BLBannerBearerLogic.FormationBannerController> _formationBannerData = new Dictionary<Formation, BLBannerBearerLogic.FormationBannerController>();
        private readonly Dictionary<Agent, Equipment> _initialSpawnEquipments = new Dictionary<Agent, Equipment>();
        private readonly BasicMissionTimer _bannerSearcherUpdateTimer;
        private readonly List<BLBannerBearerLogic.FormationBannerController> _playerFormationsRequiringUpdate = new List<BLBannerBearerLogic.FormationBannerController>();
        private bool _isMissionEnded;

        public new event Action<Formation> OnBannerBearersUpdated;

        public new event Action<Agent, bool> OnBannerBearerAgentUpdated;

        public BLBannerBearerLogic() => this._bannerSearcherUpdateTimer = new BasicMissionTimer();

        public new MissionAgentSpawnLogic AgentSpawnLogic { get; private set; }

        public new bool IsFormationBanner(Formation formation, SpawnedItemEntity spawnedItem)
        {
            if (!BannerBearerLogic.IsBannerItem(spawnedItem.WeaponCopy.Item))
                return false;
            BLBannerBearerLogic.FormationBannerController fromBannerEntity = this.GetFormationControllerFromBannerEntity(spawnedItem.GameEntity);
            return fromBannerEntity != null && fromBannerEntity.Formation == formation;
        }

        public new bool HasBannerOnGround(Formation formation)
        {
            BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(formation);
            return controllerFromFormation != null && controllerFromFormation.HasBannerOnGround();
        }

        public new BannerComponent GetActiveBanner(Formation formation)
        {
            BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(formation);
            if (controllerFromFormation == null)
                return (BannerComponent)null;
            return !controllerFromFormation.HasActiveBannerBearers() ? (BannerComponent)null : controllerFromFormation.BannerItem.BannerComponent;
        }

        public new List<Agent> GetFormationBannerBearers(Formation formation)
        {
            BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(formation);
            return controllerFromFormation != null ? controllerFromFormation.BannerBearers : new List<Agent>();
        }

        public new ItemObject GetFormationBanner(Formation formation)
        {
            ItemObject formationBanner = (ItemObject)null;
            BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(formation);
            if (controllerFromFormation != null)
                formationBanner = controllerFromFormation.BannerItem;
            return formationBanner;
        }

        public new bool IsBannerSearchingAgent(Agent agent)
        {
            if (agent.Formation != null)
            {
                BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(agent.Formation);
                if (controllerFromFormation != null)
                    return controllerFromFormation.IsBannerSearchingAgent(agent);
            }
            return false;
        }

        public new int GetMissingBannerCount(Formation formation)
        {
            BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(formation);
            if (controllerFromFormation == null || controllerFromFormation.BannerItem == null)
                return 0;
            int num = MissionGameModels.Current.BattleBannerBearersModel.GetDesiredNumberOfBannerBearersForFormation(formation) - controllerFromFormation.NumberOfBanners;
            return num <= 0 ? 0 : num;
        }

        public new Formation GetFormationFromBanner(SpawnedItemEntity spawnedItem)
        {
            GameEntity gameEntity = spawnedItem.GameEntity;
            return this.GetFormationControllerFromBannerEntity((NativeObject)gameEntity == (NativeObject)null ? spawnedItem.GameEntityWithWorldPosition.GameEntity : gameEntity)?.Formation;
        }

        public new void SetFormationBanner(Formation formation, ItemObject newBanner)
        {
            int num = newBanner == null ? 1 : (BLBannerBearerLogic.IsBannerItem(newBanner) ? 1 : 0);
            BLBannerBearerLogic.FormationBannerController bannerController = this.GetFormationControllerFromFormation(formation);
            if (bannerController != null)
            {
                if (bannerController.BannerItem != newBanner)
                    bannerController.SetBannerItem(newBanner);
            }
            else
            {
                bannerController = new BLBannerBearerLogic.FormationBannerController(formation, newBanner, this, this.Mission);
                this._formationBannerData.Add(formation, bannerController);
            }
            bannerController.UpdateBannerBearersForDeployment();
        }

        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();
            var BattleBannerBearersModel = MissionGameModels.Current.BattleBannerBearersModel;
            BattleBannerBearersModel.InitializeModel(this);
            //if (BattleBannerBearersModel is BLBattleBannerBearersModel blBannerModel)
            //{
                
            //}
            this.AgentSpawnLogic = this.Mission.GetMissionBehavior<MissionAgentSpawnLogic>();
            this.Mission.OnItemPickUp += new Action<Agent, SpawnedItemEntity>(this.OnItemPickup);
            this.Mission.OnItemDrop += new Action<Agent, SpawnedItemEntity>(this.OnItemDrop);
            this._initialSpawnEquipments.Clear();
        }

        protected override void OnEndMission()
        {
            base.OnEndMission();
            MissionGameModels.Current.BattleBannerBearersModel.FinalizeModel();
            this.Mission.OnItemPickUp -= new Action<Agent, SpawnedItemEntity>(this.OnItemPickup);
            this.Mission.OnItemDrop -= new Action<Agent, SpawnedItemEntity>(this.OnItemDrop);
            this.AgentSpawnLogic = (MissionAgentSpawnLogic)null;
            this._isMissionEnded = true;
        }

        public override void OnDeploymentFinished()
        {
            this._initialSpawnEquipments.Clear();
            this._isMissionEnded = false;
        }

        public override void OnMissionTick(float dt)
        {
            if ((double)this._bannerSearcherUpdateTimer.ElapsedTime >= 3.0)
            {
                foreach (BLBannerBearerLogic.FormationBannerController bannerController in this._formationBannerData.Values)
                    bannerController.UpdateBannerSearchers();
                this._bannerSearcherUpdateTimer.Reset();
            }
            if (this.Mission.Mode != MissionMode.Deployment || this._playerFormationsRequiringUpdate.IsEmpty<BLBannerBearerLogic.FormationBannerController>())
                return;
            foreach (BLBannerBearerLogic.FormationBannerController bannerController in this._playerFormationsRequiringUpdate)
                bannerController.UpdateBannerBearersForDeployment();
            this._playerFormationsRequiringUpdate.Clear();
        }

        public new void OnItemPickup(Agent agent, SpawnedItemEntity spawnedItem)
        {
            if (!BannerBearerLogic.IsBannerItem(spawnedItem.WeaponCopy.Item))
                return;
            GameEntity gameEntity = spawnedItem.GameEntityWithWorldPosition.GameEntity;
            BLBannerBearerLogic.FormationBannerController fromBannerEntity = this.GetFormationControllerFromBannerEntity(gameEntity);
            if (fromBannerEntity == null)
                return;
            fromBannerEntity.OnBannerEntityPickedUp(gameEntity, agent);
            fromBannerEntity.UpdateAgentStats();
        }

        public new void OnItemDrop(Agent agent, SpawnedItemEntity spawnedItem)
        {
            if (!BannerBearerLogic.IsBannerItem(spawnedItem.WeaponCopy.Item))
                return;
            BLBannerBearerLogic.FormationBannerController fromBannerEntity = this.GetFormationControllerFromBannerEntity(spawnedItem.GameEntity);
            if (fromBannerEntity == null)
                return;
            fromBannerEntity.OnBannerEntityDropped(spawnedItem.GameEntity);
            fromBannerEntity.UpdateAgentStats();
        }

        public override void OnAgentRemoved(
          Agent affectedAgent,
          Agent affectorAgent,
          AgentState agentState,
          KillingBlow blow)
        {
            if (affectedAgent.Banner == null || agentState != AgentState.Routed)
                return;
            this.RemoveBannerOfAgent(affectedAgent);
        }

        public override void OnAgentPanicked(Agent affectedAgent)
        {
            if (affectedAgent.Banner == null)
                return;
            BLBannerBearerLogic.ForceDropAgentBanner(affectedAgent);
        }

        public new void UpdateAgent(Agent agent, bool willBecomeBannerBearer)
        {
            if (willBecomeBannerBearer)
            {
                BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(agent.Formation);
                ItemObject bannerItem = controllerFromFormation.BannerItem;
                Equipment equipmentForAgent = this.CreateBannerEquipmentForAgent(agent, bannerItem);
                agent.UpdateSpawnEquipmentAndRefreshVisuals(equipmentForAgent);
                GameEntity fromEquipmentSlot = agent.GetWeaponEntityFromEquipmentSlot(EquipmentIndex.ExtraWeaponSlot);
                this.AddBannerEntity(controllerFromFormation, fromEquipmentSlot);
                controllerFromFormation.OnBannerEntityPickedUp(fromEquipmentSlot, agent);
            }
            else if (agent.Banner != null)
            {
                this.RemoveBannerOfAgent(agent);
                agent.UpdateSpawnEquipmentAndRefreshVisuals(this._initialSpawnEquipments[agent]);
            }
            agent.UpdateCachedAndFormationValues(false, false);
            agent.SetIsAIPaused(true);

            //BLAgentLabelRefreshMessage mes = new BLAgentLabelRefreshMessage()
            //{
            //    id = agent.Index,
            //};
            //var a = Mission.Current.AllAgents;
            //var b = Mission.FindAgentWithIndex(agent.Index);
            //GameNetwork.BeginBroadcastModuleEvent();
            //GameNetwork.WriteMessage(mes);
            //GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients, (NetworkCommunicator)null);

            Action<Agent, bool> bearerAgentUpdated = this.OnBannerBearerAgentUpdated;
            if (bearerAgentUpdated == null)
                return;
            bearerAgentUpdated(agent, willBecomeBannerBearer);
        }

        public new Agent SpawnBannerBearer(
          IAgentOriginBase troopOrigin,
          bool isPlayerSide,
          Formation formation,
          bool spawnWithHorse,
          bool isReinforcement,
          int formationTroopCount,
          int formationTroopIndex,
          bool isAlarmed,
          bool wieldInitialWeapons,
          bool forceDismounted,
          Vec3? initialPosition,
          Vec2? initialDirection,
          string specialActionSetSuffix = null,
          bool useTroopClassForSpawn = false)
        {
            BLBannerBearerLogic.FormationBannerController controllerFromFormation = this.GetFormationControllerFromFormation(formation);
            ItemObject bannerItem = controllerFromFormation.BannerItem;
            Agent agent = this.Mission.SpawnTroop(troopOrigin, isPlayerSide, true, spawnWithHorse, isReinforcement, formationTroopCount, formationTroopIndex, isAlarmed, wieldInitialWeapons, forceDismounted, initialPosition, initialDirection, specialActionSetSuffix, bannerItem, controllerFromFormation.Formation.FormationIndex, useTroopClassForSpawn);
            agent.UpdateCachedAndFormationValues(false, false);
            GameEntity fromEquipmentSlot = agent.GetWeaponEntityFromEquipmentSlot(EquipmentIndex.ExtraWeaponSlot);
            this.AddBannerEntity(controllerFromFormation, fromEquipmentSlot);
            controllerFromFormation.OnBannerEntityPickedUp(fromEquipmentSlot, agent);
            return agent;
        }

        public new static bool IsBannerItem(ItemObject item) => item != null && item.IsBannerItem && item.BannerComponent != null;

        private void AddBannerEntity(
          BLBannerBearerLogic.FormationBannerController formationBannerController,
          GameEntity bannerEntity)
        {
            this._bannerToFormationMap.Add(bannerEntity.Pointer, formationBannerController);
            formationBannerController.AddBannerEntity(bannerEntity);
        }

        private void RemoveBannerEntity(
          BLBannerBearerLogic.FormationBannerController formationBannerController,
          GameEntity bannerEntity)
        {
            this._bannerToFormationMap.Remove(bannerEntity.Pointer);
            formationBannerController.RemoveBannerEntity(bannerEntity);
        }

        private BLBannerBearerLogic.FormationBannerController GetFormationControllerFromFormation(
          Formation formation)
        {
            BLBannerBearerLogic.FormationBannerController bannerController;
            return !this._formationBannerData.TryGetValue(formation, out bannerController) ? (BLBannerBearerLogic.FormationBannerController)null : bannerController;
        }

        private BLBannerBearerLogic.FormationBannerController GetFormationControllerFromBannerEntity(
          GameEntity bannerEntity)
        {
            BLBannerBearerLogic.FormationBannerController bannerController;
            return this._bannerToFormationMap.TryGetValue(bannerEntity.Pointer, out bannerController) ? bannerController : (BLBannerBearerLogic.FormationBannerController)null;
        }

        private Equipment CreateBannerEquipmentForAgent(Agent agent, ItemObject bannerItem)
        {
            Equipment spawnEquipment = agent.SpawnEquipment;
            if (!this._initialSpawnEquipments.ContainsKey(agent))
                this._initialSpawnEquipments[agent] = spawnEquipment;
            Equipment equipmentForAgent = new Equipment(spawnEquipment);
            ItemObject replacementWeapon = MissionGameModels.Current.BattleBannerBearersModel.GetBannerBearerReplacementWeapon(agent.Character);
            equipmentForAgent[EquipmentIndex.WeaponItemBeginSlot] = new EquipmentElement(replacementWeapon);
            for (int index = 1; index < 4; ++index)
                equipmentForAgent[index] = new EquipmentElement();
            //equipmentForAgent[EquipmentIndex.ExtraWeaponSlot] = new EquipmentElement(bannerItem, null, agent.Banner);
            equipmentForAgent[EquipmentIndex.ExtraWeaponSlot] = new EquipmentElement(bannerItem);
            return equipmentForAgent;
        }

        private void RemoveBannerOfAgent(Agent agent)
        {
            GameEntity fromEquipmentSlot = agent.GetWeaponEntityFromEquipmentSlot(EquipmentIndex.ExtraWeaponSlot);
            BLBannerBearerLogic.FormationBannerController fromBannerEntity = this.GetFormationControllerFromBannerEntity(fromEquipmentSlot);
            if (fromBannerEntity == null)
                return;
            this.RemoveBannerEntity(fromBannerEntity, fromEquipmentSlot);
            fromBannerEntity.UpdateAgentStats();
        }

        private static void ForceDropAgentBanner(Agent agent)
        {
            if (agent != null)
            {
                ItemObject banner = agent.Banner;
            }
            agent.DropItem(EquipmentIndex.ExtraWeaponSlot);
        }

        private class FormationBannerController
        {
            private int _lastActiveBannerBearerCount;
            private bool _requiresAgentStatUpdate;
            private BLBannerBearerLogic _bannerLogic;
            private Mission _mission;
            private Dictionary<Agent, (GameEntity bannerEntity, float lastDistance)> _bannerSearchers;
            private readonly Dictionary<UIntPtr, BLBannerBearerLogic.FormationBannerController.BannerInstance> _bannerInstances;
            private MBList<Agent> _nearbyAllyAgentsListCache = new MBList<Agent>();

            public Formation Formation { get; private set; }

            public ItemObject BannerItem { get; private set; }

            public bool HasBanner => this.BannerItem != null;

            public List<Agent> BannerBearers => this._bannerInstances.Values.Where<BLBannerBearerLogic.FormationBannerController.BannerInstance>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, bool>)(instance => instance.IsOnAgent)).Select<BLBannerBearerLogic.FormationBannerController.BannerInstance, Agent>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, Agent>)(instance => instance.BannerBearer)).ToList<Agent>();

            public List<GameEntity> BannersOnGround => this._bannerInstances.Values.Where<BLBannerBearerLogic.FormationBannerController.BannerInstance>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, bool>)(instance => instance.IsOnGround)).Select<BLBannerBearerLogic.FormationBannerController.BannerInstance, GameEntity>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, GameEntity>)(instance => instance.Entity)).ToList<GameEntity>();

            public int NumberOfBannerBearers => this._bannerInstances.Values.Count<BLBannerBearerLogic.FormationBannerController.BannerInstance>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, bool>)(instance => instance.IsOnAgent));

            public int NumberOfBanners => this._bannerInstances.Count;

            public static float BannerSearchDistance => 9f;

            public FormationBannerController(
              Formation formation,
              ItemObject bannerItem,
              BLBannerBearerLogic bannerLogic,
              Mission mission)
            {
                this.Formation = formation;
                this.Formation.OnUnitAdded += new Action<Formation, Agent>(this.OnAgentAdded);
                this.Formation.OnUnitRemoved += new Action<Formation, Agent>(this.OnAgentRemoved);
                this.Formation.OnBeforeMovementOrderApplied += new Action<Formation, MovementOrder.MovementOrderEnum>(this.OnBeforeFormationMovementOrderApplied);
                this.Formation.OnAfterArrangementOrderApplied += new Action<Formation, ArrangementOrder.ArrangementOrderEnum>(this.OnAfterArrangementOrderApplied);
                this._bannerInstances = new Dictionary<UIntPtr, BLBannerBearerLogic.FormationBannerController.BannerInstance>();
                this._bannerSearchers = new Dictionary<Agent, (GameEntity, float)>();
                this._requiresAgentStatUpdate = false;
                this._lastActiveBannerBearerCount = 0;
                this._bannerLogic = bannerLogic;
                this._mission = mission;
                this.SetBannerItem(bannerItem);
            }

            public void SetBannerItem(ItemObject bannerItem)
            {
                int num = bannerItem == null ? 1 : (BLBannerBearerLogic.IsBannerItem(bannerItem) ? 1 : 0);
                this.BannerItem = bannerItem;
            }

            public bool HasBannerEntity(GameEntity bannerEntity) => (NativeObject)bannerEntity != (NativeObject)null && this._bannerInstances.Keys.Contains<UIntPtr>(bannerEntity.Pointer);

            public bool HasBannerOnGround() => this.HasBanner && this._bannerInstances.Any<KeyValuePair<UIntPtr, BLBannerBearerLogic.FormationBannerController.BannerInstance>>((Func<KeyValuePair<UIntPtr, BLBannerBearerLogic.FormationBannerController.BannerInstance>, bool>)(instance => instance.Value.IsOnGround));

            public bool HasActiveBannerBearers() => this.GetNumberOfActiveBannerBearers() > 0;

            public bool IsBannerSearchingAgent(Agent agent) => this._bannerSearchers.Keys.Contains<Agent>(agent);

            public int GetNumberOfActiveBannerBearers()
            {
                int activeBannerBearers = 0;
                if (this.HasBanner)
                {
                    BattleBannerBearersModel bannerBearersModel = MissionGameModels.Current.BattleBannerBearersModel;
                    activeBannerBearers = this._bannerInstances.Values.Count<BLBannerBearerLogic.FormationBannerController.BannerInstance>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, bool>)(instance => instance.IsOnAgent && bannerBearersModel.CanBannerBearerProvideEffectToFormation(instance.BannerBearer, this.Formation)));
                }
                return activeBannerBearers;
            }

            public void UpdateAgentStats(bool forceUpdate = false)
            {
                if (!forceUpdate && !this._requiresAgentStatUpdate)
                    return;
                this.Formation.ApplyActionOnEachUnit((Action<Agent>)(agent =>
                {
                    agent.UpdateAgentProperties();
                    agent.MountAgent?.UpdateAgentProperties();
                }));
                this._requiresAgentStatUpdate = false;
            }

            public void RepositionFormation()
            {
                this.Formation.SetMovementOrder(this.Formation.GetReadonlyMovementOrderReference());
                this.Formation.ApplyActionOnEachUnit((Action<Agent>)(agent => agent.UpdateCachedAndFormationValues(true, false)));
            }

            public void UpdateBannerSearchers()
            {
                List<GameEntity> bannersOnGround = this.BannersOnGround;
                if (!this._bannerSearchers.IsEmpty<KeyValuePair<Agent, (GameEntity, float)>>())
                {
                    List<Agent> agentList = new List<Agent>();
                    foreach (KeyValuePair<Agent, (GameEntity bannerEntity, float lastDistance)> bannerSearcher in this._bannerSearchers)
                    {
                        KeyValuePair<Agent, (GameEntity bannerEntity, float lastDistance)> searcherTuple = bannerSearcher;
                        Agent key = searcherTuple.Key;
                        if (key.IsActive())
                        {
                            if (!bannersOnGround.Any<GameEntity>((Func<GameEntity, bool>)(bannerEntity => bannerEntity.Pointer == searcherTuple.Value.bannerEntity.Pointer)))
                                agentList.Add(key);
                        }
                        else
                            agentList.Add(key);
                    }
                    foreach (Agent searcher in agentList)
                        this.RemoveBannerSearcher(searcher);
                }
                foreach (GameEntity gameEntity1 in bannersOnGround)
                {
                    GameEntity banner = gameEntity1;
                    bool flag = false;
                    if (this._bannerSearchers.IsEmpty<KeyValuePair<Agent, (GameEntity, float)>>())
                    {
                        flag = true;
                    }
                    else
                    {
                        KeyValuePair<Agent, (GameEntity, float)> keyValuePair = this._bannerSearchers
                            .FirstOrDefault<KeyValuePair<Agent, (GameEntity, float)>>((Func<KeyValuePair<Agent, (GameEntity, float)>, bool>)(tuple => tuple.Value.Item1.Pointer == banner.Pointer));

                        if (keyValuePair.Key == null)
                        {
                            flag = true;
                        }
                        else
                        {
                            Agent key = keyValuePair.Key;
                            if (key.IsActive())
                            {
                                GameEntity gameEntity2 = keyValuePair.Value.Item1;
                                float num1 = keyValuePair.Value.Item2;
                                float num2 = key.Position.AsVec2.Distance(gameEntity2.GlobalPosition.AsVec2);
                                if ((double)num2 <= (double)num1 && (double)num2 < (double)BLBannerBearerLogic.FormationBannerController.BannerSearchDistance)
                                {
                                    this._bannerSearchers[key] = (gameEntity2, num2);
                                }
                                else
                                {
                                    this.RemoveBannerSearcher(key);
                                    flag = true;
                                }
                            }
                            else
                            {
                                this.RemoveBannerSearcher(key);
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        float distance;
                        Agent searcherForBanner = this.FindBestSearcherForBanner(banner, out distance);
                        if (searcherForBanner != null)
                            this.AddBannerSearcher(searcherForBanner, banner, distance);
                    }
                }
            }

            public void UpdateBannerBearersForDeployment()
            {
                List<Agent> bannerBearers = this.BannerBearers;
                List<(Agent, bool)> source = new List<(Agent, bool)>();
                List<Agent> agentList = new List<Agent>();
                int count = 0;
                BattleBannerBearersModel bannerBearersModel = MissionGameModels.Current.BattleBannerBearersModel;
                if (bannerBearersModel.CanFormationDeployBannerBearers(this.Formation))
                {
                    count = bannerBearersModel.GetDesiredNumberOfBannerBearersForFormation(this.Formation);
                    foreach (Agent agent in bannerBearers)
                    {
                        if (count > 0 && agent.Formation == this.Formation)
                            --count;
                        else
                            agentList.Add(agent);
                    }
                }
                else
                    agentList.AddRange((IEnumerable<Agent>)bannerBearers);
                foreach (Agent agent in agentList)
                {
                    bool flag = false;
                    if (count > 0)
                    {
                        flag = true;
                        --count;
                    }
                    source.Add((agent, flag));
                }
                if (count > 0)
                {
                    List<Agent> bannerBearableAgents = this.FindBannerBearableAgents(count);
                    for (int index = 0; index < bannerBearableAgents.Count && count > 0; ++index)
                    {
                        Agent agent = bannerBearableAgents[index];
                        agentList.Add(agent);
                        source.Add((agent, true));
                        --count;
                    }
                }
                if (!source.IsEmpty<(Agent, bool)>())
                {
                    //this._bannerLogic.AgentSpawnLogic.GetSpawnHorses(this.Formation.Team.Side);
                    //int side = (int)this._mission.PlayerTeam.Side;
                    foreach ((Agent, bool) valueTuple in source)
                        this._bannerLogic.UpdateAgent(valueTuple.Item1, valueTuple.Item2);
                }
                this.UpdateAgentStats();
                this.RepositionFormation();
                Action<Formation> bannerBearersUpdated = this._bannerLogic.OnBannerBearersUpdated;
                if (bannerBearersUpdated == null)
                    return;
                bannerBearersUpdated(this.Formation);
            }

            public void AddBannerEntity(GameEntity entity)
            {
                if (this._bannerInstances.ContainsKey(entity.Pointer))
                    return;
                this._bannerInstances.Add(entity.Pointer, new BLBannerBearerLogic.FormationBannerController.BannerInstance((Agent)null, entity, BLBannerBearerLogic.FormationBannerController.BannerState.Initialized));
            }

            public void RemoveBannerEntity(GameEntity entity)
            {
                this._bannerInstances.Remove(entity.Pointer);
                this.UpdateBannerSearchers();
                this.CheckRequiresAgentStatUpdate();
            }

            public void OnBannerEntityPickedUp(GameEntity entity, Agent agent)
            {
                this._bannerInstances[entity.Pointer] = new BLBannerBearerLogic.FormationBannerController.BannerInstance(agent, entity, BLBannerBearerLogic.FormationBannerController.BannerState.OnAgent);
                if (agent.IsAIControlled)
                {
                    agent.ResetEnemyCaches();
                    agent.Defensiveness = 1f;
                }
                this.UpdateBannerSearchers();
                this.CheckRequiresAgentStatUpdate();
            }

            public void OnBannerEntityDropped(GameEntity entity)
            {
                this._bannerInstances[entity.Pointer] = new BLBannerBearerLogic.FormationBannerController.BannerInstance((Agent)null, entity, BLBannerBearerLogic.FormationBannerController.BannerState.OnGround);
                this.UpdateBannerSearchers();
                this.CheckRequiresAgentStatUpdate();
            }

            public void OnBeforeFormationMovementOrderApplied(
              Formation formation,
              MovementOrder.MovementOrderEnum orderType)
            {
                if (formation != this.Formation)
                    return;
                this.UpdateBannerBearerArrangementPositions();
            }

            public void OnAfterArrangementOrderApplied(
              Formation formation,
              ArrangementOrder.ArrangementOrderEnum orderEnum)
            {
                if (formation != this.Formation)
                    return;
                this.UpdateBannerBearerArrangementPositions();
            }

            private Agent FindBestSearcherForBanner(GameEntity banner, out float distance)
            {
                distance = float.MaxValue;
                Agent searcherForBanner = (Agent)null;
                Vec2 asVec2 = banner.GlobalPosition.AsVec2;
                this._mission.GetNearbyAllyAgents(asVec2, BLBannerBearerLogic.FormationBannerController.BannerSearchDistance, this.Formation.Team, this._nearbyAllyAgentsListCache);
                BattleBannerBearersModel bannerBearersModel = MissionGameModels.Current.BattleBannerBearersModel;
                foreach (Agent agent in (List<Agent>)this._nearbyAllyAgentsListCache)
                {
                    if (agent.Formation == this.Formation && bannerBearersModel.CanAgentPickUpAnyBanner(agent))
                    {
                        float num = agent.Position.AsVec2.Distance(asVec2);
                        if ((double)num < (double)distance && !this._bannerSearchers.ContainsKey(agent))
                        {
                            searcherForBanner = agent;
                            distance = num;
                        }
                    }
                }
                return searcherForBanner;
            }

            private List<Agent> FindBannerBearableAgents(int count)
            {
                List<Agent> source = new List<Agent>();
                if (count > 0)
                {
                    BattleBannerBearersModel bannerBearerModel = MissionGameModels.Current.BattleBannerBearersModel;
                    foreach (IFormationUnit looseDetachedOne in (List<IFormationUnit>)this.Formation.UnitsWithoutLooseDetachedOnes)
                    {
                        if (looseDetachedOne is Agent agent && agent.Banner == null && bannerBearerModel.CanAgentBecomeBannerBearer(agent))
                            source.Add(agent);
                    }
                    source = source.OrderByDescending<Agent, int>((Func<Agent, int>)(agent => bannerBearerModel.GetAgentBannerBearingPriority(agent))).ToList<Agent>();
                }
                return source;
            }

            private void UpdateBannerBearerArrangementPositions()
            {
                List<Agent> list = this._bannerInstances.Values.Where<BLBannerBearerLogic.FormationBannerController.BannerInstance>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, bool>)(instance => instance.IsOnAgent && instance.BannerBearer.Formation == this.Formation)).Select<BLBannerBearerLogic.FormationBannerController.BannerInstance, Agent>((Func<BLBannerBearerLogic.FormationBannerController.BannerInstance, Agent>)(instance => instance.BannerBearer)).ToList<Agent>();
                List<FormationArrangementModel.ArrangementPosition> bannerBearerPositions = MissionGameModels.Current.FormationArrangementsModel.GetBannerBearerPositions(this.Formation, list.Count);
                if (bannerBearerPositions == null || bannerBearerPositions.IsEmpty<FormationArrangementModel.ArrangementPosition>())
                    return;
                int index = 0;
                foreach (Agent firstUnit in list)
                {
                    if (firstUnit != null && firstUnit.IsAIControlled && firstUnit.Formation == this.Formation)
                    {
                        int fileIndex1;
                        int rankIndex1;
                        firstUnit.GetFormationFileAndRankInfo(out fileIndex1, out rankIndex1);
                        for (; index < bannerBearerPositions.Count; ++index)
                        {
                            FormationArrangementModel.ArrangementPosition arrangementPosition = bannerBearerPositions[index];
                            int fileIndex2 = arrangementPosition.FileIndex;
                            int rankIndex2 = arrangementPosition.RankIndex;
                            bool flag = fileIndex1 == fileIndex2 && rankIndex1 == rankIndex2;
                            if (!flag)
                            {
                                IFormationUnit unit = this.Formation.Arrangement.GetUnit(fileIndex2, rankIndex2);
                                if (unit != null && unit is Agent secondUnit)
                                {
                                    if (secondUnit == firstUnit)
                                        flag = true;
                                    else if (secondUnit != this.Formation.Captain)
                                    {
                                        this.Formation.SwitchUnitLocations(firstUnit, secondUnit);
                                        flag = true;
                                    }
                                }
                            }
                            if (flag)
                            {
                                ++index;
                                break;
                            }
                        }
                    }
                }
            }

            private void OnAgentAdded(Formation formation, Agent agent)
            {
                if (this.Formation != formation)
                    return;
                if (!this._bannerLogic._isMissionEnded && this._mission.Mode == MissionMode.Deployment && formation.Team.IsPlayerTeam && MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
                {
                    int countToBearBanners = MissionGameModels.Current.BattleBannerBearersModel.GetMinimumFormationTroopCountToBearBanners();
                    if (formation.CountOfUnits != countToBearBanners || this._bannerLogic._playerFormationsRequiringUpdate.Contains(this))
                        return;
                    this._bannerLogic._playerFormationsRequiringUpdate.Add(this);
                }
                else
                    this.UpdateBannerSearchers();
            }

            private void OnAgentRemoved(Formation formation, Agent agent)
            {
                if (this.Formation != formation)
                    return;
                if (!this._bannerLogic._isMissionEnded && this._mission.Mode == MissionMode.Deployment && formation.Team.IsPlayerTeam && MissionGameModels.Current.BattleInitializationModel.CanPlayerSideDeployWithOrderOfBattle())
                {
                    int countToBearBanners = MissionGameModels.Current.BattleBannerBearersModel.GetMinimumFormationTroopCountToBearBanners();
                    if (formation.CountOfUnits != countToBearBanners - 1 || this._bannerLogic._playerFormationsRequiringUpdate.Contains(this))
                        return;
                    this._bannerLogic._playerFormationsRequiringUpdate.Add(this);
                }
                else
                    this.UpdateBannerSearchers();
            }

            private void CheckRequiresAgentStatUpdate()
            {
                if (this._requiresAgentStatUpdate)
                    return;
                int activeBannerBearers = this.GetNumberOfActiveBannerBearers();
                if ((activeBannerBearers <= 0 || this._lastActiveBannerBearerCount != 0) && (activeBannerBearers != 0 || this._lastActiveBannerBearerCount <= 0))
                    return;
                this._requiresAgentStatUpdate = true;
                this._lastActiveBannerBearerCount = activeBannerBearers;
            }

            private void AddBannerSearcher(Agent searcher, GameEntity banner, float distance)
            {
                this._bannerSearchers.Add(searcher, (banner, distance));
                searcher.HumanAIComponent?.DisablePickUpForAgentIfNeeded();
            }

            private void RemoveBannerSearcher(Agent searcher)
            {
                this._bannerSearchers.Remove(searcher);
                if (!searcher.IsActive())
                    return;
                searcher.HumanAIComponent?.DisablePickUpForAgentIfNeeded();
            }

            public enum BannerState
            {
                Initialized,
                OnAgent,
                OnGround,
            }

            public struct BannerInstance
            {
                public readonly Agent BannerBearer;
                public readonly GameEntity Entity;
                private readonly BLBannerBearerLogic.FormationBannerController.BannerState State;

                public bool IsOnGround => this.State == BLBannerBearerLogic.FormationBannerController.BannerState.OnGround;

                public bool IsOnAgent => this.State == BLBannerBearerLogic.FormationBannerController.BannerState.OnAgent;

                public BannerInstance(
                  Agent bannerBearer,
                  GameEntity entity,
                  BLBannerBearerLogic.FormationBannerController.BannerState state)
                {
                    this.BannerBearer = bannerBearer;
                    this.Entity = entity;
                    this.State = state;
                }
            }
        }
    }
}