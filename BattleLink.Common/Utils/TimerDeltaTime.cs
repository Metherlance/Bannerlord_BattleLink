using System;
using System.Collections.Generic;
using System.Text;

namespace BattleLink.Common.Utils
{
    public class TimerDeltaTime
    {
        private float timePast = 0;
        private float timeToWait;

        public TimerDeltaTime(float timeToWait)
        {
            this.timeToWait = timeToWait;
        }

        public bool ResetIfCheck(float dt)
        {
            timePast += dt;
            if (timePast >= timeToWait)
            {
                timePast = 0;
                return true;
            }
            return false;
        }
    }
}
