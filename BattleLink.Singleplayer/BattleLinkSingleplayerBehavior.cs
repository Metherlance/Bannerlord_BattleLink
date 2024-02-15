using BattleLink.Common;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Utils;
using Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.Map;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.CampaignSystem.GameMenus.GameMenuOption;
using static TaleWorlds.MountAndBlade.MovementOrder;

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
            string filenameResult = "BL_MPBattle_" + getCampainTimeSec() + "_Result.csbin";
            string pathResult = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "Battles", filenameResult);

            try
            {
                InformationManager.DisplayMessage(new InformationMessage("game_menu_load_result_encounter_on_consequence - click"));
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
                if (flag || flag2)
                {
                    args.Tooltip = new TextObject("{=}CaravanBattleMission not implemented.");
                    return false;
                }

            }

            return show;
        }

        static int id = 0;
        private async void game_menu_encounter_attack_on_consequence(MenuCallbackArgs args)
        {
            InformationManager.DisplayMessage(new InformationMessage("game_menu_encounter_attack_on_consequence - click"));

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

            bool flag = MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsCaravan);
            bool flag2 = MapEvent.PlayerMapEvent.MapEventSettlement == null && MapEvent.PlayerMapEvent.PartiesOnSide(BattleSideEnum.Defender).Any((MapEventParty involvedParty) => involvedParty.Party.IsMobile && involvedParty.Party.MobileParty.IsVillager);
            if (flag || flag2)
            {
                InformationManager.DisplayMessage(new InformationMessage("Caravan not implemented"));
                return;
                //CampaignMission.OpenCaravanBattleMission(rec, flag);
            }

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


            //"Battle"
            MapEventSide att = MapEvent.PlayerMapEvent.GetMapEventSide(BattleSideEnum.Attacker);

            //MobileParty.MainParty.MapEvent
            MapEvent mapEvent = MobileParty.MainParty.MapEvent;

            MapEventSide mesDef = mapEvent.GetMapEventSide(BattleSideEnum.Defender);// to remove
            MapEventSide mesAtt = mapEvent.GetMapEventSide(BattleSideEnum.Attacker);


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

            var blInit = new MPBattleInitializer
            {
                MissionInitializerRecord = rec,
                BLCharacters = characters,
                Teams = new List<Team>()
                {
                    createTeamDto(battle.DefenderSide),
                    createTeamDto(battle.AttackerSide),
                },
                Players = players,
            };

            characters.AddRange(createCharatersDto(blInit.Teams));


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

            //refresh
            btnLoadResult.MenuContext.Refresh();

            // launch mp
            string mpExe = prop.Get("multiplayer.exe");
            string mpArgs = prop.Get("multiplayer.args");

            try
            {
                using (Process proc = Process.Start(mpExe, mpArgs))
                {
                    proc.WaitForExit();
                }
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage("Cant run : " + mpExe, new Color(1f, 0f, 0f)));
            }


            InformationManager.DisplayMessage(new InformationMessage("BL_MPBattle_" + getCampainTimeSec() + "_Initializer.xml writed"));

        }

        private IEnumerable<BLCharacter> createCharatersDto(List<Team> teams)
        {
            var characters = new List<BLCharacter>();
            foreach (var team in teams)
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
            characters = characters.GroupBy(x => x.Id).Select(x => x.First()).ToList();
            return characters;
        }

        private BLCharacter createCharaterDto(BasicCharacterObject bso)
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

                Face = new Face(bso.BodyPropertyRange),
                Equipments = new Equipments(bso.AllEquipments),
                Skills = Skills,
                Perks = Perks,

             };

            return character;
        }

        private Team createTeamDto(MapEventSide side)
        {
            var Parties = new List<Party>();
            foreach (var party in side.Parties)
            {
                Parties.Add(createPartyDto(party));
            }
            var faction = side.MapFaction;
            
            var team = new Team()
            {
                BattleSide = side.MissionSide.ToString(),
                Color = side.MapFaction.Color.ToHexadecimalString(),
                Color2 = side.MapFaction.Color2.ToHexadecimalString(),
                Culture = side.MapFaction.StringId,
                Name = side.MapFaction.Name.ToString(),
                FactionBannerKey = faction.Banner.Serialize(),
                Parties = Parties,
            };
            return team;
        }

        private Party createPartyDto(MapEventParty party)
        {
            var Troops = new List<Troop>();

            for (int i = 0; i < party.Party.MemberRoster.Count; i += 1)
            {
                var roasterMember = party.Party.MemberRoster.GetElementCopyAtIndex(i);
                Troops.Add(new Troop() { Id = roasterMember.Character.StringId, HitPoints = roasterMember.Character.HitPoints, Number = roasterMember.Number });
            }

            var partyDto = new Party()
            {
                GeneralId = party.Party.General?.StringId,
                Id = party.Party.Id,
                Index = party.Party.Index,
                Troops = Troops,
            };
            return partyDto;
        }


        private static long getCampainTimeSec()
        {
           return (long)CampaignTime.Now.ToSeconds;
        }
    }
}