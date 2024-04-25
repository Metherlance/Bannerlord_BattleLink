using Helpers;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.ComponentInterfaces;
using TaleWorlds.MountAndBlade;
using BattleLink.Common.Model;

namespace BattleLink.CommonSvMp.GameComponents
{
    public class BLSandboxBattleMoraleModel : BattleMoraleModel
    {
        public override (float affectedSideMaxMoraleLoss, float affectorSideMaxMoraleGain) CalculateMaxMoraleChangeDueToAgentIncapacitated(
          Agent affectedAgent,
          AgentState affectedAgentState,
          Agent affectorAgent,
          in KillingBlow killingBlow)
        {
            float battleImportance = affectedAgent.GetBattleImportance();
            Team team = affectedAgent.Team;

            float casualtiesFactor = this.CalculateCasualtiesFactor(team != null ? team.Side : BattleSideEnum.None);
            BLCharacterObject characterAffector = affectorAgent?.Character as BLCharacterObject;
            BLCharacterObject characterAffected = affectedAgent?.Character as BLCharacterObject;
            SkillObject skillFromWeaponClass = WeaponComponentData.GetRelevantSkillFromWeaponClass((WeaponClass)killingBlow.WeaponClass);
            bool isMeleWeapon = skillFromWeaponClass == DefaultSkills.OneHanded || skillFromWeaponClass == DefaultSkills.TwoHanded || skillFromWeaponClass == DefaultSkills.Polearm;
            bool isRangedWeapon = skillFromWeaponClass == DefaultSkills.Bow || skillFromWeaponClass == DefaultSkills.Crossbow || skillFromWeaponClass == DefaultSkills.Throwing;
            int isArea = killingBlow.WeaponRecordWeaponFlags.HasAnyFlag<WeaponFlags>(WeaponFlags.AffectsArea | WeaponFlags.AffectsAreaBig | WeaponFlags.MultiplePenetration) ? 1 : 0;
            float val2 = 0.75f;
            if (isArea != 0)
            {
                val2 = 0.25f;
                if (killingBlow.WeaponRecordWeaponFlags.HasAllFlags<WeaponFlags>(WeaponFlags.Burning | WeaponFlags.MultiplePenetration))
                    val2 += val2 * 0.25f;
            }
            else if (isRangedWeapon)
                val2 = 0.5f;
            float num2 = Math.Max(0.0f, val2);
            ExplainedNumber bonuses1 = new ExplainedNumber(battleImportance * 3f * num2);
            ExplainedNumber bonuses2 = new ExplainedNumber(battleImportance * 4f * num2 * casualtiesFactor);
            if (characterAffector != null)
            {
                BLCharacterObject character3 = affectorAgent?.Formation?.Captain?.Character as BLCharacterObject;
                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Leadership.MakeADifference, characterAffector, true, ref bonuses1);
                if (isMeleWeapon)
                {
                    if (skillFromWeaponClass == DefaultSkills.TwoHanded)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.Hope, characterAffector, true, ref bonuses1);
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.Terror, characterAffector, true, ref bonuses2);
                    }
                    if (affectorAgent != null && affectorAgent.HasMount)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.ThunderousCharge, characterAffector, true, ref bonuses2);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.ThunderousCharge, character3, ref bonuses2);
                    }
                }
                else if (isRangedWeapon)
                {
                    if (skillFromWeaponClass == DefaultSkills.Crossbow)
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.Terror, character3, ref bonuses2);
                    if (affectorAgent != null && affectorAgent.HasMount)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.AnnoyingBuzz, characterAffector, true, ref bonuses2);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.AnnoyingBuzz, character3, ref bonuses2);
                    }
                }
                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Leadership.HeroicLeader, character3, ref bonuses2);
            }
            if (characterAffected != null)
            {
                BLParty mobileParty = affectedAgent?.Origin?.BattleCombatant is BLBattleAgentOrigin battleCombatant ? battleCombatant.party : (BLParty)null;
                if (affectedAgentState == AgentState.Unconscious && mobileParty != null && mobileParty.HasPerk(DefaultPerks.Medicine.HealthAdvise, true))
                {
                    bonuses2 = new ExplainedNumber();
                }
                else
                {
                    if (affectedAgent.Formation?.Captain?.Character is BLCharacterObject character4)
                    {
                        ArrangementOrder arrangementOrder = affectedAgent.Formation.ArrangementOrder;
                        if (arrangementOrder == ArrangementOrder.ArrangementOrderShieldWall || arrangementOrder == ArrangementOrder.ArrangementOrderSquare || arrangementOrder == ArrangementOrder.ArrangementOrderSkein || arrangementOrder == ArrangementOrder.ArrangementOrderColumn)
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.TightFormations, character4, ref bonuses2);
                        if (arrangementOrder == ArrangementOrder.ArrangementOrderLine || arrangementOrder == ArrangementOrder.ArrangementOrderLoose || arrangementOrder == ArrangementOrder.ArrangementOrderCircle || arrangementOrder == ArrangementOrder.ArrangementOrderScatter)
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.LooseFormations, character4, ref bonuses2);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.StandardBearer, character4, ref bonuses2);
                    }
                    BLCharacterObject effectiveQuartermaster = mobileParty?.Quartermaster;
                    if (effectiveQuartermaster != null)
                        BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, effectiveQuartermaster, DefaultSkills.Steward, true, ref bonuses2, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                }
            }
            BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(affectedAgent.Formation);
            if (activeBanner != null)
                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMoraleShock, activeBanner, ref bonuses2);
            return (MathF.Max(bonuses2.ResultNumber, 0.0f), MathF.Max(bonuses1.ResultNumber, 0.0f));
        }

        public override (float affectedSideMaxMoraleLoss, float affectorSideMaxMoraleGain) CalculateMaxMoraleChangeDueToAgentPanicked(Agent agent)
        {
            double battleImportance = (double)agent.GetBattleImportance();
            Team team = agent.Team;

            float casualtiesFactor = this.CalculateCasualtiesFactor(team != null ? team.Side : BattleSideEnum.None);
            float a = (float)(battleImportance * 2.0);
            float num = (float)(battleImportance * (double)casualtiesFactor * 1.10000002384186);
            if (agent?.Character is BLCharacterObject)
            {
                ExplainedNumber bonuses = new ExplainedNumber(num);
                Formation formation = agent.Formation;
                BLCharacterObject character = formation?.Captain?.Character as BLCharacterObject;
                BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(formation);
                if (character != null)
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.StandardBearer, character, ref bonuses);
                BLCharacterObject effectiveQuartermaster = (agent?.Origin is BLBattleAgentOrigin battleCombatant ? battleCombatant.party : (BLParty)null)?.Quartermaster;
                if (effectiveQuartermaster != null)
                    BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Steward.PriceOfLoyalty, effectiveQuartermaster, DefaultSkills.Steward, true, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                if (activeBanner != null)
                    BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMoraleShock, activeBanner, ref bonuses);
                num = bonuses.ResultNumber;
            }
            return (MathF.Max(num, 0.0f), MathF.Max(a, 0.0f));
        }

        //TODO remove 5*
        public override float CalculateMoraleChangeToCharacter(Agent agent, float maxMoraleChange){
            float moral = maxMoraleChange / MathF.Max(1f, agent.Character.GetMoraleResistance());

            //if (agent.IsRetreating())
            //{
            //    moral = 0;
            //}
            //else
            //{
            //    moral = 100;
            //}

            return moral;
        }

        public override float GetEffectiveInitialMorale(Agent agent, float baseMorale)
        {
            ExplainedNumber stat = new ExplainedNumber(baseMorale);           
            BLBattleAgentOrigin battleCombatant = agent?.Origin as BLBattleAgentOrigin;
            BLParty party = battleCombatant == null ? (BLParty)null : battleCombatant.party;
            BLCharacterObject character = agent?.Character as BLCharacterObject;
            if (party != null && character != null)
            {
                BLCharacterObject characterGeneral = party.General;
                BLCharacterObject characterLeader = party.Leader;
                if (characterLeader != null)
                {
                    if (battleCombatant.Side == BattleSideEnum.Attacker)
                        BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Leadership.FerventAttacker, party, true, ref stat);
                    else if (battleCombatant.Side == BattleSideEnum.Defender)
                        BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Leadership.StoutDefender, party, true, ref stat);
                    if (characterLeader.Culture == character.Culture)
                        BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Leadership.GreatLeader, party, false, ref stat);
                    if (characterLeader.GetPerkValue(DefaultPerks.Leadership.WePledgeOurSwords))
                    {
                        int num = MathF.Min(battleCombatant.GetNumberOfHealthyMenOfTier(6), 10);
                        stat.Add((float)num);
                    }
                    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Throwing.LastHit, party, false, ref stat);
                    // TODO just general != leader ?
                    //PartyBase leaderParty = battleCombatant?.party?.LeaderParty;
                    //if ((leaderParty == null ? 0 : (battleCombatant != leaderParty ? 1 : 0)) != 0)
                    //    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Riding.ReliefForce, party, true, ref stat);
                    
                    //
                    //if (battleCombatant.MapEvent != null)
                    //{
                        //float partySideStrength;
                        //float opposingSideStrength;
                        //battleCombatant.MapEvent.GetStrengthsRelativeToParty(battleCombatant.Side, out partySideStrength, out opposingSideStrength);
                        //if ((double)partySideStrength < (double)opposingSideStrength)
                        //    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.OneHanded.StandUnited, party, true, ref stat);
                        //if (battleCombatant.MapEvent.IsSiegeAssault || battleCombatant.MapEvent.IsSiegeOutside)
                        //    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Leadership.UpliftingSpirit, party, true, ref stat);
                        
                        //bool flag = false;
                        //foreach (PartyBase involvedParty in battleCombatant.MapEvent.InvolvedParties)
                        //{
                        //    if (involvedParty.Side != battleCombatant.Side && involvedParty.MapFaction != null && involvedParty.Culture.IsBandit)
                        //    {
                        //        flag = true;
                        //        break;
                        //    }
                        //}
                        //if (flag)
                        //    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Scouting.Patrols, party, true, ref stat);
                    //}
                    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.OneHanded.LeadByExample, party, false, ref stat);
                }
                if (characterGeneral != null && characterGeneral.GetPerkValue(DefaultPerks.Leadership.GreatLeader))
                    stat.Add(DefaultPerks.Leadership.GreatLeader.PrimaryBonus);
                if (character.IsRanged)
                {
                    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Bow.RenownedArcher, battleCombatant.party, true, ref stat);
                    BLPerkHelper.AddPerkBonusForParty(DefaultPerks.Crossbow.Marksmen, battleCombatant.party, false, ref stat);
                }
                
                // TODO ref
                //if (party.IsDisorganized
                //    && (party.MapEvent == null || party.SiegeEvent == null || battleCombatant.Side != BattleSideEnum.Attacker) 
                //    && (characterLeader == null ? 0 : (characterLeader.GetPerkValue(DefaultPerks.Tactics.Improviser) ? 1 : 0)) == 0)
                //    stat.AddFactor(-0.2f);
            }
            return stat.ResultNumber;
        }

        public override bool CanPanicDueToMorale(Agent agent)
        {
            bool morale = true;
            if (agent.IsHuman)
            {
                BLCharacterObject character = agent.Character as BLCharacterObject;
                BLBattleAgentOrigin origin = agent.Origin as BLBattleAgentOrigin;
                BLCharacterObject leaderHero = origin?.party?.Leader;
                if (character != null && leaderHero != null && character.Tier >= (int)DefaultPerks.Leadership.LoyaltyAndHonor.PrimaryBonus && leaderHero.GetPerkValue(DefaultPerks.Leadership.LoyaltyAndHonor))
                    morale = false;
            }
            return morale;
        }

        public override float CalculateCasualtiesFactor(BattleSideEnum battleSide)
        {
            float casualtiesFactor = 1f;
            if (Mission.Current != null && battleSide != BattleSideEnum.None)
            {
                float agentRatioForSide = Mission.Current.GetRemovedAgentRatioForSide(battleSide);
                casualtiesFactor = MathF.Max(0.0f, casualtiesFactor + agentRatioForSide * 2f);
            }
            return casualtiesFactor;
        }

        public override float GetAverageMorale(Formation formation)
        {
            float num1 = 0.0f;
            int num2 = 0;
            if (formation != null)
            {
                foreach (IFormationUnit allUnit in (List<IFormationUnit>)formation.Arrangement.GetAllUnits())
                {
                    if (allUnit is Agent agent && agent.IsHuman && agent.Controller==Agent.ControllerType.AI)//agent.IsAIControlled is bugged for Client
                    {
                        ++num2;
                        num1 += agent.GetMorale();
                    }
                }
            }
            return num2 > 0 ? MBMath.ClampFloat(num1 / (float)num2, 0.0f, 100f) : 0.0f;
        }
    }
}
