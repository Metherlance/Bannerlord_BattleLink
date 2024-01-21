//using System.Collections.Generic;
//using System.Xml.Serialization;
//using TaleWorlds.Core;

//namespace BattleLink.Common.DtoSpSv
//{
//    // not used
//    public class BattleResult
//    {
//        //this._mapEvent.AttackerSide.LeaderParty.MobileParty.Party.MemberRoster
//        public SideDto attacker;
//        public SideDto defender;
//    }

//    public struct SideDto
//    {
//        public bool winner;
//        public bool surrender;
//        // TODO multiple parties
//        public PartyDto[] parties;
//    }

//    public struct PartyDto
//    {
//        [XmlIgnore]
//        public BattleSideEnum side;

//        public int partyIndex;
//        public List<TroopRosterElementDto> data;

//        public PartyDto()
//        {
//            side = BattleSideEnum.None;
//            partyIndex = -1;
//            data = new List<TroopRosterElementDto>();
//        }
//    }


//    public struct TroopRosterElementDto
//    {
//        [XmlIgnore]
//        int partyIndex;

//        public string characterStringId;
//        public int hp;

//        public int number;
//        public int woundedNumber;
//        public int xp;

//        public int killedNumber;
//        public int rootedNumber;

//    }
//}
