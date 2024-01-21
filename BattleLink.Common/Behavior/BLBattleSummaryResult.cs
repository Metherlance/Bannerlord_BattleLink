//using BattleLink.Common.DtoSpSv;
//using BattleLink.Common.Model;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Xml.Serialization;
//using TaleWorlds.Core;
//using TaleWorlds.Engine;
//using TaleWorlds.Library;
//using TaleWorlds.MountAndBlade;
//using static TaleWorlds.Library.Debug;

//namespace BattleLink.Common.Behavior
//{
//    public class BLBattleSummaryResult : MissionBehavior
//    {
//        public override MissionBehaviorType BehaviorType => MissionBehaviorType.Other;

//        private Dictionary<TroopKey, TroopRosterElementDto> dicTroops = new Dictionary<TroopKey, TroopRosterElementDto>();
//        private Dictionary<int, PartyDto> dicParty = new Dictionary<int, PartyDto>();
//        private Dictionary<int, BattleSideEnum> dicPartySide = new Dictionary<int, BattleSideEnum>();

//        public BLBattleSummaryResult()
//        {
//            foreach(var team in BLReferentialHolder.listTeam)
//            {
//                BattleSideEnum side = BattleSideEnum.Attacker.ToString().Equals(team.BattleSide) ? BattleSideEnum.Attacker: BattleSideEnum.Defender;
//                foreach (var party in team.Parties)
//                {
//                    foreach (var troop in party.Troops)
//                    {
//                        dicTroops.Add(new TroopKey() { partyIndex = party.Index, characterStringId = troop.Id }, 
//                            new TroopRosterElementDto()
//                        {
//                                characterStringId = troop.Id,
//                                number = troop.Number,
//                        });
//                    }
//                }
//            }
//        }

//        public override void OnAgentRemoved(Agent affectedAgent, Agent affectorAgent, AgentState agentState, KillingBlow blow)
//        {

//            if (!(affectedAgent.Origin is BLBattleAgentOrigin blOrigin))
//            {
//                return;
//            }
//            dicTroops.TryGetValue(new TroopKey() { partyIndex = blOrigin.partyIndex, characterStringId = affectedAgent.Character.StringId }, out TroopRosterElementDto troop);
//            switch (agentState)
//            {
//                case AgentState.Killed:
//                    troop.number--;
//                    troop.killedNumber++;
//                    break;
//                case AgentState.Routed:
//                    troop.number--;
//                    troop.rootedNumber++;
//                    break;
//                case AgentState.Unconscious:
//                    troop.number--;
//                    troop.woundedNumber++;
//                    break;
//                default:
//                    break;
//            }

//            //MBDebug.Print("RBDebugMissionLogic - OnAgentRemoved", 0, DebugColor.Cyan);///la
//        }

//        protected override void OnEndMission()
//        {
//            Mission.Current.Agents.ToList().ForEach(agent =>{
//                if (agent.Character != null && agent.Origin is BLBattleAgentOrigin blOrigin && agent.State == AgentState.Active)
//                {
//                    if (agent.Character.IsHero)
//                    {
//                        dicTroops.TryGetValue(new TroopKey() { partyIndex = blOrigin.partyIndex, characterStringId = agent.Character.StringId }, out TroopRosterElementDto troop);
//                        troop.hp = (int)agent.Health;
//                    }
//                }
//            });


//            var RoundController = base.Mission.GetMissionBehavior<MultiplayerRoundController>();
//            var sideWinner = RoundController.RoundWinner;

//            dicTroops.ToList().ForEach(pKTroop =>
//            {
//                dicParty.TryGetValue(pKTroop.Key.partyIndex, out PartyDto party);
//                party.data.Add(pKTroop.Value);
//            }); 


//            SideDto sdAtt = new SideDto(){
//                parties = dicParty.Values.Where(p=>p.side==BattleSideEnum.Attacker).ToArray(),
//                winner = sideWinner==BattleSideEnum.Attacker,
//            };

//            SideDto sdDef = new SideDto()
//            {
//                parties = dicParty.Values.Where(p => p.side == BattleSideEnum.Defender).ToArray(),
//                winner = sideWinner == BattleSideEnum.Defender,
//            };

//            BattleResult battleResult = new BattleResult(){
//                attacker = sdAtt,
//                defender = sdDef,
//            };

//            //BL_MPBattle_2185888_Initializer.xml
//            //BL_MPBattle_2185888_Result_.xml

//            string filenameInit = BLReferentialHolder.initializerFilename;
//            string filenameResult = filenameInit.Replace("_Initializer.xml", "_Result.xml");
             
//            {
//                XmlSerializer x = new XmlSerializer(typeof(BattleResult));
//                TextWriter writer = new StreamWriter(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished", filenameResult));
//                x.Serialize(writer, battleResult);
//            }

//            // cross local
//            { 
//                XmlSerializer x = new XmlSerializer(typeof(BattleResult));
//                TextWriter writer = new StreamWriter(System.IO.Path.Combine(BasePath.Name,"..", "Mount & Blade II Bannerlord", "Modules", "BattleLink.Singleplayer", "Battles", filenameResult));
//                x.Serialize(writer, battleResult);
//            }

//            File.Move(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Pending", filenameInit), System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished", filenameInit));


//            MBDebug.Print("BLAgentLogsLogic - OnEndMission", 0, DebugColor.Cyan);
//        }    
//    }

//    public class TroopKey
//    { //impelments equal hashcode
//        public int partyIndex;
//        public string characterStringId;

//        public override bool Equals(object? obj)
//        {
//            return obj is TroopKey key &&
//                   partyIndex == key.partyIndex &&
//                   characterStringId == key.characterStringId;
//        }

//        public override int GetHashCode()
//        {
//            int hashCode = 36662977;
//            hashCode = hashCode * -1521134295 + partyIndex.GetHashCode();
//            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(characterStringId);
//            return hashCode;
//        }
//    }

//}