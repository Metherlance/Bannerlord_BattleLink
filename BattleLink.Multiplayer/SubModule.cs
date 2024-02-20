using BattleLink.Common.Model;
using BattleLink.Handler;
using NetworkMessages.FromServer;
using BattleLink.Multiplayer;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using Module = TaleWorlds.MountAndBlade.Module;
using System.Runtime.Serialization;
using BattleLink.CommonSvMp.GameComponents;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.GameComponents;
using TaleWorlds.CampaignSystem;
using static TaleWorlds.CampaignSystem.Campaign;
using NetworkMessages.FromClient;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer.NetworkComponents;
using BattleLink.Client.Behavior;


namespace BattleLink.Multiplayer
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly Color green = Color.FromUint(0x008000);

        //public override void OnInitialState()
        //{

        //    base.OnInitialState();

        //    InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnSubModuleLoad", green));
        //}
        //public override void OnMissionBehaviorInitialize(Mission mission)
        //{
        //    InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnMissionBehaviorInitialize", green));//2
        //}

       // public override void OnBeforeMissionBehaviorInitialize(Mission mission)
       // {

           // mission.AddMissionBehavior(new BLBehaviorClient());

            //InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnBeforeMissionBehaviorInitialize", green)); //1
        //}

        protected override void OnSubModuleLoad()
        {
           // var a22 = FormatterServices.GetUninitializedObject(typeof(BaseNetworkComponent));

            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnSubModuleLoad", green));

            Module.CurrentModule.AddMultiplayerGameMode(new BattleLinkGameMode());

            InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnSubModuleLoad", green));

        }


        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            addReferentialHandler();

            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(new InformationMessage("BattleLink | loaded", green));
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {

            MBDebug.Print("BattleLink - InitializeGameStarter");
            base.InitializeGameStarter(game, starterObject);

            game.GameTextManager.LoadGameTexts();


            // GameModelsManager part du dernier élement
            starterObject.AddModel(new BLAgentStatCalculateModelOld());
            starterObject.AddModel(new DefaultMapWeatherModel());
            // MultiplayerAgentStatCalculateModel


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

            MBObjectManager.Instance.RegisterType<BLCharacterObject>("BLCharacter", "BLCharacters", 60U, true, true);

            MBDebug.Print("BattleLink - InitializeGameStarter - End", 0, DebugColor.Green);
        }

        //public override void OnGameInitializationFinished(Game game)
        //{
        //    MBDebug.Print("BattleLink - OnGameInitializationFinished");
        //    base.OnGameInitializationFinished(game);

        //    MBDebug.Print("BattleLink - OnGameInitializationFinished - End");
        //}



        //public override void OnAfterGameInitializationFinished(Game game, object starterObject)
        //{
        //    MBDebug.Print("BattleLink - OnAfterGameInitializationFinished - End");
        //}


        //protected override void OnApplicationTick(float dt)
        //{

        //}

        private void addReferentialHandler()
        {
            //var _missionNetworkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegistererContainer();
            //_missionNetworkMessageHandlerRegisterer.RegisterBaseHandler<BLMissionInitializerRecordMessage>(BLMissionInitializerRecordHandler.HandleServerEventMissionInitializerRecordMessage);
            //_missionNetworkMessageHandlerRegisterer.RegisterMessages();

            var fieldIndexContainer = typeof(GameNetwork).GetField("_gameNetworkMessageTypesFromServer", BindingFlags.NonPublic | BindingFlags.Static);
            var fieldContainer = typeof(GameNetwork).GetField("_fromServerBaseMessageHandlers", BindingFlags.NonPublic | BindingFlags.Static);

            {
                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(BLMissionInitializerRecordMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listMirHandler = value[indexType];
                listMirHandler.Clear();
                listMirHandler.Add(BLMissionInitializerRecordHandler.HandleServerEventMissionInitializerRecordMessage);
                // InformationManager.DisplayMessage(new InformationMessage("BLInitCultureMessage Handler"));
            }

            //{
            //    var oDicIndexType = fieldIndexContainer.GetValue(null);
            //    Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
            //    int indexType = dicIndexType.TryGetValue(typeof(BLSiegeEngineMessage), out indexType) ? indexType : -1;

            //    var valu = fieldContainer.GetValue(null);
            //    Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
            //    var listCultureHandler = value[indexType];
            //    listCultureHandler.Clear();
            //    listCultureHandler.Add(BLBLSiegeEnginesHandler.HandleServerEventSiegeEngineMessageMessage);
            //    // InformationManager.DisplayMessage(new InformationMessage("BLInitCultureMessage Handler"));
            //}

            {

                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(BLInitCultureMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCultureHandler = value[indexType];
                listCultureHandler.Clear();
                listCultureHandler.Add(BLInitCultureHandler.HandleServerEventInitCultureMessage);
               // InformationManager.DisplayMessage(new InformationMessage("BLInitCultureMessage Handler"));

            }

            {
                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(BLInitCharactersMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCharacterHandler = value[indexType];
                listCharacterHandler.Clear();
                listCharacterHandler.Add(BLInitCharactersHandler.HandleServerEventInitCharactersMessage);
              //  InformationManager.DisplayMessage(new InformationMessage("BLInitCharactersMessage Handler"));
            }

            {

                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(CreateAgent), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCreateAgentHandler = value[indexType];
                listCreateAgentHandler.Clear();
                listCreateAgentHandler.Add(BLCreateAgentHandler.HandleServerEventCreateAgent);
                //InformationManager.DisplayMessage(new InformationMessage("CreateAgent Handler"));
            }
        }

        public override void OnGameInitializationFinished(Game game)
        {
            MBDebug.Print("BattleLink - OnGameInitializationFinished");
            base.OnGameInitializationFinished(game);

            Campaign campaign = new Campaign(CampaignGameMode.Campaign);
            campaign.SetLoadingParameters(GameLoadingType.SavedCampaign);
          
            // Skills are load in InitializeDefaultGameObjects after InitializeGameStarter
            //var campaign = FormatterServices.GetUninitializedObject(typeof(Campaign));
            //FieldInfo fiCampaignCurrent = typeof(Campaign).GetField("\u003CCurrent\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            //fiCampaignCurrent.SetValue(null, campaign);

            var defaultPerks = new DefaultPerks();
            FieldInfo fiCampaignPerks = typeof(Campaign).GetField("\u003CDefaultPerks\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignPerks.SetValue(campaign, defaultPerks);

            var defaultSkillEffects = new DefaultSkillEffects();
            FieldInfo fiCampaignSkills = typeof(Campaign).GetField("\u003CDefaultSkillEffects\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignSkills.SetValue(campaign, defaultSkillEffects);

            //
            List<GameModel> _gameModels = new List<GameModel>()
            {
                new DefaultCharacterDevelopmentModel(),
                new DefaultMapWeatherModel(),
            };
            GameModels GameModels = new GameModels(_gameModels);
            FieldInfo fiCampaignModels = typeof(Campaign).GetField("_gameModels", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            fiCampaignModels.SetValue(campaign, GameModels);

            MBDebug.Print("BattleLink - OnGameInitializationFinished - End");//8
        }

    }

}


