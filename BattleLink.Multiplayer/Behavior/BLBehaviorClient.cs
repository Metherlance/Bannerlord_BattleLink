using BattleLink.Common.Model;
using BattleLink.Handler;
using NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Reflection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Client.Behavior
{
    public class BLBehaviorClient : MissionLogic
    {
        private static readonly FieldInfo fieldIndexContainer = typeof(GameNetwork).GetField("_gameNetworkMessageTypesFromServer", BindingFlags.NonPublic | BindingFlags.Static);
        private static readonly FieldInfo fieldContainer = typeof(GameNetwork).GetField("_fromServerBaseMessageHandlers", BindingFlags.NonPublic | BindingFlags.Static);

        //public override void AfterStart()
        //{
        //    base.AfterStart();
        //    MBDebug.Print("RbBehaviorClient - EarlyStart - " 0, DebugColor.Green);
        //}



        //bool notInit = true;


        //public override void OnAddTeam(Team team)
        //{
        //    MBDebug.Print("BattleLinkGameMode - OnAddTeam - ", 0, DebugColor.Green);//1
        //}


        // WTF !!!?? DElete that
        //public override List<EquipmentElement> GetExtraEquipmentElementsForCharacter(BasicCharacterObject character, bool getAllEquipments = false)
        //{
        //    if (notInit && Mission.Scene!=null)
        //    {
        //        MBDebug.Print("RbBehaviorClient - GetExtraEquipmentElementsForCharacter", 0, DebugColor.Green);
        //        IEnumerable<GameEntity> lSpawn = Mission.Scene.FindEntitiesWithTag("spawnpoint");
        //        if (lSpawn.IsEmpty())
        //        {
        //            GameEntity spawnpoint_set = Mission.Scene.FindEntityWithTag("spawnpoint_set");
        //            if (spawnpoint_set != null)
        //            {
        //                lSpawn = spawnpoint_set.GetChildren();
        //                if (lSpawn.Count() > 5 && !lSpawn.First().HasTag("sp_visual_0"))
        //                {
        //                    int index = 0;
        //                    foreach (var spawn in lSpawn)
        //                    {
        //                        spawn.AddTag("sp_visual_" + index);
        //                        index += 1;
        //                        if (index > 5)
        //                        {
        //                            break;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //        notInit = false;
        //    }
        //    return base.GetExtraEquipmentElementsForCharacter(character, getAllEquipments); ;
        //}
        public override void OnBehaviorInitialize()
        {
            base.OnBehaviorInitialize();

            {

                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(BLInitCultureMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCultureHandler = value[indexType];
                listCultureHandler.Clear();
                listCultureHandler.Add(BLInitCultureHandler.HandleServerEventInitCultureMessage);
               // InformationManager.DisplayMessage(new InformationMessage("BLInitCultureMessage Handler"));

            }

            {
                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<System.Type, int> dicIndexType = (Dictionary<System.Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(BLInitCharactersMessage), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCharacterHandler = value[indexType];
                listCharacterHandler.Clear();
                listCharacterHandler.Add(BLInitCharactersHandler.HandleServerEventInitCharactersMessage);
               // InformationManager.DisplayMessage(new InformationMessage("BLInitCharactersMessage Handler"));
            }

            {

                var oDicIndexType = fieldIndexContainer.GetValue(null);
                Dictionary<Type, int> dicIndexType = (Dictionary<Type, int>)oDicIndexType;
                int indexType = dicIndexType.TryGetValue(typeof(CreateAgent), out indexType) ? indexType : -1;

                var valu = fieldContainer.GetValue(null);
                Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>> value = (Dictionary<int, List<GameNetworkMessage.ServerMessageHandlerDelegate<GameNetworkMessage>>>)valu;
                var listCreateAgentHandler = value[indexType];
                listCreateAgentHandler.Clear();
                listCreateAgentHandler.Add(BLCreateAgentHandler.HandleServerEventCreateAgent);
               // InformationManager.DisplayMessage(new InformationMessage("CreateAgent Handler"));
            }

            MBDebug.Print("RBMultiplayerWarmupComponent - OnBehaviorInitialize ", 0, DebugColor.Green);


        }
    }
}