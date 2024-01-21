using System;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Model;

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
        agentDrivenProperties.BipedalRangedReadySpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReadySpeedMultiplier);
        agentDrivenProperties.BipedalRangedReloadSpeedMultiplier = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalRangedReloadSpeedMultiplier);
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
    private int GetSkillValueForItem(/*Parameter with token 08001A71*/BasicCharacterObject characterObject, /*Parameter with token 08001A72*/ItemObject primaryItem)
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

    public override void InitializeAgentStats(Agent agent, TaleWorlds.Core.Equipment spawnEquipment, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
    {
        agentDrivenProperties.ArmorEncumbrance = spawnEquipment.GetTotalWeightOfArmor(agent.IsHuman);
        if (!agent.IsHuman)
        {
            InitializeHorseAgentStats(agent, spawnEquipment, agentDrivenProperties);
        }
        else
        {
            agentDrivenProperties = this.InitializeHumanAgentStats(agent, agentDrivenProperties, agentBuildData);
        }
    }

    private AgentDrivenProperties InitializeHumanAgentStats(Agent agent, AgentDrivenProperties agentDrivenProperties, AgentBuildData agentBuildData)
    {
        MultiplayerClassDivisions.MPHeroClass mPHeroClassForCharacter = MultiplayerClassDivisions.GetMPHeroClassForCharacter(agent.Character);
        if (mPHeroClassForCharacter != null)
        {
            FillAgentStatsFromData(ref agentDrivenProperties, agent, mPHeroClassForCharacter, agentBuildData?.AgentMissionPeer, agentBuildData?.OwningAgentMissionPeer);
            agentDrivenProperties.SetStat(DrivenProperty.UseRealisticBlocking, MultiplayerOptions.OptionType.UseRealisticBlocking.GetBoolValue() ? 1f : 0f);
        }

        if (mPHeroClassForCharacter != null)
        {
            agent.BaseHealthLimit = mPHeroClassForCharacter.Health;
        }
        else
        {
            agent.BaseHealthLimit = 100f;
        }

        agent.HealthLimit = agent.BaseHealthLimit;
        agent.Health = agent.HealthLimit;
        return agentDrivenProperties;
    }

    private void FillAgentStatsFromData(ref AgentDrivenProperties agentDrivenProperties, Agent agent, MultiplayerClassDivisions.MPHeroClass heroClass, MissionPeer _missionPeer, MissionPeer owningMissionPeer)
    {
        MissionPeer missionPeer = _missionPeer ?? owningMissionPeer;
        if (missionPeer != null)
        {
            //MPPerkObject.MPOnSpawnPerkHandler onSpawnPerkHandler = MPPerkObject.GetOnSpawnPerkHandler(missionPeer);
            bool isPlayer = _missionPeer != null;
            for (int i = 0; i < 55; i++)
            {
                DrivenProperty drivenProperty = (DrivenProperty)i;
                float stat = agentDrivenProperties.GetStat(drivenProperty);
                if (drivenProperty == DrivenProperty.ArmorHead || drivenProperty == DrivenProperty.ArmorTorso || drivenProperty == DrivenProperty.ArmorLegs || drivenProperty == DrivenProperty.ArmorArms)
                {
                    agentDrivenProperties.SetStat(drivenProperty, stat + (float)heroClass.ArmorValue);//+ onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(isPlayer, drivenProperty, stat)
                }
                else
                {
                    agentDrivenProperties.SetStat(drivenProperty, stat);//+ onSpawnPerkHandler.GetDrivenPropertyBonusOnSpawn(isPlayer, drivenProperty, stat)
                }
            }
        }

        float topSpeedReachDuration = (heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopTopSpeedReachDuration : heroClass.HeroTopSpeedReachDuration);
        agentDrivenProperties.TopSpeedReachDuration = topSpeedReachDuration;
        float managedParameter = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMinMultiplier);
        float managedParameter2 = ManagedParameters.Instance.GetManagedParameter(ManagedParametersEnum.BipedalCombatSpeedMaxMultiplier);
        float num = (heroClass.IsTroopCharacter(agent.Character) ? heroClass.TroopCombatMovementSpeedMultiplier : heroClass.HeroCombatMovementSpeedMultiplier);
        agentDrivenProperties.CombatMaxSpeedMultiplier = managedParameter + (managedParameter2 - managedParameter) * num;
    }

    private static void InitializeHorseAgentStats(
      Agent agent,
      TaleWorlds.Core.Equipment spawnEquipment,
      AgentDrivenProperties agentDrivenProperties)
    {
        agentDrivenProperties.AiSpeciesIndex = agent.Monster.FamilyType;
        agentDrivenProperties.AttributeRiding = (float)(0.800000011920929 + (spawnEquipment[EquipmentIndex.HorseHarness].Item != null ? 0.200000002980232 : 0.0));
        float num1 = 0.0f;
        EquipmentElement equipmentElement1;
        for (int index = 1; index < 12; ++index)
        {
            equipmentElement1 = spawnEquipment[index];
            if (equipmentElement1.Item != null)
            {
                double num2 = (double)num1;
                equipmentElement1 = spawnEquipment[index];
                double modifiedMountBodyArmor = (double)equipmentElement1.GetModifiedMountBodyArmor();
                num1 = (float)(num2 + modifiedMountBodyArmor);
            }
        }
        agentDrivenProperties.ArmorTorso = num1;
        equipmentElement1 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
        HorseComponent horseComponent = equipmentElement1.Item.HorseComponent;
        EquipmentElement equipmentElement2 = spawnEquipment[EquipmentIndex.ArmorItemEndSlot];
        AgentDrivenProperties drivenProperties = agentDrivenProperties;
        ref EquipmentElement local1 = ref equipmentElement2;
        equipmentElement1 = spawnEquipment[EquipmentIndex.HorseHarness];
        ref EquipmentElement local2 = ref equipmentElement1;
        double num3 = (double)local1.GetModifiedMountCharge(in local2) * 0.00999999977648258;
        drivenProperties.MountChargeDamage = (float)num3;
        agentDrivenProperties.MountDifficulty = (float)equipmentElement2.Item.Difficulty;
    }

}
