using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Spawn.Warmup
{
    public class BLWarmupSpawnFrameBehavior : SpawnFrameBehaviorBase
    {
        public override void Initialize()
        {
            //like base method
            //this.SpawnPoints = Mission.Current.Scene.FindEntitiesWithTag("spawnpoint");
            base.Initialize();

            if (SpawnPoints.IsEmpty())
            {
                GameEntity entityWithTag = Mission.Current.Scene.FindEntityWithTag("spawnpoint_set");
                if (entityWithTag != null)
                {
                    SpawnPoints = entityWithTag.GetChildren();

                    int index = 0;
                    foreach (var spawn in SpawnPoints)
                    {
                        spawn.AddTag("sp_visual_" + index);//just for warmup round
                        //spawn.AddTag("starting");
                        //spawn.AddTag(index%2==0 ? "attacker" : "defender");

                        index += 1;
                        if (index > 5)
                        {
                            break;
                        }
                    }
                }
            }
        }
        public override MatrixFrame GetSpawnFrame(Team team, bool hasMount, bool isInitialSpawn)
        {
            return GetSpawnFrameFromSpawnPoints(SpawnPoints.ToList(), null, hasMount);
        }
    }
}
