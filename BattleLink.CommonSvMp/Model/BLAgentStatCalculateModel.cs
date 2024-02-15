using Helpers;
using System;
using System.Collections.Generic;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.ComponentInterfaces;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.CampaignSystem.Extensions;

namespace BattleLink.Common.Model;

// try to copy SandboxAgentStatCalculateModel
public class BLAgentStatCalculateModel : MultiplayerAgentStatCalculateModel
{

    public BLAgentStatCalculateModel() : base()
    {
    }

    public override void UpdateAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        if (agent.IsHuman)
        {
            UpdateHumanAgentStats(agent, agentDrivenProperties);
        }
        else if (agent.IsMount)
        {
            UpdateMountAgentStats(agent, agentDrivenProperties);
        }
    }

    private void UpdateHumanAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        MPPerkObject.MPPerkHandler perkHandler = MPPerkObject.GetPerkHandler(agent);
        BasicCharacterObject character = agent.Character;
        MissionEquipment equipment = agent.Equipment;
        float num1 = equipment.GetTotalWeightOfWeapons() * (float)(1.0 + (perkHandler != null ? (double)perkHandler.GetEncumbrance(true) : 0.0));
        EquipmentIndex wieldedItemIndex1 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        EquipmentIndex wieldedItemIndex2 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        if (wieldedItemIndex1 != EquipmentIndex.None)
        {
            ItemObject itemObject = equipment[wieldedItemIndex1].Item;
            WeaponComponent weaponComponent = itemObject.WeaponComponent;
            if (weaponComponent != null)
            {
                float realWeaponLength = weaponComponent.PrimaryWeapon.GetRealWeaponLength();
                float num2 = (weaponComponent.GetItemType() == ItemObject.ItemTypeEnum.Bow ? 4f : 1.5f) * itemObject.Weight * MathF.Sqrt(realWeaponLength) * (float)(1.0 + (perkHandler != null ? (double)perkHandler.GetEncumbrance(false) : 0.0));
                num1 += num2;
            }
        }
        if (wieldedItemIndex2 != EquipmentIndex.None)
        {
            float num3 = 1.5f * equipment[wieldedItemIndex2].Item.Weight * (float)(1.0 + (perkHandler != null ? (double)perkHandler.GetEncumbrance(false) : 0.0));
            num1 += num3;
        }
        agentDrivenProperties.WeaponsEncumbrance = num1;
        EquipmentIndex wieldedItemIndex3 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        WeaponComponentData weaponComponentData = wieldedItemIndex3 != EquipmentIndex.None ? equipment[wieldedItemIndex3].CurrentUsageItem : (WeaponComponentData)null;
        ItemObject primaryItem = wieldedItemIndex3 != EquipmentIndex.None ? equipment[wieldedItemIndex3].Item : (ItemObject)null;
        EquipmentIndex wieldedItemIndex4 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        WeaponComponentData secondaryItem = wieldedItemIndex4 != EquipmentIndex.None ? equipment[wieldedItemIndex4].CurrentUsageItem : (WeaponComponentData)null;
        agentDrivenProperties.SwingSpeedMultiplier = (float)(0.930000007152557 + 0.000699999975040555 * (double)this.GetSkillValueForItem(character, primaryItem));
        agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = agentDrivenProperties.SwingSpeedMultiplier;
        agentDrivenProperties.HandlingMultiplier = 1f;
        agentDrivenProperties.ShieldBashStunDurationMultiplier = 1f;
        agentDrivenProperties.KickStunDurationMultiplier = 1f;
        agentDrivenProperties.ReloadSpeed = (float)(0.930000007152557 + 0.000699999975040555 * (double)this.GetSkillValueForItem(character, primaryItem));
        agentDrivenProperties.MissileSpeedMultiplier = 1f;
        agentDrivenProperties.ReloadMovementPenaltyFactor = 1f;
        this.SetAllWeaponInaccuracy(agent, agentDrivenProperties, (int)wieldedItemIndex3, weaponComponentData);
        //MultiplayerClassDivisions.MPHeroClass classForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
        //float num4 = classForCharacter.IsTroopCharacter(agent.Character) ? classForCharacter.TroopMovementSpeedMultiplier : classForCharacter.HeroMovementSpeedMultiplier;
        float movementSpeedMultiplier = 1;
        agentDrivenProperties.MaxSpeedMultiplier = (float)(1.04999995231628 * ((double)movementSpeedMultiplier * (100.0 / (100.0 + (double)num1))));
        int skillValue = character.GetSkillValue(DefaultSkills.Riding);
        bool flag1 = false;
        bool flag2 = false;
        if (weaponComponentData != null)
        {
            WeaponComponentData weapon = weaponComponentData;
            int effectiveSkillForWeapon = this.GetEffectiveSkillForWeapon(agent, weapon);
            if (perkHandler != null)
                agentDrivenProperties.MissileSpeedMultiplier *= perkHandler.GetThrowingWeaponSpeed(weaponComponentData) + 1f;
            if (weapon.IsRangedWeapon)
            {
                int thrustSpeed = weapon.ThrustSpeed;
                if (!agent.HasMount)
                {
                    float num5 = MathF.Max(0.0f, (float)(1.0 - (double)effectiveSkillForWeapon / 500.0));
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = 0.125f * num5;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = 0.1f * num5;
                }
                else
                {
                    float num6 = MathF.Max(0.0f, (float)((1.0 - (double)effectiveSkillForWeapon / 500.0) * (1.0 - (double)skillValue / 1800.0)));
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = 0.025f * num6;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = 0.06f * num6;
                }
                agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = MathF.Max(0.0f, agentDrivenProperties.WeaponMaxMovementAccuracyPenalty);
                agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = MathF.Max(0.0f, agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty);
                if (weapon.RelevantSkill == DefaultSkills.Bow)
                {
                    float amount = MBMath.ClampFloat((float)(((double)thrustSpeed - 60.0) / 75.0), 0.0f, 1f);
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 6f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, amount, 1E-05f);
                }
                else if (weapon.RelevantSkill == DefaultSkills.Throwing)
                {
                    float amount = MBMath.ClampFloat((float)(((double)thrustSpeed - 85.0) / 17.0), 0.0f, 1f);
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 0.5f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.5f * MBMath.Lerp(1.5f, 0.8f, amount, 1E-05f);
                }
                else if (weapon.RelevantSkill == DefaultSkills.Crossbow)
                {
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 2.5f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.2f;
                }
                if (weapon.WeaponClass == WeaponClass.Bow)
                {
                    flag1 = true;
                    agentDrivenProperties.WeaponBestAccuracyWaitTime = (float)(0.300000011920929 + (95.75 - (double)thrustSpeed) * 0.00499999988824129);
                    float amount = MBMath.ClampFloat((float)(((double)thrustSpeed - 60.0) / 75.0), 0.0f, 1f);
                    agentDrivenProperties.WeaponUnsteadyBeginTime = (float)(0.100000001490116 + (double)effectiveSkillForWeapon * 0.00999999977648258 * (double)MBMath.Lerp(1f, 2f, amount, 1E-05f));
                    if (agent.IsAIControlled)
                        agentDrivenProperties.WeaponUnsteadyBeginTime *= 4f;
                    agentDrivenProperties.WeaponUnsteadyEndTime = 2f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                    agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
                else if (weapon.WeaponClass == WeaponClass.Javelin || weapon.WeaponClass == WeaponClass.ThrowingAxe || weapon.WeaponClass == WeaponClass.ThrowingKnife)
                {
                    agentDrivenProperties.WeaponBestAccuracyWaitTime = (float)(0.200000002980232 + (89.0 - (double)thrustSpeed) * 0.00899999961256981);
                    agentDrivenProperties.WeaponUnsteadyBeginTime = (float)(2.5 + (double)effectiveSkillForWeapon * 0.00999999977648258);
                    agentDrivenProperties.WeaponUnsteadyEndTime = 10f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                    agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                    if (weapon.WeaponClass == WeaponClass.ThrowingAxe)
                        agentDrivenProperties.WeaponInaccuracy *= 6.6f;
                }
                else
                {
                    agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.1f;
                    agentDrivenProperties.WeaponUnsteadyBeginTime = 0.0f;
                    agentDrivenProperties.WeaponUnsteadyEndTime = 0.0f;
                    agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
            }
            else if (weapon.WeaponFlags.HasAllFlags<WeaponFlags>(WeaponFlags.WideGrip))
            {
                flag2 = true;
                agentDrivenProperties.WeaponUnsteadyBeginTime = (float)(1.0 + (double)effectiveSkillForWeapon * 0.00499999988824129);
                agentDrivenProperties.WeaponUnsteadyEndTime = (float)(3.0 + (double)effectiveSkillForWeapon * 0.00999999977648258);
            }
        }
        agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;
        Agent mountAgent = agent.MountAgent;
        float num7 = mountAgent != null ? mountAgent.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) : 1f;
        agentDrivenProperties.AttributeRiding = (float)skillValue * num7;
        agentDrivenProperties.AttributeHorseArchery = MissionGameModels.Current.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
        agentDrivenProperties.BipedalRangedReadySpeedMultiplier = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
        agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);
        if (perkHandler != null)
        {
            for (int index = 55; index < 84; ++index)
            {
                DrivenProperty drivenProperty = (DrivenProperty)index;
                if (((drivenProperty == DrivenProperty.WeaponUnsteadyBeginTime ? 0 : (drivenProperty != DrivenProperty.WeaponUnsteadyEndTime ? 1 : 0)) | (flag1 ? 1 : 0) | (flag2 ? 1 : 0)) != 0 && drivenProperty != DrivenProperty.WeaponRotationalAccuracyPenaltyInRadians | flag1)
                {
                    float stat = agentDrivenProperties.GetStat(drivenProperty);
                    agentDrivenProperties.SetStat(drivenProperty, stat + perkHandler.GetDrivenPropertyBonus(drivenProperty, stat));
                }
            }
        }
        if (agent.Character != null && agent.HasMount && weaponComponentData != null)
            this.SetMountedWeaponPenaltiesOnAgent(agent, agentDrivenProperties, weaponComponentData);
        this.SetAiRelatedProperties(agent, agentDrivenProperties, weaponComponentData, secondaryItem);
    }
    private int GetSkillValueForItem(BasicCharacterObject characterObject, ItemObject primaryItem)
    {
        return characterObject.GetSkillValue(primaryItem != null ? primaryItem.RelevantSkill : DefaultSkills.Athletics);
    }

    private void SetMountedWeaponPenaltiesOnAgent(
        Agent agent,
        AgentDrivenProperties agentDrivenProperties,
        WeaponComponentData equippedWeaponComponent)
    {
        int effectiveSkill = this.GetEffectiveSkill(agent, DefaultSkills.Riding);
        float num1 = (float)(0.300000011920929 - (double)effectiveSkill * (3.0 / 1000.0));
        if ((double)num1 > 0.0)
        {
            float val2_1 = agentDrivenProperties.SwingSpeedMultiplier * (1f - num1);
            float val2_2 = agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier * (1f - num1);
            float val2_3 = agentDrivenProperties.ReloadSpeed * (1f - num1);
            float val2_4 = agentDrivenProperties.WeaponBestAccuracyWaitTime * (1f + num1);
            agentDrivenProperties.SwingSpeedMultiplier = Math.Max(0.0f, val2_1);
            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = Math.Max(0.0f, val2_2);
            agentDrivenProperties.ReloadSpeed = Math.Max(0.0f, val2_3);
            agentDrivenProperties.WeaponBestAccuracyWaitTime = Math.Max(0.0f, val2_4);
        }
        float num2 = (float)(15.0 - (double)effectiveSkill * 0.150000005960464);
        if ((double)num2 <= 0.0)
            return;
        float val2 = agentDrivenProperties.WeaponInaccuracy * (1f + num2);
        agentDrivenProperties.WeaponInaccuracy = Math.Max(0.0f, val2);
    }
    private void UpdateMountAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        MPPerkObject.MPPerkHandler perkHandler = MPPerkObject.GetPerkHandler(agent.RiderAgent);
        EquipmentElement mountElement = agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot];
        EquipmentElement equipmentElement = agent.SpawnEquipment[EquipmentIndex.HorseHarness];
        agentDrivenProperties.MountManeuver = (float)mountElement.GetModifiedMountManeuver(in equipmentElement) * (float)(1.0 + (perkHandler != null ? (double)perkHandler.GetMountManeuver() : 0.0));
        agentDrivenProperties.MountSpeed = (float)((double)(mountElement.GetModifiedMountSpeed(in equipmentElement) + 1) * 0.219999998807907 * (1.0 + (perkHandler != null ? (double)perkHandler.GetMountSpeed() : 0.0)));
        Agent riderAgent = agent.RiderAgent;
        int ridingSkill = riderAgent != null ? riderAgent.Character.GetSkillValue(DefaultSkills.Riding) : 100;
        agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(in mountElement, in equipmentElement, ridingSkill);
        agentDrivenProperties.MountSpeed *= (float)(1.0 + (double)ridingSkill * (2.0 / 625.0));
        agentDrivenProperties.MountManeuver *= (float)(1.0 + (double)ridingSkill * 0.00350000010803342);
        float num = (float)((double)mountElement.Weight / 2.0 + (equipmentElement.IsEmpty ? 0.0 : (double)equipmentElement.Weight));
        agentDrivenProperties.MountDashAccelerationMultiplier = (double)num > 200.0 ? ((double)num < 300.0 ? (float)(1.0 - ((double)num - 200.0) / 111.0) : 0.1f) : 1f;
    }

    //public override void InitializeAgentStats(Agent agent, TaleWorlds.Core.Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
    //{
    //    agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
    //    if (!agent.IsHuman)
    //    {
    //        InitializeHorseAgentStats(agent, spawnEquipment, agentDrivenProperties);
    //    }
    //    else
    //    {
    //        agentDrivenProperties = this.InitializeHumanAgentStats(agent, agentDrivenProperties, agentBuildData);
    //    }
    //}
  //  private static void InitializeHorseAgentStats(
  //Agent agent,
  //TaleWorlds.Core.Equipment spawnEquipment,
  //AgentDrivenProperties agentDrivenProperties)
  //  {
  //      agentDrivenProperties.AiSpeciesIndex = agent.Monster.FamilyType;
  //      agentDrivenProperties.AttributeRiding = (float)(0.800000011920929 + (spawnEquipment[EquipmentIndex.HorseHarness].Item != null ? 0.200000002980232 : 0.0));
  //      float num1 = 0.0f;
  //      EquipmentElement equipmentElement1;
  //      for (int index = 1; index < 12; ++index)
  //      {
  //          equipmentElement1 = spawnEquipment[index];
  //          if (equipmentElement1.Item != null)
  //          {
  //              double num2 = (double)num1;
  //              equipmentElement1 = spawnEquipment[index];
  //              double modifiedMountBodyArmor = (double)equipmentElement1.GetModifiedMountBodyArmor();
  //              num1 = (float)(num2 + modifiedMountBodyArmor);
  //          }
  //      }
  //      agentDrivenProperties.ArmorTorso = num1;
  //      equipmentElement1 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
  //      HorseComponent horseComponent = equipmentElement1.Item.HorseComponent;
  //      EquipmentElement equipmentElement2 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
  //      AgentDrivenProperties drivenProperties = agentDrivenProperties;
  //      ref EquipmentElement local1 = ref equipmentElement2;
  //      equipmentElement1 = spawnEquipment[EquipmentIndex.HorseHarness];
  //      ref EquipmentElement local2 = ref equipmentElement1;
  //      double num3 = (double)local1.GetModifiedMountCharge(in local2) * 0.00999999977648258;
  //      drivenProperties.MountChargeDamage = (float)num3;
  //      agentDrivenProperties.MountDifficulty = (float)equipmentElement2.Item.Difficulty;
  //  }

    public override void InitializeAgentStats(Agent agent, Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
    {
        agentDrivenProperties.ArmorEncumbrance = GetEffectiveArmorEncumbrance(agent, spawnEquipment);
        if (agent.IsHero)
        {
            CharacterObject obj = agent.Character as CharacterObject;
            AgentFlag agentFlag = agent.GetAgentFlags();
            if (obj.GetPerkValue(DefaultPerks.Bow.HorseMaster))
            {
                agentFlag |= AgentFlag.CanUseAllBowsMounted;
            }

            if (obj.GetPerkValue(DefaultPerks.Crossbow.MountedCrossbowman))
            {
                agentFlag |= AgentFlag.CanReloadAllXBowsMounted;
            }

            if (obj.GetPerkValue(DefaultPerks.TwoHanded.ProjectileDeflection))
            {
                agentFlag |= AgentFlag.CanDeflectArrowsWith2HSword;
            }

            agent.SetAgentFlags(agentFlag);
        }
        else
        {
            agent.HealthLimit = GetEffectiveMaxHealth(agent);
            agent.Health = agent.HealthLimit;
        }

        UpdateAgentStats(agent, agentDrivenProperties);
    }

    //private AgentDrivenProperties InitializeHumanAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
    //{
    //    MultiplayerClassDivisions.MPHeroClass mPHeroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
    //    if (mPHeroClassForCharacter != null)
    //    {
    //        FillAgentStatsFromData(ref agentDrivenProperties, agent, mPHeroClassForCharacter, agentBuildData?.AgentMissionPeer, agentBuildData?.OwningAgentMissionPeer);
    //        agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, MultiplayerOptions.OptionType.UseRealisticBlocking.GetBoolValue() ? 1f : 0f);
    //    }

    //    if (mPHeroClassForCharacter != null)
    //    {
    //        agent.BaseHealthLimit = mPHeroClassForCharacter.Health;
    //    }
    //    else
    //    {
    //        agent.BaseHealthLimit = 100f;
    //    }

    //    agent.HealthLimit = agent.BaseHealthLimit;
    //    agent.Health = agent.HealthLimit;
    //    return agentDrivenProperties;
    //}

    //private void FillAgentStatsFromData(ref AgentDrivenProperties agentDrivenProperties, Agent agent, MultiplayerClassDivisions.MPHeroClass heroClass, MissionPeer _missionPeer, MissionPeer owningMissionPeer)
    //{
    //    MissionPeer missionPeer = _missionPeer ?? owningMissionPeer;
    //    if (missionPeer != null)
    //    {
    //        //MPPerkObject.MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler(missionPeer);
    //        bool isPlayer = _missionPeer != null;
    //        for (int i = 0; i < 55; i++)
    //        {
    //            DrivenProperty drivenProperty = (DrivenProperty)i;
    //            float stat = agentDrivenProperties.GetStat(drivenProperty);
    //            if (drivenProperty == DrivenProperty.ArmorHead || drivenProperty == DrivenProperty.ArmorTorso || drivenProperty == DrivenProperty.ArmorLegs || drivenProperty == DrivenProperty.ArmorArms)
    //            {
    //                agentDrivenProperties.SetStat(drivenProperty, stat + (float)heroClass.ArmorValue);//+ onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(isPlayer, drivenProperty, stat)
    //            }
    //            else
    //            {
    //                agentDrivenProperties.SetStat(drivenProperty, stat);//+ onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(isPlayer, drivenProperty, stat)
    //            }
    //        }
    //    }

    //    float topSpeedReachDuration = (heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopTopSpeedReachDuration : heroClass.HeroTopSpeedReachDuration);
    //    agentDrivenProperties.TopSpeedReachDuration = topSpeedReachDuration;
    //    float managedParameter = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
    //    float managedParameter2 = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
    //    float num = (heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopCombatMovementSpeedMultiplier : heroClass.HeroCombatMovementSpeedMultiplier);
    //    agentDrivenProperties.CombatMaxSpeedMultiplier = managedParameter + (managedParameter2 - managedParameter) * num;
    //}

    public override bool CanAgentRideMount(Agent agent, Agent targetMount)
    {
        return agent.CheckSkillForMounting(targetMount);
    }

    public override void InitializeMissionEquipment(Agent agent)
    {
        if (!agent.IsHuman || !(agent.Character is CharacterObject characterObject))
        {
            return;
        }

        PartyBase partyBase = (PartyBase)(agent?.Origin?.BattleCombatant);
        MapEvent mapEvent = partyBase?.MapEvent;
        MobileParty mobileParty = ((partyBase != null && partyBase.IsMobile) ? partyBase.MobileParty : null);
        CharacterObject characterObject2 = PartyBaseHelper.GetVisualPartyLeader(partyBase);
        if (characterObject2 == characterObject)
        {
            characterObject2 = null;
        }

        MissionEquipment equipment = agent.Equipment;
        for (int i = 0; i < 5; i++)
        {
            EquipmentIndex equipmentIndex = (EquipmentIndex)i;
            MissionWeapon missionWeapon = equipment[equipmentIndex];
            if (missionWeapon.IsEmpty)
            {
                continue;
            }

            WeaponComponentData currentUsageItem = missionWeapon.CurrentUsageItem;
            if (currentUsageItem == null)
            {
                continue;
            }

            if (currentUsageItem.IsConsumable && currentUsageItem.RelevantSkill != null)
            {
                ExplainedNumber bonuses = new ExplainedNumber(0f, includeDescriptions: false, null);
                if (currentUsageItem.RelevantSkill == DefaultSkills.Bow)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.DeepQuivers, characterObject, isPrimaryBonus: true, ref bonuses);
                    if (characterObject2 != null && characterObject2.GetPerkValue(DefaultPerks.Bow.DeepQuivers))
                    {
                        bonuses.Add(DefaultPerks.Bow.DeepQuivers.SecondaryBonus);
                    }
                }
                else if (currentUsageItem.RelevantSkill == DefaultSkills.Crossbow)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Fletcher, characterObject, isPrimaryBonus: true, ref bonuses);
                    if (characterObject2 != null && characterObject2.GetPerkValue(DefaultPerks.Crossbow.Fletcher))
                    {
                        bonuses.Add(DefaultPerks.Crossbow.Fletcher.SecondaryBonus);
                    }
                }
                else if (currentUsageItem.RelevantSkill == DefaultSkills.Throwing)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.WellPrepared, characterObject, isPrimaryBonus: true, ref bonuses);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.Resourceful, characterObject, isPrimaryBonus: true, ref bonuses);
                    if (agent.HasMount)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.Saddlebags, characterObject, isPrimaryBonus: true, ref bonuses);
                    }

                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Throwing.WellPrepared, mobileParty, isPrimaryBonus: false, ref bonuses);
                }

                int num = MathF.Round(bonuses.ResultNumber);
                ExplainedNumber stat = new ExplainedNumber(missionWeapon.Amount + num);
                if (mobileParty != null && mapEvent != null && mapEvent.AttackerSide == partyBase.MapEventSide && mapEvent.EventType == MapEvent.BattleTypes.Siege)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Engineering.MilitaryPlanner, mobileParty, isPrimaryBonus: true, ref stat);
                }

                int num2 = MathF.Round(stat.ResultNumber);
                if (num2 != missionWeapon.Amount)
                {
                    equipment.SetAmountOfSlot(equipmentIndex, (short)num2, addOverflowToMaxAmount: true);
                }
            }
            else if (currentUsageItem.IsShield)
            {
                ExplainedNumber bonuses2 = new ExplainedNumber(missionWeapon.HitPoints);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Engineering.Scaffolds, characterObject, isPrimaryBonus: false, ref bonuses2);
                int num3 = MathF.Round(bonuses2.ResultNumber);
                if (num3 != missionWeapon.HitPoints)
                {
                    equipment.SetHitPointsOfSlot(equipmentIndex, (short)num3, addOverflowToMaxHitPoints: true);
                }
            }
        }
    }



    public override int GetEffectiveSkill(Agent agent, SkillObject skill)
    {
        ExplainedNumber bonuses = new ExplainedNumber(base.GetEffectiveSkill(agent, skill));
        CharacterObject characterObject = agent.Character as CharacterObject;
        Formation formation = agent.Formation;
        PartyBase partyBase = (PartyBase)(agent.Origin?.BattleCombatant);
        MobileParty mobileParty = ((partyBase != null && partyBase.IsMobile) ? partyBase.MobileParty : null);
        CharacterObject characterObject2 = formation?.Captain?.Character as CharacterObject;
        if (characterObject2 == characterObject)
        {
            characterObject2 = null;
        }

        if (characterObject2 != null)
        {
            bool flag = skill == DefaultSkills.Bow || skill == DefaultSkills.Crossbow || skill == DefaultSkills.Throwing;
            bool flag2 = skill == DefaultSkills.OneHanded || skill == DefaultSkills.TwoHanded || skill == DefaultSkills.Polearm;
            if ((characterObject.IsInfantry && flag) || (characterObject.IsRanged && flag2))
            {
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.FlexibleFighter, characterObject2, ref bonuses);
            }
        }

        if (skill == DefaultSkills.Bow)
        {
            if (characterObject2 != null)
            {
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.DeadAim, characterObject2, ref bonuses);
                if (characterObject.HasMount())
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.HorseMaster, characterObject2, ref bonuses);
                }
            }
        }
        else if (skill == DefaultSkills.Throwing)
        {
            if (characterObject2 != null)
            {
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.StrongArms, characterObject2, ref bonuses);
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.RunningThrow, characterObject2, ref bonuses);
            }
        }
        else if (skill == DefaultSkills.Crossbow && characterObject2 != null)
        {
            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.DonkeysSwiftness, characterObject2, ref bonuses);
        }

        if (mobileParty != null && mobileParty.HasPerk(DefaultPerks.Roguery.OneOfTheFamily) && characterObject.Occupation == Occupation.Bandit && (skill.CharacterAttribute == DefaultCharacterAttributes.Vigor || skill.CharacterAttribute == DefaultCharacterAttributes.Control))
        {
            bonuses.Add(DefaultPerks.Roguery.OneOfTheFamily.PrimaryBonus, DefaultPerks.Roguery.OneOfTheFamily.Name);
        }

        if (characterObject.HasMount())
        {
            if (skill == DefaultSkills.Riding && characterObject2 != null)
            {
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.NimbleSteed, characterObject2, ref bonuses);
            }
        }
        else
        {
            if (mobileParty != null && formation != null)
            {
                bool num = skill == DefaultSkills.OneHanded || skill == DefaultSkills.TwoHanded || skill == DefaultSkills.Polearm;
                bool flag3 = formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.ShieldWall;
                if (num && flag3)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Polearm.Phalanx, mobileParty, isPrimaryBonus: true, ref bonuses);
                }
            }

            if (characterObject2 != null)
            {
                if (skill == DefaultSkills.OneHanded)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.WrappedHandles, characterObject2, ref bonuses);
                }
                else if (skill == DefaultSkills.TwoHanded)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.StrongGrip, characterObject2, ref bonuses);
                }
                else if (skill == DefaultSkills.Polearm)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.CleanThrust, characterObject2, ref bonuses);
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.CounterWeight, characterObject2, ref bonuses);
                }
            }
        }

        return (int)bonuses.ResultNumber;
    }

    public override float GetWeaponDamageMultiplier(Agent agent, WeaponComponentData weapon)
    {
        ExplainedNumber stat = new ExplainedNumber(1f);
        SkillObject skillObject = weapon?.RelevantSkill;
        if (agent.Character is CharacterObject character && skillObject != null)
        {
            if (skillObject == DefaultSkills.OneHanded)
            {
                int effectiveSkill = GetEffectiveSkill(agent, skillObject);
                SkillHelper.AddSkillBonusForCharacter(skillObject, DefaultSkillEffects.OneHandedDamage, character, ref stat, effectiveSkill);
            }
            else if (skillObject == DefaultSkills.TwoHanded)
            {
                int effectiveSkill2 = GetEffectiveSkill(agent, skillObject);
                SkillHelper.AddSkillBonusForCharacter(skillObject, DefaultSkillEffects.TwoHandedDamage, character, ref stat, effectiveSkill2);
            }
            else if (skillObject == DefaultSkills.Polearm)
            {
                int effectiveSkill3 = GetEffectiveSkill(agent, skillObject);
                SkillHelper.AddSkillBonusForCharacter(skillObject, DefaultSkillEffects.PolearmDamage, character, ref stat, effectiveSkill3);
            }
            else if (skillObject == DefaultSkills.Bow)
            {
                int effectiveSkill4 = GetEffectiveSkill(agent, skillObject);
                SkillHelper.AddSkillBonusForCharacter(skillObject, DefaultSkillEffects.BowDamage, character, ref stat, effectiveSkill4);
            }
            else if (skillObject == DefaultSkills.Throwing)
            {
                int effectiveSkill5 = GetEffectiveSkill(agent, skillObject);
                SkillHelper.AddSkillBonusForCharacter(skillObject, DefaultSkillEffects.ThrowingDamage, character, ref stat, effectiveSkill5);
            }
        }

        return Math.Max(0f, stat.ResultNumber);
    }

    public override float GetKnockBackResistance(Agent agent)
    {
        if (agent.IsHuman)
        {
            int effectiveSkill = GetEffectiveSkill(agent, DefaultSkills.Athletics);
            float val = DefaultSkillEffects.KnockBackResistance.GetPrimaryValue(effectiveSkill) * 0.01f;
            return Math.Max(0f, val);
        }

        return float.MaxValue;
    }

    public override float GetKnockDownResistance(Agent agent, StrikeType strikeType = StrikeType.Invalid)
    {
        if (agent.IsHuman)
        {
            int effectiveSkill = GetEffectiveSkill(agent, DefaultSkills.Athletics);
            float num = DefaultSkillEffects.KnockDownResistance.GetPrimaryValue(effectiveSkill) * 0.01f;
            if (agent.HasMount)
            {
                num += 0.1f;
            }
            else if (strikeType == StrikeType.Thrust)
            {
                num += 0.15f;
            }

            return Math.Max(0f, num);
        }

        return float.MaxValue;
    }

    public override float GetDismountResistance(Agent agent)
    {
        if (agent.IsHuman)
        {
            int effectiveSkill = GetEffectiveSkill(agent, DefaultSkills.Riding);
            float val = DefaultSkillEffects.DismountResistance.GetPrimaryValue(effectiveSkill) * 0.01f;
            return Math.Max(0f, val);
        }

        return float.MaxValue;
    }

    public override float GetWeaponInaccuracy(Agent agent, WeaponComponentData weapon, int weaponSkill)
    {
        CharacterObject characterObject = agent.Character as CharacterObject;
        CharacterObject characterObject2 = agent.Formation?.Captain?.Character as CharacterObject;
        if (characterObject == characterObject2)
        {
            characterObject2 = null;
        }

        float a = 0f;
        if (weapon.IsRangedWeapon)
        {
            ExplainedNumber stat = new ExplainedNumber(1f);
            if (characterObject != null)
            {
                if (weapon.RelevantSkill == DefaultSkills.Bow)
                {
                    SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Bow, DefaultSkillEffects.BowAccuracy, characterObject, ref stat, weaponSkill, isBonusPositive: false);
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.QuickAdjustments, characterObject2, ref stat);
                }
                else if (weapon.RelevantSkill == DefaultSkills.Crossbow)
                {
                    SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Crossbow, DefaultSkillEffects.CrossbowAccuracy, characterObject, ref stat, weaponSkill, isBonusPositive: false);
                }
                else if (weapon.RelevantSkill == DefaultSkills.Throwing)
                {
                    SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Throwing, DefaultSkillEffects.ThrowingAccuracy, characterObject, ref stat, weaponSkill, isBonusPositive: false);
                }
            }

            a = (100f - (float)weapon.Accuracy) * stat.ResultNumber * 0.001f;
        }
        else if (weapon.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
        {
            a = 1f - (float)weaponSkill * 0.01f;
        }

        return MathF.Max(a, 0f);
    }

    public override float GetInteractionDistance(Agent agent)
    {
        if (agent.HasMount && agent.Character is CharacterObject characterObject && characterObject.GetPerkValue(DefaultPerks.Throwing.LongReach))
        {
            return 3f;
        }

        return base.GetInteractionDistance(agent);
    }

    public override float GetMaxCameraZoom(Agent agent)
    {
        CharacterObject characterObject = agent.Character as CharacterObject;
        ExplainedNumber bonuses = new ExplainedNumber(1f);
        if (characterObject != null)
        {
            MissionEquipment equipment = agent.Equipment;
            EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
            WeaponComponentData weaponComponentData = ((wieldedItemIndex != EquipmentIndex.None) ? equipment[wieldedItemIndex].CurrentUsageItem : null);
            if (weaponComponentData != null)
            {
                if (weaponComponentData.RelevantSkill == DefaultSkills.Bow)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.EagleEye, characterObject, isPrimaryBonus: true, ref bonuses);
                }
                else if (weaponComponentData.RelevantSkill == DefaultSkills.Crossbow)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.LongShots, characterObject, isPrimaryBonus: true, ref bonuses);
                }
                else if (weaponComponentData.RelevantSkill == DefaultSkills.Throwing)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.Focus, characterObject, isPrimaryBonus: true, ref bonuses);
                }
            }
        }

        return bonuses.ResultNumber;
    }

    public List<PerkObject> GetPerksOfAgent(CharacterObject agentCharacter, SkillObject skill = null, bool filterPerkRole = false, SkillEffect.PerkRole perkRole = SkillEffect.PerkRole.Personal)
    {
        List<PerkObject> list = new List<PerkObject>();
        if (agentCharacter != null)
        {
            foreach (PerkObject item in PerkObject.All)
            {
                if (!agentCharacter.GetPerkValue(item) || (skill != null && skill != item.Skill))
                {
                    continue;
                }

                if (filterPerkRole)
                {
                    if (item.PrimaryRole == perkRole || item.SecondaryRole == perkRole)
                    {
                        list.Add(item);
                    }
                }
                else
                {
                    list.Add(item);
                }
            }
        }

        return list;
    }

    public override string GetMissionDebugInfoForAgent(Agent agent)
    {
        string text = "";
        text += "Base: Initial stats modified only by skills\n";
        text += "Effective (Eff): Stats that are modified by perks & mission effects\n\n";
        string text2 = "{0,-20}";
        text = text + string.Format(text2, "Name") + ": " + agent.Name + "\n";
        text = text + string.Format(text2, "Age") + ": " + (int)agent.Age + "\n";
        text = text + string.Format(text2, "Health") + ": " + agent.Health + "\n";
        int num = (agent.IsHuman ? agent.Character.MaxHitPoints() : agent.Monster.HitPoints);
        text = text + string.Format(text2, "Max.Health") + ": " + num + "(Base)\n";
        text = text + string.Format(text2, "") + "  " + MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveMaxHealth(agent) + "(Eff)\n";
        text = text + string.Format(text2, "Team") + ": " + ((agent.Team == null) ? "N/A" : (agent.Team.IsAttacker ? "Attacker" : "Defender")) + "\n";
        if (agent.IsHuman)
        {
            string format = text2 + ": {1,4:G}, {2,4:G}";
            text += "-------------------------------------\n";
            text = text + string.Format(text2 + ": {1,4}, {2,4}", "Skills", "Base", "Eff") + "\n";
            text += "-------------------------------------\n";
            foreach (SkillObject item in Skills.All)
            {
                int skillValue = agent.Character.GetSkillValue(item);
                int effectiveSkill = MissionGameModels.Current.AgentStatCalculateModel.GetEffectiveSkill(agent, item);
                string text3 = string.Format(format, item.Name, skillValue, effectiveSkill);
                text = text + text3 + "\n";
            }

            text += "-------------------------------------\n";
            CharacterObject agentCharacter = agent.Character as CharacterObject;
            string debugPerkInfoForAgent = GetDebugPerkInfoForAgent(agentCharacter);
            if (debugPerkInfoForAgent.Length > 0)
            {
                text = text + string.Format(text2 + ": ", "Perks") + "\n";
                text += "-------------------------------------\n";
                text += debugPerkInfoForAgent;
                text += "-------------------------------------\n";
            }

            CharacterObject characterObject = agent.Formation?.Captain?.Character as CharacterObject;
            string debugPerkInfoForAgent2 = GetDebugPerkInfoForAgent(characterObject, filterPerkRole: true, SkillEffect.PerkRole.Captain);
            if (debugPerkInfoForAgent2.Length > 0)
            {
                text = string.Concat(text, string.Format(text2 + ": ", "Captain Perks"), characterObject.Name, "\n");
                text += "-------------------------------------\n";
                text += debugPerkInfoForAgent2;
                text += "-------------------------------------\n";
            }

            CharacterObject visualPartyLeader = PartyBaseHelper.GetVisualPartyLeader(((PartyBase)(agent.Origin?.BattleCombatant))?.MobileParty?.Party);
            string debugPerkInfoForAgent3 = GetDebugPerkInfoForAgent(visualPartyLeader, filterPerkRole: true, SkillEffect.PerkRole.PartyLeader);
            if (debugPerkInfoForAgent3.Length > 0)
            {
                text = string.Concat(text, string.Format(text2 + ": ", "Party Leader Perks"), visualPartyLeader.Name, "\n");
                text += "-------------------------------------\n";
                text += debugPerkInfoForAgent3;
                text += "-------------------------------------\n";
            }
        }

        return text;
    }

    public float GetEffectiveArmorEncumbrance(Agent agent, Equipment equipment)
    {
        float totalWeightOfArmor = equipment.GetTotalWeightOfArmor(agent.IsHuman);
        float num = 1f;
        if (agent.Character is CharacterObject characterObject && characterObject.GetPerkValue(DefaultPerks.Athletics.FormFittingArmor))
        {
            num += DefaultPerks.Athletics.FormFittingArmor.PrimaryBonus;
        }

        return MathF.Max(0f, totalWeightOfArmor * num);
    }

    public override float GetEffectiveMaxHealth(Agent agent)
    {
        if (agent.IsHero)
        {
            return agent.Character.MaxHitPoints();
        }

        float baseHealthLimit = agent.BaseHealthLimit;
        ExplainedNumber stat = new ExplainedNumber(baseHealthLimit);
        if (agent.IsHuman)
        {
            CharacterObject characterObject = agent.Character as CharacterObject;
            MobileParty mobileParty = ((PartyBase)((agent?.Origin)?.BattleCombatant))?.MobileParty;
            CharacterObject characterObject2 = mobileParty?.LeaderHero?.CharacterObject;
            if (characterObject != null && characterObject2 != null)
            {
                PerkHelper.AddPerkBonusForParty(DefaultPerks.TwoHanded.ThickHides, mobileParty, isPrimaryBonus: false, ref stat);
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Polearm.HardyFrontline, mobileParty, isPrimaryBonus: true, ref stat);
                if (characterObject.IsRanged)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Crossbow.PickedShots, mobileParty, isPrimaryBonus: false, ref stat);
                }

                if (!agent.HasMount)
                {
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Athletics.WellBuilt, mobileParty, isPrimaryBonus: false, ref stat);
                    PerkHelper.AddPerkBonusForParty(DefaultPerks.Polearm.HardKnock, mobileParty, isPrimaryBonus: false, ref stat);
                    if (characterObject.IsInfantry)
                    {
                        PerkHelper.AddPerkBonusForParty(DefaultPerks.OneHanded.UnwaveringDefense, mobileParty, isPrimaryBonus: false, ref stat);
                    }
                }

                if (characterObject2.GetPerkValue(DefaultPerks.Medicine.MinisterOfHealth))
                {
                    int num = (int)((float)MathF.Max(characterObject2.GetSkillValue(DefaultSkills.Medicine) - Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, 0) * DefaultPerks.Medicine.MinisterOfHealth.PrimaryBonus);
                    if (num > 0)
                    {
                        stat.Add(num);
                    }
                }
            }
        }
        else
        {
            Agent riderAgent = agent.RiderAgent;
            if (riderAgent != null)
            {
                CharacterObject character = riderAgent?.Character as CharacterObject;
                MobileParty party = ((PartyBase)(riderAgent?.Origin?.BattleCombatant))?.MobileParty;
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Medicine.Sledges, party, isPrimaryBonus: false, ref stat);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.Veterinary, character, isPrimaryBonus: true, ref stat);
                PerkHelper.AddPerkBonusForParty(DefaultPerks.Riding.Veterinary, party, isPrimaryBonus: false, ref stat);
            }
        }

        return stat.ResultNumber;
    }

    public override float GetEnvironmentSpeedFactor(Agent agent)
    {
        _ = agent.Mission.Scene;
        float num = 1f;
        if (!agent.Mission.Scene.IsAtmosphereIndoor)
        {
            if (agent.Mission.Scene.GetRainDensity() > 0f)
            {
                num *= 0.9f;
            }

            if (!agent.IsHuman && CampaignTime.Now.IsNightTime)
            {
                num *= 0.9f;
            }
        }

        return num;
    }

    private string GetDebugPerkInfoForAgent(CharacterObject agentCharacter, bool filterPerkRole = false, SkillEffect.PerkRole perkRole = SkillEffect.PerkRole.Personal)
    {
        string text = "";
        string format = "{0,-18}";
        if (GetPerksOfAgent(agentCharacter, null, filterPerkRole, perkRole).Count > 0)
        {
            foreach (SkillObject item in Skills.All)
            {
                List<PerkObject> perksOfAgent = GetPerksOfAgent(agentCharacter, item, filterPerkRole, perkRole);
                if (perksOfAgent == null || perksOfAgent.Count <= 0)
                {
                    continue;
                }

                string text2 = string.Format(format, item.Name) + ": ";
                int num = 5;
                int num2 = 0;
                foreach (PerkObject item2 in perksOfAgent)
                {
                    string text3 = item2.Name.ToString();
                    if (num2 == num)
                    {
                        text2 = text2 + "\n" + string.Format(format, "") + "  ";
                        num2 = 0;
                    }

                    text2 = text2 + text3 + ", ";
                    num2++;
                }

                text2 = text2.Remove(text2.LastIndexOf(","));
                text = text + text2 + "\n";
            }
        }

        return text;
    }

    private void UpdateHumanStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        Equipment spawnEquipment = agent.SpawnEquipment;
        agentDrivenProperties.ArmorHead = spawnEquipment.GetHeadArmorSum();
        agentDrivenProperties.ArmorTorso = spawnEquipment.GetHumanBodyArmorSum();
        agentDrivenProperties.ArmorLegs = spawnEquipment.GetLegArmorSum();
        agentDrivenProperties.ArmorArms = spawnEquipment.GetArmArmorSum();
        BasicCharacterObject character = agent.Character;
        CharacterObject characterObject = character as CharacterObject;
        MissionEquipment equipment = agent.Equipment;
        float num = equipment.GetTotalWeightOfWeapons();
        float effectiveArmorEncumbrance = GetEffectiveArmorEncumbrance(agent, spawnEquipment);
        int weight = agent.Monster.Weight;
        EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        EquipmentIndex wieldedItemIndex2 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        if (wieldedItemIndex != EquipmentIndex.None)
        {
            ItemObject item = equipment[wieldedItemIndex].Item;
            WeaponComponent weaponComponent = item.WeaponComponent;
            if (weaponComponent != null)
            {
                ItemObject.ItemTypeEnum itemType = weaponComponent.GetItemType();
                bool flag = false;
                if (characterObject != null)
                {
                    bool flag2 = itemType == ItemObject.ItemTypeEnum.Bow && characterObject.GetPerkValue(DefaultPerks.Bow.RangersSwiftness);
                    bool flag3 = itemType == ItemObject.ItemTypeEnum.Crossbow && characterObject.GetPerkValue(DefaultPerks.Crossbow.LooseAndMove);
                    flag = flag2 || flag3;
                }

                if (!flag)
                {
                    float realWeaponLength = weaponComponent.PrimaryWeapon.GetRealWeaponLength();
                    num += 4f * MathF.Sqrt(realWeaponLength) * item.Weight;
                }
            }
        }

        if (wieldedItemIndex2 != EquipmentIndex.None)
        {
            ItemObject item2 = equipment[wieldedItemIndex2].Item;
            WeaponComponentData primaryWeapon = item2.PrimaryWeapon;
            if (primaryWeapon != null && primaryWeapon.WeaponFlags.HasAnyFlag(WeaponFlags.CanBlockRanged) && (characterObject == null || !characterObject.GetPerkValue(DefaultPerks.OneHanded.ShieldBearer)))
            {
                num += 1.5f * item2.Weight;
            }
        }

        agentDrivenProperties.WeaponsEncumbrance = num;
        agentDrivenProperties.ArmorEncumbrance = effectiveArmorEncumbrance;
        float num2 = effectiveArmorEncumbrance + num;
        EquipmentIndex wieldedItemIndex3 = agent.GetWieldedItemIndex(Agent.HandIndex.MainHand);
        WeaponComponentData weaponComponentData = ((wieldedItemIndex3 != EquipmentIndex.None) ? equipment[wieldedItemIndex3].CurrentUsageItem : null);
        EquipmentIndex wieldedItemIndex4 = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        WeaponComponentData secondaryItem = ((wieldedItemIndex4 != EquipmentIndex.None) ? equipment[wieldedItemIndex4].CurrentUsageItem : null);
        agentDrivenProperties.SwingSpeedMultiplier = 0.93f;
        agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = 0.93f;
        agentDrivenProperties.HandlingMultiplier = 1f;
        agentDrivenProperties.ShieldBashStunDurationMultiplier = 1f;
        agentDrivenProperties.KickStunDurationMultiplier = 1f;
        agentDrivenProperties.ReloadSpeed = 0.93f;
        agentDrivenProperties.MissileSpeedMultiplier = 1f;
        agentDrivenProperties.ReloadMovementPenaltyFactor = 1f;
        SetAllWeaponInaccuracy(agent, agentDrivenProperties, (int)wieldedItemIndex3, weaponComponentData);
        int effectiveSkill = GetEffectiveSkill(agent, DefaultSkills.Athletics);
        int effectiveSkill2 = GetEffectiveSkill(agent, DefaultSkills.Riding);
        if (weaponComponentData != null)
        {
            WeaponComponentData weaponComponentData2 = weaponComponentData;
            int effectiveSkillForWeapon = GetEffectiveSkillForWeapon(agent, weaponComponentData2);
            if (weaponComponentData2.IsRangedWeapon)
            {
                int thrustSpeed = weaponComponentData2.ThrustSpeed;
                if (!agent.HasMount)
                {
                    float num3 = MathF.Max(0f, 1f - (float)effectiveSkillForWeapon / 500f);
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = MathF.Max(0f, 0.125f * num3);
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = MathF.Max(0f, 0.1f * num3);
                }
                else
                {
                    float num4 = MathF.Max(0f, (1f - (float)effectiveSkillForWeapon / 500f) * (1f - (float)effectiveSkill2 / 1800f));
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = MathF.Max(0f, 0.025f * num4);
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = MathF.Max(0f, 0.12f * num4);
                }

                if (weaponComponentData2.RelevantSkill == DefaultSkills.Bow)
                {
                    float value = ((float)thrustSpeed - 45f) / 90f;
                    value = MBMath.ClampFloat(value, 0f, 1f);
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 6f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 4.5f / MBMath.Lerp(0.75f, 2f, value);
                }
                else if (weaponComponentData2.RelevantSkill == DefaultSkills.Throwing)
                {
                    float value2 = ((float)thrustSpeed - 89f) / 13f;
                    value2 = MBMath.ClampFloat(value2, 0f, 1f);
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 0.5f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.5f * MBMath.Lerp(1.5f, 0.8f, value2);
                }
                else if (weaponComponentData2.RelevantSkill == DefaultSkills.Crossbow)
                {
                    agentDrivenProperties.WeaponMaxMovementAccuracyPenalty *= 2.5f;
                    agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty *= 1.2f;
                }

                if (weaponComponentData2.WeaponClass == WeaponClass.Bow)
                {
                    agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.3f + (95.75f - (float)thrustSpeed) * 0.005f;
                    float value3 = ((float)thrustSpeed - 45f) / 90f;
                    value3 = MBMath.ClampFloat(value3, 0f, 1f);
                    agentDrivenProperties.WeaponUnsteadyBeginTime = 0.6f + (float)effectiveSkillForWeapon * 0.01f * MBMath.Lerp(2f, 4f, value3);
                    if (agent.IsAIControlled)
                    {
                        agentDrivenProperties.WeaponUnsteadyBeginTime *= 4f;
                    }

                    agentDrivenProperties.WeaponUnsteadyEndTime = 2f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                    agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
                else if (weaponComponentData2.WeaponClass == WeaponClass.Javelin || weaponComponentData2.WeaponClass == WeaponClass.ThrowingAxe || weaponComponentData2.WeaponClass == WeaponClass.ThrowingKnife)
                {
                    agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.2f + (89f - (float)thrustSpeed) * 0.009f;
                    agentDrivenProperties.WeaponUnsteadyBeginTime = 2.5f + (float)effectiveSkillForWeapon * 0.01f;
                    agentDrivenProperties.WeaponUnsteadyEndTime = 10f + agentDrivenProperties.WeaponUnsteadyBeginTime;
                    agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.025f;
                }
                else
                {
                    agentDrivenProperties.WeaponBestAccuracyWaitTime = 0.1f;
                    agentDrivenProperties.WeaponUnsteadyBeginTime = 0f;
                    agentDrivenProperties.WeaponUnsteadyEndTime = 0f;
                    agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = 0.1f;
                }
            }
            else if (weaponComponentData2.WeaponFlags.HasAllFlags(WeaponFlags.WideGrip))
            {
                agentDrivenProperties.WeaponUnsteadyBeginTime = 1f + (float)effectiveSkillForWeapon * 0.005f;
                agentDrivenProperties.WeaponUnsteadyEndTime = 3f + (float)effectiveSkillForWeapon * 0.01f;
            }
        }

        agentDrivenProperties.TopSpeedReachDuration = 2.5f + MathF.Max(5f - (1f + (float)effectiveSkill * 0.01f), 1f) / 3.5f - MathF.Min((float)weight / ((float)weight + num2), 0.8f);
        float num5 = 0.7f * (1f + 3f * DefaultSkillEffects.AthleticsSpeedFactor.PrimaryBonus);
        float num6 = (num5 - 0.7f) / 300f;
        float num7 = 0.7f + num6 * (float)effectiveSkill;
        float num8 = MathF.Max(0.2f * (1f - DefaultSkillEffects.AthleticsWeightFactor.GetPrimaryValue(effectiveSkill) * 0.01f), 0f) * num2 / (float)weight;
        float num9 = MBMath.ClampFloat(num7 - num8, 0f, num5);
        agentDrivenProperties.MaxSpeedMultiplier = GetEnvironmentSpeedFactor(agent) * num9;
        float managedParameter = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
        float managedParameter2 = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
        float amount = MathF.Min(num2 / (float)weight, 1f);
        agentDrivenProperties.CombatMaxSpeedMultiplier = MathF.Min(MBMath.Lerp(managedParameter2, managedParameter, amount), 1f);
        agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = 0.3f;
        float num10 = agent.MountAgent?.GetAgentDrivenPropertyValue(DrivenProperty.AttributeRiding) ?? 1f;
        agentDrivenProperties.AttributeRiding = (float)effectiveSkill2 * num10;
        agentDrivenProperties.AttributeHorseArchery = MissionGameModels.Current.StrikeMagnitudeModel.CalculateHorseArcheryFactor(character);
        agentDrivenProperties.BipedalRangedReadySpeedMultiplier = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
        agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = TaleWorlds.Core.ManagedParameters.Instance.GetManagedParameter(TaleWorlds.Core.ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);
        if (characterObject != null)
        {
            if (weaponComponentData != null)
            {
                SetWeaponSkillEffectsOnAgent(agent, characterObject, agentDrivenProperties, weaponComponentData);
                if (agent.HasMount)
                {
                    SetMountedPenaltiesOnAgent(agent, agentDrivenProperties, weaponComponentData);
                }
            }

            SetPerkAndBannerEffectsOnAgent(agent, characterObject, agentDrivenProperties, weaponComponentData);
        }

        SetAiRelatedProperties(agent, agentDrivenProperties, weaponComponentData, secondaryItem);
        float num11 = 1f;
        if (!agent.Mission.Scene.IsAtmosphereIndoor)
        {
            float rainDensity = agent.Mission.Scene.GetRainDensity();
            float fog = agent.Mission.Scene.GetFog();
            if (rainDensity > 0f || fog > 0f)
            {
                num11 += MathF.Min(0.3f, rainDensity + fog);
            }

            if (!MBMath.IsBetween(agent.Mission.Scene.TimeOfDay, 4f, 20.01f))
            {
                num11 += 0.1f;
            }
        }

        agentDrivenProperties.AiShooterError *= num11;
    }

    private void UpdateHorseStats(Agent agent, AgentDrivenProperties agentDrivenProperties)
    {
        Equipment spawnEquipment = agent.SpawnEquipment;
        EquipmentElement mountElement = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
        ItemObject item = mountElement.Item;
        EquipmentElement harness = spawnEquipment[EquipmentIndex.HorseHarness];
        agentDrivenProperties.AiSpeciesIndex = (int)item.Id.InternalValue;
        agentDrivenProperties.AttributeRiding = 0.8f + ((harness.Item != null) ? 0.2f : 0f);
        float num = 0f;
        for (int i = 1; i < 12; i++)
        {
            if (spawnEquipment[i].Item != null)
            {
                num += (float)spawnEquipment[i].GetModifiedMountBodyArmor();
            }
        }

        agentDrivenProperties.ArmorTorso = num;
        int modifiedMountManeuver = mountElement.GetModifiedMountManeuver(in harness);
        int num2 = mountElement.GetModifiedMountSpeed(in harness) + 1;
        int num3 = 0;
        float environmentSpeedFactor = GetEnvironmentSpeedFactor(agent);
        bool flag = Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(MobileParty.MainParty.Position2D) == MapWeatherModel.WeatherEventEffectOnTerrain.Wet;
        Agent riderAgent = agent.RiderAgent;
        if (riderAgent != null)
        {
            CharacterObject characterObject = riderAgent.Character as CharacterObject;
            Formation formation = riderAgent.Formation;
            Agent agent2 = formation?.Captain;
            if (agent2 == riderAgent)
            {
                agent2 = null;
            }

            CharacterObject captainCharacter = agent2?.Character as CharacterObject;
            BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(formation);
            ExplainedNumber stat = new ExplainedNumber(modifiedMountManeuver);
            ExplainedNumber stat2 = new ExplainedNumber(num2);
            num3 = GetEffectiveSkill(agent.RiderAgent, DefaultSkills.Riding);
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Riding, DefaultSkillEffects.HorseManeuver, agent.RiderAgent.Character as CharacterObject, ref stat, num3);
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Riding, DefaultSkillEffects.HorseSpeed, agent.RiderAgent.Character as CharacterObject, ref stat2, num3);
            if (activeBanner != null)
            {
                BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedMountMovementSpeed, activeBanner, ref stat2);
            }

            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.NimbleSteed, characterObject, isPrimaryBonus: true, ref stat);
            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.SweepingWind, characterObject, isPrimaryBonus: true, ref stat2);
            ExplainedNumber bonuses = new ExplainedNumber(agentDrivenProperties.ArmorTorso);
            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.ToughSteed, captainCharacter, ref bonuses);
            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.ToughSteed, characterObject, isPrimaryBonus: true, ref bonuses);
            if (characterObject.GetPerkValue(DefaultPerks.Riding.TheWayOfTheSaddle))
            {
                float value = (float)MathF.Max(num3 - Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus, 0) * DefaultPerks.Riding.TheWayOfTheSaddle.PrimaryBonus;
                stat.Add(value);
            }

            if (harness.Item == null)
            {
                stat.AddFactor(-0.1f);
                stat2.AddFactor(-0.1f);
            }

            if (flag)
            {
                stat2.AddFactor(-0.25f);
            }

            agentDrivenProperties.ArmorTorso = bonuses.ResultNumber;
            agentDrivenProperties.MountManeuver = stat.ResultNumber;
            agentDrivenProperties.MountSpeed = environmentSpeedFactor * 0.22f * (1f + stat2.ResultNumber);
        }
        else
        {
            agentDrivenProperties.MountManeuver = modifiedMountManeuver;
            agentDrivenProperties.MountSpeed = environmentSpeedFactor * 0.22f * (float)(1 + num2);
        }

        float num4 = mountElement.Weight / 2f + (harness.IsEmpty ? 0f : harness.Weight);
        agentDrivenProperties.MountDashAccelerationMultiplier = ((!(num4 > 200f)) ? 1f : ((num4 < 300f) ? (1f - (num4 - 200f) / 111f) : 0.1f));
        if (flag)
        {
            agentDrivenProperties.MountDashAccelerationMultiplier *= 0.75f;
        }

        agentDrivenProperties.TopSpeedReachDuration = Game.Current.BasicModels.RidingModel.CalculateAcceleration(in mountElement, in harness, num3);
        agentDrivenProperties.MountChargeDamage = (float)mountElement.GetModifiedMountCharge(in harness) * 0.004f;
        agentDrivenProperties.MountDifficulty = mountElement.Item.Difficulty;
    }

    private void SetPerkAndBannerEffectsOnAgent(Agent agent, CharacterObject agentCharacter, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
    {
        CharacterObject characterObject = agent.Formation?.Captain?.Character as CharacterObject;
        if (agent.Formation?.Captain == agent)
        {
            characterObject = null;
        }

        ItemObject itemObject = null;
        EquipmentIndex wieldedItemIndex = agent.GetWieldedItemIndex(Agent.HandIndex.OffHand);
        if (wieldedItemIndex != EquipmentIndex.None)
        {
            itemObject = agent.Equipment[wieldedItemIndex].Item;
        }

        BannerComponent activeBanner = MissionGameModels.Current.BattleBannerBearersModel.GetActiveBanner(agent.Formation);
        bool flag = equippedWeaponComponent?.IsRangedWeapon ?? false;
        bool flag2 = equippedWeaponComponent?.IsMeleeWeapon ?? false;
        bool flag3 = itemObject?.PrimaryWeapon.IsShield ?? false;
        ExplainedNumber bonuses = new ExplainedNumber(agentDrivenProperties.CombatMaxSpeedMultiplier);
        ExplainedNumber bonuses2 = new ExplainedNumber(agentDrivenProperties.MaxSpeedMultiplier);
        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.FleetOfFoot, agentCharacter, isPrimaryBonus: true, ref bonuses);
        ExplainedNumber bonuses3 = new ExplainedNumber(agentDrivenProperties.KickStunDurationMultiplier);
        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.DirtyFighting, agentCharacter, isPrimaryBonus: true, ref bonuses3);
        agentDrivenProperties.KickStunDurationMultiplier = bonuses3.ResultNumber;
        if (equippedWeaponComponent != null)
        {
            ExplainedNumber bonuses4 = new ExplainedNumber(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier);
            if (flag2)
            {
                ExplainedNumber bonuses5 = new ExplainedNumber(agentDrivenProperties.SwingSpeedMultiplier);
                ExplainedNumber bonuses6 = new ExplainedNumber(agentDrivenProperties.HandlingMultiplier);
                if (!agent.HasMount)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Fury, agentCharacter, isPrimaryBonus: true, ref bonuses6);
                    if (characterObject != null)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.Fury, characterObject, ref bonuses6);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.OnTheEdge, characterObject, ref bonuses5);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.BladeMaster, characterObject, ref bonuses5);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.SwiftSwing, characterObject, ref bonuses5);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.BladeMaster, characterObject, ref bonuses4);
                    }
                }

                if (equippedWeaponComponent.RelevantSkill == DefaultSkills.OneHanded)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.SwiftStrike, agentCharacter, isPrimaryBonus: true, ref bonuses5);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.OneHanded.WayOfTheSword, agentCharacter, DefaultSkills.OneHanded, applyPrimaryBonus: true, ref bonuses5, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.OneHanded.WayOfTheSword, agentCharacter, DefaultSkills.OneHanded, applyPrimaryBonus: true, ref bonuses4, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.WrappedHandles, agentCharacter, isPrimaryBonus: true, ref bonuses6);
                }
                else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.TwoHanded)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.OnTheEdge, agentCharacter, isPrimaryBonus: true, ref bonuses5);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.TwoHanded.WayOfTheGreatAxe, agentCharacter, DefaultSkills.TwoHanded, applyPrimaryBonus: true, ref bonuses5, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.TwoHanded.WayOfTheGreatAxe, agentCharacter, DefaultSkills.TwoHanded, applyPrimaryBonus: true, ref bonuses4, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.TwoHanded.StrongGrip, agentCharacter, isPrimaryBonus: true, ref bonuses6);
                }
                else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Polearm)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.Footwork, agentCharacter, isPrimaryBonus: true, ref bonuses);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.SwiftSwing, agentCharacter, isPrimaryBonus: true, ref bonuses5);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Polearm.WayOfTheSpear, agentCharacter, DefaultSkills.Polearm, applyPrimaryBonus: true, ref bonuses5, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Polearm.WayOfTheSpear, agentCharacter, DefaultSkills.Polearm, applyPrimaryBonus: true, ref bonuses4, Campaign.Current.Models.CharacterDevelopmentModel.MaxSkillRequiredForEpicPerkBonus);
                    if (equippedWeaponComponent.SwingDamageType != DamageTypes.Invalid)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Polearm.CounterWeight, agentCharacter, isPrimaryBonus: true, ref bonuses6);
                    }
                }

                agentDrivenProperties.SwingSpeedMultiplier = bonuses5.ResultNumber;
                agentDrivenProperties.HandlingMultiplier = bonuses6.ResultNumber;
            }

            if (flag)
            {
                ExplainedNumber bonuses7 = new ExplainedNumber(agentDrivenProperties.WeaponInaccuracy);
                ExplainedNumber bonuses8 = new ExplainedNumber(agentDrivenProperties.WeaponMaxMovementAccuracyPenalty);
                ExplainedNumber bonuses9 = new ExplainedNumber(agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty);
                ExplainedNumber bonuses10 = new ExplainedNumber(agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians);
                ExplainedNumber bonuses11 = new ExplainedNumber(agentDrivenProperties.WeaponUnsteadyBeginTime);
                ExplainedNumber bonuses12 = new ExplainedNumber(agentDrivenProperties.WeaponUnsteadyEndTime);
                ExplainedNumber bonuses13 = new ExplainedNumber(agentDrivenProperties.ReloadMovementPenaltyFactor);
                ExplainedNumber bonuses14 = new ExplainedNumber(agentDrivenProperties.ReloadSpeed);
                ExplainedNumber bonuses15 = new ExplainedNumber(agentDrivenProperties.MissileSpeedMultiplier);
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.NockingPoint, agentCharacter, isPrimaryBonus: true, ref bonuses13);
                if (characterObject != null)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.LooseAndMove, characterObject, ref bonuses2);
                }

                if (activeBanner != null)
                {
                    BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.DecreasedRangedAccuracyPenalty, activeBanner, ref bonuses7);
                }

                if (agent.HasMount)
                {
                    if (agentCharacter.GetPerkValue(DefaultPerks.Riding.Sagittarius))
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.Sagittarius, agentCharacter, isPrimaryBonus: true, ref bonuses8);
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Riding.Sagittarius, agentCharacter, isPrimaryBonus: true, ref bonuses9);
                    }

                    if (characterObject != null && characterObject.GetPerkValue(DefaultPerks.Riding.Sagittarius))
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.Sagittarius, characterObject, ref bonuses8);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Riding.Sagittarius, characterObject, ref bonuses9);
                    }

                    if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Bow && agentCharacter.GetPerkValue(DefaultPerks.Bow.MountedArchery))
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.MountedArchery, agentCharacter, isPrimaryBonus: true, ref bonuses8);
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.MountedArchery, agentCharacter, isPrimaryBonus: true, ref bonuses9);
                    }

                    if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing && agentCharacter.GetPerkValue(DefaultPerks.Throwing.MountedSkirmisher))
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.MountedSkirmisher, agentCharacter, isPrimaryBonus: true, ref bonuses8);
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.MountedSkirmisher, agentCharacter, isPrimaryBonus: true, ref bonuses9);
                    }
                }

                bool flag4 = false;
                if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Bow)
                {
                    flag4 = true;
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.BowControl, agentCharacter, isPrimaryBonus: true, ref bonuses8);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.RapidFire, agentCharacter, isPrimaryBonus: true, ref bonuses14);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.QuickAdjustments, agentCharacter, isPrimaryBonus: true, ref bonuses10);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.Discipline, agentCharacter, isPrimaryBonus: true, ref bonuses11);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.Discipline, agentCharacter, isPrimaryBonus: true, ref bonuses12);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Bow.QuickDraw, agentCharacter, isPrimaryBonus: true, ref bonuses4);
                    if (characterObject != null)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.RapidFire, characterObject, ref bonuses14);
                        if (!agent.HasMount)
                        {
                            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Bow.NockingPoint, characterObject, ref bonuses2);
                        }
                    }

                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Bow.Deadshot, agentCharacter, DefaultSkills.Bow, applyPrimaryBonus: true, ref bonuses14, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                }
                else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Crossbow)
                {
                    flag4 = true;
                    if (agent.HasMount)
                    {
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Steady, agentCharacter, isPrimaryBonus: true, ref bonuses8);
                        PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Steady, agentCharacter, isPrimaryBonus: true, ref bonuses10);
                    }

                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.WindWinder, agentCharacter, isPrimaryBonus: true, ref bonuses14);
                    if (characterObject != null)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Crossbow.WindWinder, characterObject, ref bonuses14);
                    }

                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.DonkeysSwiftness, agentCharacter, isPrimaryBonus: true, ref bonuses8);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Crossbow.Marksmen, agentCharacter, isPrimaryBonus: true, ref bonuses4);
                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Crossbow.MightyPull, agentCharacter, DefaultSkills.Crossbow, applyPrimaryBonus: true, ref bonuses14, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                }
                else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing)
                {
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.QuickDraw, agentCharacter, isPrimaryBonus: true, ref bonuses14);
                    PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Throwing.PerfectTechnique, agentCharacter, isPrimaryBonus: true, ref bonuses15);
                    if (characterObject != null)
                    {
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.QuickDraw, characterObject, ref bonuses14);
                        PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Throwing.PerfectTechnique, characterObject, ref bonuses15);
                    }

                    PerkHelper.AddEpicPerkBonusForCharacter(DefaultPerks.Throwing.UnstoppableForce, agentCharacter, DefaultSkills.Throwing, applyPrimaryBonus: true, ref bonuses15, Campaign.Current.Models.CharacterDevelopmentModel.MinSkillRequiredForEpicPerkBonus);
                }

                if (flag4 && Campaign.Current.Models.MapWeatherModel.GetWeatherEffectOnTerrainForPosition(MobileParty.MainParty.Position2D) == MapWeatherModel.WeatherEventEffectOnTerrain.Wet)
                {
                    bonuses15.AddFactor(-0.2f);
                }

                agentDrivenProperties.ReloadMovementPenaltyFactor = bonuses13.ResultNumber;
                agentDrivenProperties.ReloadSpeed = bonuses14.ResultNumber;
                agentDrivenProperties.MissileSpeedMultiplier = bonuses15.ResultNumber;
                agentDrivenProperties.WeaponInaccuracy = bonuses7.ResultNumber;
                agentDrivenProperties.WeaponMaxMovementAccuracyPenalty = bonuses8.ResultNumber;
                agentDrivenProperties.WeaponMaxUnsteadyAccuracyPenalty = bonuses9.ResultNumber;
                agentDrivenProperties.WeaponUnsteadyBeginTime = bonuses11.ResultNumber;
                agentDrivenProperties.WeaponUnsteadyEndTime = bonuses12.ResultNumber;
                agentDrivenProperties.WeaponRotationalAccuracyPenaltyInRadians = bonuses10.ResultNumber;
            }

            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = bonuses4.ResultNumber;
        }

        if (flag3)
        {
            ExplainedNumber bonuses16 = new ExplainedNumber(agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder);
            if (characterObject != null)
            {
                Formation formation = agent.Formation;
                if (formation != null && formation.ArrangementOrder.OrderEnum == ArrangementOrder.ArrangementOrderEnum.ShieldWall)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.ShieldWall, characterObject, ref bonuses16);
                }

                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.ArrowCatcher, characterObject, ref bonuses16);
            }

            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.ArrowCatcher, agentCharacter, isPrimaryBonus: true, ref bonuses16);
            agentDrivenProperties.AttributeShieldMissileCollisionBodySizeAdder = bonuses16.ResultNumber;
            ExplainedNumber bonuses17 = new ExplainedNumber(agentDrivenProperties.ShieldBashStunDurationMultiplier);
            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.OneHanded.Basher, agentCharacter, isPrimaryBonus: true, ref bonuses17);
            agentDrivenProperties.ShieldBashStunDurationMultiplier = bonuses17.ResultNumber;
        }
        else
        {
            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.MorningExercise, agentCharacter, isPrimaryBonus: true, ref bonuses2);
            PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Medicine.SelfMedication, agentCharacter, isPrimaryBonus: false, ref bonuses2);
            if (!(flag3 || flag))
            {
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Athletics.Sprint, agentCharacter, isPrimaryBonus: true, ref bonuses2);
            }

            if (equippedWeaponComponent == null && itemObject == null)
            {
                PerkHelper.AddPerkBonusForCharacter(DefaultPerks.Roguery.FleetFooted, agentCharacter, isPrimaryBonus: true, ref bonuses2);
            }

            if (characterObject != null)
            {
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.MorningExercise, characterObject, ref bonuses2);
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.ShieldBearer, characterObject, ref bonuses2);
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.OneHanded.FleetOfFoot, characterObject, ref bonuses2);
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.TwoHanded.RecklessCharge, characterObject, ref bonuses2);
                PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Polearm.Footwork, characterObject, ref bonuses2);
                if (agentCharacter.Tier >= 3)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.FormFittingArmor, characterObject, ref bonuses2);
                }

                if (agentCharacter.IsInfantry)
                {
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.Sprint, characterObject, ref bonuses2);
                }
            }
        }

        if (agent.IsHero)
        {
            ItemObject item = (Mission.Current.DoesMissionRequireCivilianEquipment ? agentCharacter.FirstCivilianEquipment : agentCharacter.FirstBattleEquipment)[EquipmentIndex.Body].Item;
            if (item != null && item.IsCivilian && agentCharacter.GetPerkValue(DefaultPerks.Roguery.SmugglerConnections))
            {
                agentDrivenProperties.ArmorTorso += DefaultPerks.Roguery.SmugglerConnections.PrimaryBonus;
            }
        }

        float num = 0f;
        float num2 = 0f;
        bool flag5 = false;
        if (characterObject != null)
        {
            if (agent.HasMount && characterObject.GetPerkValue(DefaultPerks.Riding.DauntlessSteed))
            {
                num += DefaultPerks.Riding.DauntlessSteed.SecondaryBonus;
                flag5 = true;
            }
            else if (!agent.HasMount && characterObject.GetPerkValue(DefaultPerks.Athletics.IgnorePain))
            {
                num += DefaultPerks.Athletics.IgnorePain.SecondaryBonus;
                flag5 = true;
            }

            if (characterObject.GetPerkValue(DefaultPerks.Engineering.Metallurgy))
            {
                num += DefaultPerks.Engineering.Metallurgy.SecondaryBonus;
                flag5 = true;
            }
        }

        if (!agent.HasMount && agentCharacter.GetPerkValue(DefaultPerks.Athletics.IgnorePain))
        {
            num2 += DefaultPerks.Athletics.IgnorePain.PrimaryBonus;
            flag5 = true;
        }

        if (flag5)
        {
            float num3 = 1f + num2;
            agentDrivenProperties.ArmorHead = MathF.Max(0f, (agentDrivenProperties.ArmorHead + num) * num3);
            agentDrivenProperties.ArmorTorso = MathF.Max(0f, (agentDrivenProperties.ArmorTorso + num) * num3);
            agentDrivenProperties.ArmorArms = MathF.Max(0f, (agentDrivenProperties.ArmorArms + num) * num3);
            agentDrivenProperties.ArmorLegs = MathF.Max(0f, (agentDrivenProperties.ArmorLegs + num) * num3);
        }

        if (Mission.Current != null && Mission.Current.HasValidTerrainType)
        {
            switch (Mission.Current.TerrainType)
            {
                case TerrainType.Snow:
                case TerrainType.Forest:
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.ExtendedSkirmish, characterObject, ref bonuses2);
                    break;
                case TerrainType.Steppe:
                case TerrainType.Plain:
                case TerrainType.Desert:
                    PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.DecisiveBattle, characterObject, ref bonuses2);
                    break;
            }
        }

        if (agentCharacter.Tier >= 3 && agentCharacter.IsInfantry)
        {
            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Athletics.FormFittingArmor, characterObject, ref bonuses2);
        }

        if (agent.Formation != null && agent.Formation.CountOfUnits <= 15)
        {
            PerkHelper.AddPerkBonusFromCaptain(DefaultPerks.Tactics.SmallUnitTactics, characterObject, ref bonuses2);
        }

        if (activeBanner != null)
        {
            BannerHelper.AddBannerBonusForBanner(DefaultBannerEffects.IncreasedTroopMovementSpeed, activeBanner, ref bonuses2);
        }

        agentDrivenProperties.MaxSpeedMultiplier = bonuses2.ResultNumber;
        agentDrivenProperties.CombatMaxSpeedMultiplier = bonuses.ResultNumber;
    }

    private void SetWeaponSkillEffectsOnAgent(Agent agent, CharacterObject agentCharacter, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
    {
        int effectiveSkill = GetEffectiveSkill(agent, equippedWeaponComponent.RelevantSkill);
        ExplainedNumber stat = new ExplainedNumber(agentDrivenProperties.SwingSpeedMultiplier);
        ExplainedNumber stat2 = new ExplainedNumber(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier);
        ExplainedNumber stat3 = new ExplainedNumber(agentDrivenProperties.ReloadSpeed);
        if (equippedWeaponComponent.RelevantSkill == DefaultSkills.OneHanded)
        {
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.OneHanded, DefaultSkillEffects.OneHandedSpeed, agentCharacter, ref stat, effectiveSkill);
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.OneHanded, DefaultSkillEffects.OneHandedSpeed, agentCharacter, ref stat2, effectiveSkill);
        }
        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.TwoHanded)
        {
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.TwoHanded, DefaultSkillEffects.TwoHandedSpeed, agentCharacter, ref stat, effectiveSkill);
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.TwoHanded, DefaultSkillEffects.TwoHandedSpeed, agentCharacter, ref stat2, effectiveSkill);
        }
        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Polearm)
        {
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Polearm, DefaultSkillEffects.PolearmSpeed, agentCharacter, ref stat, effectiveSkill);
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Polearm, DefaultSkillEffects.PolearmSpeed, agentCharacter, ref stat2, effectiveSkill);
        }
        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Crossbow)
        {
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Crossbow, DefaultSkillEffects.CrossbowReloadSpeed, agentCharacter, ref stat3, effectiveSkill);
        }
        else if (equippedWeaponComponent.RelevantSkill == DefaultSkills.Throwing)
        {
            SkillHelper.AddSkillBonusForCharacter(DefaultSkills.Throwing, DefaultSkillEffects.ThrowingSpeed, agentCharacter, ref stat2, effectiveSkill);
        }

        agentDrivenProperties.SwingSpeedMultiplier = Math.Max(0f, stat.ResultNumber);
        agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = Math.Max(0f, stat2.ResultNumber);
        agentDrivenProperties.ReloadSpeed = Math.Max(0f, stat3.ResultNumber);
    }

    private void SetMountedPenaltiesOnAgent(Agent agent, AgentDrivenProperties agentDrivenProperties, WeaponComponentData equippedWeaponComponent)
    {
        int effectiveSkill = GetEffectiveSkill(agent, DefaultSkills.Riding);
        float num = 0.01f * MathF.Max(0f, DefaultSkillEffects.MountedWeaponSpeedPenalty.GetPrimaryValue(effectiveSkill));
        if (num > 0f)
        {
            ExplainedNumber explainedNumber = new ExplainedNumber(agentDrivenProperties.WeaponBestAccuracyWaitTime);
            ExplainedNumber explainedNumber2 = new ExplainedNumber(agentDrivenProperties.SwingSpeedMultiplier);
            ExplainedNumber explainedNumber3 = new ExplainedNumber(agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier);
            ExplainedNumber explainedNumber4 = new ExplainedNumber(agentDrivenProperties.ReloadSpeed);
            explainedNumber2.AddFactor(0f - num);
            explainedNumber3.AddFactor(0f - num);
            explainedNumber4.AddFactor(0f - num);
            explainedNumber.AddFactor(num);
            agentDrivenProperties.SwingSpeedMultiplier = Math.Max(0f, explainedNumber2.ResultNumber);
            agentDrivenProperties.ThrustOrRangedReadySpeedMultiplier = Math.Max(0f, explainedNumber3.ResultNumber);
            agentDrivenProperties.ReloadSpeed = Math.Max(0f, explainedNumber4.ResultNumber);
            agentDrivenProperties.WeaponBestAccuracyWaitTime = Math.Max(0f, explainedNumber.ResultNumber);
        }

        float num2 = 5f - (float)effectiveSkill * 0.05f;
        if (num2 > 0f)
        {
            ExplainedNumber explainedNumber5 = new ExplainedNumber(agentDrivenProperties.WeaponInaccuracy);
            explainedNumber5.AddFactor(num2);
            agentDrivenProperties.WeaponInaccuracy = Math.Max(0f, explainedNumber5.ResultNumber);
        }
    }
    
    //not used
    //public static float CalculateMaximumSpeedMultiplier(int athletics, float baseWeight, float totalEncumbrance)
    //{
    //    return MathF.Min((200f + (float)athletics) / 300f * (baseWeight * 2f / (baseWeight * 2f + totalEncumbrance)), 1f);
    //}
}
