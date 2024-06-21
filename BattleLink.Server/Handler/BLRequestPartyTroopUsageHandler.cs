using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using NetworkMessages.FromServer;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Server.Handler
{
    public class BLRequestPartyTroopUsageHandler
    {
        /// <summary>
        /// Server method - Handle usage request of specific character by player.
        /// </summary>
        public static bool HandleClientEventRequestPartyTroopUsageMessage(NetworkCommunicator peer, BLRequestPartyTroopUsageMessage message)
        {
            MissionPeer missionPeer = peer.GetComponent<MissionPeer>();


            MBDebug.Print("HandleClientEventRequestPartyTroopUsageMessage - ", 0, DebugColor.Green);

            //Log($"{missionPeer.Name} is requesting to use {message.Character.Name} ({CharactersAvailability[message.Character].Slots} remaining).", LogLevel.Debug);

           // BLRequestPartyTroopUsageMessage mes = (BLRequestPartyTroopUsageMessage)message;

            
            var partyTroop = message.characterObject;

            if (partyTroop.Id!=null)
            {
                //bool hadPreviousSelection = _characterSelected.TryGetValue(missionPeer.Peer.Id, out BasicCharacterObject previousSelection);
                //if (hadPreviousSelection)
                //{
                //    CharactersAvailability[previousSelection].FreeSlot();
                //    if (CharactersAvailability[previousSelection].IsAvailable)
                //    {
                //        ChangeCharacterAvailability(previousSelection, true);
                //    }
                //}
                //ReserveCharacterSlot(message.Character);
                //_characterSelected[missionPeer.Peer.Id] = message.Character;
               // SendMessageToPeer($"You reserved {message.Character.Name} ({CharactersAvailability[message.Character].Taken}/{CharactersAvailability[message.Character].Slots})", peer);
                SendMessageToPeer($"You reserved)", peer);
            }
            else
            {
                MBDebug.Print("HandleClientEventRequestPartyTroopUsageMessage - There is no slot remaining for ", 0, DebugColor.Green);
                //SendMessageToPeer($"There is no slot remaining for {message.Character.Name} !", peer);
                SendMessageToPeer($"There is no slot remaining for  !", peer);
            }
            return true;
        }

        public static void SendMessageToPeer(string message, NetworkCommunicator peer)
        {
            GameNetwork.BeginModuleEventAsServer(peer);
            GameNetwork.WriteMessage(new ServerMessage(message));
            GameNetwork.EndModuleEventAsServer();
        }

    }
}
