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
    public class BLStrikeMagnitudeModel : StrikeMagnitudeCalculationModel
    {
        public override float CalculateHorseArcheryFactor(BasicCharacterObject characterObject)
        {
            return 100f;
        }

        public override float CalculateStrikeMagnitudeForMissile(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float missileSpeed)
        {
            BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
            WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
            float missileTotalDamage = collisionData.MissileTotalDamage;
            float missileStartingBaseSpeed = collisionData.MissileStartingBaseSpeed;
            float num = missileSpeed;
            float num2 = missileSpeed - missileStartingBaseSpeed;
            if (num2 > 0f)
            {
                ExplainedNumber bonuses = new ExplainedNumber(0f, includeDescriptions: false, null);
                if (attackerAgentCharacter is BLCharacterObject characterObject && characterObject.IsHero)
                {
                    WeaponClass ammoClass = currentUsageItem.AmmoClass;
                    if (ammoClass == WeaponClass.Stone || ammoClass == WeaponClass.ThrowingAxe || ammoClass == WeaponClass.ThrowingKnife || ammoClass == WeaponClass.Javelin)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.RunningThrow, characterObject, isPrimaryBonus: true, ref bonuses);
                    }
                }

                num += num2 * bonuses.ResultNumber;
            }

            num /= missileStartingBaseSpeed;
            return num * num * missileTotalDamage;
        }

        public override float CalculateStrikeMagnitudeForSwing(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float swingSpeed, float impactPointAsPercent, float extraLinearSpeed)
        {
            BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
            BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
            bool doesAttackerHaveMountAgent = attackInformation.DoesAttackerHaveMountAgent;
            WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
            BLCharacterObject characterObject = attackerAgentCharacter as BLCharacterObject;
            ExplainedNumber bonuses = new ExplainedNumber(extraLinearSpeed);
            if (characterObject != null && extraLinearSpeed > 0f)
            {
                SkillObject relevantSkill = currentUsageItem.RelevantSkill;
                BLCharacterObject captainCharacter = attackerCaptainCharacter as BLCharacterObject;
                if (doesAttackerHaveMountAgent)
                {
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.NomadicTraditions, captainCharacter, ref bonuses);
                }
                else
                {
                    if (relevantSkill == DefaultSkills.TwoHanded)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.RecklessCharge, characterObject, isPrimaryBonus: true, ref bonuses);
                    }

                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DashAndSlash, characterObject, isPrimaryBonus: true, ref bonuses);
                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.SurgingBlow, characterObject, isPrimaryBonus: true, ref bonuses);
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.SurgingBlow, captainCharacter, ref bonuses);
                }

                if (relevantSkill == DefaultSkills.Polearm)
                {
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Lancer, captainCharacter, ref bonuses);
                    if (doesAttackerHaveMountAgent)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Lancer, characterObject, isPrimaryBonus: true, ref bonuses);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.UnstoppableForce, captainCharacter, ref bonuses);
                    }
                }
            }

            ItemObject item = weapon.Item;
            float num = CombatStatCalculator.CalculateStrikeMagnitudeForSwing(swingSpeed, impactPointAsPercent, item.Weight, currentUsageItem.GetRealWeaponLength(), currentUsageItem.Inertia, currentUsageItem.CenterOfMass, bonuses.ResultNumber);
            if (item.IsCraftedByPlayer)
            {
                ExplainedNumber bonuses2 = new ExplainedNumber(num);
                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crafting.SharpenedEdge, characterObject, isPrimaryBonus: true, ref bonuses2);
                num = bonuses2.ResultNumber;
            }

            return num;
        }

        public override float CalculateStrikeMagnitudeForThrust(in AttackInformation attackInformation, in AttackCollisionData collisionData, in MissionWeapon weapon, float thrustWeaponSpeed, float extraLinearSpeed, bool isThrown = false)
        {
            BasicCharacterObject attackerAgentCharacter = attackInformation.AttackerAgentCharacter;
            BasicCharacterObject attackerCaptainCharacter = attackInformation.AttackerCaptainCharacter;
            bool doesAttackerHaveMountAgent = attackInformation.DoesAttackerHaveMountAgent;
            ItemObject item = weapon.Item;
            float weight = item.Weight;
            WeaponComponentData currentUsageItem = weapon.CurrentUsageItem;
            BLCharacterObject characterObject = attackerAgentCharacter as BLCharacterObject;
            ExplainedNumber bonuses = new ExplainedNumber(extraLinearSpeed);
            if (characterObject != null && extraLinearSpeed > 0f)
            {
                SkillObject relevantSkill = currentUsageItem.RelevantSkill;
                BLCharacterObject captainCharacter = attackerCaptainCharacter as BLCharacterObject;
                if (doesAttackerHaveMountAgent)
                {
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.NomadicTraditions, captainCharacter, ref bonuses);
                }
                else
                {
                    if (relevantSkill == DefaultSkills.TwoHanded)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.RecklessCharge, characterObject, isPrimaryBonus: true, ref bonuses);
                    }

                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DashAndSlash, characterObject, isPrimaryBonus: true, ref bonuses);
                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.SurgingBlow, characterObject, isPrimaryBonus: true, ref bonuses);
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.SurgingBlow, captainCharacter, ref bonuses);
                }

                if (relevantSkill == DefaultSkills.Polearm)
                {
                    BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Lancer, captainCharacter, ref bonuses);
                    if (doesAttackerHaveMountAgent)
                    {
                        BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Lancer, characterObject, isPrimaryBonus: true, ref bonuses);
                        BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.UnstoppableForce, captainCharacter, ref bonuses);
                    }
                }
            }

            float num = CombatStatCalculator.CalculateStrikeMagnitudeForThrust(thrustWeaponSpeed, weight, bonuses.ResultNumber, isThrown);
            if (item.IsCraftedByPlayer)
            {
                ExplainedNumber bonuses2 = new ExplainedNumber(num);
                BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crafting.SharpenedTip, characterObject, isPrimaryBonus: true, ref bonuses2);
                num = bonuses2.ResultNumber;
            }

            return num;
        }

        public override float ComputeRawDamage(DamageTypes damageType, float magnitude, float armorEffectiveness, float absorbedDamageRatio)
        {
            float bluntDamageFactorByDamageType = GetBluntDamageFactorByDamageType(damageType);
            float num = 50f / (50f + armorEffectiveness);
            float num2 = magnitude * num;
            float num3 = bluntDamageFactorByDamageType * num2;
            float num4;
            switch (damageType)
            {
                case DamageTypes.Cut:
                    num4 = MathF.Max(0f, num2 - armorEffectiveness * 0.5f);
                    break;
                case DamageTypes.Pierce:
                    num4 = MathF.Max(0f, num2 - armorEffectiveness * 0.33f);
                    break;
                case DamageTypes.Blunt:
                    num4 = MathF.Max(0f, num2 - armorEffectiveness * 0.2f);
                    break;
                default:
                    Debug.FailedAssert("Given damage type is invalid.", "C:\\Develop\\MB3\\Source\\Bannerlord\\SandBox\\GameComponents\\SandboxStrikeMagnitudeModel.cs", "ComputeRawDamage", 210);
                    return 0f;
            }

            num3 += (1f - bluntDamageFactorByDamageType) * num4;
            return num3 * absorbedDamageRatio;
        }

        public override float GetBluntDamageFactorByDamageType(DamageTypes damageType)
        {
            float result = 0f;
            switch (damageType)
            {
                case DamageTypes.Blunt:
                    result = 0.6f;
                    break;
                case DamageTypes.Cut:
                    result = 0.1f;
                    break;
                case DamageTypes.Pierce:
                    result = 0.25f;
                    break;
            }

            return result;
        }

        public override float CalculateAdjustedArmorForBlow(float baseArmor, BasicCharacterObject attackerCharacter, BasicCharacterObject attackerCaptainCharacter, BasicCharacterObject victimCharacter, BasicCharacterObject victimCaptainCharacter, WeaponComponentData weaponComponent)
        {
            bool flag = false;
            float num = baseArmor;
            BLCharacterObject characterObject = attackerCharacter as BLCharacterObject;
            BLCharacterObject characterObject2 = attackerCaptainCharacter as BLCharacterObject;
            if (attackerCharacter == characterObject2)
            {
                characterObject2 = null;
            }

            if (num > 0f && characterObject != null)
            {
                if (weaponComponent != null && weaponComponent.RelevantSkill == DefaultSkills.Crossbow && characterObject.GetPerkValue(DefaultPerks.Crossbow.Piercer) && baseArmor < DefaultPerks.Crossbow.Piercer.PrimaryBonus)
                {
                    flag = true;
                }

                if (flag)
                {
                    num = 0f;
                }
                else
                {
                    ExplainedNumber bonuses = new ExplainedNumber(baseArmor);
                    BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.Vandal, characterObject, isPrimaryBonus: true, ref bonuses);
                    if (weaponComponent != null)
                    {
                        if (weaponComponent.RelevantSkill == DefaultSkills.OneHanded)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.ChinkInTheArmor, characterObject, isPrimaryBonus: true, ref bonuses);
                        }
                        else if (weaponComponent.RelevantSkill == DefaultSkills.Bow)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.Bodkin, characterObject, isPrimaryBonus: true, ref bonuses);
                            if (characterObject2 != null)
                            {
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.Bodkin, characterObject2, ref bonuses);
                            }
                        }
                        else if (weaponComponent.RelevantSkill == DefaultSkills.Crossbow)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Puncture, characterObject, isPrimaryBonus: true, ref bonuses);
                            if (characterObject2 != null)
                            {
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.Puncture, characterObject2, ref bonuses);
                            }
                        }
                        else if (weaponComponent.RelevantSkill == DefaultSkills.Throwing)
                        {
                            BLPerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.WeakSpot, characterObject, isPrimaryBonus: true, ref bonuses);
                            if (characterObject2 != null)
                            {
                                BLPerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.WeakSpot, characterObject2, ref bonuses);
                            }
                        }
                    }

                    float num2 = bonuses.ResultNumber - baseArmor;
                    num = MathF.Max(0f, baseArmor - num2);
                }
            }

            return num;
        }
    }
}