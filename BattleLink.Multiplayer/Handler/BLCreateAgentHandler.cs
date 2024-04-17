using BattleLink.Common.Model;
using NetworkMessages.FromServer;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.Handler
{
    public class BLCreateAgentHandler
    {
        public static void HandleServerEventCreateAgent(GameNetworkMessage baseMessage)
        {
            CreateAgent createAgent = (CreateAgent)baseMessage;
            BasicCharacterObject character = createAgent.Character;
            //if (character == null)
            //{
            //    var fieldIndexContainer = typeof(GameNetwork).GetField("_gameNetworkMessageTypesFromServer", BindingFlags.NonPublic | BindingFlags.Static);
            //    var fieldContainer = typeof(GameNetwork).GetField("_fromServerBaseMessageHandlers", BindingFlags.NonPublic | BindingFlags.Static);

            //    {
            //        var oDicIndexType = fieldIndexContainer.GetValue(null);
            //        Dictionary<Type, int> dicIndexType = (Dictionary<Type, int>)oDicIndexType;
            //        int indexType = dicIndexType.TryGetValue(typeof(BLInitCultureMessage), out indexType) ? indexType : -1;

            //        var valu = fieldContainer.GetValue(null);
            //        Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
            //        var listCreateAgentHandler = value[indexType];
            //    }

            //    {
            //        var oDicIndexType = fieldIndexContainer.GetValue(null);
            //        Dictionary<Type, int> dicIndexType = (Dictionary<Type, int>)oDicIndexType;
            //        int indexType = dicIndexType.TryGetValue(typeof(BLInitCharactersMessage), out indexType) ? indexType : -1;

            //        var valu = fieldContainer.GetValue(null);
            //        Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
            //        var listCreateAgentHandler = value[indexType];
            //    }

            //    {
            //        var oDicIndexType = fieldIndexContainer.GetValue(null);
            //        Dictionary<Type, int> dicIndexType = (Dictionary<Type, int>)oDicIndexType;
            //        int indexType = dicIndexType.TryGetValue(typeof(CreateAgent), out indexType) ? indexType : -1;

            //        var valu = fieldContainer.GetValue(null);
            //        Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
            //        var listCreateAgentHandler = value[indexType];
            //    }


            //    MBDebug.Print("HandleServerEventCreateAgent - character not found", 0, DebugColor.Red);
            //}
            NetworkCommunicator peer = createAgent.Peer;
            MissionPeer missionPeer = peer != null ? peer.GetComponent<MissionPeer>() : (MissionPeer)null;
            TaleWorlds.MountAndBlade.Team teamFromTeamIndex = Mission.MissionNetworkHelper.GetTeamFromTeamIndex(createAgent.TeamIndex);
            BLParty party = new BLParty()
            {
                partyIndex=-1,
            };
            var origin = new BLBattleAgentOrigin(character, party, teamFromTeamIndex);
            AgentBuildData agentBuildData1 = new AgentBuildData(character).MissionPeer(createAgent.IsPlayerAgent ? missionPeer : (MissionPeer)null).Monster(createAgent.Monster)
                .TroopOrigin(origin)
                .Equipment(createAgent.SpawnEquipment)
                .EquipmentSeed(createAgent.BodyPropertiesSeed);

            Vec3 position = createAgent.Position;
            ref Vec3 local1 = ref position;
            agentBuildData1.InitialPosition(in local1);
            Vec2 vec2 = createAgent.Direction;
            vec2 = vec2.Normalized();
            agentBuildData1.InitialDirection(in vec2).MissionEquipment(createAgent.MissionEquipment).Team(teamFromTeamIndex).Index(createAgent.AgentIndex).MountIndex(createAgent.MountAgentIndex).IsFemale(createAgent.IsFemale).ClothingColor1(createAgent.ClothingColor1).ClothingColor2(createAgent.ClothingColor2);
            Formation formation = (Formation)null;
            if (teamFromTeamIndex != null && createAgent.FormationIndex >= 0 && !GameNetwork.IsReplay)
            {
                formation = teamFromTeamIndex.GetFormation((FormationClass)createAgent.FormationIndex);
                agentBuildData1.Formation(formation);
            }

            //if (createAgent.IsPlayerAgent)
            //{

            //}
            //else
            //{
            //    //    agentBuildData3.BodyProperties(TaleWorlds.Core.BodyProperties.GetRandomBodyProperties(agentBuildData3.AgentRace, agentBuildData3.AgentIsFemale, character.GetBodyPropertiesMin(), character.GetBodyPropertiesMax(), (int)agentBuildData3.AgentOverridenSpawnEquipment.HairCoverType, agentBuildData3.AgentEquipmentSeed, character.HairTags, character.BeardTags, character.TattooTags));
            //}
            agentBuildData1.BodyProperties(createAgent.BodyPropertiesValue);
            createAgent.Character.BodyPropertyRange.Init(createAgent.BodyPropertiesValue, createAgent.BodyPropertiesValue);

            agentBuildData1.Age((int)createAgent.BodyPropertiesValue.Age);

            Banner banner = (Banner)null;
            if (formation != null)
            {
                if (!string.IsNullOrEmpty(formation.BannerCode))
                {
                    if (formation.Banner == null)
                    {
                        banner = new Banner(formation.BannerCode, teamFromTeamIndex.Color, teamFromTeamIndex.Color2);
                        formation.Banner = banner;
                    }
                    else
                    {
                        banner = formation.Banner;
                    }
                }
            }
            else if (missionPeer != null)
            {
                banner = new Banner(missionPeer.Peer.BannerCode, teamFromTeamIndex.Color, teamFromTeamIndex.Color2);
            }
            else
            {
                banner = teamFromTeamIndex.Banner;
            }
            agentBuildData1.Banner(banner);

            //agentBuildData1.Character.IsHero = true;
            //agentBuildData1.Character.IsPlayerCharacter = true;

            // Agent agent = 
            Mission.Current.SpawnAgent(agentBuildData1);
            //if (agent.IsFemale)
            //{
            //   var a = agent.Mount;
            //}
            //else
            //{
            //    var a = agent.Character;
            //}
        }

    }
}