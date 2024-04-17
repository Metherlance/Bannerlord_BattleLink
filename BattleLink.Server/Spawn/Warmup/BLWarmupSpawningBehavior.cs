using BattleLink.Common.Behavior;
using BattleLink.Common.DtoSpSv;
using BattleLink.Common.Model;
using System;
using System.Linq;
using System.Reflection;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
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
                    )//|| !missionPeer.TeamInitialPerkInfoReady || !missionPeer.SpawnTimer.Check(base.Mission.CurrentTime)
                {
                    continue;
                }

                Team team = missionPeer.Team;
                BLCharacterObject character = getRandomCharacter(team);

                AgentBuildData agentBuildData = buidAgentData(team, character);

                agentBuildData.MissionPeer(missionPeer);

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
            // indicate too if the head agent is hide or not
            Equipment equipment = Equipment.GetRandomEquipmentElements(character, randomEquipmentModifier: false, isCivilianEquipment: false, MBRandom.RandomInt());// 0

            AgentBuildData agentBuildData = new AgentBuildData(character)
                .Team(team)
                .VisualsIndex(0)
                .Equipment(equipment)
                //.IsFemale(missionPeer.Peer.IsFemale)
                .IsFemale(character.IsFemale)
                // base.GetBodyProperties uses the player-defined body properties but some body properties may have been
                // causing crashes. So here we send the body properties from the characters.xml which we know are safe.
                // Note that what is sent here doesn't matter since it's ignored by the client.
                .BodyProperties(character.GetBodyPropertiesMin());
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

            // indicate too if the head agent is hide or not
            Equipment equipment = Equipment.GetRandomEquipmentElements(character, randomEquipmentModifier: false, isCivilianEquipment: false, MBRandom.RandomInt());// 0
            agentBuildData.Equipment(equipment);


            // moins de bug que character.GetBodyProperties / FaceGen    mais il ya plus de tatouage...
            var bodyProperties = getRandomBodyProperties(character.GetBodyPropertiesMin(), character.GetBodyPropertiesMax());
            agentBuildData.BodyProperties(bodyProperties);


            Agent agent = Mission.Current.SpawnAgent(agentBuildData);
            bool hasExtraSlotEquipped = agentBuildData.AgentOverridenSpawnEquipment[EquipmentIndex.ExtraWeaponSlot].Item != null;
            if (!agent.HasMount || hasExtraSlotEquipped)
            {
                agent.WieldInitialWeapons();
            }
            agent.AIStateFlags |= Agent.AIStateFlag.Alarmed;
        }

        private BodyProperties getRandomBodyProperties(BodyProperties bodyPropertiesMin, BodyProperties bodyPropertiesMax)
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
                new StaticBodyProperties(RandomULong(keyPart1Min, keyPart1Max), RandomULong(keyPart2Min, keyPart2Max),
                RandomULong(keyPart3Min, keyPart3Max), RandomULong(keyPart4Min, keyPart4Max),
                RandomULong(keyPart5Min, keyPart5Max), RandomULong(keyPart6Min, keyPart6Max),
                RandomULong(keyPart7Min, keyPart7Max), RandomULong(keyPart8Min, keyPart8Max)));
        }

        float RandomFloat(float min, float max)
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
