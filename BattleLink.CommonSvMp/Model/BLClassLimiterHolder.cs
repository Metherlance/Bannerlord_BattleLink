using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.CommonSvMp.Model
{
    public class BLClassLimiterHolder
    {
        //private static readonly ClassLimiterModel instance = new();
        //public static ClassLimiterModel Instance { get { return instance; } }

        //public event Action<BasicCharacterObject, bool> CharacterAvailabilityChanged;
        //public Dictionary<BasicCharacterObject, CharacterAvailability> CharactersAvailability { get; private set; }
        //public Dictionary<BasicCharacterObject, bool> CharactersAvailable { get; private set; }

        private Dictionary<NetworkCommunicator, BLPartyTroopAvaibilityMessage> _characterSelected = new();
        private Dictionary<(int, int, BasicCharacterObject), BLPartyTroopAvaibilityMessage> _characterSelectedPeer = new();



    }
}
