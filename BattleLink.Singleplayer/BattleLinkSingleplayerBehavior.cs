using BattleLink.Common;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Utils;
using BattleLink.Singleplayer.Patch;
using Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.CampaignSystem.GameMenus.GameMenuOption;
using static TaleWorlds.CampaignSystem.MapEvents.MapEvent;
using SiegeWeapon = BattleLink.Common.DtoSpSv.SiegeWeapon;
using SideDto = BattleLink.Common.DtoSpSv.SideDto;

namespace BattleLink.Singleplayer
{

    public class BattleLinkSingleplayerBehavior : CampaignBehaviorBase
    {
        private static readonly TextObject EnemyNotAttackableTooltip = GameTexts.FindText("str_enemy_not_attackable_tooltip");
        private MenuCallbackArgs btnLoadResult = null;

        public override void RegisterEvents()
        {
            //CampaignEvents.OnWorkshopChangedEvent.AddNonSerializedListener(this, OnWorkshopChangedEvent);
            //CampaignEvents.DailyTickTownEvent.AddNonSerializedListener(this, DailyTick);
            //CampaignEvents.LocationCharactersAreReadyToSpawnEvent.AddNonSerializedListener(this, LocationCharactersAreReadyToSpawn);
            //CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
            CampaignEvents.OnSessionLaunchedEvent.AddNonSerializedListener(this, OnSessionLaunched);
        }

        public override void SyncData(IDataStore dataStore)
        {
            //dataStore.SyncData("artisanWorkshops", ref artisanWorkshops);
        }

        private void OnSessionLaunched(CampaignGameStarter starter)
        {
            //_artisanBeer = MBObjectManager.Instance.GetObject<ItemObject>("artisan_beer");
            //_artisanBrewer = MBObjectManager.Instance.GetObject<CharacterObject>("artisan_brewer");
            //AddDialogs(starter);
            AddGameMenus(starter);
        }

        private void AddGameMenus(CampaignGameStarter starter)
        {
            //for (int i = 0; i < 40; i++)
            //{
            //starter.AddGameMenuOption("encounter", "attackMP", "{=attackMP}Attack! (MP)",
            //    game_menu_encounter_attack_on_condition,
            //    game_menu_encounter_attack_on_consequence,
            //    false, 2);

            MethodInfo fieldAttackCondition = typeof(EncounterGameMenuBehavior).GetMethod("game_menu_encounter_order_attack_on_condition", BindingFlags.NonPublic | BindingFlags.Instance);
            //MethodInfo fieldConsequence2 = typeof(EncounterGameMenuBehavior).GetMethod("menu_siege_strategies_lead_assault_on_consequence", BindingFlags.NonPublic | BindingFlags.Static);

            MenuCallbackArgs args = null;
            GameMenuOption.OnConditionDelegate action = (GameMenuOption.OnConditionDelegate)Delegate.CreateDelegate(typeof(GameMenuOption.OnConditionDelegate), args, fieldAttackCondition);


            //fieldAttackCondition.CreateDelegate(typeof(GameMenuOption.OnConditionDelegate));
            //new GameMenuOption.OnConditionDelegate(fieldAttackCondition.);

            //typeof(EncounterGameMenuBehavior).De


            //GameMenuOption.OnConditionDelegate game_menu_encounter_order_attack_on_condition = (GameMenuOption.OnConditionDelegate)fieldAttackCondition.CreateDelegate(typeof(GameMenuOption.OnConditionDelegate));
            starter.AddGameMenuOption("encounter", "attackMP", "{=attackMP}Attack! (MP)",
                action,
                game_menu_encounter_order_attack_mp_on_consequence,
                false, 3);

            starter.AddGameMenuOption("encounter", "attackMPLoadResult", "{=attackMPLoadResult}Load Result! (MP)",
                game_menu_load_result_encounter_on_condition,
                game_menu_load_result_encounter_on_consequence,
                false, 4);

            // }

            //game_menu_siege_strategies_order_assault_on_condition
            //game_menu_siege_strategies_lead_assault_on_condition
            //menu_siege_strategies_lead_assault_on_consequence

            // looks SiegeEventCampaignBehavior


            //var a = new GameMenuOption.OnConditionDelegate((object) null, __methodptr(game_menu_siege_strategies_lead_assault_on_condition)),
            //FieldInfo fieldCondition = typeof(SiegeEventCampaignBehavior).GetField("game_menu_siege_strategies_order_assault_on_condition", BindingFlags.NonPublic | BindingFlags.Static);
            //FieldInfo fieldConsequence = typeof(SiegeEventCampaignBehavior).GetField("menu_siege_strategies_lead_assault_on_consequence", BindingFlags.NonPublic | BindingFlags.Static);
            //MethodInfo methodMissionBehaviorOnGetAgentState = typeof(MissionBehavior).GetMethod("OnGetAgentState", BindingFlags.NonPublic | BindingFlags.Instance);
            //GameMenuOption.OnConditionDelegate condition;
            //GameMenuOption.OnConsequenceDelegate consequence;

            MethodInfo fieldCondition = typeof(SiegeEventCampaignBehavior).GetMethod("game_menu_siege_strategies_order_assault_on_condition", BindingFlags.NonPublic | BindingFlags.Static);
            //MethodInfo fieldConsequence = typeof(SiegeEventCampaignBehavior).GetMethod("menu_siege_strategies_lead_assault_on_consequence", BindingFlags.NonPublic | BindingFlags.Static);

            GameMenuOption.OnConditionDelegate game_menu_siege_strategies_order_assault_on_condition = (GameMenuOption.OnConditionDelegate)fieldCondition.CreateDelegate(typeof(GameMenuOption.OnConditionDelegate));
            //GameMenuOption.OnConditionDelegate condition2 = (GameMenuOption.OnConditionDelegate)Delegate.CreateDelegate(typeof(GameMenuOption.OnConditionDelegate), fieldCondition);

            starter.AddGameMenuOption("menu_siege_strategies", "attackMP", "{=attackMP}Attack! (MP)",
               game_menu_siege_strategies_order_assault_on_condition,
               menu_siege_strategies_lead_assault_on_consequence,
               false, 2);
            //starter.AddGameMenuOption("menu_siege_strategies", "attackMPLoadResult", "{=attackMPLoadResult}Load Result! (MP)",
            //    game_menu_load_result_encounter_on_condition,
            //    game_menu_load_result_encounter_on_consequence,
            //    false, 3);

            //starter.AddGameMenuOption("encounter", "attack", "{=o1pZHZOF}{ATTACK_TEXT}!", new GameMenuOption.OnConditionDelegate(this.game_menu_encounter_attack_on_condition), new GameMenuOption.OnConsequenceDelegate(this.game_menu_encounter_attack_on_consequence));
        }

        private void CheckEnemyAttackableHonorably(MenuCallbackArgs args)
        {
            if ((MobileParty.MainParty.Army == null || MobileParty.MainParty.Army.LeaderParty == MobileParty.MainParty) && !PlayerEncounter.PlayerIsDefender)
            {
                IFaction mapFaction = PlayerEncounter.EncounteredParty.MapFaction;
                if (mapFaction != null && mapFaction.NotAttackableByPlayerUntilTime.IsFuture)
                {
                    args.IsEnabled = false;
                    args.Tooltip = EnemyNotAttackableTooltip;
                }
            }
        }

        private bool game_menu_load_result_encounter_on_condition(MenuCallbackArgs args)
        {
            // check if result was send
            bool show = MenuHelper.EncounterAttackCondition(args);
            args.optionLeaveType = LeaveType.WaitQuest;
            btnLoadResult = args;
            if (show)
            {
                //MB2_BL_2185888_res.json
                //string json = JsonUtils.SerializeObject(battleResult, 10);
                // private string GetDirectoryFullPath(PlatformDirectoryPath directoryPath);
                //string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MB2_BL_2185888_res.json"); 
                string pathResult = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "Battles", "BL_MPBattle_" + getCampainTimeSec() + "_Result.xml");
                string pathInitializer = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "Battles", "BL_MPBattle_" + getCampainTimeSec() + "_Initializer.xml");
                args.IsEnabled = File.Exists(pathInitializer) || File.Exists(pathResult);
                if (!args.IsEnabled)
                {
                    args.Tooltip = new TextObject("{=UL8za0AO1818}Battle json not send.");
                }
            }
            return show;
        }

        private async void game_menu_load_result_encounter_on_consequence(MenuCallbackArgs args)
        {
            InformationManager.DisplayMessage(new InformationMessage("game_menu_load_result_encounter_on_consequence - click"));
            loadResult();
        }

        private static async void loadResult()
        {
            string filenameResult = "BL_MPBattle_" + getCampainTimeSec() + "_Result.csbin";
            string pathResult = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "Battles", filenameResult);

            try
            {
                //retreive result (local  web)
                //if not found msg);
                List<IBLLog> mpBattleLogs = null;
                if (File.Exists(pathResult))
                {
                    mpBattleLogs = BattleResultLogs.Deserialize<List<IBLLog>>(pathResult);
                }

                //search on server
                if (mpBattleLogs == null)
                {
                    PropertiesUtils prop = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "config.properties"));
                    string server = prop.Get("server.ip.port");

                    using (var client = new HttpClient())
                    {
                        client.DefaultRequestHeaders.Add("File-Name", filenameResult);

                        // Always catch network exceptions for async methods
                        try
                        {
                            // string result = await client.GetStringAsync(server + "/battlelink/api/battles/");
                            var result = await client.GetByteArrayAsync(server + "/battlelink/api/battles/");

                            File.WriteAllBytes(pathResult, result);

                            mpBattleLogs = BattleResultLogs.Deserialize<List<IBLLog>>(pathResult);
                        }
                        catch (Exception ex)
                        {
                            // Details in ex.Message and ex.HResult.
                            InformationManager.DisplayMessage(new InformationMessage("Error: " + ex.Message, new Color(1f, 0f, 0f)));
                        }
                    }

                }

                if (mpBattleLogs == null)
                {
                    InformationManager.DisplayMessage(new InformationMessage("File not found " + filenameResult, new Color(1f, 0f, 0f)));
                    return;
                }

                //Execute the battle
                BattleLinkLogReader.ExecBattle(mpBattleLogs);

            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("Error: " + e.Message, new Color(1f, 0f, 0f)));
            }
        }


        public static List<T> enumerate2List<T>(IEnumerator<T> e)
        {
            var list = new List<T>();
            while (e.MoveNext())
            {
                list.Add(e.Current);
            }
            return list;
        }

        //private static readonly TextObject _waitSiegeEquipmentsText = new TextObject("{=bCuxzp1N}You need to wait for the siege equipment to be prepared.", (Dictionary<string, object>)null);
        //private static readonly TextObject _woundedAssaultText = new TextObject("{=gzYuWR28}You are wounded, and in no condition to lead an assault.", (Dictionary<string, object>)null);
        //private static readonly TextObject _noCommandText = new TextObject("{=1Hd19nq5}You are not in command of this siege.", (Dictionary<string, object>)null);
        //private static bool game_menu_siege_strategies_lead_assault_on_condition(MenuCallbackArgs args)
        //{
        //    args.optionLeaveType = GameMenuOption.LeaveType.LeadAssault;
        //    if (MobileParty.MainParty.BesiegedSettlement == null)
        //        return false;
        //    if (Campaign.Current.Models.EncounterModel.GetLeaderOfSiegeEvent(MobileParty.MainParty.BesiegedSettlement.SiegeEvent, PlayerSiege.PlayerSide) == Hero.MainHero)
        //    {
        //        Settlement settlement = PlayerEncounter.EncounteredParty != null ? PlayerEncounter.EncounteredParty.Settlement : PlayerSiege.PlayerSiegeEvent.BesiegedSettlement;
        //        if (PlayerSiege.PlayerSide == BattleSideEnum.Attacker && !settlement.SiegeEvent.BesiegerCamp.IsPreparationComplete)
        //        {
        //            args.IsEnabled = false;


        //            args.Tooltip = _waitSiegeEquipmentsText;
        //        }
        //        else if (Hero.MainHero.IsWounded)
        //        {
        //            args.IsEnabled = false;
        //            args.Tooltip = _woundedAssaultText;
        //        }
        //    }
        //    else
        //    {
        //        args.IsEnabled = false;
        //        args.Tooltip = _noCommandText;
        //    }
        //    return true;
        //}

        private void menu_siege_strategies_lead_assault_on_consequence(MenuCallbackArgs args)
        {
            MissionStatePatch.CampaignTimeSecBattleMP = getCampainTimeSec();

            MethodInfo fieldConsequence = typeof(SiegeEventCampaignBehavior).GetMethod("menu_siege_strategies_lead_assault_on_consequence", BindingFlags.NonPublic | BindingFlags.Static);
            fieldConsequence.Invoke(null, new object[] { args });

            //GameMenu.SwitchToMenu("encounter"); 
            // GameMenu.SwitchToMenu("assault_town");

            btnLoadResult.MenuContext.Refresh();
        }

        private void game_menu_encounter_order_attack_mp_on_consequence(MenuCallbackArgs args)
        {
            MissionStatePatch.CampaignTimeSecBattleMP = getCampainTimeSec();


            //Campaign.Current.GameMenuManager.AddGameMenuOption()
            EncounterGameMenuBehavior egmb = Campaign.Current.GetCampaignBehavior<EncounterGameMenuBehavior>();
            
            MethodInfo fieldConsequence = typeof(EncounterGameMenuBehavior).GetMethod("game_menu_encounter_attack_on_consequence", BindingFlags.NonPublic | BindingFlags.Instance);
            //MethodInfo fieldConsequence = typeof(EncounterGameMenuBehavior).GetMethod("game_menu_encounter_order_attack_on_consequence", BindingFlags.NonPublic | BindingFlags.Instance);
            fieldConsequence.Invoke(egmb, new object[] { args });


            btnLoadResult.MenuContext.Refresh();
        }

        //private bool game_menu_encounter_attack_on_condition(MenuCallbackArgs args)
        //{
        //    this.CheckEnemyAttackableHonorably(args);
        //    bool show = MenuHelper.EncounterAttackCondition(args);
        //    args.optionLeaveType = LeaveType.SiegeAmbush; //(LeaveType) id++;
        //    if (show)
        //    {
        //        if (PlayerEncounter.Current == null)
        //        {
        //            return false;
        //        }

        //        MapEvent battle = PlayerEncounter.Battle;
        //        Settlement mapEventSettlement = MobileParty.MainParty.MapEvent.MapEventSettlement;
        //        //if (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside)
        //        //{
        //        //    args.Tooltip = new TextObject("{=}CaravanBattleMission not implemented.");
        //        //    return false;
        //        //}

        //        //if (PlayerEncounter.Current == null
        //        //   || (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside))
        //        //{
        //        //    args.Tooltip = new TextObject("{=}Raid SiegeAmbush Siege Village Hideout mission not implemented.");
        //        //    return false;
        //        //}

        //        //bool flag = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsCaravan);
        //        //bool flag2 = MapEvent.PlayerMapEvent.MapEventSettlement == null && MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsVillager);
        //        //if (flag || flag2)
        //        //{
        //        //    args.Tooltip = new TextObject("{=}CaravanBattleMission not implemented.");
        //        //    return false;
        //        //}

        //    }

        //    return show;
        //}

        static int id = 0;
        //private async void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
        //{
        //    InformationManager.DisplayMessage(new InformationMessage("game_menu_encounter_attack_on_consequence - click"));

        //    MapEvent battle = PlayerEncounter.Battle;
        //    PartyBase leaderParty = battle.GetLeaderParty(PartyBase.MainParty.OpponentSide);
        //    BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, leaderParty);
        //    if (PlayerEncounter.Current == null)
        //    {
        //        InformationManager.DisplayMessage(new InformationMessage("Not implemented"));
        //        return;
        //    }

        //    Settlement mapEventSettlement = MobileParty.MainParty.MapEvent.MapEventSettlement;
        //    bool isSallyOut = mapEventSettlement != null && battle.IsSallyOut;
        //    bool isSiege = mapEventSettlement != null && battle.IsSiegeAssault && !isSallyOut;
        //    //if (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside)
        //    //{
        //    //    InformationManager.DisplayMessage(new InformationMessage("Siege Not implemented"));
        //    //    return;
        //    //}

        //    bool isCaravan = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsCaravan);
        //    bool isVillager = MapEvent.PlayerMapEvent.MapEventSettlement == null && MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsVillager);
        //    //if (isCaravan || isVillager)
        //    //{
        //    //    InformationManager.DisplayMessage(new InformationMessage("Caravan not implemented"));
        //    //    return;
        //    //    //CampaignMission.OpenCaravanBattleMission(rec, flag);
        //    //}


        //    MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
        //    string battleSceneForMapPatch;
        //    if (isSiege || isSallyOut)
        //    {
        //        //string sceneLevels = Campaign.Current.Models.LocationModel.GetUpgradeLevelTag(3) + " siege";
        //        //rec.SceneLevels = sceneLevels;
        //        int wallLevel = mapEventSettlement.Town.GetWallLevel();
        //        battleSceneForMapPatch = mapEventSettlement.LocationComplex.GetLocationWithId("center").GetSceneName(wallLevel);
        //    }
        //    else
        //    {
        //        battleSceneForMapPatch = PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition);
        //    }




        //    //MissionInitializerRecord rec = new MissionInitializerRecord(battleSceneForMapPatch);
        //    //rec.TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
        //    //rec.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
        //    //rec.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
        //    //rec.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
        //    //rec.NeedsRandomTerrain = false;
        //    //rec.PlayingInCampaignMode = true;
        //    //rec.RandomTerrainSeed = MBRandom.RandomInt(10000);
        //    ////rec.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.GetLogicalPosition());
        //    //rec.SceneHasMapPatch = true;
        //    ////rec.DecalAtlasGroup = 2;
        //    //rec.PatchCoordinates = mapPatchAtPosition.normalizedCoordinates;
        //    //rec.PatchEncounterDir = (battle.AttackerSide.LeaderParty.Position2D - battle.DefenderSide.LeaderParty.Position2D).Normalized();
        //    //float timeOfDay = Campaign.CurrentTime % 24f;
        //    //if (Campaign.Current != null)
        //    //{
        //    //    rec.TimeOfDay = timeOfDay;
        //    //}
        //    string sceneLevels ="";
        //    var decalAtlas = DecalAtlasGroup.Battle;
        //    if (mapEventSettlement!=null)
        //    {
        //        decalAtlas = DecalAtlasGroup.Town;
        //    }
        //    if (isSallyOut || isSiege)
        //    {
        //        int wallLevel = mapEventSettlement.Town.GetWallLevel();
        //        sceneLevels = Campaign.Current.Models.LocationModel.GetUpgradeLevelTag(wallLevel) + " siege";
        //        //rec.SceneLevels = sceneLevels;
        //        //rec.DecalAtlasGroup = (int)DecalAtlasGroup.Town;
        //    }
        //    MissionInitializerRecord rec = SandBoxMissions.CreateSandBoxMissionInitializerRecord(battleSceneForMapPatch, sceneLevels, decalAtlasGroup: decalAtlas);

        //    createAndSendFile(rec);

        //    //refresh
        //    btnLoadResult.MenuContext.Refresh();

        //}

        static public async void createAndSendFile(MissionInitializerRecord rec, string missionName="", InitializeMissionBehaviorsDelegate behaviorsDelegate=null)
        {
            //MapEvent battle = PlayerEncounter.Battle;
            //MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
            //rec.PatchCoordinates = mapPatchAtPosition.normalizedCoordinates;
            //rec.PatchEncounterDir = (battle.AttackerSide.LeaderParty.Position2D - battle.DefenderSide.LeaderParty.Position2D).Normalized();
            //rec.TimeOfDay = Campaign.CurrentTime % 24f;



            //"Battle"
            //MapEventSide att = MapEvent.PlayerMapEvent.GetMapEventSide(BattleSideEnum.Attacker);

            //MobileParty.MainParty.MapEvent
            MapEvent mapEvent = MobileParty.MainParty.MapEvent;

            //MapEventSide mesDef = mapEvent.GetMapEventSide(BattleSideEnum.Defender);// to remove
            //MapEventSide mesAtt = mapEvent.GetMapEventSide(BattleSideEnum.Attacker);


            var players = new List<Player>();
            foreach (var row in File.ReadAllLines(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "players.txt")))
            {
                var pair = row.Split('=');
                var pairToopParty = pair[1].Split('|');
                players.Add(new Player()
                {
                    UserName = pair[0],
                    TroopId = pairToopParty[0],
                    PartyIndex = int.Parse(pairToopParty[1]),
                });
            }

            var characters = new List<BLCharacter>();

            var Sides = new List<SideDto>()
            {
                createSideDto(mapEvent.DefenderSide),
                createSideDto(mapEvent.AttackerSide),
            };

            Siege siege = null;
            if (BattleTypes.Siege == mapEvent.EventType)
            {
                SiegeEvent siegeEvent = PlayerSiege.PlayerSiegeEvent;
                //Settlement besiegedSettlement = PlayerSiege.BesiegedSettlement;

                ////besiegedSettlement.SiegeEngines
                //foreach (var siegeEngine in besiegedSettlement.SiegeEngineMissiles)
                //{

                //}
                //MBReadOnlyList<SiegeEvent.SiegeEngineConstructionProgress> deployedSiegeEngines = besiegedSettlement.SiegeEngines.DeployedSiegeEngines;
                //for (int index = 0; index < deployedSiegeEngines.Count; ++index)
                //{
                //    SiegeEvent.SiegeEngineConstructionProgress constructionProgress = deployedSiegeEngines[index];
                //    if (constructionProgress.IsActive && (double)constructionProgress.Hitpoints > 0.0 && constructionProgress.SiegeEngine != DefaultSiegeEngineTypes.Preparations)
                //        MissionSiegeWeapon.CreateCampaignWeapon(constructionProgress.SiegeEngine, index, constructionProgress.Hitpoints, constructionProgress.MaxHitPoints);
                //}

                var siegeEnginesAtt = siegeEvent.BesiegerCamp.SiegeEngines.DeployedSiegeEngines
                    .Where(siegeEngineConstructionProgress => siegeEngineConstructionProgress.IsActive && siegeEngineConstructionProgress.Hitpoints > 0f && siegeEngineConstructionProgress.SiegeEngine != DefaultSiegeEngineTypes.Preparations)
                    .Select(siegeEngine => {
                    
                        int slotIndex = siegeEngine.SiegeEngine.IsRanged ?
                        ((IReadOnlyList<SiegeEvent.SiegeEngineConstructionProgress>)siegeEvent.BesiegerCamp.SiegeEngines.DeployedRangedSiegeEngines).FindIndex<SiegeEvent.SiegeEngineConstructionProgress>((Func<SiegeEvent.SiegeEngineConstructionProgress, bool>)(engine => engine == siegeEngine)) :
                        ((IReadOnlyList<SiegeEvent.SiegeEngineConstructionProgress>)siegeEvent.BesiegerCamp.SiegeEngines.DeployedMeleeSiegeEngines).FindIndex<SiegeEvent.SiegeEngineConstructionProgress>((Func<SiegeEvent.SiegeEngineConstructionProgress, bool>)(engine => engine == siegeEngine));
                        
                        return new SiegeWeapon()
                        {
                            slotIndex = slotIndex,
                            health = siegeEngine.Hitpoints,
                            maxHealth = siegeEngine.MaxHitPoints,
                            type = siegeEngine.SiegeEngine.StringId,
                        };
                    }).ToList();

                var siegeEnginesDef = siegeEvent.BesiegedSettlement.SiegeEngines.DeployedSiegeEngines
                    .Where(siegeEngineConstructionProgress => siegeEngineConstructionProgress.IsActive && siegeEngineConstructionProgress.Hitpoints > 0f && siegeEngineConstructionProgress.SiegeEngine != DefaultSiegeEngineTypes.Preparations)
                    .Select(siegeEngine => {

                        int slotIndex = siegeEngine.SiegeEngine.IsRanged ?
                        ((IReadOnlyList<SiegeEvent.SiegeEngineConstructionProgress>)siegeEvent.BesiegedSettlement.SiegeEngines.DeployedRangedSiegeEngines).FindIndex<SiegeEvent.SiegeEngineConstructionProgress>((Func<SiegeEvent.SiegeEngineConstructionProgress, bool>)(engine => engine == siegeEngine)) :
                        ((IReadOnlyList<SiegeEvent.SiegeEngineConstructionProgress>)siegeEvent.BesiegedSettlement.SiegeEngines.DeployedMeleeSiegeEngines).FindIndex<SiegeEvent.SiegeEngineConstructionProgress>((Func<SiegeEvent.SiegeEngineConstructionProgress, bool>)(engine => engine == siegeEngine));

                        return new SiegeWeapon()
                        {
                            slotIndex = slotIndex,
                            health = siegeEngine.Hitpoints,
                            maxHealth = siegeEngine.MaxHitPoints,
                            type = siegeEngine.SiegeEngine.StringId,
                        };
                    }).ToList();

                var wallHP = siegeEvent.BesiegedSettlement.SettlementWallSectionHitPointsRatioList.ToArray();

                siege = new Siege()
                {
                    wallHitPointsPercentages = wallHP,
                    siegeWeaponsAtt = siegeEnginesAtt,
                    siegeWeaponsDef = siegeEnginesDef,
                };
            }

            var campaignTime = CampaignTime.Now;
            FieldInfo fieldCampaignTimeTick = typeof(CampaignTime).GetField("_numTicks", BindingFlags.NonPublic | BindingFlags.Instance);
            long campaignTimeTick = (long)fieldCampaignTimeTick.GetValue(campaignTime);
            var gameId = Campaign.Current.UniqueGameId;
            Vec3 partyPosLogical = MobileParty.MainParty.GetLogicalPosition();


            var battleDto = new Battle()
            {
               missionName = missionName,   
               mapEventType = mapEvent.EventType.ToString(),
               Sides = Sides,
               siege = siege,
               campaignTimeTick = campaignTimeTick,
               gameId = gameId,
               partyPosLogical = new Vec3Dto(partyPosLogical),
            }; 

            var blInit = new MPBattleInitializer
            {
                MissionInitializerRecord = rec,
                BLCharacters = characters,
                battle = battleDto,
                Players = players,
            };

            characters.AddRange(createCharatersDto(blInit.battle.Sides));


            string filenameInitializer = "BL_MPBattle_" + getCampainTimeSec() + "_Initializer.xml";
            string pathInitializer = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "Battles", filenameInitializer);

            {
                XmlSerializer x = new XmlSerializer(typeof(MPBattleInitializer));
                using (TextWriter writer = new StreamWriter(pathInitializer))
                {
                    x.Serialize(writer, blInit);
                }
            }

            PropertiesUtils prop = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "config.properties"));
            string server = prop.Get("server.ip.port");


            //// Save the content to a file
            //

            //Console.WriteLine($"Send file: {filePath}");

            //// Respond to the client

            //// Get a response stream and write the response to it.
            //context.Response.ContentType = "application/octet-stream";
            //context.Response.ContentLength64 = buffer.Length;
            //context.Response.OutputStream.Write(buffer, 0, buffer.Length);
            //context.Response.OutputStream.Close();
            //context.Response.StatusCode = 200;
            //context.Response.Close();

            string contentInitilizerXml = File.ReadAllText(pathInitializer);
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("File-Name", filenameInitializer);

                // Always catch network exceptions for async methods
                try
                {
                    var requestUri = new Uri($"{server}/battlelink/api/battles/");

                    // ***

                    string date = DateTimeOffset.UtcNow.ToString("o", CultureInfo.InvariantCulture);

                    // Compute the signature.
                    string secret = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "config.properties")).Get("secret");
                    string signature = SignHttp.ComputeSignature(date + contentInitilizerXml + secret);
                    // Concatenate the string, which will be used in the authorization header.
                    // Add a date header.
                    client.DefaultRequestHeaders.Add("x-ms-date", date);

                    client.DefaultRequestHeaders.Add("signature", signature);

                    // ***

                    HttpContent content = new StringContent(contentInitilizerXml);

                    HttpResponseMessage result = await client.PostAsync(requestUri, content);
                    if (!result.IsSuccessStatusCode)
                    {
                        InformationManager.DisplayMessage(new InformationMessage("Error http code" + result.StatusCode+" "+ result.ReasonPhrase, new Color(1f, 0f, 0f)));
                        return;
                    }
                }
                catch (Exception ex)
                {
                    // Details in ex.Message and ex.HResult.
                    InformationManager.DisplayMessage(new InformationMessage("Error: " + ex.Message, new Color(1f, 0f, 0f)));
                    return;
                }
            }

            // cross local
            //{
            //    XmlSerializer x = new XmlSerializer(typeof(MPBattleInitializer));
            //    using (TextWriter writer = new StreamWriter(System.IO.Path.Combine(BasePath.Name, "..", "Mount & Blade II Dedicated Server", "Modules", "BattleLink", "Battles", "Pending", "BL_MPBattle_" + Campaign.CurrentTime + "_Initializer.xml")))
            //    {
            //        x.Serialize(writer, blInit);
            //    }
            //}


            /// XmlDocument doc = new XmlDocument();
            //doc.Save(System.IO.Path.Combine(BasePath.Name, "Modules", "MB2_BL_MPBattleInitializer2_"+ Campaign.CurrentTime + ".xml"));


            //PlayerEncounter.StartAttackMission();
            MapEvent.PlayerMapEvent.BeginWait();

            // launch mp
            string mpExe = prop.Get("multiplayer.exe");
            string mpArgs = prop.Get("multiplayer.args");

            try
            {
                using (Process proc = Process.Start(mpExe, mpArgs))
                {
                    proc.WaitForExit();
                }

                // load result
                loadResult();
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("Cant run : " + mpExe, new Color(1f, 0f, 0f)));
            }


            InformationManager.DisplayMessage(new InformationMessage("BL_MPBattle_" + getCampainTimeSec() + "_Initializer.xml writed"));

        }

        private static IEnumerable<BLCharacter> createCharatersDto(List<SideDto> sides)
        {
            var characters = new List<BLCharacter>();
            foreach (var side in sides) 
            { 
                foreach (var team in side.Teams)
                {
                    foreach (var party in team.Parties)
                    {
                        foreach (var troop in party.Troops)
                        {
                            var character = MBObjectManager.Instance.GetObject<BasicCharacterObject>(troop.Id);
                            characters.Add(createCharaterDto(character));
                        }
                    }
                }
            }
            characters = characters.GroupBy(x => x.Id).Select(x => x.First()).ToList();
            return characters;
        }

        private static BLCharacter createCharaterDto(BasicCharacterObject bso)
        {
           
            var Skills = new Skills
            {
                Skill = new List<Skill>()
                {
                    new Skill() { Id = "Riding", Value =  bso.GetSkillValue(DefaultSkills.Riding) },
                    new Skill() { Id = "OneHanded", Value =  bso.GetSkillValue(DefaultSkills.OneHanded) },
                    new Skill() { Id = "TwoHanded", Value =  bso.GetSkillValue(DefaultSkills.TwoHanded) },
                    new Skill() { Id = "Polearm", Value =  bso.GetSkillValue(DefaultSkills.Polearm) },
                    new Skill() { Id = "Crossbow", Value =  bso.GetSkillValue(DefaultSkills.Crossbow) },
                    new Skill() { Id = "Bow", Value =  bso.GetSkillValue(DefaultSkills.Bow) },
                    new Skill() { Id = "Throwing", Value =  bso.GetSkillValue(DefaultSkills.Throwing) },
                    new Skill() { Id = "Athletics", Value =  bso.GetSkillValue(DefaultSkills.Athletics) },
                    new Skill() { Id = "Tactics", Value =  bso.GetSkillValue(DefaultSkills.Tactics) },
                }
            };

            CharacterObject co = bso as CharacterObject;
            Perks Perks = null;
            if (co!=null && co.IsHero)
            {
                List<Perk> lPerks = new List<Perk>();
                foreach (PerkObject perk in PerkObject.All)
                {
                    if (co.HeroObject.GetPerkValue(perk))
                    {
                        lPerks.Add(new Perk() { Id = perk.StringId });
                    }
                }
                Perks = new Perks() { Perk = lPerks };
            }

             var character = new BLCharacter()
             {
                Id = bso.StringId,
                Level = bso.Level,
                Name = bso.Name.ToString(),
                DefaultGroup = bso.DefaultFormationClass.ToString(),
                IsHero = bso.IsHero,
                IsFemale = bso.IsFemale,
                Occupation = co!=null ? co.Occupation.ToString() : null,
                Culture = "Culture."+bso.Culture?.StringId ?? null,

                Face = new Face(bso.BodyPropertyRange),
                Equipments = new Equipments(bso.AllEquipments),
                Skills = Skills,
                Perks = Perks,

             };

            return character;
        }

        private static SideDto createSideDto(MapEventSide side)
        {
//            List<MissionSiegeWeapon> activeSiegeEngines = PlayerSiege.PlayerSiegeEvent.GetPreparedAndActiveSiegeEngines(PlayerSiege.PlayerSiegeEvent.GetSiegeEventSide(side.MissionSide));

            //var partiesByGeneral = new Dictionary<string, List<MapEventParty>>();
            var partiesByGeneral = side.Parties
                //cities hasn t troops
                .Where(p=>p.Party.MemberRoster.Count>0)
                //general can be null here
                .GroupBy(party => party.Party.General)
                //.Select(gb=>gb.ToList());
            .Select(grp => grp.ToList())
            .ToList();
            // .ToDictionary(gdc => gdc.Key, gdc => gdc.ToList());
            //.Select(grp => grp.ToList());
            //partiesByGeneral.ToList();

            List<TeamDto> teams = partiesByGeneral.Select(parties => { return createTeamDto(parties); }).ToList();

            //var Parties = new List<Party>();
            //foreach (var party in side.Parties)
            //{
            //    //cities hasn t troops
            //    if (party.Party.MemberRoster.Count>0)
            //    {
            //        Parties.Add(createPartyDto(party));
            //    }
            //}
            //var faction = side.MapFaction;
            
            //var team = new Team()
            //{
            //    Color = side.MapFaction.Color.ToHexadecimalString(),
            //    Color2 = side.MapFaction.Color2.ToHexadecimalString(),
            //    Culture = side.MapFaction.Culture.StringId,
            //    Faction = side.MapFaction.StringId,
            //    Name = side.MapFaction.Name.ToString(),
            //    FactionBannerKey = faction.Banner.Serialize(),
            //    Parties = Parties,
            //};

            //var teams = new List<TeamDto>()
            //{
                
            //};

            var sideDto = new SideDto()
            {
                BattleSide = side.MissionSide.ToString(),
                Teams = teams,
                Culture = side.MapFaction.Culture.StringId,
            };

            return sideDto;
        }

        private static TeamDto createTeamDto(List<MapEventParty> listParty)
        {
            int index=0;
            int indexPartyGeneral = 0;
            string generalId = null;

            var Parties = new List<Party>();
            foreach (var party in listParty)
            {
                Parties.Add(createPartyDto(party));
                
                if (party.Party.General!=null && party.Party.LeaderHero!=null && party.Party.LeaderHero.StringId.Equals(party.Party.General.StringId))
                {
                    indexPartyGeneral = index;
                    generalId = party.Party.General.StringId;
                }
                index += 1;

            }
            //var faction = listParty[0].MapFaction;

            var team = new TeamDto()
            {
                Parties = Parties,
                generalId = generalId,
                partyGeneralIndex = indexPartyGeneral,
            };

            return team;
        }

        private static Party createPartyDto(MapEventParty mapEventParty)
        {
            var party = mapEventParty.Party;

            var Troops = new List<Troop>();
            for (int i = 0; i < party.MemberRoster.Count; i += 1)
            {
                var roasterMember = party.MemberRoster.GetElementCopyAtIndex(i);
                Troops.Add(new Troop() { Id = roasterMember.Character.StringId, HitPoints = roasterMember.Character.HitPoints, Number = roasterMember.Number });
            }

            var partyDto = new Party()
            {
               // GeneralId = party.General?.StringId,
                Id = party.Id,
                Index = party.Index,
                Color = party.MapFaction.Color.ToHexadecimalString(),
                Color2 = party.MapFaction.Color2.ToHexadecimalString(),
               // Culture = party.MapFaction.StringId,
                Name = party.MapFaction.Name.ToString(),
                FactionBannerKey = party.MapFaction.Banner.Serialize(),
                Troops = Troops,
            };
            return partyDto;
        }


        public static long getCampainTimeSec()
        {
           return (long)CampaignTime.Now.ToSeconds;
        }
    }
}