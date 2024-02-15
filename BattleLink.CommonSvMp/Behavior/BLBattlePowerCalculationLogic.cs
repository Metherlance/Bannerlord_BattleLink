using System.Collections.Generic;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Common.Behavior
{
    public class BLBattlePowerCalculationLogic
    {
        private Dictionary<Team, float>[] _sidePowerData;

        public bool IsTeamPowersCalculated { get; private set; }


        public new Mission Mission { get; set; }

        public void setMission(Mission mission)
        {
            this.Mission = mission;
            for (int index = 0; index < 2; ++index)
            {
                this._sidePowerData[index].Clear();
            }
            this.IsTeamPowersCalculated = false;
        }

        public BLBattlePowerCalculationLogic()
        {
            this._sidePowerData = new Dictionary<Team, float>[2];
            for (int index = 0; index < 2; ++index)
            {
                this._sidePowerData[index] = new Dictionary<Team, float>();
            }
            this.IsTeamPowersCalculated = false;
        }

        public float GetTotalTeamPower(Team team)
        {
            if (!this.IsTeamPowersCalculated)
            {
                this.CalculateTeamPowers();
            }
            return this._sidePowerData[(int)team.Side][team];
        }

        private void CalculateTeamPowers()
        {
            List<Team> teams = this.Mission.Teams.Where(t=>t.Side != BattleSideEnum.None).ToList();
            foreach (Team key in teams)
            {
                this._sidePowerData[(int)key.Side].Add(key, 0.0f);
            }

            foreach (Team key in teams)
            {
                Dictionary<Team, float> dictionary = this._sidePowerData[(int)key.Side];
                foreach (Agent troopOrigin in key.ActiveAgents)
                {
                    dictionary[key] += troopOrigin.CharacterPowerCached;
                }
            }

            foreach (Team team in teams)
            {
                team.QuerySystem.Expire();
            }
            this.IsTeamPowersCalculated = true;
        }
    }
}
