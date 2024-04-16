using BattleLink.Common.DtoSpSv;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Server.Behavior
{
    public class BLMissionAgentSpawnLogic : MissionAgentSpawnLogic
    {
        public static BLMissionAgentSpawnLogic newSpawnLogic(string mapEventType)
        {
            Mission.BattleSizeType battleSizeType;
            if ("FieldBattle".Equals(mapEventType))
            {
                battleSizeType = Mission.BattleSizeType.Battle;
            }
            else if ("Siege".Equals(mapEventType))
            {
                battleSizeType = Mission.BattleSizeType.Siege;
            }
            else
            {
                battleSizeType = Mission.BattleSizeType.SallyOut;
            }
            var logic = new BLMissionAgentSpawnLogic(battleSizeType);
            return logic;
        }

        public BLMissionAgentSpawnLogic(Mission.BattleSizeType battleSizeType) : base(new IMissionTroopSupplier[] {null,null}, BattleSideEnum.Attacker, battleSizeType)
        {

        }

        public BLMissionAgentSpawnLogic(IMissionTroopSupplier[] suppliers, BattleSideEnum playerSide, Mission.BattleSizeType battleSizeType) : base(suppliers, playerSide, battleSizeType)
        {
        }

        public override void AfterStart()
        {
        }

        public override void OnMissionTick(float dt)
        {
        }

        public override void OnBehaviorInitialize()
        {
        }

        protected override void OnEndMission()
        {
        }

        public new void StartSpawner(BattleSideEnum side)
        {
        }

        public new void StopSpawner(BattleSideEnum side)
        {
        }

        public new bool IsSideSpawnEnabled(BattleSideEnum side)
        {
            return true;
        }
        public new float GetReinforcementInterval()
        {
            return 0;
        }

        public new bool IsSideDepleted(BattleSideEnum side)
        {
            return false;
        }


    }
}
