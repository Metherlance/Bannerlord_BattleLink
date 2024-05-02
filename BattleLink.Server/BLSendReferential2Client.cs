using BattleLink.Common.Behavior;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Server
{
    public sealed class BLSendReferential2Client : IUdpNetworkHandler
    {
        public static BLMissionInitializerRecordMessage initRecordMessage;
        //private static BLSiegeEngineMessage siegeEngineMessage;
        private static List<BLInitCharactersMessage> listCharacterMessage;
        //private static List<BLInitCultureMessage> listCultureMessage;
        private static List<BLInitTeamMessage> listTeamMessage;
        private static List<BLTeamCharactersMessage> listTeamCharacters;

        //static init singleton / check id already present



        public static void setCharacters(List<BLCharacterObject> characters) 
        {

            //prepare message of character
            listCharacterMessage = new List<BLInitCharactersMessage>();
            int index = 0;
            foreach (BLCharacterObject character in characters)
            {
                var eqRoasterNotCiv = character.AllEquipments.Where(e => !e.IsCivilian).ToList();

                listCharacterMessage.Add(new BLInitCharactersMessage()
                {
                    mbguid = character.Id.InternalValue,
                    id = character.StringId,
                    name = character.Name.ToString(),
                    isFemale = character.IsFemale,
                    defaultGroup = character.DefaultFormationGroup,

                    occupation = (int)character.Occupation,

                    bodyPropertiesValue = character.BodyPropertyRange.BodyPropertyMin,
                    bodyPropertiesValueMax = character.BodyPropertyRange.BodyPropertyMax,

                    indexDic = index,

                    skillRiding = character.GetSkillValue(DefaultSkills.Riding),
                    skillOneHanded = character.GetSkillValue(DefaultSkills.OneHanded),
                    skillTwoHanded = character.GetSkillValue(DefaultSkills.TwoHanded),
                    skillPolearm = character.GetSkillValue(DefaultSkills.Polearm),
                    skillCrossbow = character.GetSkillValue(DefaultSkills.Crossbow),
                    skillBow = character.GetSkillValue(DefaultSkills.Bow),
                    skillThrowing = character.GetSkillValue(DefaultSkills.Throwing),
                    skillAthletics = character.GetSkillValue(DefaultSkills.Athletics),

                    culture = character.Culture?.StringId,

                    equipmentRoasters = eqRoasterNotCiv

                });
                index += 1;
            }

            // no need to sync here, we will do it in HandleLateNewClientAfterLoadingFinished

            //foreach (var characterMessage in listCharacterMessage)
            //{
            //    GameNetwork.BeginBroadcastModuleEvent();
            //    GameNetwork.WriteMessage(characterMessage);
            //    GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients, (NetworkCommunicator)null);
            //}

        }

        public static void setTeamMessage()
        {
            listTeamMessage = new List<BLInitTeamMessage>();
            foreach (var side in BLReferentialHolder.listTeam)
            {
                foreach (var team in side.Teams)
                {
                    Party party = team.getPartyGeneral();

                    listTeamMessage.Add(new BLInitTeamMessage()
                    {
                        id = team.missionTeamsIndex,
                        name = party.Name,
                        //Color = Convert.ToUInt32(party.Color, 16),
                        //Color2 = Convert.ToUInt32(party.Color2, 16),
                        FactionBannerKey = party.FactionBannerKey,
                    });
                }
            }

            foreach (var teamMessage in listTeamMessage)
            {            
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(teamMessage);
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients, (NetworkCommunicator)null);
            }

        }

        public static void setTeamCharactersMessage()
        {
            listTeamCharacters = new List<BLTeamCharactersMessage>();
            foreach (var side in BLReferentialHolder.listTeam)
            {
                foreach (var team in side.Teams)
                {
                    Dictionary<string, int> dic = new Dictionary<string, int>();

                    foreach (var party in team.Parties)
                    {
                        foreach (var troop in party.Troops)
                        {
                            if(dic.TryGetValue(troop.Id, out var nb))
                            {
                                dic.Add(troop.Id, nb+troop.Number);
                            }
                            else
                            {
                                dic.Add(troop.Id, troop.Number);
                            }
                        }
                    }

                    BLTeamCharactersMessage teamMes = new BLTeamCharactersMessage()
                    {
                        teamId = BLReferentialHolder.getTeamBy(team).TeamIndex,
                        characterObjects = new List<BLCharacterObject>(),
                    };
                    foreach (KeyValuePair<string, int> entry in dic)
                    {
                        BLCharacterObject character = MBObjectManager.Instance.GetObject<BLCharacterObject>(entry.Key);
                        teamMes.characterObjects.Add(character);
                    }
                    listTeamCharacters.Add(teamMes);

                }
            }

            foreach (var teamMessage in listTeamCharacters)
            {
                GameNetwork.BeginBroadcastModuleEvent();
                GameNetwork.WriteMessage(teamMessage);
                GameNetwork.EndBroadcastModuleEvent(GameNetwork.EventBroadcastFlags.IncludeUnsynchronizedClients, (NetworkCommunicator)null);
            }

        }

        public static void setInitRecord(MissionInitializerRecord mir, Battle battle)
        {
            var timeOfDay = ((double)battle.campaignTimeTick / 36000000.0) % 24d;

            initRecordMessage = new BLMissionInitializerRecordMessage()
            {
                SceneName = mir.SceneName,
                SceneLevels = mir.SceneLevels,
                TimeOfDay = mir.TimeOfDay,

                DisableDynamicPointlightShadows = mir.DisableDynamicPointlightShadows,
                DecalAtlasGroup = mir.DecalAtlasGroup,
                RainInfoDensity = mir.AtmosphereOnCampaign.RainInfo.Density,
                SnowInfoDensity = mir.AtmosphereOnCampaign.SnowInfo.Density,
                SkyInfoBrightness = mir.AtmosphereOnCampaign.SkyInfo.Brightness,
                
                TimeInfoTimeOfDay = mir.AtmosphereOnCampaign.TimeInfo.TimeOfDay,
                TimeInfoNightTimeFactor = mir.AtmosphereOnCampaign.TimeInfo.NightTimeFactor,
                TimeInfoDrynessFactor = mir.AtmosphereOnCampaign.TimeInfo.DrynessFactor,
                TimeInfoWinterTimeFactor = mir.AtmosphereOnCampaign.TimeInfo.WinterTimeFactor,
                TimeInfoSeason = mir.AtmosphereOnCampaign.TimeInfo.Season,

                TerrainType = mir.TerrainType,
                NeedsRandomTerrain = mir.NeedsRandomTerrain,
                RandomTerrainSeed = mir.RandomTerrainSeed,
                AtmosphereName = mir.AtmosphereOnCampaign.AtmosphereName,

                SunInfoAltitude = mir.AtmosphereOnCampaign.SunInfo.Altitude,
                SunInfoAngle = mir.AtmosphereOnCampaign.SunInfo.Angle,
                SunInfoColor = mir.AtmosphereOnCampaign.SunInfo.Color,
                SunInfoBrightness = mir.AtmosphereOnCampaign.SunInfo.Brightness,
                SunInfoMaxBrightness = mir.AtmosphereOnCampaign.SunInfo.MaxBrightness,
                SunInfoSize = mir.AtmosphereOnCampaign.SunInfo.Size,
                SunInfoRayStrength = mir.AtmosphereOnCampaign.SunInfo.RayStrength,

                AmbientInfoEnvironmentMultiplier = mir.AtmosphereOnCampaign.AmbientInfo.EnvironmentMultiplier,
                AmbientInfoAmbientColor = mir.AtmosphereOnCampaign.AmbientInfo.AmbientColor,
                AmbientInfoMieScatterStrength = mir.AtmosphereOnCampaign.AmbientInfo.MieScatterStrength,
                AmbientInfoRayleighConstant = mir.AtmosphereOnCampaign.AmbientInfo.RayleighConstant,

                PostProInfoMinExposure = mir.AtmosphereOnCampaign.PostProInfo.MinExposure,
                PostProInfoMaxExposure = mir.AtmosphereOnCampaign.PostProInfo.MaxExposure,
                PostProInfoBrightpassThreshold = mir.AtmosphereOnCampaign.PostProInfo.BrightpassThreshold,
                PostProInfoMiddleGray = mir.AtmosphereOnCampaign.PostProInfo.MiddleGray,

                FogInfoDensity = mir.AtmosphereOnCampaign.FogInfo.Density,
                FogInfoColor = mir.AtmosphereOnCampaign.FogInfo.Color,
                FogInfoFalloff = mir.AtmosphereOnCampaign.FogInfo.Falloff,

                InterpolatedAtmosphereName = mir.AtmosphereOnCampaign.InterpolatedAtmosphereName,

                campaignTimeTick = battle.campaignTimeTick,
                gameId = battle.gameId,
                partyPosLogicalX= battle.partyPosLogical.x,
                partyPosLogicalY = battle.partyPosLogical.y,
                partyPosLogicalZ = battle.partyPosLogical.z,

            };
        }

        //public void setSiegeEngines(Battle battle)
        //{
        //    var siege = battle.siege;
        //    if (siege!=null)
        //    {
        //        List<SiegeEngine> siegeEnginesAtt = siege.siegeWeaponsAtt.Select(s=>new SiegeEngine()
        //        {
        //            type = s.type,
        //            index = s.index,
        //            health = s.health,
        //        }).ToList();
        //        List<SiegeEngine> siegeEnginesDef = siege.siegeWeaponsDef.Select(s => new SiegeEngine()
        //        {
        //            type = s.type,
        //            index = s.index,
        //            health = s.health,
        //        }).ToList();

        //        siegeEngineMessage = new BLSiegeEngineMessage()
        //        {
        //            wallHitPointsPercentages = siege.wallHitPointsPercentages,
        //            siegeEngineAtt = siegeEnginesAtt,
        //            siegeEngineDef = siegeEnginesDef,
        //        };
        //    }
        //}


        public void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            //GameNetwork.BeginModuleEventAsServer(networkPeer);
            //GameNetwork.WriteMessage(initRecordMessage);
            //GameNetwork.EndModuleEventAsServer();

            //MBDebug.Print("BLSendReferential2Client - HandleEarlyNewClientAfterLoadingFinished - " + networkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
        {
        }

        public void HandleLateNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            GameNetwork.BeginModuleEventAsServer(networkPeer);
            GameNetwork.WriteMessage(initRecordMessage);
            GameNetwork.EndModuleEventAsServer();

            //if (siegeEngineMessage!=null)
            //{
            //    GameNetwork.BeginModuleEventAsServer(networkPeer);
            //    GameNetwork.WriteMessage(siegeEngineMessage);
            //    GameNetwork.EndModuleEventAsServer();
            //}


            //do before BaseNetworkComponent (init team)
            //foreach (var cultureMessage in listCultureMessage)
            //{
            //    GameNetwork.BeginModuleEventAsServer(networkPeer);
            //    GameNetwork.WriteMessage(cultureMessage);
            //    GameNetwork.EndModuleEventAsServer();
            //}

            // not necessary before BaseNetworkComponent
            foreach (var characterMessage in listCharacterMessage)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(characterMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            foreach (var teamMessage in listTeamMessage)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(teamMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            foreach (var teamCharactersMessage in listTeamCharacters)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(teamCharactersMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            MBDebug.Print("BLSendReferential2Client - HandleLateNewClientAfterLoadingFinished - " + networkPeer.UserName + " - end ", 0, DebugColor.Green);
        }



        public void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
           MBDebug.Print("BLSendReferential2Client - HandleLateNewClientAfterSynchronized - " + networkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
           MBDebug.Print("BLSendReferential2Client - HandleNewClientAfterLoadingFinished - " + networkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
           MBDebug.Print("BLSendReferential2Client - HandleNewClientAfterSynchronized - " + networkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandleNewClientConnect(PlayerConnectionInfo clientConnectionInfo)
        {
            GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
            GameNetwork.WriteMessage(initRecordMessage);
            GameNetwork.EndModuleEventAsServer();

            //if (siegeEngineMessage != null)
            //{
            //    GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
            //    GameNetwork.WriteMessage(siegeEngineMessage);
            //    GameNetwork.EndModuleEventAsServer();
            //}

            //do before BaseNetworkComponent (init team)
            //foreach (var cultureMessage in listCultureMessage)
            //{
            //    GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
            //    GameNetwork.WriteMessage(cultureMessage);
            //    GameNetwork.EndModuleEventAsServer();
            //}

            // not necessary before BaseNetworkComponent
            foreach (var characterMessage in listCharacterMessage)
            {
                GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
                GameNetwork.WriteMessage(characterMessage);
                GameNetwork.EndModuleEventAsServer();
            }


            foreach (var teamMessage in listTeamMessage)
            {
                GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
                GameNetwork.WriteMessage(teamMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            foreach (var teamCharactersMessage in listTeamCharacters)
            {
                GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
                GameNetwork.WriteMessage(teamCharactersMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            MBDebug.Print("BLSendReferential2Client - HandleNewMission - " + clientConnectionInfo.NetworkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandlePlayerDisconnect(NetworkCommunicator networkPeer)
        {
        }

        public void OnDisconnectedFromServer()
        {
        }

        public void OnEveryoneUnSynchronized()
        {
            MBDebug.Print("BLSendReferential2Client - OnEveryoneUnSynchronized - end ", 0, DebugColor.Green);
        }

        public void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
        }

        public void OnUdpNetworkHandlerClose()
        {
        }

        public void OnUdpNetworkHandlerTick(float dt)
        {
            //MBDebug.Print("BLSendReferential2Client - OnUdpNetworkHandlerTick - end ", 0, DebugColor.Green);
        }


    }
}
