using BattleLink.Common.Model;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ComponentInterfaces;

namespace BattleLink.CommonSvMp.GameComponents
{
    public class BLAgentApplyDamageModel : AgentApplyDamageModel
    {
        private const float SallyOutSiegeEngineDamageMultiplier = 4.5f;

        public override float CalculateDamage(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float baseDamage)
        {
            Formation attackerFormation = attackInformation.AttackerFormation;
            BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(attackerFormation);
            Agent agent = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerAgent.RiderAgent : attackInformation.AttackerAgent);
            BLCharacterObject characterObject = (attackInformation.IsAttackerAgentMount ? attackInformation.AttackerRiderAgentCharacter : attackInformation.AttackerAgentCharacter) as BLCharacterObject;
            BLCharacterObject captainCharacter = attackInformation.AttackerCaptainCharacter as BLCharacterObject;
            bool flag = attackInformation.IsAttackerAgentHuman && !attackInformation.DoesAttackerHaveMountAgent;
            bool flag2 = attackInformation.DoesAttackerHaveMountAgent || attackInformation.DoesAttackerHaveRiderAgent;
            BLCharacterObject characterObject2 = (attackInformation.IsVictimAgentMount ? attackInformation.VictimRiderAgentCharacter : attackInformation.VictimAgentCharacter) as BLCharacterObject;
            BLCharacterObject characterObject3 = attackInformation.VictimCaptainCharacter as BLCharacterObject;
            bool flag3 = attackInformation.IsVictimAgentHuman && !attackInformation.DoesVictimHaveMountAgent;
            bool flag4 = attackInformation.DoesVictimHaveMountAgent || attackInformation.DoesVictimHaveRiderAgent;
            Formation victimFormation = attackInformation.VictimFormation;
            BannerComponent activeBanner2 = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
            WeaponComponentData currentUsageItem = attackInformation.VictimMainHandWeapon.CurrentUsageItem;
            bool flag5 = collisionData.AttackBlockedWithShield || collisionData.CollidedWithShieldOnBack;
            float b = 0f;
            WeaponComponentData currentUsageItem2 = weapon.CurrentUsageItem;
            bool flag6 = false;
            if (currentUsageItem2 != null && currentUsageItem2.IsConsumable && collisionData.CollidedWithShieldOnBack && characterObject2 != null && characterObject2.GetPerkValue(DefaultPerks.Crossbow.Pavise))
            {
                float num = MBMath.ClampFloat(DefaultPerks.Crossbow.Pavise.PrimaryBonus, 0f, 1f);
                flag6 = MBRandom.RandomFloat <= num;
            }

            if (!flag6)
            {
                ExplainedNumber bonuses = new ExplainedNumber(baseDamage);
                if (characterObject != null)
                {
                    if (currentUsageItem2 != null)
                    {
                        if (currentUsageItem2.IsMeleeWeapon)
                        {
                            if (currentUsageItem2.RelevantSkill == DefaultSkills.OneHanded)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.DeadlyPurpose, characterObject, isPrimaryBonus: true, ref bonuses);
                                if (flag2)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.Cavalry, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (attackInformation.OffHandItem.IsEmpty)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.Duelist, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (currentUsageItem2.WeaponClass == WeaponClass.Mace || currentUsageItem2.WeaponClass == WeaponClass.OneHandedAxe)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.ToBeBlunt, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (flag5)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.Prestige, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Roguery.Carver, captainCharacter, ref bonuses);
                                BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.OneHanded.WayOfTheSword, characterObject, DefaultSkills.OneHanded, applyPrimaryBonus: false, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            }
                            else if (currentUsageItem2.RelevantSkill == DefaultSkills.TwoHanded)
                            {
                                if (flag5)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.WoodChopper, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.WoodChopper, captainCharacter, ref bonuses);
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.ShieldBreaker, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.ShieldBreaker, captainCharacter, ref bonuses);
                                }

                                if (currentUsageItem2.WeaponClass == WeaponClass.TwoHandedAxe || currentUsageItem2.WeaponClass == WeaponClass.TwoHandedMace)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.HeadBasher, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (attackInformation.IsVictimAgentMount)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.BeastSlayer, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.BeastSlayer, captainCharacter, ref bonuses);
                                }

                                if (attackInformation.AttackerHitPointRate < 0.5f)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.Berserker, characterObject, isPrimaryBonus: true, ref bonuses);
                                }
                                else if (attackInformation.AttackerHitPointRate > 0.9f)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.Confidence, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.BladeMaster, characterObject, isPrimaryBonus: true, ref bonuses);
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Roguery.DashAndSlash, captainCharacter, ref bonuses);
                                BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.TwoHanded.WayOfTheGreatAxe, characterObject, DefaultSkills.TwoHanded, applyPrimaryBonus: false, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            }
                            else if (currentUsageItem2.RelevantSkill == DefaultSkills.Polearm)
                            {
                                if (flag2)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Cavalry, characterObject, isPrimaryBonus: true, ref bonuses);
                                }
                                else
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Pikeman, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (collisionData.StrikeType == 1)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.CleanThrust, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.SharpenTheTip, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (attackInformation.IsVictimAgentMount)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.SteedKiller, characterObject, isPrimaryBonus: true, ref bonuses);
                                    if (flag)
                                    {
                                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.SteedKiller, captainCharacter, ref bonuses);
                                    }
                                }

                                if (attackInformation.IsHeadShot)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Guards, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Phalanx, captainCharacter, ref bonuses);
                                BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Polearm.WayOfTheSpear, characterObject, DefaultSkills.Polearm, applyPrimaryBonus: false, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                            }
                            else if (currentUsageItem2.IsShield)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.Basher, characterObject, isPrimaryBonus: true, ref bonuses);
                            }

                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Powerful, characterObject, isPrimaryBonus: true, ref bonuses);
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.Powerful, captainCharacter, ref bonuses);
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Engineering.ImprovedTools, captainCharacter, ref bonuses);
                            if (weapon.Item != null && weapon.Item.ItemType == ItemObject.ItemTypeEnum.Thrown)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.FlexibleFighter, characterObject, isPrimaryBonus: true, ref bonuses);
                            }

                            if (flag2)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.MountedWarrior, characterObject, isPrimaryBonus: true, ref bonuses);
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.MountedWarrior, captainCharacter, ref bonuses);
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.Cavalry, captainCharacter, ref bonuses);
                            }
                            else
                            {
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.DeadlyPurpose, captainCharacter, ref bonuses);
                                if (collisionData.StrikeType == 1)
                                {
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.SharpenTheTip, captainCharacter, ref bonuses);
                                }
                            }

                            if (activeBanner != null)
                            {
                                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamage, activeBanner, ref bonuses);
                                if (attackInformation.DoesVictimHaveMountAgent)
                                {
                                    BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMeleeDamageAgainstMountedTroops, activeBanner, ref bonuses);
                                }
                            }
                        }
                        else if (currentUsageItem2.IsConsumable)
                        {
                            if (currentUsageItem2.RelevantSkill == DefaultSkills.Bow && collisionData.CollisionBoneIndex != -1)
                            {
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.BowControl, captainCharacter, ref bonuses);
                                if (attackInformation.IsHeadShot)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.DeadAim, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.StrongBows, characterObject, isPrimaryBonus: true, ref bonuses);
                                if (characterObject.Tier >= 3)
                                {
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.StrongBows, captainCharacter, ref bonuses);
                                }

                                if (attackInformation.IsVictimAgentMount)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.HunterClan, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Bow.Deadshot, characterObject, DefaultSkills.Bow, applyPrimaryBonus: false, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                            }
                            else if (currentUsageItem2.RelevantSkill == DefaultSkills.Crossbow && collisionData.CollisionBoneIndex != -1)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Engineering.TorsionEngines, characterObject, isPrimaryBonus: false, ref bonuses);
                                if (attackInformation.IsVictimAgentMount)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Unhorser, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.Unhorser, captainCharacter, ref bonuses);
                                }

                                if (attackInformation.IsHeadShot)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Sheriff, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (flag3)
                                {
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.Sheriff, captainCharacter, ref bonuses);
                                }

                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.HammerBolts, captainCharacter, ref bonuses);
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Engineering.DreadfulSieger, captainCharacter, ref bonuses);
                                BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Crossbow.MightyPull, characterObject, DefaultSkills.Crossbow, applyPrimaryBonus: false, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                            }
                            else if (currentUsageItem2.RelevantSkill == DefaultSkills.Throwing)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.StrongArms, characterObject, isPrimaryBonus: true, ref bonuses);
                                if (flag5)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.ShieldBreaker, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.ShieldBreaker, captainCharacter, ref bonuses);
                                    if (currentUsageItem2.WeaponClass == WeaponClass.ThrowingAxe)
                                    {
                                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.Splinters, characterObject, isPrimaryBonus: true, ref bonuses);
                                    }

                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.Splinters, captainCharacter, ref bonuses);
                                }

                                if (attackInformation.IsVictimAgentMount)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.Hunter, characterObject, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.Hunter, captainCharacter, ref bonuses);
                                }

                                if (flag2)
                                {
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.MountedSkirmisher, captainCharacter, ref bonuses);
                                }

                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.Impale, captainCharacter, ref bonuses);
                                if (flag4)
                                {
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.KnockOff, captainCharacter, ref bonuses);
                                }

                                if (attackInformation.VictimAgentHealth <= attackInformation.VictimAgentMaxHealth * 0.5f)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.LastHit, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                if (attackInformation.IsHeadShot)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.HeadHunter, characterObject, isPrimaryBonus: true, ref bonuses);
                                }

                                BLPerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Throwing.UnstoppableForce, characterObject, DefaultSkills.Throwing, applyPrimaryBonus: false, ref bonuses, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                            }

                            if (flag2)
                            {
                                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.HorseArcher, characterObject, isPrimaryBonus: true, ref bonuses);
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.HorseArcher, captainCharacter, ref bonuses);
                            }

                            if (activeBanner != null)
                            {
                                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedRangedDamage, activeBanner, ref bonuses);
                            }
                        }

                        if (weapon.Item != null && weapon.Item.IsCivilian)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.Carver, characterObject, isPrimaryBonus: true, ref bonuses);
                        }
                    }

                    if (collisionData.IsHorseCharge)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.FullSpeed, characterObject, isPrimaryBonus: true, ref bonuses);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.FullSpeed, captainCharacter, ref bonuses);
                        if (characterObject.GetPerkValue(DefaultPerks.Riding.TheWayOfTheSaddle))
                        {
                            float value = (float)MathF.Max(MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(agent, DefaultSkills.Riding) - Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, 0) * DefaultPerks.Riding.TheWayOfTheSaddle.PrimaryBonus;
                            bonuses.Add(value);
                        }

                        if (activeBanner != null)
                        {
                            BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedChargeDamage, activeBanner, ref bonuses);
                        }
                    }

                    if (flag)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.HeadBasher, captainCharacter, ref bonuses);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.RecklessCharge, captainCharacter, ref bonuses);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Pikeman, captainCharacter, ref bonuses);
                        if (flag4)
                        {
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Braced, captainCharacter, ref bonuses);
                        }
                    }

                    if (flag2)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Cavalry, captainCharacter, ref bonuses);
                    }

                    if (currentUsageItem2 == null && collisionData.IsAlternativeAttack && characterObject.GetPerkValue(DefaultPerks.Athletics.StrongLegs))
                    {
                        bonuses.AddFactor(1f);
                    }

                    if (flag5)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Engineering.WallBreaker, captainCharacter, ref bonuses);
                    }

                    if (collisionData.EntityExists)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.Vandal, captainCharacter, ref bonuses);
                    }

                    if (characterObject2 != null)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.Coaching, captainCharacter, ref bonuses);
                        if (characterObject2.Culture.IsBandit)
                        {
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.LawKeeper, captainCharacter, ref bonuses);
                        }

                        if (flag2 && flag3)
                        {
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.Gensdarmes, captainCharacter, ref bonuses);
                        }
                    }

                    if (characterObject.Culture.IsBandit)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Roguery.PartnersInCrime, captainCharacter, ref bonuses);
                    }
                }

                float num2 = 1f;
                if (Mission.Current.IsSallyOutBattle)
                {
                    DestructableComponent hitObjectDestructibleComponent = attackInformation.HitObjectDestructibleComponent;
                    if (hitObjectDestructibleComponent != null && hitObjectDestructibleComponent.GameEntity.GetFirstScriptOfType<SiegeWeapon>() != null)
                    {
                        num2 *= 4.5f;
                    }
                }

                bonuses = new ExplainedNumber(bonuses.ResultNumber * num2);
                if (attackInformation.DoesAttackerHaveMountAgent && (currentUsageItem2 == null || currentUsageItem2.RelevantSkill != DefaultSkills.Crossbow))
                {
                    int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(agent, DefaultSkills.Riding);
                    float value2 = -0.01f * MathF.Max(0f, DefaultSkillEffects.MountedWeaponDamagePenalty.GetPrimaryValue(effectiveSkill));
                    bonuses.AddFactor(value2);
                }

                if (characterObject2 != null)
                {
                    if (currentUsageItem2 != null)
                    {
                        if (currentUsageItem2.IsConsumable)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.SkirmishPhaseMaster, characterObject2, isPrimaryBonus: true, ref bonuses);
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.Skirmisher, characterObject3, ref bonuses);
                            if (characterObject2.IsRanged)
                            {
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.SkirmishPhaseMaster, characterObject3, ref bonuses);
                            }

                            if (currentUsageItem != null)
                            {
                                if (currentUsageItem.WeaponClass == WeaponClass.Crossbow)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.CounterFire, characterObject2, isPrimaryBonus: true, ref bonuses);
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.CounterFire, characterObject3, ref bonuses);
                                }
                                else if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing)
                                {
                                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.Skirmisher, characterObject2, isPrimaryBonus: true, ref bonuses);
                                }
                            }

                            if (activeBanner2 != null)
                            {
                                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAttackDamage, activeBanner2, ref bonuses);
                            }
                        }
                        else if (currentUsageItem2.IsMeleeWeapon)
                        {
                            if (characterObject3 != null)
                            {
                                Formation victimFormation2 = attackInformation.VictimFormation;
                                if (victimFormation2 != null && victimFormation2.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.ShieldWall)
                                {
                                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.Basher, characterObject3, ref bonuses);
                                }
                            }

                            if (activeBanner2 != null)
                            {
                                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedMeleeAttackDamage, activeBanner2, ref bonuses);
                            }
                        }
                    }

                    if (flag5)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.SteelCoreShields, characterObject2, isPrimaryBonus: true, ref bonuses);
                        if (flag3)
                        {
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.SteelCoreShields, characterObject3, ref bonuses);
                        }

                        if (collisionData.AttackBlockedWithShield && !collisionData.CorrectSideShieldBlock)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.ShieldWall, characterObject2, isPrimaryBonus: true, ref bonuses);
                        }
                    }

                    if (collisionData.IsHorseCharge)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.SureFooted, characterObject2, isPrimaryBonus: true, ref bonuses);
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Braced, characterObject2, isPrimaryBonus: true, ref bonuses);
                        if (characterObject3 != null)
                        {
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.SureFooted, characterObject3, ref bonuses);
                            BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.Braced, characterObject3, ref bonuses);
                        }
                    }

                    if (collisionData.IsFallDamage)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.StrongLegs, characterObject2, isPrimaryBonus: true, ref bonuses);
                    }

                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.EliteReserves, characterObject3, ref bonuses);
                }

                b = bonuses.ResultNumber;
            }

            return MathF.Max(0f, b);
        }

        public override bool DecideCrushedThrough(Agent attackerAgent, Agent defenderAgent, float totalAttackEnergy, Agent.UsageDirection attackDirection, StrikeType strikeType, WeaponComponentData defendItem, bool isPassiveUsage)
        {
            EquipmentIndex wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
            if (wieldedItemIndex == EquipmentIndex.None)
            {
                wieldedItemIndex = attackerAgent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            }

            if (((wieldedItemIndex != EquipmentIndex.None) ? attackerAgent.Equipment[wieldedItemIndex].CurrentUsageItem : null) == null || isPassiveUsage || strikeType != 0 || attackDirection != 0)
            {
                return false;
            }

            float num = 58f;
            if (defendItem != null && defendItem.IsShield)
            {
                num *= 1.2f;
            }

            return totalAttackEnergy > num;
        }

        public override void DecideMissileWeaponFlags(Agent attackerAgent, MissionWeapon missileWeapon, ref WeaponFlags missileWeaponFlags)
        {
            if (attackerAgent?.Character is BLCharacterObject characterObject && missileWeapon.CurrentUsageItem.WeaponClass == WeaponClass.Javelin && characterObject.GetPerkValue(DefaultPerks.Throwing.Impale))
            {
                missileWeaponFlags |= WeaponFlags.CanPenetrateShield;
            }
        }

        public override bool CanWeaponIgnoreFriendlyFireChecks(WeaponComponentData weapon)
        {
            if (weapon != null && weapon.IsConsumable && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanPenetrateShield) && weapon.WeaponFlags.HasAnyFlag(WeaponFlags.MultiplePenetration))
            {
                return true;
            }

            return false;
        }

        public override bool CanWeaponDismount(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            if (!MBMath.IsBetween((int)blow.VictimBodyPart, 0, 6))
            {
                return false;
            }

            if (!attackerAgent.HasMount && blow.StrikeType == StrikeType.Swing && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanHook))
            {
                return true;
            }

            if (blow.StrikeType == StrikeType.Thrust && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanDismount))
            {
                return true;
            }

            if (attackerAgent.Character is BLCharacterObject characterObject)
            {
                if (attackerWeapon.RelevantSkill != DefaultSkills.Crossbow || !attackerWeapon.IsConsumable || !characterObject.GetPerkValue(DefaultPerks.Crossbow.HammerBolts))
                {
                    if (attackerWeapon.RelevantSkill == DefaultSkills.Throwing && attackerWeapon.IsConsumable)
                    {
                        return characterObject.GetPerkValue(DefaultPerks.Throwing.KnockOff);
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        public override void CalculateDefendedBlowStunMultipliers(Agent attackerAgent, Agent defenderAgent, CombatCollisionResult collisionResult, WeaponComponentData attackerWeapon, WeaponComponentData defenderWeapon, out float attackerStunMultiplier, out float defenderStunMultiplier)
        {
            float num = 1f;
            float b = 1f;
            if (attackerAgent.Character is BLCharacterObject characterObject && (collisionResult == CombatCollisionResult.Blocked || collisionResult == CombatCollisionResult.Parried) && characterObject.GetPerkValue(DefaultPerks.Athletics.MightyBlow))
            {
                num += num * DefaultPerks.Athletics.MightyBlow.PrimaryBonus;
            }

            defenderStunMultiplier = MathF.Max(0f, num);
            attackerStunMultiplier = MathF.Max(0f, b);
        }

        public override bool CanWeaponKnockback(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            if (MBMath.IsBetween((int)collisionData.VictimHitBodyPart, 0, 6) && !attackerWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown))
            {
                if (!attackerWeapon.IsConsumable && (blow.BlowFlag & BlowFlags.CrushThrough) == 0)
                {
                    if (blow.StrikeType == StrikeType.Thrust)
                    {
                        return blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.WideGrip);
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        public override bool CanWeaponKnockDown(Agent attackerAgent, Agent victimAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            if (attackerWeapon.WeaponClass == WeaponClass.Boulder)
            {
                return true;
            }

            BoneBodyPartType victimHitBodyPart = collisionData.VictimHitBodyPart;
            bool flag = MBMath.IsBetween((int)victimHitBodyPart, 0, 6);
            if (!victimAgent.HasMount && victimHitBodyPart == BoneBodyPartType.Legs)
            {
                flag = true;
            }

            if (flag && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanKnockDown))
            {
                if (!attackerWeapon.IsPolearm || blow.StrikeType != StrikeType.Thrust)
                {
                    if (attackerWeapon.IsMeleeWeapon && blow.StrikeType == StrikeType.Swing)
                    {
                        return MissionCombatMechanicsHelper.DecideSweetSpotCollision(in collisionData);
                    }

                    return false;
                }

                return true;
            }

            return false;
        }

        public override float GetDismountPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            float num = 0f;
            if (blow.StrikeType == StrikeType.Swing && blow.WeaponRecord.WeaponFlags.HasAnyFlag(WeaponFlags.CanHook))
            {
                num += 0.25f;
            }

            if (attackerWeapon != null && attackerAgent.Character is BLCharacterObject characterObject)
            {
                if (attackerWeapon.RelevantSkill == DefaultSkills.Polearm && characterObject.GetPerkValue(DefaultPerks.Polearm.Braced))
                {
                    num += DefaultPerks.Polearm.Braced.PrimaryBonus;
                }
                else if (attackerWeapon.RelevantSkill == DefaultSkills.Crossbow && attackerWeapon.IsConsumable && characterObject.GetPerkValue(DefaultPerks.Crossbow.HammerBolts))
                {
                    num += DefaultPerks.Crossbow.HammerBolts.PrimaryBonus;
                }
                else if (attackerWeapon.RelevantSkill == DefaultSkills.Throwing && attackerWeapon.IsConsumable && characterObject.GetPerkValue(DefaultPerks.Throwing.KnockOff))
                {
                    num += DefaultPerks.Throwing.KnockOff.PrimaryBonus;
                }
            }

            return MathF.Max(0f, num);
        }

        public override float GetKnockBackPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            float num = 0f;
            if (attackerWeapon != null && attackerWeapon.RelevantSkill == DefaultSkills.Polearm && attackerAgent?.Character is BLCharacterObject characterObject && blow.StrikeType == StrikeType.Thrust && characterObject.GetPerkValue(DefaultPerks.Polearm.KeepAtBay))
            {
                num += DefaultPerks.Polearm.KeepAtBay.PrimaryBonus;
            }

            return num;
        }

        public override float GetKnockDownPenetration(Agent attackerAgent, WeaponComponentData attackerWeapon, in Blow blow, in AttackCollisionData collisionData)
        {
            float num = 0f;
            if (attackerWeapon.WeaponClass == WeaponClass.Boulder)
            {
                num += 0.25f;
            }
            else if (attackerWeapon.IsMeleeWeapon)
            {
                BLCharacterObject characterObject = attackerAgent?.Character as BLCharacterObject;
                if (blow.StrikeType == StrikeType.Swing)
                {
                    if (collisionData.VictimHitBodyPart == BoneBodyPartType.Legs)
                    {
                        num += 0.1f;
                    }

                    if (characterObject != null && attackerWeapon.RelevantSkill == DefaultSkills.TwoHanded && characterObject.GetPerkValue(DefaultPerks.TwoHanded.ShowOfStrength))
                    {
                        num += DefaultPerks.TwoHanded.ShowOfStrength.PrimaryBonus;
                    }
                }

                if (collisionData.VictimHitBodyPart == BoneBodyPartType.Head)
                {
                    num += 0.15f;
                }

                if (characterObject != null && attackerWeapon.RelevantSkill == DefaultSkills.Polearm && characterObject.GetPerkValue(DefaultPerks.Polearm.HardKnock))
                {
                    num += DefaultPerks.Polearm.HardKnock.PrimaryBonus;
                }
            }

            return num;
        }

        public override float GetHorseChargePenetration()
        {
            return 0.4f;
        }

        public override float CalculateStaggerThresholdDamage(Agent defenderAgent, in Blow blow)
        {
            float num = 1f;
            BLCharacterObject characterObject = defenderAgent.Character as BLCharacterObject;
            BLCharacterObject characterObject2 = defenderAgent.Formation?.Captain?.Character as BLCharacterObject;
            if (characterObject != null)
            {
                if (characterObject2 == characterObject)
                {
                    characterObject2 = null;
                }

                ExplainedNumber bonuses = new ExplainedNumber(1f);
                if (defenderAgent.HasMount)
                {
                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.DauntlessSteed, characterObject, isPrimaryBonus: true, ref bonuses);
                }
                else
                {
                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Spartan, characterObject, isPrimaryBonus: true, ref bonuses);
                }

                WeaponComponentData currentUsageItem = defenderAgent.WieldedWeapon.CurrentUsageItem;
                if (currentUsageItem != null && currentUsageItem.WeaponClass == WeaponClass.Crossbow && defenderAgent.WieldedWeapon.IsReloading)
                {
                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.DeftHands, characterObject, isPrimaryBonus: true, ref bonuses);
                    if (characterObject2 != null)
                    {
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.DeftHands, characterObject2, ref bonuses);
                    }
                }

                num = bonuses.ResultNumber;
            }

            TaleWorlds.Core.ManagedParametersEnum managedParameterEnum = ((blow.DamageType == DamageTypes.Cut) ? TaleWorlds.Core.ManagedParametersEnum.DamageInterruptAttackThresholdCut : ((blow.DamageType != DamageTypes.Pierce) ? TaleWorlds.Core.ManagedParametersEnum.DamageInterruptAttackThresholdBlunt : TaleWorlds.Core.ManagedParametersEnum.DamageInterruptAttackThresholdPierce));
            return TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(managedParameterEnum) * num;
        }

        public override float CalculateAlternativeAttackDamage(BasicCharacterObject attackerCharacter, WeaponComponentData weapon)
        {
            if (weapon == null)
            {
                return 2f;
            }

            if (weapon.WeaponClass == WeaponClass.LargeShield)
            {
                return 2f;
            }

            if (weapon.WeaponClass == WeaponClass.SmallShield)
            {
                return 1f;
            }

            if (weapon.IsTwoHanded)
            {
                return 2f;
            }

            return 1f;
        }

        public override float CalculatePassiveAttackDamage(BasicCharacterObject attackerCharacter, in AttackCollisionData collisionData, float baseDamage)
        {
            if (attackerCharacter is BLCharacterObject characterObject && collisionData.AttackBlockedWithShield && characterObject.GetPerkValue(DefaultPerks.Polearm.UnstoppableForce))
            {
                baseDamage *= DefaultPerks.Polearm.UnstoppableForce.PrimaryBonus;
            }

            return baseDamage;
        }

        public override MeleeCollisionReaction DecidePassiveAttackCollisionReaction(Agent attacker, Agent defender, bool isFatalHit)
        {
            MeleeCollisionReaction result = MeleeCollisionReaction.Bounced;
            if (isFatalHit && attacker.HasMount)
            {
                float num = 0.05f;
                if (attacker.Character is BLCharacterObject characterObject && characterObject.GetPerkValue(DefaultPerks.Polearm.Skewer))
                {
                    num += DefaultPerks.Polearm.Skewer.PrimaryBonus;
                }

                if (MBRandom.RandomFloat < num)
                {
                    result = MeleeCollisionReaction.SlicedThrough;
                }
            }

            return result;
        }

        public override float CalculateShieldDamage(in AttackInformation attackInformation, float baseDamage)
        {
            Formation victimFormation = attackInformation.VictimFormation;
            ExplainedNumber bonuses = new ExplainedNumber(baseDamage);
            BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(victimFormation);
            if (activeBanner != null)
            {
                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedShieldDamage, activeBanner, ref bonuses);
            }

            return bonuses.ResultNumber;
        }

        public override float GetDamageMultiplierForBodyPart(BoneBodyPartType bodyPart, DamageTypes type, bool isHuman, bool isMissile)
        {
            float result = 1f;
            switch (bodyPart)
            {
                case BoneBodyPartType.None:
                    result = 1f;
                    break;
                case BoneBodyPartType.Head:
                    switch (type)
                    {
                        case DamageTypes.Invalid:
                            result = 1.5f;
                            break;
                        case DamageTypes.Cut:
                            result = 1.2f;
                            break;
                        case DamageTypes.Pierce:
                            result = ((!isHuman) ? 1.2f : (isMissile ? 2f : 1.25f));
                            break;
                        case DamageTypes.Blunt:
                            result = 1.2f;
                            break;
                    }

                    break;
                case BoneBodyPartType.Neck:
                    switch (type)
                    {
                        case DamageTypes.Invalid:
                            result = 1.5f;
                            break;
                        case DamageTypes.Cut:
                            result = 1.2f;
                            break;
                        case DamageTypes.Pierce:
                            result = ((!isHuman) ? 1.2f : (isMissile ? 2f : 1.25f));
                            break;
                        case DamageTypes.Blunt:
                            result = 1.2f;
                            break;
                    }

                    break;
                case BoneBodyPartType.Chest:
                case BoneBodyPartType.Abdomen:
                case BoneBodyPartType.ShoulderLeft:
                case BoneBodyPartType.ShoulderRight:
                case BoneBodyPartType.ArmLeft:
                case BoneBodyPartType.ArmRight:
                    result = (isHuman ? 1f : 0.8f);
                    break;
                case BoneBodyPartType.Legs:
                    result = 0.8f;
                    break;
            }

            return result;
        }

        public override bool DecideAgentShrugOffBlow(Agent victimAgent, AttackCollisionData collisionData, in Blow blow)
        {
            return MissionCombatMechanicsHelper.DecideAgentShrugOffBlow(victimAgent, collisionData, in blow);
        }

        public override bool DecideAgentDismountedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            return MissionCombatMechanicsHelper.DecideAgentDismountedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
        }

        public override bool DecideAgentKnockedBackByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            return MissionCombatMechanicsHelper.DecideAgentKnockedBackByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
        }

        public override bool DecideAgentKnockedDownByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            return MissionCombatMechanicsHelper.DecideAgentKnockedDownByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
        }

        public override bool DecideMountRearedByBlow(Agent attackerAgent, Agent victimAgent, in AttackCollisionData collisionData, WeaponComponentData attackerWeapon, in Blow blow)
        {
            return MissionCombatMechanicsHelper.DecideMountRearedByBlow(attackerAgent, victimAgent, in collisionData, attackerWeapon, in blow);
        }
    }
}
