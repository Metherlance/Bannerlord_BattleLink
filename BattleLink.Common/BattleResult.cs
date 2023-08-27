using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.SaveSystem;

namespace BattleLink.Common
{
    public class BattleResult
    {
        //this._mapEvent.AttackerSide.LeaderParty.MobileParty.Party.MemberRoster
        public SideDto attacker;
        public SideDto defender;
    }

    public struct SideDto
    {
        public bool winner;
        public bool surrender;
        // TODO multiple parties
        public PartyDto party;
    }

    public struct PartyDto
    {
        public TroopRosterElementDto[] data;
    }


    public struct TroopRosterElementDto
    {
        public string characterStringId;
        public int hp;

        public int number;
        public int woundedNumber;
        public int xp;

        public int killedNumber;
        public int rootedNumber;

    }
}
