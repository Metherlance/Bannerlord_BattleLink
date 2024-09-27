using BattleLink.Common.Debug;
using BattleLink.Singleplayer.Patch;
using BattleLink.Web.Common;
using HarmonyLib;
using Helpers;
using ProtoBuf;
using SandBox.GameComponents;
using System;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Singleplayer
{
    //*EditLord*
    public class SubModule : MBSubModuleBase
    {

        //protected override void OnApplicationTick(float dt)
        //{
        //    if (Campaign.Current != null)
        //    {
        //        if (PlayerEncounter.Current != null)
        //        {
        //            MapEventParty.Equals(null, null);
        //            //var e = PlayerEncounter.Battle;
        //            var a = PlayerEncounter.Current;
        //            //var s = PlayerEncounter.BattleState;
        //            MBDebug.Print("OnSubModuleLoad", 0, DebugColor.Cyan);
        //        }

        //    }
        //}

        [ProtoContract]
        public class TestMessage
        {
            [ProtoMember(1)]
            public string Message { get; set; }
        }

        public static void Main()
        {
            try
            {
                //var testMessage = new TestMessage { Message = "Hello, World!" };
                var testMessage = new InitCampaignData { Id = "Hello, World!" };
                byte[] serializedMessage;

                // Sérialisation
                using (var memoryStream = new System.IO.MemoryStream())
                {
                    Serializer.Serialize(memoryStream, testMessage);
                    serializedMessage = memoryStream.ToArray();
                }

                //var serializedMessag = new PacketSp2Sw { PacketType = PacketSp2SwType.Init, Data = serializedMessage };
                //using (var memoryStream = new System.IO.MemoryStream())
                //{

                //    Serializer.Serialize(memoryStream, serializedMessag);
                //    byte[] serializedMessage3 = memoryStream.ToArray();
                //}


                // Désérialisation
                using (var memoryStream = new System.IO.MemoryStream(serializedMessage))
                {
                    var deserializedMessage = Serializer.Deserialize<TestMessage>(memoryStream);
                    Console.WriteLine($"Deserialized Message: {deserializedMessage.Message}");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"Exception: {e}");
            }
        }


        protected override void OnSubModuleLoad()
        {
            Main();
            UdpSpSw.start();


            //Agent a22 = (Agent)FormatterServices.GetUninitializedObject(typeof(Agent));
            //Scene a22 = (Scene)FormatterServices.GetUninitializedObject(typeof(Scene));
            //var a = a22.GetSunDirection();
            var a2 = FormatterServices.GetUninitializedObject(typeof(SandboxBattleMoraleModel));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(TeamAISiegeAttacker));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(UsableMachine));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(TacticDefendCastle));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(SiegeLane));



            //var a2 = FormatterServices.GetUninitializedObject(typeof(TeamAISiegeComponent));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(CharacterObject));
            //new SandboxBattleBannerBearersModel();
            //new BattlePowerCalculationLogic();
            //var a = FormatterServices.GetUninitializedObject(typeof(AgentBuildData));
            //var a = FormatterServices.GetUninitializedObject(typeof(MissionCombatantsLogic));
            //var a = FormatterServices.GetUninitializedObject(typeof(TacticComponent));

            //var a2 = (Mission)FormatterServices.GetUninitializedObject(typeof(MissionAgentSpawnLogic));

            //var a2 = (PlayerEncounter)FormatterServices.GetUninitializedObject(typeof(PlayerEncounter));
            //a2.SetupFields(null,null);

            base.OnSubModuleLoad();

            var harmony = new Harmony("BattleLink.Singleplayer.Patch.Permanent");
            var OpenNew = AccessTools.Method(typeof(MissionState), "OpenNew");
            var OpenNewPrefix = AccessTools.Method(typeof(MissionStatePatch), "OpenNew");
            harmony.Patch(OpenNew, prefix: new HarmonyMethod(OpenNewPrefix));


            // custom battle MP
            //Module.CurrentModule.AddInitialStateOption(new InitialStateOption("CustomBattleMP", new TextObject("{}Custom Battle MP"), 5001, () =>
            //{
            //    InformationManager.DisplayMessage(new InformationMessage("Custom Battle MP - Click"));
            //    MissionStatePatch.CampaignTimeSecBattleMP = DateTime.UtcNow.ToBinary();
            //    MBGameManager.StartNewGame(new CustomGameManager());
            //},
            //() => (false,null)
            //));


        }

        //protected override void OnSubModuleUnloaded()
        //{
        //    base.OnSubModuleUnloaded();
        //    MBDebug.Print("OnSubModuleUnloaded", 0, DebugColor.Cyan);

        //}

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(new InformationMessage("BattleLink | loaded", new Color(0, 1, 0)));

        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            MBDebug.Print("InitializeGameStarter", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("InitializeGameStarter"));
            if (starterObject is CampaignGameStarter campaignstarter)
            {
                campaignstarter.AddBehavior(new BattleLinkSingleplayerBehavior());
            }
        }

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            MBDebug.Print("OnBeforeMissionBehaviorInitialize", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnBeforeMissionBehaviorInitialize"));

            //var a = mission.Scene.GetSunDirection();

            mission.AddMissionBehavior(new BLDebugMissionLogic());
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            MBDebug.Print("OnCampaignStart", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnCampaignStart"));
        }


        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            MBDebug.Print("OnGameStart", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnGameStart"));

            if (game.GameType is Campaign)
            {
                ((CampaignGameStarter)gameStarterObject).AddBehavior(new CampaignSync());
            }
        }

        protected override void OnApplicationTick(float dt)
        {
            MBDebug.Print("OnApplicationTick", 0, DebugColor.Cyan);
            //InformationManager.DisplayMessage(new InformationMessage("OnApplicationTick"));
        }

        protected override void AfterAsyncTickTick(float dt)
        {
            MBDebug.Print("AfterAsyncTickTick", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("AfterAsyncTickTick"));
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            MBDebug.Print("OnGameLoaded", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnGameLoaded"));
        }

        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            MBDebug.Print("OnNewGameCreated", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnNewGameCreated"));
        }

        public override void BeginGameStart(Game game)
        {
            MBDebug.Print("BeginGameStart", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("BeginGameStart"));
        }

        public override void RegisterSubModuleObjects(bool isSavedCampaign)
        {
            MBDebug.Print("RegisterSubModuleObjects", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("RegisterSubModuleObjects"));
        }

        public override void AfterRegisterSubModuleObjects(bool isSavedCampaign)
        {
            MBDebug.Print("AfterRegisterSubModuleObjects", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("AfterRegisterSubModuleObjects"));
        }

        //public override void OnMultiplayerGameStart(Game game, object starterObject)
        //{
        //    MBDebug.Print("OnMultiplayerGameStart", 0, DebugColor.Cyan);
        //}

        public override void OnGameInitializationFinished(Game game)
        {
            MBDebug.Print("OnGameInitializationFinished", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnGameInitializationFinished"));
        }

        public override void OnAfterGameInitializationFinished(Game game, object starterObject)
        {
            MBDebug.Print("OnAfterGameInitializationFinished", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnAfterGameInitializationFinished"));
        }

        public override bool DoLoading(Game game)
        {
            MBDebug.Print("DoLoading", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("DoLoading"));
            return true;
        }

        public override void OnGameEnd(Game game)
        {
            MBDebug.Print("OnGameEnd", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnGameEnd"));
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            MBDebug.Print("OnMissionBehaviorInitialize", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnMissionBehaviorInitialize"));
        }

        public override void OnInitialState()
        {
            MBDebug.Print("OnInitialState", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnInitialState"));
        }

        //protected override void OnNetworkTick(float dt)
        //{
        //}

    }

}