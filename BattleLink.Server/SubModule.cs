using BattleLink.Common.Behavior;
using BattleLink.Common.Debug;
using BattleLink.CommonSvMp.GameComponents;
using BattleLink.Server.Api;
using NetworkMessages.FromServer;
using SandBox.GameComponents;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.NetworkComponents;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using Module = TaleWorlds.MountAndBlade.Module;

namespace BattleLink.Server
{
    public class SubModule : MBSubModuleBase
    {


        protected override void OnSubModuleLoad()
        {
            SandboxBattleMoraleModel campaign = (SandboxBattleMoraleModel)FormatterServices.GetUninitializedObject(typeof(SandboxBattleMoraleModel));
            //campaign.GetAverageMorale((Formation)null);
            // new Mission(new MissionInitializerRecord(),null);
            // var campaign = FormatterServices.GetUninitializedObject(typeof(MultiplayerIntermissionMapItemAdded));
            //var campaign = FormatterServices.GetUninitializedObject(typeof(BaseNetworkComponent));
            //var campaign = FormatterServices.GetUninitializedObject(typeof(BehaviorAssaultWalls));
            //  var campaign = FormatterServices.GetUninitializedObject(typeof(SiegeWeapon));
            //var campaign = FormatterServices.GetUninitializedObject(typeof(UsableMachine));
            //var campaign = FormatterServices.GetUninitializedObject(typeof(SiegeWeaponController));
            // var campaign = FormatterServices.GetUninitializedObject(typeof(FormationAI));

            MBDebug.Print("BattleLink - OnSubModuleLoad");
            base.OnSubModuleLoad();

            // ex in MultiplayerMissions
            Module.CurrentModule.AddMultiplayerGameMode(new BattleLinkGameMode());

            //keep this
            ServerListener.Start();
            //ListedServerCommandManager.ServerSideIntermissionManager.AddMapToAutomatedBattlePool("battle_terrain_biome_131");

            MBDebug.Print("BattleLink - OnSubModuleLoad - End", 0, DebugColor.Green);//1
        }

        protected override void OnSubModuleUnloaded()
        {
            ServerListener.Stop();
            MBDebug.Print("BattleLink - OnSubModuleUnloaded");
            base.OnSubModuleUnloaded();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            MBDebug.Print("BattleLink - OnBeforeInitialModuleScreenSetAsRoot");//2
            base.OnBeforeInitialModuleScreenSetAsRoot();
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            var a = GameNetwork.NetworkHandlers;

            MBDebug.Print("BattleLink - InitializeGameStarter");
            base.InitializeGameStarter(game, starterObject);

            // DefautlPerks



            // sealed BLCharacterObject... needs to duplicate everything
            // GameModelsManager iterate from last element to first
            starterObject.AddModel(new BLAgentStatCalculateModelOld());
            starterObject.AddModel(new DefaultMapWeatherModel());
            starterObject.AddModel(new BLBattleBannerBearersModel());
            starterObject.AddModel(new BLSandboxBattleMoraleModel());
            //starterObject.AddModel(new BLAgentApplyDamageModel());
            //starterObject.AddModel(new BLStrikeMagnitudeModel());
            //starterObject.AddModel(new SandboxApplyWeatherEffectsModel());



            {
                var xmlDocument = MBObjectManager.GetMergedXmlForManaged("CraftingPieces", false, false, "MultiplayerGame");
                MBObjectManager.Instance.LoadXml(xmlDocument);
            }
            {
                // MBCharacterSkills
                var xmlDocument = MBObjectManager.GetMergedXmlForManaged("SkillSets", false, false, "MultiplayerGame");
                MBObjectManager.Instance.LoadXml(xmlDocument);
            }

            {
                var xmlDocument = MBObjectManager.GetMergedXmlForManaged("EquipmentRosters", false, false, "MultiplayerGame");
                MBObjectManager.Instance.RegisterType<MBEquipmentRoster>("EquipmentRoster", "EquipmentRosters", 51U, true, false);
                MBObjectManager.Instance.LoadXml(xmlDocument);
            }

            MBObjectManager.Instance.RegisterType<PerkObject>("Perk", "Perks", 19U, true, false);
            MBObjectManager.Instance.RegisterType<SkillObject>("Skill", "Skills", 9U, true, false);
            MBObjectManager.Instance.RegisterType<SkillEffect>("SkillEffect", "SkillEffects", 53U, true, false);

            //game.GameTextManager.LoadGameTexts();
            MBDebug.Print("BattleLink - InitializeGameStarter - End", 0, DebugColor.Green);//3
        }


        //private void LoadXmlNpcToMp(XmlDocument docOri)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    XmlDeclaration xmlDeclaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
        //    XmlElement root = doc.DocumentElement;
        //    doc.InsertBefore(xmlDeclaration, root);

        //    XmlElement element1 = doc.CreateElement(string.Empty, "MPCharacters", string.Empty);
        //    doc.AppendChild(element1);

        //  //  docOri.ChildNodes[1].Value = "MPCharacters";
        //    for (XmlNode xmlNode = docOri.ChildNodes[1].ChildNodes[0]; xmlNode != null; xmlNode = xmlNode.NextSibling)
        //    {
        //        XmlNode importNode = doc.ImportNode(xmlNode, true);
        //        element1.AppendChild(importNode);


        //        if ("NPCCharacter".Equals(importNode.Name))
        //        {
        //            importNode.Attributes.RemoveNamedItem("skill_template");
        //            importNode.Attributes.RemoveNamedItem("is_basic_troop");
        //            importNode.Attributes.RemoveNamedItem("upgrade_requires");

        //            var child2 = importNode.SelectSingleNode("Traits");
        //            if (child2!=null)
        //            {
        //                importNode.RemoveChild(importNode.SelectSingleNode("Traits"));
        //            }
        //        }
        //    }
        //    doc.Save(System.IO.Path.Combine(BasePath.Name, "Modules", "SandBoxCoreMP", "ModuleData", "MPNPCCharacters.xml"));
        //}

        private int OnApplicationTickCount = 0;
        protected override void OnApplicationTick(float delta)
        {
            if (OnApplicationTickCount++ % 100 == 0)
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

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            MBDebug.Print("BattleLink - OnGameStart");
            base.OnGameStart(game, gameStarterObject);//4
        }

        private int AfterAsyncTickTickCount = 0;
        protected override void AfterAsyncTickTick(float dt)
        {
            if (AfterAsyncTickTickCount++ % 100 == 0)
            {
                //   MBDebug.Print("BattleLink - AfterAsyncTickTick "+ AfterAsyncTickTickCount);
            }
            base.AfterAsyncTickTick(dt);
        }

        public override void OnGameLoaded(Game game, object initializerObject)
        {
            MBDebug.Print("BattleLink - OnGameLoaded");
            base.OnGameLoaded(game, initializerObject);
        }

        public override void OnNewGameCreated(Game game, object initializerObject)
        {
            MBDebug.Print("BattleLink - OnNewGameCreated");
            base.OnNewGameCreated(game, initializerObject);
        }

        public override void BeginGameStart(Game game)
        {
            MBDebug.Print("BattleLink - BeginGameStart");
            base.BeginGameStart(game);//6
        }

        public override void OnCampaignStart(Game game, object starterObject)
        {
            MBDebug.Print("BattleLink - OnCampaignStart");
            base.OnCampaignStart(game, starterObject);
        }

        public override void RegisterSubModuleObjects(bool isSavedCampaign)
        {
            MBDebug.Print("BattleLink - RegisterSubModuleObjects");
            base.RegisterSubModuleObjects(isSavedCampaign);
        }

        public override void AfterRegisterSubModuleObjects(bool isSavedCampaign)
        {
            MBDebug.Print("BattleLink - AfterRegisterSubModuleObjects");
            base.AfterRegisterSubModuleObjects(isSavedCampaign);
        }

        public override void OnMultiplayerGameStart(Game game, object starterObject)
        {
            MBDebug.Print("BattleLink - OnMultiplayerGameStart");
            base.OnMultiplayerGameStart(game, starterObject);//7
        }

        public override void OnGameInitializationFinished(Game game)
        {
            MBDebug.Print("BattleLink - OnGameInitializationFinished");
            base.OnGameInitializationFinished(game);

            // Skills are load in InitializeDefaultGameObjects after InitializeGameStarter
            var campaign = FormatterServices.GetUninitializedObject(typeof(Campaign));
            FieldInfo fiCampaignCurrent = typeof(Campaign).GetField("\u003CCurrent\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            fiCampaignCurrent.SetValue(null, campaign);
            
            var defaultPerks = new DefaultPerks();
            FieldInfo fiCampaignPerks =  typeof(Campaign).GetField("\u003CDefaultPerks\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignPerks.SetValue(campaign, defaultPerks);

            var defaultSkillEffects = new DefaultSkillEffects();
            FieldInfo fiCampaignSkills = typeof(Campaign).GetField("\u003CDefaultSkillEffects\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignSkills.SetValue(campaign, defaultSkillEffects);

            FieldInfo fiCampaignGameMode = typeof(Campaign).GetField("\u003CGameMode\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignGameMode.SetValue(campaign, CampaignGameMode.Campaign);

            List<GameModel> _gameModels = new List<GameModel>()
            {
                new DefaultCharacterDevelopmentModel(),
            };
            GameModels GameModels = new GameModels(_gameModels);
            FieldInfo fiCampaignModels = typeof(Campaign).GetField("_gameModels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignModels.SetValue(campaign, GameModels);

            MBDebug.Print("BattleLink - OnGameInitializationFinished - End");//8
        }

        public override void OnAfterGameInitializationFinished(Game game, object starterObject)
        {
            MBDebug.Print("BattleLink - OnAfterGameInitializationFinished");
            base.OnAfterGameInitializationFinished(game, starterObject);
        }

        public override bool DoLoading(Game game)
        {
            MBDebug.Print("BattleLink - DoLoading", 0, DebugColor.Green);//9
            return base.DoLoading(game);
        }

        public override void OnGameEnd(Game game)
        {
            MBDebug.Print("BattleLink - OnGameEnd");
            base.OnGameEnd(game);
        }

        public override void OnMissionBehaviorInitialize(Mission mission)
        {
            MBDebug.Print("BattleLink - OnMissionBehaviorInitialize");
            base.OnMissionBehaviorInitialize(mission);
         
            //mission.AddMissionBehavior(new BLDebugMissionLogic());
            mission.AddMissionBehavior(new BLCommonMissionEndLogic());

            MBDebug.Print("BattleLink - OnMissionBehaviorInitialize");
        }

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            MBDebug.Print("BattleLink - OnBeforeMissionBehaviorInitialize");//10
            base.OnBeforeMissionBehaviorInitialize(mission);

        }

        public override void OnInitialState()
        {
            MBDebug.Print("BattleLink - OnInitialState");
            base.OnInitialState();
        }

    }



}