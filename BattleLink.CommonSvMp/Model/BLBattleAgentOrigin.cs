using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Model
{
    public class BLBattleAgentOrigin : BasicBattleAgentOrigin
    {
        //public BasicCharacterObject BLTroop { get;  set; }
        public int partyIndex; // put IBattleCombatant PartyBase instead?

        public BLBattleAgentOrigin(BasicCharacterObject? troop, int partyIndex) : base(troop)
        {
            this.partyIndex = partyIndex;
        }
        public BLBattleAgentOrigin(BasicCharacterObject? troop) : base(troop)
        {
            this.partyIndex = -1;
        }
    }
}
