using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
//using HarmonyLib;
using System.Reflection;
using System;
using TaleWorlds.Core;
using TaleWorlds.ModuleManager;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade.DedicatedCustomServer;
using static TaleWorlds.MountAndBlade.Mission;
using System.Collections.ObjectModel;
using TaleWorlds.Library;
using static TaleWorlds.Library.Debug;
using Module = TaleWorlds.MountAndBlade.Module;
using TaleWorlds.MountAndBlade.Source.Missions;
using BattleLink.Common;
using RealmsBattle.Common;
using RealmsBattle.Common.Behavior;
using TaleWorlds.MountAndBlade.Multiplayer;

namespace BattleLink.Server
{
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            MBDebug.Print("BattleLink - OnSubModuleLoad");
            base.OnSubModuleLoad();

            Module.CurrentModule.AddMultiplayerGameMode(new BattleLinkGameMode(
                (Mission) =>
                {
                    return new MissionBehavior[]{
                                (MissionBehavior) MissionLobbyComponent.CreateBehavior(),
                                (MissionBehavior) new RBMissionMpGameMode(MultiplayerGameType.Captain),//MissionMultiplayerFlagDomination
                                (MissionBehavior) new MultiplayerRoundController(),//RB
                                (MissionBehavior) new RBMultiplayerWarmupComponent(),
                                (MissionBehavior) new MissionMultiplayerGameModeFlagDominationClient(),
                                (MissionBehavior) new MultiplayerTimerComponent(),
                                (MissionBehavior) new MultiplayerMissionAgentVisualSpawnComponent(),
                                (MissionBehavior) new ConsoleMatchStartEndHandler(),
                                (MissionBehavior) new SpawnComponent(new FlagDominationSpawnFrameBehavior(), new RBFlagDominationSpawningBehavior()),
                                (MissionBehavior) new MissionLobbyEquipmentNetworkComponent(),
                                (MissionBehavior) new MultiplayerTeamSelectComponent(),
                                (MissionBehavior) new MissionHardBorderPlacer(),
                                (MissionBehavior) new MissionBoundaryPlacer(),
                                (MissionBehavior) new AgentVictoryLogic(),
                                (MissionBehavior) new AgentHumanAILogic(),
                                (MissionBehavior) new MissionAgentPanicHandler(),
                                (MissionBehavior) new MissionBoundaryCrossingHandler(),
                                (MissionBehavior) new MultiplayerPollComponent(),
                                (MissionBehavior) new MultiplayerAdminComponent(),
                                (MissionBehavior) new MultiplayerGameNotificationsComponent(),
                                (MissionBehavior) new MissionOptionsComponent(),
                                (MissionBehavior) new MissionScoreboardComponent((IScoreboardData) new CaptainScoreboardData()),
                                (MissionBehavior) new EquipmentControllerLeaveLogic(),
                                (MissionBehavior) new MultiplayerPreloadHelper()
                     };
                }
            ));

            MBDebug.Print("BattleLink - OnSubModuleLoad - End", 0, DebugColor.Green);
        }

        protected override void OnSubModuleUnloaded()
        {
            MBDebug.Print("BattleLink - OnSubModuleUnloaded");
            base.OnSubModuleUnloaded();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            MBDebug.Print("BattleLink - OnBeforeInitialModuleScreenSetAsRoot");
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            MBDebug.Print("BattleLink - InitializeGameStarter");
            base.InitializeGameStarter(game, starterObject);
            game.GameTextManager.LoadGameTexts();
            MBDebug.Print("BattleLink - InitializeGameStarter - End", 0, DebugColor.Green);
        }

        private int OnApplicationTickCount = 0;
        protected override void OnApplicationTick(float delta)
        {
            if (OnApplicationTickCount++ % 100 ==0)
            {
               // MBDebug.Print("BattleLink - OnApplicationTick"+ OnApplicationTickCount);
            }
            base.OnApplicationTick(delta);
        }

        public override void OnConfigChanged()
        {
            MBDebug.Print("BattleLink - OnConfigChanged");
            base.OnConfigChanged();
        }

        protected override void OnGameStart(/*Parameter with token 08001714*/Game game, /*Parameter with token 08001715*/IGameStarter gameStarterObject)
        {
            MBDebug.Print("BattleLink - OnGameStart");
            base.OnGameStart(game, gameStarterObject);
        }

        private int AfterAsyncTickTickCount = 0;
        protected override void AfterAsyncTickTick(/*Parameter with token 08001717*/float dt)
        {
            if (AfterAsyncTickTickCount++ % 100 == 0)
            {
             //   MBDebug.Print("BattleLink - AfterAsyncTickTick "+ AfterAsyncTickTickCount);
            }
            base.AfterAsyncTickTick(dt);
        }

        public override void OnGameLoaded(/*Parameter with token 0800171A*/Game game, /*Parameter with token 0800171B*/object initializerObject)
        {
            MBDebug.Print("BattleLink - OnGameLoaded");
            base.OnGameLoaded(game, initializerObject);
        }

        public override void OnNewGameCreated(/*Parameter with token 0800171C*/Game game, /*Parameter with token 0800171D*/object initializerObject)
        {
            MBDebug.Print("BattleLink - OnNewGameCreated");
            base.OnNewGameCreated(game, initializerObject);
        }

        public override void BeginGameStart(/*Parameter with token 0800171E*/Game game)
        {
            MBDebug.Print("BattleLink - BeginGameStart");
            base.BeginGameStart(game);
        }

        public override void OnCampaignStart(/*Parameter with token 0800171F*/Game game, /*Parameter with token 08001720*/object starterObject)
        {
            MBDebug.Print("BattleLink - OnCampaignStart");
            base.OnCampaignStart(game, starterObject);
        }

        public override void RegisterSubModuleObjects(/*Parameter with token 08001721*/bool isSavedCampaign)
        {
            MBDebug.Print("BattleLink - RegisterSubModuleObjects");
            base.RegisterSubModuleObjects(isSavedCampaign);
        }

        public override void AfterRegisterSubModuleObjects(/*Parameter with token 08001722*/bool isSavedCampaign)
        {
            MBDebug.Print("BattleLink - AfterRegisterSubModuleObjects");
            base.AfterRegisterSubModuleObjects(isSavedCampaign);
        }

        public override void OnMultiplayerGameStart(/*Parameter with token 08001723*/Game game, /*Parameter with token 08001724*/object starterObject)
        {
            MBDebug.Print("BattleLink - OnMultiplayerGameStart");
            base.OnMultiplayerGameStart(game, starterObject);
        }

        public override void OnGameInitializationFinished(/*Parameter with token 08001725*/Game game)
        {
            MBDebug.Print("BattleLink - OnGameInitializationFinished");
            base.OnGameInitializationFinished(game);
        }

        public override void OnAfterGameInitializationFinished(/*Parameter with token 08001726*/Game game, /*Parameter with token 08001727*/object starterObject)
        {
            MBDebug.Print("BattleLink - OnAfterGameInitializationFinished");
            base.OnAfterGameInitializationFinished(game, starterObject);
        }

        public override bool DoLoading(/*Parameter with token 08001728*/Game game)
        {
            MBDebug.Print("BattleLink - DoLoading");
            return base.DoLoading(game);
        }

        public override void OnGameEnd(/*Parameter with token 08001729*/Game game)
        {
            MBDebug.Print("BattleLink - OnGameEnd");
            base.OnGameEnd(game);
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            MBDebug.Print("BattleLink - OnMissionBehaviorInitialize");
            base.OnMissionBehaviorInitialize(mission);
            MBDebug.Print("BattleLink - OnMissionBehaviorInitialize");
        }

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            MBDebug.Print("BattleLink - OnBeforeMissionBehaviorInitialize");
            base.OnBeforeMissionBehaviorInitialize(mission);

        }

        public override void OnInitialState()
        {
            MBDebug.Print("BattleLink - OnInitialState");
            base.OnInitialState();
        }

    }

   
            

}