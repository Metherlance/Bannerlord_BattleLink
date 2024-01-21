using System.Linq;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Spawn.Battle
{
    public class BLFlagDominationSpawnFrameBehavior : SpawnFrameBehaviorBase
    {
        public override void Initialize()
        {
            base.Initialize();
        }

        public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
        {
            return GetSpawnFrameFromSpawnPoints(SpawnPoints.ToList(), null, hasMount);
        }
    }
}
