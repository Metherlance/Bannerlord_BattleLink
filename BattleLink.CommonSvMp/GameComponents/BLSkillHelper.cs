using BattleLink.Common.Model;
using Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;

namespace BattleLink.CommonSvMp.GameComponents
{
    public static class BLSkillHelper
    {
        private static readonly TextObject _textLeader;
        private static readonly TextObject _textPersonal;
        private static readonly TextObject _textScout;
        private static readonly TextObject _textQuartermaster;
        private static readonly TextObject _textEngineer;
        private static readonly TextObject _textPartyLeader;
        private static readonly TextObject _textSurgeon;
        private static readonly TextObject _textSergeant;
        private static readonly TextObject _textGovernor;
        private static readonly TextObject _textClanLeader;

        static BLSkillHelper()
        {
            _textLeader = new TextObject("{=SrfYbg3x}Leader", (Dictionary<string, object>)null);
            _textPersonal = new TextObject("{=UxAl9iyi}Personal", (Dictionary<string, object>)null);
            _textScout = new TextObject("{=92M0Pb5T}Scout", (Dictionary<string, object>)null);
            _textQuartermaster = new TextObject("{=redwEIlW}Quartermaster", (Dictionary<string, object>)null);
            _textEngineer = new TextObject("{=7h6cXdW7}Engineer", (Dictionary<string, object>)null);
            _textPartyLeader = new TextObject("{=ggpRTQQl}Party Leader", (Dictionary<string, object>)null);
            _textSurgeon = new TextObject("{=QBPrRdQJ}Surgeon", (Dictionary<string, object>)null);
            _textSergeant = new TextObject("{=g9VIbA9s}Sergeant", (Dictionary<string, object>)null);
            _textGovernor = new TextObject("{=Fa2nKXxI}Governor", (Dictionary<string, object>)null);
            _textClanLeader = new TextObject("{=pqfz386V}Clan Leader", (Dictionary<string, object>)null);
        }

        public static void AddSkillBonusForCharacter(SkillObject skill, SkillEffect skillEffect, Agent agent, ref ExplainedNumber stat, int baseSkillOverride = -1, bool isBonusPositive = true, int extraSkillValue = 0)
        {
            BLCharacterObject character = agent.Character as BLCharacterObject;
            int skillLevel = ((baseSkillOverride >= 0) ? baseSkillOverride : character.GetSkillValue(skill)) + extraSkillValue;
            int num = (isBonusPositive ? 1 : (-1));
            if (skillEffect.PrimaryRole == SkillEffect.PerkRole.Personal || skillEffect.SecondaryRole == SkillEffect.PerkRole.Personal)
            {
                float num2 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Personal) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
                AddToStat(ref stat, skillEffect.IncrementType, (float)num * num2, _textPersonal);
            }

            // for me total non sense, it is personal skill activate only if you have a role in party ... wtf
            //Hero heroObject = character.HeroObject;
            if (character.IsHero && agent.Origin is BLBattleAgentOrigin origin && origin.party!=null)
            {
                BLParty party = origin.party;

                if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Engineer || skillEffect.SecondaryRole == SkillEffect.PerkRole.Engineer) && character.IsHero && party?.Engineer == character)
                {
                    float num3 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Engineer) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
                    AddToStat(ref stat, skillEffect.IncrementType, (float)num * num3, _textEngineer);
                }

                if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Quartermaster || skillEffect.SecondaryRole == SkillEffect.PerkRole.Quartermaster) && character.IsHero && party?.Quartermaster == character)
                {
                    float num4 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Quartermaster) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
                    AddToStat(ref stat, skillEffect.IncrementType, (float)num * num4, _textQuartermaster);
                }

                if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Scout || skillEffect.SecondaryRole == SkillEffect.PerkRole.Scout) && character.IsHero && party?.Scout == character)
                {
                    float num5 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Scout) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
                    AddToStat(ref stat, skillEffect.IncrementType, (float)num * num5, _textScout);
                }

                if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Surgeon || skillEffect.SecondaryRole == SkillEffect.PerkRole.Surgeon) && character.IsHero && party?.Surgeon == character)
                {
                    float num6 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.Surgeon) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
                    AddToStat(ref stat, skillEffect.IncrementType, (float)num * num6, _textSurgeon);
                }

                if ((skillEffect.PrimaryRole == SkillEffect.PerkRole.PartyLeader || skillEffect.SecondaryRole == SkillEffect.PerkRole.PartyLeader) && character.IsHero && party?.Leader == character)
                {
                    float num7 = ((skillEffect.PrimaryRole == SkillEffect.PerkRole.PartyLeader) ? skillEffect.GetPrimaryValue(skillLevel) : skillEffect.GetSecondaryValue(skillLevel));
                    AddToStat(ref stat, skillEffect.IncrementType, (float)num * num7, _textPartyLeader);
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
                    stat.AddFactor(number * 0.01f, text);
                    break;
            }
        }
    }
}
