﻿using BattleLink.Common.Behavior;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Diamond;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;
using BodyProperties = TaleWorlds.Core.BodyProperties;
using Equipment = TaleWorlds.Core.Equipment;
using Team = TaleWorlds.MountAndBlade.Team;

namespace BattleLink.Common.Spawn.Warmup
{

    public class BLWarmupSpawningBehavior : WarmupSpawningBehavior
    {
        private static readonly FieldInfo fieldHasCalledSpawningEnded = typeof(SpawningBehaviorBase).GetField("_hasCalledSpawningEnded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo fieldOnSpawningEnded = typeof(SpawningBehaviorBase).GetField("OnSpawningEnded", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly EventInfo eventOnSpawningEnded = typeof(SpawningBehaviorBase).GetEvent("OnSpawningEnded", BindingFlags.Public | BindingFlags.Instance);

        public BLWarmupSpawningBehavior() : base()
        {
        }

        public override void OnTick(float dt)
        {

            SpawnAgents();

            SpawnBots();

            // base.OnTick(dt);// no base call

            if (IsSpawningEnabled || !IsRoundInProgress())
            {
                return;
            }


            bool _hasCalledSpawningEnded = (bool)fieldHasCalledSpawningEnded.GetValue(this);
            if (SpawningDelayTimer >= SpawningEndDelay && !_hasCalledSpawningEnded)
            {
                Mission.Current.AllowAiTicking = true;//p.Raise("SomethingHappening", EventArgs.Empty);

                if (fieldOnSpawningEnded.GetValue(this) != null)
                {
                    eventOnSpawningEnded.GetRaiseMethod().Invoke(this, new object[] { });//this.Raise
                }
                fieldHasCalledSpawningEnded.SetValue(this, true);
            }

            SpawningDelayTimer += dt;

        }

        protected override void SpawnAgents()
        {
            foreach (NetworkCommunicator networkPeer in GameNetwork.NetworkPeers)
            {
                if (!networkPeer.IsSynchronized)
                {
                    continue;
                }

                MissionPeer missionPeer = networkPeer.GetComponent<MissionPeer>();
                if (missionPeer == null
                    || missionPeer.ControlledAgent != null
                    || missionPeer.Team == null
                    || missionPeer.Team == Mission.SpectatorTeam
                    || !missionPeer.TeamInitialPerkInfoReady
                    )//|| !missionPeer.SpawnTimer.Check(base.Mission.CurrentTime)
                {
                    continue;
                }

                int selectedTroopIndex = missionPeer.SelectedTroopIndex;

                
                Team team = missionPeer.Team;//JetBrains*45445*80
                BLCharacterObject character = getRandomCharacter(team);

                AgentBuildData agentBuildData = buidAgentData(team, character);

                agentBuildData.MissionPeer(missionPeer);

                if (character.IsHero)
                {
                    agentBuildData.BodyProperties(character.GetBodyPropertiesMin());
                }
                else
                {
                    var playerData = missionPeer.GetNetworkPeer().PlayerConnectionInfo.GetParameter<PlayerData>("PlayerData");
                    agentBuildData.BodyProperties(playerData.BodyProperties).Race(playerData.Race).IsFemale(playerData.IsFemale);
                }


                bool firstSpawn = missionPeer.SpawnCountThisRound == 0;
                MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(missionPeer.Team, agentBuildData.hasMount(), firstSpawn);
                Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();
                // Randomize direction so players don't go all straight.
                initialDirection.RotateCCW(MBRandom.RandomFloatRanged(-MathF.PI / 3f, MathF.PI / 3f));
                agentBuildData.InitialPosition(in spawnFrame.origin).InitialDirection(in initialDirection);

                Agent agent = Mission.SpawnAgent(agentBuildData);

                agent.WieldInitialWeapons();
                //if (!agentBuildData.hasMount() || agentBuildData.hasExtraSlotEquipped())
                //{
                //}

                missionPeer.HasSpawnedAgentVisuals = true;

                agent.AIStateFlags |= Agent.AIStateFlag.Alarmed;
            }
        }

        public static AgentBuildData buidAgentData(Team team, BLCharacterObject character)
        {
            //CompressionBasic.RandomSeedCompressionInfo
            var equipmentSeed = MBRandom.RandomInt(2000);

            // indicate too if the head agent is hide or not
            Equipment equipment = Equipment.GetRandomEquipmentElements(character, randomEquipmentModifier: false, isCivilianEquipment: false, equipmentSeed);

            AgentBuildData agentBuildData = new AgentBuildData(character)
                .Team(team)
                .VisualsIndex(0)
                .Equipment(equipment)
                .IsFemale(character.IsFemale);
                // base.GetBodyProperties uses the player-defined body properties but some body properties may have been
                // causing crashes. So here we send the body properties from the characters.xml which we know are safe.
                // Note that what is sent here doesn't matter since it's ignored by the client.
                //.BodyProperties(character.GetBodyPropertiesMin());
            agentBuildData.ClothingColor1(team.Color).ClothingColor2(team.Color2).Banner(team.Banner);

            return agentBuildData;
        }

        public static BLCharacterObject getRandomCharacter(Team team)
        {
            (var sideDto, var teamDto) = BLReferentialHolder.getTeamDtoBy(team);

            var party = teamDto.Parties[MBRandom.RandomInt(teamDto.Parties.Count)];
            var troops = party.Troops;
            string stringId = troops[MBRandom.RandomInt(troops.Count)].Id;
            var character = MBObjectManager.Instance.GetObject<BLCharacterObject>(stringId);
            return character;

        }

        private void SpawnBots()
        {
            int nbMaxBotTeam1 = MultiplayerOptions.OptionType.NumberOfBotsTeam1.GetIntValue();
            int nbMaxBotTeam2 = MultiplayerOptions.OptionType.NumberOfBotsTeam2.GetIntValue();
            if (GameMode.IsGameModeUsingOpposingTeams && (nbMaxBotTeam1 > 0 || nbMaxBotTeam2 > 0))
            {
                for (int index = Mission.Current.AttackerTeam.ActiveAgents.FindAll(a => a.IsAIControlled).Count(); index < nbMaxBotTeam1; index += 1)
                {
                    SpawnBot(Mission.Current.AttackerTeam);
                }
                for (int index = Mission.Current.DefenderTeam.ActiveAgents.FindAll(a => a.IsAIControlled).Count(); index < nbMaxBotTeam2; index += 1)
                {
                    SpawnBot(Mission.Current.DefenderTeam);
                }
            }
        }

        private void SpawnBot(Team team)
        {
            BLCharacterObject character = getRandomCharacter(team);
            MBDebug.Print("SpawnBot " + character.StringId, 0, DebugColor.Green);

            bool hasMount = character.Equipment[EquipmentIndex.Horse].Item != null;
            MatrixFrame spawnFrame = SpawnComponent.GetSpawnFrame(team, hasMount, true);
            Vec2 initialDirection = spawnFrame.rotation.f.AsVec2.Normalized();


            AgentBuildData agentBuildData = new AgentBuildData(character)
                .TroopOrigin(new BLBattleAgentOrigin(character))
                //.TroopOrigin(new BasicBattleAgentOrigin(characterXml))
                .Team(team)
                .VisualsIndex(0)
                .IsFemale(character.IsFemale);


            agentBuildData.ClothingColor1(team.Color).ClothingColor2(team.Color2).Banner(team.Banner);


            agentBuildData.InitialPosition(in spawnFrame.origin).InitialDirection(in initialDirection);

            //CompressionBasic.RandomSeedCompressionInfo
            var equipmentSeed = MBRandom.RandomInt(2000);

            // indicate too if the head agent is hide or not
            Equipment equipment = Equipment.GetRandomEquipmentElements(character, randomEquipmentModifier: false, isCivilianEquipment: false, equipmentSeed);// 0
            agentBuildData.Equipment(equipment).EquipmentSeed(MBRandom.RandomInt(2000));


            // moins de bug que character.GetBodyProperties / FaceGen    mais il ya plus de tatouage...
            var bodyProperties = //character.GetBodyProperties(equipment, equipmentSeed);
            getRandomBodyProperties(character.GetBodyPropertiesMin(), character.GetBodyPropertiesMax());
            agentBuildData.BodyProperties(bodyProperties);


            Agent agent = Mission.Current.SpawnAgent(agentBuildData);
            bool hasExtraSlotEquipped = agentBuildData.AgentOverridenSpawnEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
            if (!agent.HasMount || hasExtraSlotEquipped)
            {
                agent.WieldInitialWeapons();
            }
            agent.AIStateFlags |= Agent.AIStateFlag.Alarmed;
        }

        public static BodyProperties getRandomBodyProperties(BodyProperties bodyPropertiesMin, BodyProperties bodyPropertiesMax)
        {
            float ageMin = bodyPropertiesMin.DynamicProperties.Age;
            float weightMin = bodyPropertiesMin.DynamicProperties.Weight;
            float buildMin = bodyPropertiesMin.DynamicProperties.Build;
            ulong keyPart1Min = bodyPropertiesMin.StaticProperties.KeyPart1;
            ulong keyPart2Min = bodyPropertiesMin.StaticProperties.KeyPart2;
            ulong keyPart3Min = bodyPropertiesMin.StaticProperties.KeyPart3;
            ulong keyPart4Min = bodyPropertiesMin.StaticProperties.KeyPart4;
            ulong keyPart5Min = bodyPropertiesMin.StaticProperties.KeyPart5;
            ulong keyPart6Min = bodyPropertiesMin.StaticProperties.KeyPart6;
            ulong keyPart7Min = bodyPropertiesMin.StaticProperties.KeyPart7;
            ulong keyPart8Min = bodyPropertiesMin.StaticProperties.KeyPart8;

            float ageMax = bodyPropertiesMax.DynamicProperties.Age;
            float weightMax = bodyPropertiesMax.DynamicProperties.Weight;
            float buildMax = bodyPropertiesMax.DynamicProperties.Build;
            ulong keyPart1Max = bodyPropertiesMax.StaticProperties.KeyPart1;
            ulong keyPart2Max = bodyPropertiesMax.StaticProperties.KeyPart2;
            ulong keyPart3Max = bodyPropertiesMax.StaticProperties.KeyPart3;
            ulong keyPart4Max = bodyPropertiesMax.StaticProperties.KeyPart4;
            ulong keyPart5Max = bodyPropertiesMax.StaticProperties.KeyPart5;
            ulong keyPart6Max = bodyPropertiesMax.StaticProperties.KeyPart6;
            ulong keyPart7Max = bodyPropertiesMax.StaticProperties.KeyPart7;
            ulong keyPart8Max = bodyPropertiesMax.StaticProperties.KeyPart8;

            return new BodyProperties(new DynamicBodyProperties(RandomFloat(ageMin, ageMax), RandomFloat(weightMin, weightMax), RandomFloat(buildMin, buildMax)),
                new StaticBodyProperties(
                // 3rd short contain tatoos
                RandomULongSplitU8(keyPart1Min, keyPart1Max),
                RandomULongSplitU8(keyPart2Min, keyPart2Max),
                RandomULongSplitU8(keyPart3Min, keyPart3Max),
                RandomULongSplitU8(keyPart4Min, keyPart4Max),
                RandomULongSplitU8(keyPart5Min, keyPart5Max),
                RandomULongSplitU8(keyPart6Min, keyPart6Max),
                RandomULongSplitU8(keyPart7Min, keyPart7Max),
                RandomULongSplitU8(keyPart8Min, keyPart8Max)));
        }

        static float RandomFloat(float min, float max)
        {
            return MBRandom.RandomFloatRanged(min, max);
        }


        private static Random rand = new Random();
        public static ulong RandomULong(ulong min, ulong max)
        {
            if (max <= min)
            {
                return min;
            }

            ulong uRange = max - min;

            //Prevent a modolo bias; see https://stackoverflow.com/a/10984975/238419
            //for more information.
            //In the worst case, the expected number of calls is 2 (though usually it's
            //much closer to 1) so this loop doesn't really hurt performance at all.
            ulong ulongRand;
            do
            {
                byte[] buf = new byte[8];
                rand.NextBytes(buf);
                ulongRand = (ulong)BitConverter.ToInt64(buf, 0);
            } while (ulongRand > ulong.MaxValue - (ulong.MaxValue % uRange + 1) % uRange);

            return ulongRand % uRange + min;
        }


        public static ulong RandomULongSplitU8(ulong min, ulong max)
        {
            if (max <= min)
            {
                return min;
            }


            byte[] tabMin = BitConverter.GetBytes(min);
            byte[] tabMax = BitConverter.GetBytes(max);
            byte[] tabRes = new byte[8];
           // ulong reconstituted = 0;
            for (int i = 0 ; i<8; i+=1)
            {
                int iMin = tabMin[i] & 0xff;
                int iMax = tabMax[i] & 0xff;
                // fix me! search each characteristics face...
                if (iMax <= iMin)
                {
                    tabRes[i] = (byte)iMin;
                }
                else
                {
                    tabRes[i] = (byte)MBRandom.RandomInt(iMin, iMax);
                }
                //ulong res = (ulong)MBRandom.RandomInt(iMin, iMax);
                //reconstituted = reconstituted | (ulong)(res << (i * 8));
            }

            //return reconstituted;
            var res= BitConverter.ToUInt64(tabRes,0);
            return res;
        }

        public static ulong RandomULongSplitU16(ulong min, ulong max)
        {
            if (max <= min)
            {
                return min;
            }

            ushort min1Half = (ushort)(min >> 48 & 0xffff);
            ushort min2Half = (ushort)(min >> 32 & 0xffff);
            ushort min3Half = (ushort)(min >> 16 & 0xffff);
            ushort min4Half = (ushort)(min & 0xffff);

            ushort max1Half = (ushort)(max >> 48 & 0xffff);
            ushort max2Half = (ushort)(max >> 32 & 0xffff);
            ushort max3Half = (ushort)(max >> 16 & 0xffff);
            ushort max4Half = (ushort)(max & 0xffff);

            ushort firstHalf = (ushort)MBRandom.RandomInt(min1Half,max1Half);
            ushort secondHalf = (ushort)MBRandom.RandomInt(min2Half, max2Half);
            ushort thirdHalf = (ushort)MBRandom.RandomInt(min3Half, max3Half);
            ushort fourthHalf = (ushort)MBRandom.RandomInt(min4Half, max4Half);


            ulong reconstituted = (ulong)((firstHalf << 48) | (secondHalf << 32) | (thirdHalf << 16) | (fourthHalf));
            return reconstituted;
        }
    }

    public static class AgentBuildDataExt
    {
        public static bool hasMount(this AgentBuildData agentBuildData)
        {
            return agentBuildData.AgentOverridenSpawnEquipment[EquipmentIndex.Horse].Item != null;
        }
        public static bool hasExtraSlotEquipped(this AgentBuildData agentBuildData)
        {
            return agentBuildData.AgentOverridenSpawnEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
        }

    }

}
