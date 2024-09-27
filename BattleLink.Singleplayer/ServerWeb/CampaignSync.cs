using BattleLink.Common.DtoSpSv;
using BattleLink.Web.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Issues;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Settlements.Buildings;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Siege;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;
using TaleWorlds.CampaignSystem.CharacterCreationContent;
using StoryMode.CharacterCreationContent;
using System.Text.RegularExpressions;

namespace BattleLink.Singleplayer
{
    internal class CampaignSync : CampaignBehaviorBase
    {
        public override void RegisterEvents()
        {
            CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameInit));
            CampaignEvents.OnGameLoadedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(OnGameInit));
            //CampaignEvents.MapEventStarted.AddNonSerializedListener(this, new Action<MapEvent, PartyBase, PartyBase>(OnMapEventStarted));
            //CampaignEvents.MapEventEnded.AddNonSerializedListener(this, new Action<MapEvent>(OnMapEventEnded));
        }

        public override void SyncData(IDataStore dataStore)
        {

        }

        private void OnGameInit(CampaignGameStarter campaignGameStarter)
        {
            InformationManager.DisplayMessage(new InformationMessage("OnGameInit"));
            try
            {
                var campaign = Campaign.Current;

                var BasicCharacterObjects = MBObjectManager.Instance.GetObjectTypeList<BasicCharacterObject>();
                var basicCharacters = BasicCharacterObjects.Select(c=>new BasicCharacterData
                {
                    SubId = (short)c.Id.SubId,
                    StringId = c.StringId,
                    Name = c.Name.ToString(),
                    Level = (byte)c.Level,
                }).ToList();

                //var heroes = campaign.AliveHeroes.Select(h => new HeroData
                //{
                //    SubId = (short)h.Id.SubId,
                //    Name = h.Name.ToString(),
                //    Level = (byte)h.Level,
                //    StringId = h.StringId,
                //    Gold = h.Gold,
                //}).ToList();

                var playerCharacters = new List<CharacterObject>();
                var players = new List<PlayerData>();
                foreach (var row in File.ReadAllLines(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink.Singleplayer", "players.txt")))
                {
                    var pair = row.Split('=');
                    var pairToopParty = pair[1].Split('|');
                    var troopStringId = pairToopParty[0];
                    var partyIndex = int.Parse(pairToopParty[1]);

                    var character = MBObjectManager.Instance.GetObject<CharacterObject>(troopStringId);
                    playerCharacters.Add(character);
                    short heroSubId = (short)character.HeroObject.Id.SubId;


                    players.Add(new PlayerData()
                    {
                        UserName = pair[0],
                        TroopStringId = troopStringId,
                    });
                }

                //campaign.AliveHeroes
                var heroes = playerCharacters.Select(h => new HeroData
                {
                    SubId = (short)h.Id.SubId,
                    Name = h.Name.ToString(),
                    Level = (byte)h.Level,
                    StringId = h.StringId,
                    Gold = h.HeroObject.Gold,

                    AttributeSubIds = getAttributeSubIds(h),
                    SkillSubIds = getSkillSubIds(h),
                    PerkSubIds = getPerkSubIds(h),


                }).ToList();

                //CreateLists()
                //var AllTraits = MBObjectManager.Instance.GetObjectTypeList<TraitObject>();
                //var AllEquipmentRosters = MBObjectManager.Instance.GetObjectTypeList<MBEquipmentRoster>();
                //var AllPolicies = MBObjectManager.Instance.GetObjectTypeList<PolicyObject>();
                //var AllBuildingTypes = MBObjectManager.Instance.GetObjectTypeList<BuildingType>();
                //var AllIssueEffects = MBObjectManager.Instance.GetObjectTypeList<IssueEffect>();
                //var AllSiegeStrategies = MBObjectManager.Instance.GetObjectTypeList<SiegeStrategy>();
                //var AllVillageTypes = MBObjectManager.Instance.GetObjectTypeList<VillageType>();
                //var AllSkillEffects = MBObjectManager.Instance.GetObjectTypeList<SkillEffect>();
                //var AllFeats = MBObjectManager.Instance.GetObjectTypeList<FeatObject>();
                //var AllSiegeEngineTypes = MBObjectManager.Instance.GetObjectTypeList<SiegeEngineType>();
                //var AllItemCategories = MBObjectManager.Instance.GetObjectTypeList<ItemCategory>();


                var AllCharacterAttributes = MBObjectManager.Instance.GetObjectTypeList<CharacterAttribute>();
                var attributes = AllCharacterAttributes.Select(a => new AttributeData
                {
                    SubId = (short)a.Id.SubId,
                    StringId = a.StringId,
                    Name = a.Name.ToString(),
                    Abbreviation = a.Abbreviation.ToString(),
                    Skills = a.Skills.Select(s => new SkillData
                    {
                        SubId = (short)s.Id.SubId,
                        StringId = s.StringId,
                        Name = s.Name.ToString(),
                        Description = s.Description.ToString(),
                        HowToLearnSkillText = s.HowToLearnSkillText.ToString()
                    }).ToArray()
                }).ToList();

                var AllPerks = MBObjectManager.Instance.GetObjectTypeList<PerkObject>();
                var perks = AllPerks.Select(p => new PerkData
                {
                    SubId = (short)p.Id.SubId,
                    StringId = p.StringId,
                    Name = p.Name.ToString(),
                    Description = p.Description.ToString(),
                    RequiredSkillValue = p.RequiredSkillValue,
                    AlternativePerkSubId = (short)(p.AlternativePerk?.Id.SubId ?? 0),
                    SkillSubId = (short)(p.Skill?.Id.SubId ?? 0),
                }).ToList();

                // skill are in attributes
                //var AllSkills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
                //var skills = AllSkills.Select(p => new SkillData
                //{
                //    SubId = (short)p.Id.SubId,
                //    StringId = p.StringId,
                //    Name = p.Name.ToString(),
                //    Description = p.Description.ToString(),
                //    HowToLearnSkillText = p.HowToLearnSkillText.Value,
                //}).ToList();

                var mobileParties = campaign.MobileParties.Select(m=>new MobilePartyData
                {
                    SubId = (short)m.Id.SubId,
                    Name = m.Name.ToString(),
                    StringId = m.StringId,
                    Party = new PartyBaseData
                    {
                        MemberRoster = new TroopRosterData
                        {
                            Data = m.MemberRoster.GetTroopRoster().Select(t => new TroopRosterElementData
                            {
                                CharacterSubId = (short)t.Character.Id.SubId,
                                Number = (short)t.Number,
                                WoundedNumber = t.WoundedNumber.ToString()
                            }).ToList(),
                        },
                        ItemRoster = new ItemRosterData
                        {
                            Data = m.ItemRoster.Select(i => new ItemRosterElementData
                            {
                                EquipmentElement = new EquipmentElementData
                                {
                                    ItemSubId = (short)i.EquipmentElement.Item.Id.SubId,
                                },
                                Amount = (short)i.Amount,
                            }).ToList(),
                        },
                    }

                }).ToList();

                var settlement = campaign.Settlements.Select(m => new SettlementData
                {
                    SubId = (short)m.Id.SubId,
                    Name = m.Name.ToString(),
                    StringId = m.StringId,
                    Party = new PartyBaseData
                    {
                        MemberRoster = new TroopRosterData
                        {
                            Data = m.Party.MemberRoster.GetTroopRoster().Select(t => new TroopRosterElementData
                            {
                                CharacterSubId = (short)t.Character.Id.SubId,
                                Number = (short)t.Number,
                                WoundedNumber = t.WoundedNumber.ToString()
                            }).ToList(),
                        },
                        ItemRoster = new ItemRosterData
                        {
                            Data = m.ItemRoster.Select(i => new ItemRosterElementData
                            {
                                EquipmentElement = new EquipmentElementData
                                {
                                    ItemSubId = (short)i.EquipmentElement.Item.Id.SubId,
                                },
                                Amount = (short)i.Amount,
                            }).ToList(),
                        },
                    }

                }).ToList();

                var AllItems = MBObjectManager.Instance.GetObjectTypeList<ItemObject>();
                var items = AllItems.Select(i => new ItemObjectData
                {
                    SubId = (short)i.Id.SubId,
                    StringId = i.StringId,
                    Name = i.Name.ToString(),
                }).ToList();

                var AllCultures = MBObjectManager.Instance.GetObjectTypeList<CultureObject>();
                var cultures = AllCultures.Select(c => new CultureData
                {
                    SubId = (short)c.Id.SubId,
                    StringId = c.StringId,
                    Name = c.Name.ToString(),
                    Text = c.EncyclopediaText.ToString(),
                    IsMainCulture = c.IsMainCulture,
                }).ToList();

                #region CharacterCreation
                var cc = new CharacterCreation();
                var sccc = new SandboxCharacterCreationContent();
                sccc.Initialize(cc);

                TaleWorlds.Core.BodyProperties bp = new TaleWorlds.Core.BodyProperties();
                cc.ChangeFaceGenChars(new List<FaceGenChar>() { new FaceGenChar(bp, 0, null, false), new FaceGenChar(bp, 0, null, false) });

                var ccData = cc.CharacterCreationMenus.Select(m => new CharacterCreationMenuData
                {
                    Text = m.Text.ToString(),
                    Title = m.Title.ToString(),
                    CharacterCreationCategories = m.CharacterCreationCategories.SelectMany(c => c.CharacterCreationOptions.Select(o => new CharacterCreationOptionData
                    {
                        Id = (short)o.Id,
                        Text = o.Text.ToString(),
                        DesciptionText = o.DescriptionText.ToString(),
                        ConditionCultureOccupation = getConditionCultureOccupation(cc, sccc, AllCultures, m, c, o),
                    })).ToList(),
                }).ToList();
                #endregion


                // campaign.Settlements.Select

                var campaignData = new InitCampaignData { Id = Campaign.Current.UniqueGameId, Heroes = heroes, Players = players, Items = items, 
                    Attributes = attributes, 
                    Perks = attributes,
                    MobileParties = mobileParties,
                    Settlement = settlement,
                    BasicCharacters = basicCharacters,
                    Cultures = cultures,
                    CharacterCreationMenus = ccData,
                };

                UdpSpSw.Send(PacketSp2SwType.Init, campaignData, LiteNetLib.DeliveryMethod.ReliableUnordered);
            }
            catch (Exception e)
            {
                InformationManager.DisplayMessage(new InformationMessage(e.Message));
            }
        }

        private List<short> getConditionCultureOccupation(CharacterCreation cc, SandboxCharacterCreationContent sccc, MBReadOnlyList<CultureObject> allCultures, CharacterCreationMenu m, CharacterCreationCategory creationCategory, CharacterCreationOption o)
        {

            CharacterCreationOnCondition categoryCondition = creationCategory.CategoryCondition;
            CharacterCreationOnCondition optionCondition = o.OnCondition;
            if (categoryCondition == null && optionCondition==null)
            {
                return null;
            }

            List<short> conditions = new List<short>();
            if (categoryCondition != null)
            {
                foreach (var culture in allCultures)
                {
                    sccc.SetSelectedCulture(culture, cc);
                    if (categoryCondition())
                    {
                        conditions.Add((short)culture.Id.SubId);
                        conditions.Add(0);
                    }
                }
            }
            else
            {
                CharacterCreationMenu familyMenu = cc.CharacterCreationMenus.FirstOrDefault(m2 => "Family".Equals(m2.Title.ToString()));
                familyMenu.OnInit(cc);
                foreach (var culture in allCultures)
                {
                    sccc.SetSelectedCulture(culture, cc);
                    foreach (var familyCat in familyMenu.CharacterCreationCategories)
                    {
                        if (familyCat.CategoryCondition())
                        {
                            foreach (var familyOpt in familyCat.CharacterCreationOptions)
                            {
                                familyOpt.OnSelect(cc);
                                if (optionCondition())
                                {
                                    conditions.Add((short)culture.Id.SubId);
                                    conditions.Add((short)(familyOpt.Id));
                                }
                            }

                        }
                    }
                }

                  
            }

            return conditions;
        }

        private short getCharCreationConditionCultureId(CharacterCreationCategory creationCategory, SandboxCharacterCreationContent sccc, MBReadOnlyList<CultureObject> allCultures)
        {

            CharacterCreationOnCondition categoryCondition = creationCategory.CategoryCondition;
            if (categoryCondition == null)
            {
                return 0;
            }
            foreach (var culture in allCultures)
            {
                sccc.SetSelectedCulture(culture, new CharacterCreation());
                if (categoryCondition())
                {
                    return (short)culture.Id.SubId;
                }
            }
            return 0;
        }

        private Dictionary<short, short> getAttributeSubIds(CharacterObject c)
        {
            if (!c.IsHero)
            {
                return null;
            }

            var AllAttributes = MBObjectManager.Instance.GetObjectTypeList<CharacterAttribute>();
            return AllAttributes.ToDictionary(a => (short)a.Id.SubId, a => (short)c.HeroObject.GetAttributeValue(a));
        }

        private List<short> getPerkSubIds(CharacterObject h)
        {
            var AllPerks = MBObjectManager.Instance.GetObjectTypeList<PerkObject>();

            return AllPerks.Where(p=>h.GetPerkValue(p)).Select(p=>(short)p.Id.SubId).ToList();

        }

        private Dictionary<short, short> getSkillSubIds(CharacterObject h)
        {
            var AllSkills = MBObjectManager.Instance.GetObjectTypeList<SkillObject>();
            return AllSkills.ToDictionary(p =>(short)p.Id.SubId, p => (short)h.GetSkillValue(p));
        }
    }
}