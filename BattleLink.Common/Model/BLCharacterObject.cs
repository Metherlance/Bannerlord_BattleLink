using TaleWorlds.Core;

namespace BattleLink.Common.Model
{
    public class BLCharacterObject : BasicCharacterObject
    {
        public BLCharacterObject() : base()
        {
            Id = new TaleWorlds.ObjectSystem.MBGUID();
        }
    }
}
