using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using BattleLink.Handler;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.GameNetwork;

namespace BattleLink.Client.Behavior
{
    public class BLBehaviorClient : MissionLogic
    {
        private static readonly FieldInfo fieldIndexContainer = typeof(GameNetwork).GetField("_gameNetworkMessageTypesFromServer", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo fieldContainer = typeof(GameNetwork).GetField("_fromServerBaseMessageHandlers", BindingFlags.NonPublic | BindingFlags.Static);

        //public override void AfterStart()
        //{
        //    base.AfterStart();
        //    MBDebug.Print("RbBehaviorClient - EarlyStart - " 0, DebugColor.Green);
        //}



        //bool notInit = true;

        //public override void OnBehaviorInitialize()
        //{
        //    base.OnBehaviorInitialize();
        //}

        //public override void OnAfterMissionCreated()
        //{
        //    var _missionNetworkMessageHandlerRegisterer = new GameNetwork.NetworkMessageHandlerRegistererContainer();
        //    _missionNetworkMessageHandlerRegisterer.RegisterBaseHandler<BLMissionInitializerRecordMessage>(BLMissionInitializerRecordHandler.HandleServerEventMissionInitializerRecordMessage);
        //    _missionNetworkMessageHandlerRegisterer.RegisterMessages();
        //}

        //public override void OnRemoveBehavior()
        //{
        //    base.OnRemoveBehavior();
        //    AddRemoveMessageHandlers(NetworkMessageHandlerRegisterer.RegisterMode.Remove);
        //}

        //public void AddRemoveMessageHandlers(NetworkMessageHandlerRegisterer.RegisterMode mode)
        //{
        //    NetworkMessageHandlerRegisterer reg = new NetworkMessageHandlerRegisterer(mode);
        //    //reg.Register<BLMissionInitializerRecordMessage>(BLMissionInitializerRecordHandler.HandleServerEventMissionInitializerRecordMessage);

        //    reg.RegisterBaseHandler<BLMissionInitializerRecordMessage>(BLMissionInitializerRecordHandler.HandleServerEventMissionInitializerRecordMessage);
        //}


        public override void OnAddTeam(Team team)
        {
            //// reason : bs HandleServerEventAddTeam
            //BLInitTeamMessage bLInitTeamMessage = BLInitTeamHandler.teamInfos[team.TeamIndex];
            //if (bLInitTeamMessage!=null)
            //{
            //    Banner banner = new Banner(bLInitTeamMessage.FactionBannerKey);
            //    team.Banner = banner;
            //}

            //Scene scene = Mission.Current.Scene;
            //var a = scene.TimeOfDay;

            MBDebug.Print("BattleLinkGameMode - OnAddTeam - ", 0, DebugColor.Green);//1
        }

        public override void OnRenderingStarted()
        {
            Scene scene = Mission.Current.Scene;
            var a = scene.TimeOfDay;

            MBDebug.Print("BattleLinkGameMode - OnRenderingStarted - ", 0, DebugColor.Green);//1
        }

        //public override void OnAfterMissionCreated()
        //{
        //    // no Mission.Current.Scene here
        //    MBDebug.Print("BattleLinkGameMode - OnAfterMissionCreated - ", 0, DebugColor.Green);//1
        //}



        // WTF !!!?? DElete that
        //public override List<EquipmentElement> GetExtraEquipmentElementsForCharacter(BasicCharacterObject character, bool getAllEquipments = false)
        //{
        //    if (notInit && Mission.Scene!=null)
        //    {
        //        MBDebug.Print("RbBehaviorClient - GetExtraEquipmentElementsForCharacter", 0, DebugColor.Green);
        //        IEnumerable<GameEntity> lSpawn = Mission.Scene.FindEntitiesWithTag("spawnpoint");
        //        if (lSpawn.IsEmpty())
        //        {
        //            GameEntity spawnpoint_set = Mission.Scene.FindEntityWithTag("spawnpoint_set");
        //            if (spawnpoint_set != null)
        //            {
        //                lSpawn = spawnpoint_set.GetChildren();
        //                if (lSpawn.Count() > 5 && !lSpawn.First().HasTag("sp_visual_0"))
        //                {
        //                    int index = 0;
        //                    foreach (var spawn in lSpawn)
        //                    {
        //                        spawn.AddTag("sp_visual_" + index);
        //                        index += 1;
        //                        if (index > 5)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        notInit = false;
        //    }
        //    return base.GetExtraEquipmentElementsForCharacter(character, getAllEquipments); ;
        //}
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();


            //AddRemoveMessageHandlers(NetworkMessageHandlerRegisterer.RegisterMode.Add);

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
                int indexType = dicIndexType.TryGetValue(typeof(BLInitTeamMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCultureHandler = value[indexType];
                listCultureHandler.Clear();
                listCultureHandler.Add(BLInitTeamHandler.HandleServerEventInitTeamMessage);
                // InformationManager.DisplayMessage(new InformationMessage("BLInitTeamMessage Handler"));

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
               // InformationManager.DisplayMessage(new InformationMessage("BLInitCharactersMessage Handler"));
            }

            {

                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<Type, int> dicIndexType = (Dictionary<Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(CreateAgent), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCreateAgentHandler = value[indexType];
                listCreateAgentHandler.Clear();
                listCreateAgentHandler.Add(BLCreateAgentHandler.HandleServerEventCreateAgent);
               // InformationManager.DisplayMessage(new InformationMessage("CreateAgent Handler"));
            }

            {

                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(BLTeamCharactersMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCultureHandler = value[indexType];
                listCultureHandler.Clear();
                listCultureHandler.Add(BLTeamCharactersHandler.HandleServerTeamCharactersMessage);
                // InformationManager.DisplayMessage(new InformationMessage("BLInitTeamMessage Handler"));

            }


            // come on .... mir doesnt works ... replace:
            //CampaignSystem.GameState.IMapStateHandler.AfterTick(float dt) Ligne 1566
            //MapScreen.TickVisuals(float realDt) Ligne 2086
            // or
            // sun was init by SandBoxMissions.CreateSandBoxMissionInitializerRecord(scene, sceneLevels, decalAtlasGroup: DecalAtlasGroup.Town)
            var scene = Mission.Current.Scene;
            if (BLMissionInitializerRecordHandler.message != null && scene.GetName().Equals(BLMissionInitializerRecordHandler.message.SceneName))
            {
                //scene.TimeOfDay = BLMissionInitializerRecordHandler.message.TimeOfDay;
                //scene.SetRainDensity(BLMissionInitializerRecordHandler.message.RainInfoDensity);
                //scene.SetSnowDensity(BLMissionInitializerRecordHandler.message.SnowInfoDensity);
                //scene.SetSkyBrightness(BLMissionInitializerRecordHandler.message.SkyInfoBrightness);
                //scene.SetDrynessFactor(BLMissionInitializerRecordHandler.message.TimeInfoDrynessFactor);
                //scene.SetWinterTimeFactor(BLMissionInitializerRecordHandler.message.TimeInfoWinterTimeFactor);

                ////scene.SetSun(ref BLMissionInitializerRecordHandler.message.SunInfoColor, BLMissionInitializerRecordHandler.message.SunInfoAltitude, BLMissionInitializerRecordHandler.message.SunInfoAngle, BLMissionInitializerRecordHandler.message.SunInfoRayStrength);
                //scene.SetSunAngleAltitude(altitude: BLMissionInitializerRecordHandler.message.SunInfoAltitude, angle: BLMissionInitializerRecordHandler.message.SunInfoAngle);
                //scene.SetSunSize(BLMissionInitializerRecordHandler.message.SunInfoSize);
                //scene.set
                // SunInfoRayStrength?


                //    long season = BLMissionInitializerRecordHandler.message.TimeInfoSeason;


                //    float timeFactorForSnow;
                //    new DefaultMapWeatherModel().GetSeasonTimeFactorOfCampaignTime(CampaignTime.Seconds((season * 1814400L+((long)(3600f* scene.TimeOfDay)))), out timeFactorForSnow, out float _, false);
                //    MBMapScene.SetSeasonTimeFactor(scene, timeFactorForSnow);

                //MapWeatherVisualManager


            }

            MBDebug.Print("RBMultiplayerWarmupComponent - OnBehaviorInitialize ", 0, DebugColor.Green);


        }

       
    }
}