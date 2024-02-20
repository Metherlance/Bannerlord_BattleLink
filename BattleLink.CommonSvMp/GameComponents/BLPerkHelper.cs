using BattleLink.Common.Model;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;

namespace BattleLink.CommonSvMp.GameComponents
{
    public static class BLPerkHelper
    {
        public static IEnumerable<PerkObject> GetCaptainPerksForTroopUsages(TroopUsageFlags troopUsageFlags)
        {
            List<PerkObject> list = new List<PerkObject>();
            foreach (PerkObject item in PerkObject.All)
            {
                bool num = item.PrimaryTroopUsageMask != TroopUsageFlags.Undefined && troopUsageFlags.HasAllFlags(item.PrimaryTroopUsageMask);
                bool flag = item.SecondaryTroopUsageMask != TroopUsageFlags.Undefined && troopUsageFlags.HasAllFlags(item.SecondaryTroopUsageMask);
                if (num || flag)
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public static bool PlayerHasAnyItemDonationPerk()
        {
            if (!MobileParty.MainParty.HasPerk(DefaultPerks.Steward.GivingHands))
            {
                return MobileParty.MainParty.HasPerk(DefaultPerks.Steward.PaidInPromise, checkSecondaryRole: true);
            }

            return true;
        }

        public static void AddPerkBonusForParty(PerkObject perk, BLParty party, bool isPrimaryBonus, ref ExplainedNumber stat)
        {
            BLCharacterObject hero = party?.Leader;
            if (hero == null)
            {
                return;
            }

            bool flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.PartyLeader;
            bool flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.PartyLeader;
            if ((flag || flag2) && hero.GetPerkValue(perk))
            {
                float num = (flag ? perk.PrimaryBonus : perk.SecondaryBonus);
                //if (hero.Clan != Clan.PlayerClan)
                //{
                //    num *= 1.8f;
                //}

                if (flag)
                {
                    AddToStat(ref stat, perk.PrimaryIncrementType, num, perk.Name);
                }
                else
                {
                    AddToStat(ref stat, perk.SecondaryIncrementType, num, perk.Name);
                }
            }

            flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.ClanLeader;
            flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.ClanLeader;
            if ((flag || flag2) && party.ClanLeader != null && party.ClanLeader.GetPerkValue(perk))
            {
                if (flag)
                {
                    AddToStat(ref stat, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                }
                else
                {
                    AddToStat(ref stat, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
                }
            }

            flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.PartyMember;
            flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.PartyMember;
            if (flag || flag2)
            {
                //if (hero.Clan != Clan.PlayerClan)
                //{
                //    if (hero.GetPerkValue(perk))
                //    {
                //        AddToStat(ref stat, flag ? perk.PrimaryIncrementType : perk.SecondaryIncrementType, flag ? perk.PrimaryBonus : perk.SecondaryBonus, perk.Name);
                //    }
                //}
                //else
                //{
                    //foreach (TroopRosterElement item in party.MemberRoster.GetTroopRoster())
                    //{
                    //    if (item.Character.IsHero && item.Character.GetPerkValue(perk))
                    //    {
                    //        AddToStat(ref stat, flag ? perk.PrimaryIncrementType : perk.SecondaryIncrementType, flag ? perk.PrimaryBonus : perk.SecondaryBonus, perk.Name);
                    //    }
                    //}
                //}
            }

            //if (hero.Clan != Clan.PlayerClan)
            //{
            //    return;
            //}

            flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.Engineer;
            flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.Engineer;
            if (flag || flag2)
            {
                BLCharacterObject effectiveEngineer = party.Engineer;
                if (effectiveEngineer != null && effectiveEngineer.GetPerkValue(perk))
                {
                    if (flag)
                    {
                        AddToStat(ref stat, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                    }
                    else
                    {
                        AddToStat(ref stat, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
                    }
                }
            }

            flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.Scout;
            flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.Scout;
            if (flag || flag2)
            {
                BLCharacterObject effectiveScout = party.Scout;
                if (effectiveScout != null && effectiveScout.GetPerkValue(perk))
                {
                    if (flag)
                    {
                        AddToStat(ref stat, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                    }
                    else
                    {
                        AddToStat(ref stat, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
                    }
                }
            }

            flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.Surgeon;
            flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.Surgeon;
            if (flag || flag2)
            {
                BLCharacterObject effectiveSurgeon = party.Surgeon;
                if (effectiveSurgeon != null && effectiveSurgeon.GetPerkValue(perk))
                {
                    if (flag)
                    {
                        AddToStat(ref stat, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                    }
                    else
                    {
                        AddToStat(ref stat, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
                    }
                }
            }

            flag = isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.Quartermaster;
            flag2 = !isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.Quartermaster;
            if (!(flag || flag2))
            {
                return;
            }

            BLCharacterObject effectiveQuartermaster = party.Quartermaster;
            if (effectiveQuartermaster != null && effectiveQuartermaster.GetPerkValue(perk))
            {
                if (flag)
                {
                    AddToStat(ref stat, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                }
                else
                {
                    AddToStat(ref stat, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
                }
            }
        }

        private static void AddToStat(ref ExplainedNumber stat, SkillEffect.EffectIncrementType effectIncrementType, float number, TextObject text)
        {
            switch (effectIncrementType)
            {
                case SkillEffect.EffectIncrementType.Add:
                    stat.Add(number, text);
                    break;
                case SkillEffect.EffectIncrementType.AddFactor:
                    stat.AddFactor(number, text);
                    break;
            }
        }

        public static void AddPerkBonusForCharacter(PerkObject perk, BLCharacterObject character, bool isPrimaryBonus, ref ExplainedNumber bonuses)
        {
            if (isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.Personal)
            {
                if (character.GetPerkValue(perk))
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                }
            }
            else if (!isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.Personal && character.GetPerkValue(perk))
            {
                AddToStat(ref bonuses, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
            }

            //if (isPrimaryBonus && perk.PrimaryRole == SkillEffect.PerkRole.ClanLeader)
            //{
            //    if (character.IsHero && character.HeroObject.Clan?.Leader != null && character.HeroObject.Clan.Leader.GetPerkValue(perk))
            //    {
            //        AddToStat(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
            //    }
            //}
            //else if (!isPrimaryBonus && perk.SecondaryRole == SkillEffect.PerkRole.ClanLeader && character.IsHero && character.HeroObject.Clan.Leader != null && character.HeroObject.Clan.Leader.GetPerkValue(perk))
            //{
            //    AddToStat(ref bonuses, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
            //}
        }

        public static void AddEpicPerkBonusForCharacter(PerkObject perk, BLCharacterObject character, SkillObject skillType, bool applyPrimaryBonus, ref ExplainedNumber bonuses, int skillRequired)
        {
            if (!character.GetPerkValue(perk))
            {
                return;
            }

            int skillValue = character.GetSkillValue(skillType);
            if (skillValue > skillRequired)
            {
                if (applyPrimaryBonus)
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus * (float)(skillValue - skillRequired), perk.Name);
                }
                else
                {
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, perk.SecondaryBonus * (float)(skillValue - skillRequired), perk.Name);
                }
            }
        }

        public static void AddPerkBonusFromCaptain(PerkObject perk, BLCharacterObject captainCharacter, ref ExplainedNumber bonuses)
        {
            if (perk.PrimaryRole == SkillEffect.PerkRole.Captain)
            {
                if (captainCharacter != null && captainCharacter.GetPerkValue(perk))
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                }
            }
            else if (perk.SecondaryRole == SkillEffect.PerkRole.Captain && captainCharacter != null && captainCharacter.GetPerkValue(perk))
            {
                AddToStat(ref bonuses, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
            }
        }

        public static void AddPerkBonusForTown(PerkObject perk, Town town, ref ExplainedNumber bonuses)
        {
            bool flag = perk.PrimaryRole == SkillEffect.PerkRole.Governor;
            bool flag2 = perk.SecondaryRole == SkillEffect.PerkRole.Governor;
            if (!(flag || flag2))
            {
                return;
            }

            Hero governor = town.Governor;
            if (governor != null && governor.GetPerkValue(perk) && governor.CurrentSettlement != null && governor.CurrentSettlement == town.Settlement)
            {
                if (flag)
                {
                    AddToStat(ref bonuses, perk.PrimaryIncrementType, perk.PrimaryBonus, perk.Name);
                }
                else
                {
                    AddToStat(ref bonuses, perk.SecondaryIncrementType, perk.SecondaryBonus, perk.Name);
                }
            }
        }

        public static bool GetPerkValueForTown(PerkObject perk, Town town)
        {
            if (perk.PrimaryRole == SkillEffect.PerkRole.ClanLeader || perk.SecondaryRole == SkillEffect.PerkRole.ClanLeader)
            {
                Hero hero = town.Owner.Settlement.OwnerClan?.Leader;
                if (hero != null && hero.GetPerkValue(perk))
                {
                    return true;
                }
            }

            if (perk.PrimaryRole == SkillEffect.PerkRole.Governor || perk.SecondaryRole == SkillEffect.PerkRole.Governor)
            {
                Hero governor = town.Governor;
                if (governor != null && governor.GetPerkValue(perk) && governor.CurrentSettlement != null && governor.CurrentSettlement == town.Settlement)
                {
                    return true;
                }
            }

            return false;
        }

        public static List<PerkObject> GetGovernorPerksForHero(Hero hero)
        {
            List<PerkObject> list = new List<PerkObject>();
            foreach (PerkObject item in PerkObject.All)
            {
                if ((item.PrimaryRole == SkillEffect.PerkRole.Governor || item.SecondaryRole == SkillEffect.PerkRole.Governor) && hero.GetPerkValue(item))
                {
                    list.Add(item);
                }
            }

            return list;
        }

        public static (TextObject, TextObject) GetGovernorEngineeringSkillEffectForHero(Hero governor)
        {
            if (governor != null && governor.GetSkillValue(DefaultSkills.Engineering) > 0)
            {
                SkillEffect townProjectBuildingBonus = DefaultSkillEffects.TownProjectBuildingBonus;
                TextObject description = townProjectBuildingBonus.Description;
                float num = ((townProjectBuildingBonus.PrimaryRole == SkillEffect.PerkRole.Governor) ? townProjectBuildingBonus.PrimaryBonus : townProjectBuildingBonus.SecondaryBonus);
                description.SetTextVariable("a0", (float)governor.GetSkillValue(DefaultSkills.Engineering) * num);
                return (DefaultSkills.Engineering.Name, description);
            }

            return (TextObject.Empty, new TextObject("{=0rBsbw1T}No effect"));
        }

        public static void SetDescriptionTextVariable(TextObject description, float bonus, SkillEffect.EffectIncrementType effectIncrementType)
        {
            float num = ((effectIncrementType == SkillEffect.EffectIncrementType.AddFactor) ? (bonus * 100f) : bonus);
            string text = $"{num:0.#}";
            if (bonus > 0f)
            {
                description.SetTextVariable("VALUE", "+" + text);
            }
            else
            {
                description.SetTextVariable("VALUE", text ?? "");
            }
        }

        public static int AvailablePerkCountOfHero(Hero hero)
        {
            MBList<PerkObject> mBList = new MBList<PerkObject>();
            foreach (PerkObject item in PerkObject.All)
            {
                SkillObject skill = item.Skill;
                if ((float)hero.GetSkillValue(skill) >= item.RequiredSkillValue && !hero.GetPerkValue(item) && (item.AlternativePerk == null || !hero.GetPerkValue(item.AlternativePerk)) && !mBList.Contains(item.AlternativePerk))
                {
                    mBList.Add(item);
                }
            }

            return mBList.Count;
        }
    }
}
