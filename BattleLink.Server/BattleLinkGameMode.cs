using BattleLink.Common;
using BattleLink.Common.Behavior;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using BattleLink.Common.Spawn;
using BattleLink.Common.Spawn.Battle;
using BattleLink.Common.Utils;
using BattleLink.CommonSvMp.Behavior;
using BattleLink.CommonSvMp.Behavior.Siege;
using BattleLink.Server.Behavior;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ListedServer;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.Source.Missions;
using TaleWorlds.MountAndBlade.Source.Missions.Handlers.Logic;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;

namespace BattleLink.Server
{
    public class BattleLinkGameMode : MissionBasedMultiplayerGameMode
    {

        public BattleLinkGameMode() : base("BattleLink")
        {
        }


        public override void StartMultiplayerGame(string _scene)
        {
            MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - " + _scene, 0, DebugColor.Green);

            try
            {

                //0,256 -> -512 2048
                CompressionMission.AgentOffsetCompressionInfo = new CompressionInfo.Integer(-512, 11);

                MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
                MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;

                MBObjectManager.Instance.RegisterType<BLCharacterObject>("BLCharacter", "BLCharacters", 60U, true, true);
                MBObjectManager.Instance.ClearAllObjectsWithType(typeof(BLCharacterObject));

                BLReferentialHolder.basicCharacterObjects = new List<BLCharacterObject>();

                BLMissionMpDomination.setNextBLMapForTesting();

                // load next battle and launch default mission
                FileInfo fileInfo;
                if (null == BLReferentialHolder.nextBattleInitializerFilePath)
                {
                    BLMissionMpDomination.setNextBLMap();

                    MBDebug.Print("BL - StartMultiplayerGame - no pending battle or finished battle", 0, DebugColor.Green);
                    //MultiplayerMissions.OpenCaptainMission(_scene);
                    string gamemodeDefault = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "config.properties")).Get("gamemode.default");
                    Module.CurrentModule.StartMultiplayerGame(gamemodeDefault, _scene);
                    taskEndAndSetNextMission = EndAndSetNextMission();
                    return;
                }
                else
                {
                    fileInfo = new FileInfo(BLReferentialHolder.nextBattleInitializerFilePath);
                }                               

                // Declare an object variable of the type to be deserialized.
                XmlSerializer serializer = new XmlSerializer(typeof(MPBattleInitializer));
                MPBattleInitializer battleInitializer = null;
                using (Stream reader = new FileStream(fileInfo.FullName, FileMode.Open))
                {
                    // Call the Deserialize method to restore the object's state.
                    battleInitializer = (MPBattleInitializer)serializer.Deserialize(reader);
                }
                BLReferentialHolder.initializerFilename = fileInfo.Name;
                MBDebug.Print("BL - StartMultiplayerGame - file loaded: " + fileInfo.FullName, 0, DebugColor.Green);


                var mir = battleInitializer.MissionInitializerRecord;
                var sceneName = mir.SceneName;


                //add map to server map pool
                // AutomatedMapPool and not UsableMaps
                if (!ListedServerCommandManager.ServerSideIntermissionManager.AutomatedMapPool.Contains(sceneName))
                {
                    //ListedServerCommandManager.ServerSideIntermissionManager.AddMapToAutomatedBattlePool(sceneName);
                    ListedServerCommandManager.ServerSideIntermissionManager.AddMapToUsableMaps(sceneName);
                }
                //var a = MultiplayerOptions.Instance.GetOptionFromOptionType(MultiplayerOptions.OptionType.Map);

                if (!_scene.Equals(sceneName))
                {
                    MultiplayerIntermissionVotingManager.Instance.IsCultureVoteEnabled = false;
                    MultiplayerIntermissionVotingManager.Instance.IsMapVoteEnabled = false;


                    //var NativeOptions = ConfigManager.Instance.GetNativeOptionsCopy();
                    //SetNativeOption(OptionType.GameType, GameMode);
                    //GameModeSettings.SetNativeOption(OptionType.Map, mapCardVM.MapID);
                    // MultiplayerOptions.OptionType.Map.SetStrValue(sceneName);
                    // TaleWorlds.MountAndBlade.Module.StartMultiplayerGame("BattleLink", sceneName);

                    MBDebug.Print("BL - StartMultiplayerGame - reloading the correct scene", 0, DebugColor.Green);
                    // close task
                    // MultiplayerMissions.OpenCaptainMission(_scene);
                    string gamemodeDefault = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "config.properties")).Get("gamemode.default");
                    new MissionBasedMultiplayerGameMode(gamemodeDefault).StartMultiplayerGame(_scene);
                   // MultiplayerMissions.OpenCaptainMission(_scene);

                    //end mission and restart
                    taskEndAndSetNextMission = EndAndSetNextMission();
                    return;
                }


                BLReferentialHolder.currentBattleInitializerPending = BLReferentialHolder.nextBattleInitializerPending;

                //string gameTyp = MultiplayerOptions.OptionType.GameType.GetStrValue();
                //string ma = MultiplayerOptions.OptionType.Map.GetStrValue();

                //MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.GameType, MultiplayerOptionsAccessMode.CurrentMapOptions).GetValue(out string gameTyp2);
                //MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.Map, MultiplayerOptionsAccessMode.CurrentMapOptions).GetValue(out string map2);

                mir.PlayingInCampaignMode = false;//else crash

                BLReferentialHolder.battle = battleInitializer.battle;
                BLReferentialHolder.listTeam = battleInitializer.battle.Sides;
                BLReferentialHolder.listPlayer = battleInitializer.Players;

                //le faire dans l'export
                //basicCharacterObjects = basicCharacterObjects.GroupBy(x => x.StringId).Select(x => x.First()).ToList();
                //

                //Note banner are in StdAssets\FactionIcons\LargeIcons , so I cant generated them on the fly or change MultiplayerFactionBannerWidget.class not a priority
                // this.IconWidget.Sprite = this.Context.SpriteData.GetSprite("StdAssets\\FactionIcons\\LargeIcons\\" + this.FactionCode);

                //List<string> cultureBaseName = new List<string>()
                //{
                //    "empire", "vlandia", "sturgia", "battania", "aserai", "khuzait", "neutral_culture", "looters"
                //};
                //foreach (var side in BLReferentialHolder.listTeam)
                //{
                //    foreach (var team in side.Teams)
                //    {
                //        Party party = team.Parties[team.partyGeneralIndex];

                //        BasicCultureObject basicCultureObject;
                //        if (!cultureBaseName.Contains(side.Culture))
                //        {
                //            XmlDocument doc2 = new XmlDocument();
                //            XmlElement culture = doc2.CreateElement("Culture");

                //            culture.SetAttribute("id", team.Culture);
                //            culture.SetAttribute("name", team.Name);
                //            culture.SetAttribute("is_main_culture", "true");
                //            culture.SetAttribute("default_face_key", "000fa92e90004202aced5d976886573d5d679585a376fdd605877a7764b8987c00000000000007520000037f0000000f00000037049140010000000000000000");


                //            culture.SetAttribute("color", team.Color);
                //            culture.SetAttribute("color2", team.Color2);
                //            culture.SetAttribute("cloth_alternative_color1", team.Color);
                //            culture.SetAttribute("cloth_alternative_color2", team.Color2);

                //            culture.SetAttribute("banner_background_color1", team.Color);
                //            culture.SetAttribute("banner_background_color2", team.Color);
                //            culture.SetAttribute("banner_foreground_color1", team.Color2);
                //            culture.SetAttribute("banner_foreground_color2", team.Color2);


                //            culture.SetAttribute("faction_banner_key", team.FactionBannerKey);


                //            basicCultureObject = (BasicCultureObject)MBObjectManager.Instance.CreateObjectFromXmlNode(culture);
                //        }
                //        else
                //        {
                //            basicCultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(team.Culture);
                //        }


                //        //Create dummy character hero for culture
                //        var characterDummy = MBObjectManager.Instance.GetObject<BLCharacterObject>("bl_character_" + team.Culture);
                //        if (characterDummy != null)
                //        {
                //            MBObjectManager.Instance.UnregisterObject(characterDummy);
                //        }
                //        var heroClass = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>("bl_character_" + team.Culture + "_class_division");
                //        if (heroClass != null)
                //        {
                //            MBObjectManager.Instance.UnregisterObject(heroClass);
                //        }


                //        //var heroes = MultiplayerClassDivisions.GetMPHeroClasses(basicCultureObject);
                //        //if (heroes.IsEmpty())
                //        //{
                //        // Create dummy character and dummy class division/hero for the faction/culture
                //        var characterXml = BLBasicObject2Xml.createDummyCharacter(basicCultureObject);
                //        BLCharacterObject blCharacterObject = (BLCharacterObject)MBObjectManager.Instance.CreateObjectFromXmlNode(characterXml);
                //        BLReferentialHolder.basicCharacterObjects.Add(blCharacterObject);

                //        var classDivisionXml = BLBasicObject2Xml.createClassDivision(blCharacterObject);
                //        var classDivision = MBObjectManager.Instance.CreateObjectFromXmlNode(classDivisionXml);
                //        //}

                //    }
                //}

                XmlSerializer x = new XmlSerializer(typeof(List<BLCharacter>));
                XmlDocument doc = new XmlDocument();
                XPathNavigator nav = doc.CreateNavigator();
                XmlWriter writer = nav.AppendChild();
                writer.WriteStartDocument();
                //serializer.WriteObject(writer, new foo { bar = 42 });
                serializer.Serialize(writer, new MPBattleInitializer()
                {
                    BLCharacters = battleInitializer.BLCharacters
                });
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();

                foreach (XmlNode node in doc.DocumentElement.ChildNodes[1])
                {
                    var basicCharacterObject = (BLCharacterObject)MBObjectManager.Instance.CreateObjectFromXmlNode(node);
                    //MBObjectManager.Instance.FindRegisteredType<BLCharacterObject>(basicCharacterObject.StringId);
                    BLReferentialHolder.basicCharacterObjects.Add(basicCharacterObject);


                    XmlElement classDivision = BLBasicObject2Xml.createClassDivision(basicCharacterObject);
                    var classD = MBObjectManager.Instance.CreateObjectFromXmlNode(classDivision);

                }

                BLSendReferential2Client.setCharacters(BLReferentialHolder.basicCharacterObjects);


                //var a = MultiplayerOptions.OptionType.CultureTeam1.GetStrValue();
                //var a1 = MultiplayerOptions.OptionType.CultureTeam2.GetStrValue();

                if (BattleSideEnum.Attacker.ToString().Equals(BLReferentialHolder.listTeam[0].BattleSide))
                {
                    MultiplayerOptions.OptionType.CultureTeam1.SetValue(BLReferentialHolder.listTeam[0].Culture);
                    MultiplayerOptions.OptionType.CultureTeam2.SetValue(BLReferentialHolder.listTeam[1].Culture);
                }
                else
                {
                    MultiplayerOptions.OptionType.CultureTeam1.SetValue(BLReferentialHolder.listTeam[1].Culture);
                    MultiplayerOptions.OptionType.CultureTeam2.SetValue(BLReferentialHolder.listTeam[0].Culture);
                }
                //MultiplayerOptions.OptionType.CultureTeam1.SetValue("empire");
                //MultiplayerOptions.OptionType.CultureTeam2.SetValue("vlandia");

                //    MultiplayerOptions.OptionType.CultureTeam1.SetValue(listCultureMessage[0].id);
                //MultiplayerOptions.OptionType.CultureTeam2.SetValue(listCultureMessage[1].id);


                List<IUdpNetworkHandler> gameNetworkHandler = GameNetwork.NetworkHandlers;
                var referentialUdpNetwork = new BLSendReferential2Client();
                BLSendReferential2Client.setInitRecord(mir, BLReferentialHolder.battle);

                //foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
                //{
                //    if (!networkPeer.IsServerPeer)
                //    {
                //        GameNetwork.BeginModuleEventAsServer(networkPeer);
                //        GameNetwork.WriteMessage((GameNetworkMessage)new MultiplayerIntermissionMapItemAdded(mir.SceneName));
                //        GameNetwork.EndModuleEventAsServer();
                //    }
                //}

                //referentialUdpNetwork.setSiegeEngines(BLReferentialHolder.battle);

                if (!gameNetworkHandler.Contains(referentialUdpNetwork))
                {
                    gameNetworkHandler.Insert(1, new BLSendReferential2Client());
                }
                //referentialUdpNetwork.HandleNewMission();


                // based on CaptainGameMode
                InitializeMissionBehaviorsDelegate behaviors = (Mission) =>
                {
                    var listBehaviors = new List<MissionBehavior>{
                            BLMissionAgentSpawnLogic.newSpawnLogic(BLReferentialHolder.battle.mapEventType),
                            (MissionBehavior) MissionLobbyComponent.CreateBehavior(),
                            (MissionBehavior) new BLMissionMpDomination(),//MissionMultiplayerFlagDomination
                            //(MissionBehavior) new MissionMultiplayerFlagDomination(MultiplayerGameType.Captain),//MissionMultiplayerFlagDomination
                            (MissionBehavior) new MultiplayerRoundController(),//RB
                            (MissionBehavior) new BLMultiplayerWarmupComponent(),
                            //(MissionBehavior) new MultiplayerWarmupComponent(),
                            (MissionBehavior) new MissionMultiplayerGameModeFlagDominationClient(),
                            (MissionBehavior) new MultiplayerTimerComponent(),
                            (MissionBehavior) new BannerBearerLogic(),
                            (MissionBehavior) new SpawnComponent(new FlagDominationSpawnFrameBehavior(), new BLFlagDominationSpawningBehavior()),
                            //(MissionBehavior) new SpawnComponent(new FlagDominationSpawnFrameBehavior(), new FlagDominationSpawningBehavior()),
                            (MissionBehavior) new MissionLobbyEquipmentNetworkComponent(),
                            (MissionBehavior) new MultiplayerTeamSelectComponent(),
                            (MissionBehavior) new MissionHardBorderPlacer(),
                            (MissionBehavior) new MissionBoundaryPlacer(),
                            (MissionBehavior) new AgentVictoryLogic(),
                            (MissionBehavior) new AgentHumanAILogic(),
                            new BLGeneralsAndCaptainsAssignmentLogic(),
                            (MissionBehavior) new MissionAgentPanicHandler(),
                            (MissionBehavior) new MissionBoundaryCrossingHandler(),
                            (MissionBehavior) new MultiplayerPollComponent(),
                            (MissionBehavior) new MultiplayerAdminComponent(),
                            (MissionBehavior) new MultiplayerGameNotificationsComponent(),
                            (MissionBehavior) new MissionOptionsComponent(),
                            (MissionBehavior) new MissionScoreboardComponent((IScoreboardData) new CaptainScoreboardData()),
                            (MissionBehavior) new EquipmentControllerLeaveLogic(),
                            (MissionBehavior) new MultiplayerPreloadHelper(),
                            //new BLServerMissionLogic(),
                            //new BLAgentLogsLogic(),
                             //new BattleAgentLogic(),
                            // new RBDebugMissionLogic(),
                            //new BannerBearerLogic(),
                    };

                     if (BLReferentialHolder.battle.siege!=null)
                     {
                        // note: needs 100 men +5/sw for the attacker else TacticBreachWalls will stop using siege engine

                        var siege = BLReferentialHolder.battle.siege;
                        int siegeIndex = 0;
                        var wAtt = siege.siegeWeaponsAtt.Select(x =>
                        {
                            SiegeEngineType type = MBObjectManager.Instance.GetObject<SiegeEngineType>(x.type);
                            var s =  MissionSiegeWeapon.CreateCampaignWeapon(type, siegeIndex++, x.health, x.maxHealth);
                            return s;
                        }
                        ).ToList();
                        siegeIndex = 0;
                        var wDef = siege.siegeWeaponsDef.Select(x =>
                        {
                            SiegeEngineType type = MBObjectManager.Instance.GetObject<SiegeEngineType>(x.type);
                            return MissionSiegeWeapon.CreateCampaignWeapon(type, siegeIndex++, x.health, x.maxHealth);
                        }
                        ).ToList();

                        listBehaviors.Add(new AmmoSupplyLogic(new List<BattleSideEnum> { BattleSideEnum.Defender }));
                        listBehaviors.Add(new MissionSiegeEnginesLogic(wDef,wAtt));
                        bool isSallyOut = false;
                        bool isPlayerAttacker =true;
                        bool isReliefForceAttack = false;

                        //data.Type == DefaultSiegeEngineTypes.SiegeTower

                        bool hasAnySiegeTower = wAtt.Exists(s=>{
                            return DefaultSiegeEngineTypes.SiegeTower == s.Type;
                        });
                        listBehaviors.Add(new SiegeMissionPreparationHandler(isSallyOut, isReliefForceAttack, siege.wallHitPointsPercentages, hasAnySiegeTower));

                        listBehaviors.Add(new EquipmentControllerLeaveLogic());

                        listBehaviors.Add(new BLSiegeDeploymentHandler());
                        listBehaviors.Add(new BLSiegeDeploymentMissionController());


                    }
                    else
                    {
                        //listBehaviors.Add(new BLDeploymentMissionController());                        
                    }
                    return listBehaviors.ToArray();
                };

                MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - end", 0, DebugColor.Green);

                MissionState.OpenNew("BattleLink", mir, behaviors);

                MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - end2", 0, DebugColor.Green);
            }
            catch (Exception e)
            {
                MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - " + e.Message + "\n" + e.StackTrace, 0, DebugColor.Red);
            }


            //MissionState.OpenNew("BattleLink", new MissionInitializerRecord("battle_terrain_f"), behaviors);
        }

        private static Task? taskEndAndSetNextMission = null;
        // TODO private class
        public static async Task EndAndSetNextMission()
        {
            if (null != taskEndAndSetNextMission && !taskEndAndSetNextMission.IsCompleted && !taskEndAndSetNextMission.IsFaulted)
            {
                MBDebug.Print("BattleLinkGameMode - EndAndSetNextMission - already have EndAndSetNextMission", 0, DebugColor.Red);
                return;
            }
			//dont stop if we resolve a pending battle
            if (BLReferentialHolder.currentBattleInitializerPending)
            {
                MBDebug.Print("BattleLinkGameMode - EndAndSetNextMission - battle in pending state is processing -> no stop", 0, DebugColor.Red);
                return;
            }

            MBDebug.Print("BattleLinkGameMode - EndAndSetNextMission - start", 0, DebugColor.Green);

            //set the next map
            BLMissionMpDomination.setNextBLMap();

            // return;
            do
            {
                await Task.Delay(1000);
                // wait the wrong mission init and at least 1 player joined
            } while (Mission.Current == null || GameNetwork.NetworkPeers.Count == 0);//Mission.Current.Agents.Count==0 ||

            // terminate the mission
            MissionLobbyComponent lobbyComp = Mission.Current.GetMissionBehavior<MissionLobbyComponent>();
            lobbyComp.SetStateEndingAsServer();

            //set the next map
            BLMissionMpDomination.setNextBLMap();

            taskEndAndSetNextMission = null;

            MBDebug.Print("BattleLinkGameMode - EndAndSetNextMission - SetStateEndingAsServer ok", 0, DebugColor.Green);
            return;
        }

    }

}
