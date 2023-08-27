using BattleLink.Common;
using Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Replay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.CampaignSystem.Actions.KillCharacterAction;
using static TaleWorlds.CampaignSystem.GameMenus.GameMenuOption;
using static TaleWorlds.MountAndBlade.MovementOrder;
using BattleResult = BattleLink.Common.BattleResult;

namespace BattleLink.Singleplayer
{

    public class BattleLinkSingleplayerBehavior : CampaignBehaviorBase
    {
        private static readonly TextObject EnemyNotAttackableTooltip = GameTexts.FindText("str_enemy_not_attackable_tooltip");

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
                starter.AddGameMenuOption("encounter", "attackMP", "{=attackMP}Attack! (MP)",
                    game_menu_encounter_attack_on_condition, 
                    game_menu_encounter_attack_on_consequence,
                    false, 2);
                starter.AddGameMenuOption("encounter", "attackMPLoadResult", "{=attackMPLoadResult}Load Result! (MP)",
                    game_menu_load_result_encounter_on_condition,
                    game_menu_load_result_encounter_on_consequence,
                    false, 3);

            // }
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
            if (show)
            {
                //MB2_BL_2185888_res.json
                //string json = JsonUtils.SerializeObject(battleResult, 10);
                // private string GetDirectoryFullPath(PlatformDirectoryPath directoryPath);
                //string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MB2_BL_2185888_res.json"); 
                string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MB2_BL_"+ Campaign.CurrentTime + "_send.json");
                args.IsEnabled = File.Exists(path);
                if (!args.IsEnabled)
                {
                    args.Tooltip = new TextObject("{=UL8za0AO1818}Battle json not send.");
                }
            }
            return show;
        }

        private void game_menu_load_result_encounter_on_consequence(MenuCallbackArgs args)
        {
            //retreive result (local  web)
            //if not found msg
            string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MB2_BL_" + Campaign.CurrentTime + "_res.json");
            if (File.Exists(path))
            {
                BattleResult battleResult = JsonUtils.Deserialize<BattleResult>(path);


                MapEvent battle = PlayerEncounter.Battle;

                // copy of EncounterAttackConsequence
                PlayerEncounter.StartAttackMission();
                MapEvent.PlayerMapEvent.BeginWait();


                //battle.AttackerSide.Parties.ElementAt(0).CommitXpGain();
                SideDto playerSide;

                applyKilledWonded(battle.AttackerSide, battleResult.attacker);
                applyKilledWonded(battle.DefenderSide, battleResult.defender);

                CampaignBattleResult campaignBattleResult = PlayerEncounter.CampaignBattleResult;
                Type t = typeof(CampaignBattleResult);
                var p = t.GetProperty("PlayerVictory", BindingFlags.Public | BindingFlags.Instance);
                p.SetValue(campaignBattleResult, true);

                PlayerEncounter.Update();


                //campaignBattleResult.PlayerVictory = true;

                //Type t = typeof(CampaignBattleResult);
                //var f = t.GetField("<PlayerVictory>k__BackingField");
                //f.SetValue(campaignBattleResult, true);

                //object safeValue = (o == null) ? null : Convert.ChangeType(o, t);
                //p.SetValue(dst, safeValue);

                //\u003CPlayerVictory\u003Ek__BackingField

                //battle.BattleState = BattleState.AttackerVictory;
                //_mapEvent.BattleState = true
            }
            else
            {

            }

           
        }
        private static void applyKilledWonded(MapEventSide side, SideDto sideDto)
        {
            MapEventParty party = side.Parties.ElementAt(0);
            party.Update();// refresh id

            List<FlattenedTroopRosterElement> listTroop = enumerate2List(party.Troops.GetEnumerator());
            foreach (TroopRosterElementDto unit in sideDto.party.data)
            {
                List<FlattenedTroopRosterElement> listTroopOfUnit = listTroop.Where(troopUnique => troopUnique.Troop.StringId == unit.characterStringId).ToList();

                int indexTroop = 0;
                for (int indexWounded = 0; indexWounded < unit.woundedNumber; indexWounded += 1)
                {
                    FlattenedTroopRosterElement elem = listTroopOfUnit.ElementAt(indexTroop);
                    party.OnTroopWounded(elem.Descriptor);
                    if (elem.Troop.IsHero)
                    {
                        elem.Troop.HeroObject.MakeWounded(null, KillCharacterActionDetail.WoundedInBattle);
                    }
                    indexTroop += 1;
                }
                for (int indexKilled = 0; indexKilled < unit.killedNumber; indexKilled += 1)
                {
                    FlattenedTroopRosterElement elem = listTroopOfUnit.ElementAt(indexTroop);
                    party.OnTroopKilled(elem.Descriptor);
                    if (elem.Troop.IsHero)
                    {
                        elem.Troop.HeroObject.AddDeathMark(null, KillCharacterActionDetail.DiedInBattle);
                    }
                    indexTroop += 1;
                }
                if (indexTroop==0)
                {
                    FlattenedTroopRosterElement elem = listTroopOfUnit.ElementAt(indexTroop);
                    if (elem.Troop.IsHero)
                    {
                        elem.Troop.HeroObject.HitPoints = unit.hp;
                    }
                }


                //TODO xp
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

        private bool game_menu_encounter_attack_on_condition(MenuCallbackArgs args)
        {
            this.CheckEnemyAttackableHonorably(args);
            bool show = MenuHelper.EncounterAttackCondition(args);
            args.optionLeaveType = LeaveType.SiegeAmbush; //(LeaveType) id++;
            if (show)
            {
                if (PlayerEncounter.Current == null)
                {
                    return false;
                }

                MapEvent battle = PlayerEncounter.Battle;
                Settlement mapEventSettlement = MobileParty.MainParty.MapEvent.MapEventSettlement;
                if (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside)
                {
                    args.Tooltip = new TextObject("{=}CaravanBattleMission not implemented.");
                    return false;
                }

                if (PlayerEncounter.Current == null
                   || (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside))
                {
                    args.Tooltip = new TextObject("{=}Raid SiegeAmbush Siege Village Hideout mission not implemented.");
                    return false;
                }

                bool flag = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsCaravan);
                bool flag2 = MapEvent.PlayerMapEvent.MapEventSettlement == null && MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsVillager);
                if(flag || flag2)
                {
                    args.Tooltip = new TextObject("{=}CaravanBattleMission not implemented.");
                    return false;
                }

            }

            return show;
        }

        static int id = 0;
        private void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
        {
            InformationManager.DisplayMessage(new InformationMessage("game_menu_encounter_attack_on_consequence - click"));
            EncounterAttackConsequence(args);
        }

        public static void EncounterAttackConsequence(MenuCallbackArgs args)
        {
            MapEvent battle = PlayerEncounter.Battle;
            PartyBase leaderParty = battle.GetLeaderParty(PartyBase.MainParty.OpponentSide);
            BeHostileAction.ApplyEncounterHostileAction(PartyBase.MainParty, leaderParty);
            if (PlayerEncounter.Current == null)
            {
                InformationManager.DisplayMessage(new InformationMessage("Not implemented"));
                return;
            }

            Settlement mapEventSettlement = MobileParty.MainParty.MapEvent.MapEventSettlement;
            if (mapEventSettlement != null && !battle.IsSallyOut && !battle.IsSiegeOutside)
            {
                InformationManager.DisplayMessage(new InformationMessage("Siege Not implemented"));
                return;
            }
            else
            {
                MapPatchData mapPatchAtPosition = Campaign.Current.MapSceneWrapper.GetMapPatchAtPosition(MobileParty.MainParty.Position2D);
                string battleSceneForMapPatch = PlayerEncounter.GetBattleSceneForMapPatch(mapPatchAtPosition);
                MissionInitializerRecord rec = new MissionInitializerRecord(battleSceneForMapPatch);
                rec.TerrainType = (int)Campaign.Current.MapSceneWrapper.GetFaceTerrainType(MobileParty.MainParty.CurrentNavigationFace);
                rec.DamageToPlayerMultiplier = Campaign.Current.Models.DifficultyModel.GetDamageToPlayerMultiplier();
                rec.DamageToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
                rec.DamageFromPlayerToFriendsMultiplier = Campaign.Current.Models.DifficultyModel.GetPlayerTroopsReceivedDamageMultiplier();
                rec.NeedsRandomTerrain = false;
                rec.PlayingInCampaignMode = true;
                rec.RandomTerrainSeed = MBRandom.RandomInt(10000);
                //rec.AtmosphereOnCampaign = Campaign.Current.Models.MapWeatherModel.GetAtmosphereModel(MobileParty.MainParty.GetLogicalPosition());
                rec.SceneHasMapPatch = true;
                //rec.DecalAtlasGroup = 2;
                rec.PatchCoordinates = mapPatchAtPosition.normalizedCoordinates;
                rec.PatchEncounterDir = (battle.AttackerSide.LeaderParty.Position2D - battle.DefenderSide.LeaderParty.Position2D).Normalized();
                float timeOfDay = Campaign.CurrentTime % 24f;
                if (Campaign.Current != null)
                {
                    rec.TimeOfDay = timeOfDay;
                }

                bool flag = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsCaravan);
                bool flag2 = MapEvent.PlayerMapEvent.MapEventSettlement == null && MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsVillager);
                if (flag || flag2)
                {
                    InformationManager.DisplayMessage(new InformationMessage("Caravan not implemented"));
                    return;
                    //CampaignMission.OpenCaravanBattleMission(rec, flag);
                }
                else
                {
                    //"Battle"
                    MapEventSide att = MapEvent.PlayerMapEvent.GetMapEventSide(BattleSideEnum.Attacker);

                    //MobileParty.MainParty.MapEvent
                    MapEvent mapEvent = MobileParty.MainParty.MapEvent;

                    MapEventSide mesDef = mapEvent.GetMapEventSide(BattleSideEnum.Defender);
                    MapEventSide mesAtt = mapEvent.GetMapEventSide(BattleSideEnum.Defender);

                    List<BattleSetting> _data = new List<BattleSetting>();
                    _data.Add(new BattleSetting()
                    {
                        rec = rec,
                        //mapEvent = mapEvent,
                        mesDef = mesDef,
                        mesAtt = mesAtt,
                        Id = 1,
                        SSN = 2,
                        Message = "A Message"
                    });;

                   // string json = JsonConvert.SerializeObject(_data.ToArray());
                    string json = JsonUtils.SerializeObject(_data.ToArray(), 10);
                    // private string GetDirectoryFullPath(PlatformDirectoryPath directoryPath);
                    string path = Path.Combine(Path.GetTempPath(),"MB2_BL_"+ Campaign.CurrentTime+ "_send.json");
                        // Path.Combine(BasePath.Name, "Modules", "BattleLink", "ModuleData", "config.xml");
                    File.WriteAllText(path, json);


//                    CampaignMission.OpenBattleMission(rec);
                }
            }

            //PlayerEncounter.StartAttackMission();
            MapEvent.PlayerMapEvent.BeginWait();

        }

    }
}