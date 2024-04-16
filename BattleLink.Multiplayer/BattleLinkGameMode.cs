using BattleLink.Client.Behavior;
using BattleLink.Common.Behavior;
using BattleLink.Handler;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Multiplayer
{
    public class BattleLinkGameMode : MissionBasedMultiplayerGameMode
    {

        public BattleLinkGameMode() : base("BattleLink")
        {
        }

        public override void StartMultiplayerGame(string _scene)
        {
            MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - " + _scene, 0, DebugColor.Green);

            //0,256 -> -512 2048
            CompressionMission.AgentOffsetCompressionInfo = new CompressionInfo.Integer(-512, 11);

            InitializeMissionBehaviorsDelegate behaviors = (Mission) => {
                var listBehaviors = new List<MissionBehavior>{

                          (MissionBehavior) MissionLobbyComponent.CreateBehavior(),
                          (MissionBehavior) new MultiplayerAchievementComponent(),
                          //(MissionBehavior) new BLMultiplayerWarmupComponent(),
                          (MissionBehavior) new MultiplayerWarmupComponent(),
                          (MissionBehavior) new MissionMultiplayerGameModeFlagDominationClient(),
                          (MissionBehavior) new MultiplayerRoundComponent(),
                          (MissionBehavior) new MultiplayerTimerComponent(),
                          (MissionBehavior) new MultiplayerMissionAgentVisualSpawnComponent(),
                          new MissionGauntletOptionsUIHandler(),
                          //(MissionBehavior) new ConsoleMatchStartEndHandler(),
                          (MissionBehavior) new MissionLobbyEquipmentNetworkComponent(),
                          (MissionBehavior) new MultiplayerTeamSelectComponent(),
                          (MissionBehavior) new MissionHardBorderPlacer(),
                          (MissionBehavior) new MissionBoundaryPlacer(),
                          (MissionBehavior) new AgentVictoryLogic(),
                          (MissionBehavior) new MissionBoundaryCrossingHandler(),
                          (MissionBehavior) new MultiplayerPollComponent(),
                          (MissionBehavior) new MultiplayerAdminComponent(),
                          (MissionBehavior) new MultiplayerGameNotificationsComponent(),
                          (MissionBehavior) new MissionOptionsComponent(),
                          (MissionBehavior) new MissionScoreboardComponent((IScoreboardData) new CaptainScoreboardData()),
                          (MissionBehavior) MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
                          (MissionBehavior) new EquipmentControllerLeaveLogic(),
                          (MissionBehavior) new MissionRecentPlayersComponent(),
                          (MissionBehavior) new MultiplayerPreloadHelper(),
                          new BLBehaviorClient(),
                          //,new RBDebugMissionLogic()
                          //,new RBDebugMissionBehavior()
                          new BLDeploymentMissionView()
                };
                //if (BLBLSiegeEnginesHandler.message != null)
                //{
                //    var siege = BLBLSiegeEnginesHandler.message;
                //    var wAtt = siege.siegeEngineAtt.Select(x =>
                //    {
                //        SiegeEngineType type = MBObjectManager.Instance.GetObject<SiegeEngineType>(x.type);
                //        return MissionSiegeWeapon.CreateCampaignWeapon(type, x.index, x.health, x.health);
                //    }
                //    ).ToList();
                //    var wDef = siege.siegeEngineDef.Select(x =>
                //    {
                //        SiegeEngineType type = MBObjectManager.Instance.GetObject<SiegeEngineType>(x.type);
                //        return MissionSiegeWeapon.CreateCampaignWeapon(type, x.index, x.health, x.health);
                //    }
                //    ).ToList();

                //    listBehaviors.Add(new MissionSiegeEnginesLogic(wDef, wAtt));
                //}
                return listBehaviors.ToArray();
            };


            MissionInitializerRecord mir;
            if (BLMissionInitializerRecordHandler.message!=null && _scene.Equals(BLMissionInitializerRecordHandler.message.SceneName))
            {
                var iTimeOfDay = ((int)BLMissionInitializerRecordHandler.message.TimeOfDay) % 24;
                var campaign = Campaign.Current;

                //FieldInfo fiCampaignMapTimeTracker = typeof(Campaign).GetField("\u003CMapTimeTracker\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //var mapTimeTraker = fiCampaignMapTimeTracker.GetValue(campaign);                
                //FieldInfo fiMapTimeTrakerTick = mapTimeTraker.GetType().GetField("_numTicks", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //fiMapTimeTrakerTick.SetValue(mapTimeTraker, BLMissionInitializerRecordHandler.message.campaignTimeTick);

                //FieldInfo fiCampaignUniqueGameId = typeof(Campaign).GetField("\u003CUniqueGameId\u003Ek__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                //fiCampaignUniqueGameId.SetValue(campaign, BLMissionInitializerRecordHandler.message.gameId);

                //Vec3 MainPartyLogicalPosition = new Vec3(BLMissionInitializerRecordHandler.message.partyPosLogicalX, BLMissionInitializerRecordHandler.message.partyPosLogicalY, BLMissionInitializerRecordHandler.message.partyPosLogicalZ);

                //var level = BLMissionInitializerRecordHandler.message.SceneLevels;
                //var decalAtlasGroup = (DecalAtlasGroup) BLMissionInitializerRecordHandler.message.DecalAtlasGroup;                
                //mir = SandBoxMissions.CreateSandBoxMissionInitializerRecord(_scene, level, decalAtlasGroup: decalAtlasGroup);

                mir = new MissionInitializerRecord(_scene);

                mir.SceneLevels = BLMissionInitializerRecordHandler.message.SceneLevels;
                mir.TimeOfDay = iTimeOfDay;

                mir.DisableDynamicPointlightShadows= BLMissionInitializerRecordHandler.message.DisableDynamicPointlightShadows;
                mir.DecalAtlasGroup= BLMissionInitializerRecordHandler.message.DecalAtlasGroup;


                mir.TerrainType = BLMissionInitializerRecordHandler.message.TerrainType;
                mir.NeedsRandomTerrain = BLMissionInitializerRecordHandler.message.NeedsRandomTerrain;
                mir.RandomTerrainSeed = BLMissionInitializerRecordHandler.message.RandomTerrainSeed;

                //mir.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MainPartyLogicalPosition);

                mir.AtmosphereOnCampaign.AtmosphereName = BLMissionInitializerRecordHandler.message.AtmosphereName;

                mir.AtmosphereOnCampaign.RainInfo.Density = BLMissionInitializerRecordHandler.message.RainInfoDensity;
                mir.AtmosphereOnCampaign.SnowInfo.Density = BLMissionInitializerRecordHandler.message.SnowInfoDensity;
                mir.AtmosphereOnCampaign.SkyInfo.Brightness = BLMissionInitializerRecordHandler.message.SkyInfoBrightness;
                
                mir.AtmosphereOnCampaign.TimeInfo.TimeOfDay = iTimeOfDay;
                mir.AtmosphereOnCampaign.TimeInfo.NightTimeFactor = BLMissionInitializerRecordHandler.message.TimeInfoNightTimeFactor;
                mir.AtmosphereOnCampaign.TimeInfo.DrynessFactor = BLMissionInitializerRecordHandler.message.TimeInfoDrynessFactor;
                mir.AtmosphereOnCampaign.TimeInfo.WinterTimeFactor = BLMissionInitializerRecordHandler.message.TimeInfoWinterTimeFactor;
                mir.AtmosphereOnCampaign.TimeInfo.Season = BLMissionInitializerRecordHandler.message.TimeInfoSeason;


                mir.AtmosphereOnCampaign.SunInfo.Altitude = BLMissionInitializerRecordHandler.message.SunInfoAltitude;
                mir.AtmosphereOnCampaign.SunInfo.Angle = BLMissionInitializerRecordHandler.message.SunInfoAngle;
                mir.AtmosphereOnCampaign.SunInfo.Color = BLMissionInitializerRecordHandler.message.SunInfoColor;
                mir.AtmosphereOnCampaign.SunInfo.Brightness = BLMissionInitializerRecordHandler.message.SunInfoBrightness;
                mir.AtmosphereOnCampaign.SunInfo.MaxBrightness = BLMissionInitializerRecordHandler.message.SunInfoMaxBrightness;
                mir.AtmosphereOnCampaign.SunInfo.Size = BLMissionInitializerRecordHandler.message.SunInfoSize;
                mir.AtmosphereOnCampaign.SunInfo.RayStrength = BLMissionInitializerRecordHandler.message.SunInfoRayStrength;

                mir.AtmosphereOnCampaign.AmbientInfo.RayleighConstant = BLMissionInitializerRecordHandler.message.AmbientInfoRayleighConstant;
                mir.AtmosphereOnCampaign.AmbientInfo.MieScatterStrength = BLMissionInitializerRecordHandler.message.AmbientInfoMieScatterStrength;
                mir.AtmosphereOnCampaign.AmbientInfo.AmbientColor = BLMissionInitializerRecordHandler.message.AmbientInfoAmbientColor;
                mir.AtmosphereOnCampaign.AmbientInfo.EnvironmentMultiplier = BLMissionInitializerRecordHandler.message.AmbientInfoEnvironmentMultiplier;

                mir.AtmosphereOnCampaign.PostProInfo.MinExposure = BLMissionInitializerRecordHandler.message.PostProInfoMinExposure;
                mir.AtmosphereOnCampaign.PostProInfo.BrightpassThreshold = BLMissionInitializerRecordHandler.message.PostProInfoBrightpassThreshold;
                mir.AtmosphereOnCampaign.PostProInfo.MaxExposure = BLMissionInitializerRecordHandler.message.PostProInfoMaxExposure;
                mir.AtmosphereOnCampaign.PostProInfo.MiddleGray = BLMissionInitializerRecordHandler.message.PostProInfoMiddleGray;

                mir.AtmosphereOnCampaign.FogInfo.Density = BLMissionInitializerRecordHandler.message.FogInfoDensity;
                mir.AtmosphereOnCampaign.FogInfo.Color = BLMissionInitializerRecordHandler.message.FogInfoColor;
                mir.AtmosphereOnCampaign.FogInfo.Falloff = BLMissionInitializerRecordHandler.message.FogInfoFalloff;

                mir.AtmosphereOnCampaign.InterpolatedAtmosphereName = BLMissionInitializerRecordHandler.message.InterpolatedAtmosphereName;

                // fix ???
                if (string.IsNullOrEmpty(mir.AtmosphereOnCampaign.AtmosphereName) && !string.IsNullOrEmpty(mir.AtmosphereOnCampaign.InterpolatedAtmosphereName))
                {
                    // BannerlordMission CreateAtmosphereInfoForMission ???
                    List<string> atmosphereNameByTime = new List<string>()
                    {
                        "TOD_01_00_SemiCloudy",//0
                        "TOD_01_00_SemiCloudy",
                        "TOD_01_00_SemiCloudy",
                        "TOD_06_00_SemiCloudy",
                        "TOD_06_00_SemiCloudy",
                        "TOD_06_00_SemiCloudy",
                        "TOD_06_00_SemiCloudy",//6
                        "TOD_06_00_SemiCloudy",
                        "TOD_06_00_SemiCloudy",
                        "TOD_06_00_SemiCloudy",
                        "TOD_12_00_SemiCloudy",
                        "TOD_12_00_SemiCloudy",
                        "TOD_12_00_SemiCloudy",//12
                        "TOD_12_00_SemiCloudy",
                        "TOD_04_00_SemiCloudy",
                        "TOD_04_00_SemiCloudy",//15
                        "TOD_04_00_SemiCloudy",
                        "TOD_03_00_SemiCloudy",
                        "TOD_03_00_SemiCloudy",//18
                        "TOD_03_00_SemiCloudy",
                        "TOD_03_00_SemiCloudy",
                        "TOD_01_00_SemiCloudy",
                        "TOD_01_00_SemiCloudy",//22
                        "TOD_01_00_SemiCloudy",
                    };
                    mir.AtmosphereOnCampaign.AtmosphereName = atmosphereNameByTime[iTimeOfDay];//mir.AtmosphereOnCampaign.InterpolatedAtmosphereName;
                }

            }
            else
            {
                mir = new MissionInitializerRecord(_scene);
            }

            MissionState.OpenNew("BattleLink", mir, behaviors);

        }
    }
}
