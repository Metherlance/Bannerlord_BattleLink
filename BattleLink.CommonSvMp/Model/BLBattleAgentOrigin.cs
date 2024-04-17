using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Model
{
    public class BLBattleAgentOrigin : BasicBattleAgentOrigin, IAgentOriginBase
    {
        //public BasicCharacterObject BLTroop { get;  set; }
        //public int partyIndex; // put IBattleCombatant PartyBase instead?
        public BLParty party;

        //public BLBattleAgentOrigin(BasicCharacterObject? troop, int partyIndex) : base(troop)
        //{
        //    this.partyIndex = partyIndex;
        //}

        //IAgentOriginBase.FactionColor factionColor;

        public Banner banner;
        Banner IAgentOriginBase.Banner => banner;

        public uint factionColor;
        uint IAgentOriginBase.FactionColor => factionColor;

        public uint factionColor2;
        uint IAgentOriginBase.FactionColor2 => factionColor2;

        //public BLBattleAgentOrigin(BasicCharacterObject? troop, BLParty party, Team team) : base(troop)
        //{
        //    this.party = party;
        //}

        public BLBattleAgentOrigin(BasicCharacterObject? troop, BLParty party, Team team) : base(troop)
        {
            this.party = party;
            if (team!=null)
            {
                banner = team.Banner;
                factionColor = team.Color;
                factionColor2 = team.Color2;
            }
        }

        public BLBattleAgentOrigin(BasicCharacterObject? troop, BLParty party) : base(troop)
        {
            this.party = party;
        }

        public BLBattleAgentOrigin(BasicCharacterObject? troop) : base(troop)
        {
            //this.partyIndex = -1;
            party = null;
        }
    }

    public class BLParty
    {
        public int partyIndex;
        public string partyId;
        public BLCharacterObject Leader;
        public BLCharacterObject ClanLeader;// no relevent perk for battle with clan leader
        public BLCharacterObject Surgeon;
        public BLCharacterObject Engineer;
        public BLCharacterObject Scout;
        public BLCharacterObject Quartermaster;
        public BLCharacterObject General;

        public bool HasPerk(PerkObject perk, bool checkSecondaryRole = false)
        {
            switch (checkSecondaryRole ? perk.SecondaryRole : perk.PrimaryRole)
            {
                case SkillEffect.PerkRole.Scout:
                    return Scout?.GetPerkValue(perk) ?? false;// EffectiveScout?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.Engineer:
                    return Engineer?.GetPerkValue(perk) ?? false;// EffectiveEngineer?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.Quartermaster:
                    return Quartermaster?.GetPerkValue(perk) ?? false;// EffectiveQuartermaster?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.Surgeon:
                    return Surgeon?.GetPerkValue(perk) ?? false;// EffectiveSurgeon?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.PartyLeader:
                    return Leader?.GetPerkValue(perk) ?? false;// LeaderHero?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.ArmyCommander:
                    return General?.GetPerkValue(perk) ?? false;// Army?.LeaderParty?.LeaderHero?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.PartyMember:
                    //foreach (TroopRosterElement item in MemberRoster.GetTroopRoster())
                    //{
                    //    if (item.Character.IsHero && item.Character.HeroObject.GetPerkValue(perk))
                    //    {
                    //        return true;
                    //    }
                    //}
                    return false;
                case SkillEffect.PerkRole.Personal:
                    //Debug.FailedAssert("personal perk is called in party", "C:\\Develop\\MB3\\Source\\Bannerlord\\TaleWorlds.CampaignSystem\\Party\\MobileParty.cs", "HasPerk", 1997);
                    return false;// Leader?.GetPerkValue(perk) ?? false;
                case SkillEffect.PerkRole.ClanLeader:
                    if (Leader != null)
                    {
                        return false;// Leader.Clan?.Leader?.GetPerkValue(perk) ?? false;
                    }

                    return false;
                default:
                    return false;
            }
        }

    }

}
