using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using TaleWorlds.Core;
using TaleWorlds.DotNet;
using TaleWorlds.Engine;
using TaleWorlds.InputSystem;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade.Missions.Handlers;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using System.Reflection;

namespace BattleLink.Views
{
    [OverrideView(typeof(OrderTroopPlacer))]
    public class BLOrderTroopPlacer : OrderTroopPlacer
    {

        private static readonly FieldInfo fieldInit = typeof(OrderTroopPlacer).GetField("_initialized", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void OnMissionTick(float dt)
        {
            bool _initialized = (bool) fieldInit.GetValue(this);

            if (!_initialized)
            {
                MissionPeer missionPeer = (GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>() : null);
                if (base.Mission.PlayerTeam != null)
                {
                    base.OnMissionTick(dt);
                }
                else if (missionPeer != null && missionPeer.Team != null && missionPeer.Team.Side != BattleSideEnum.None)
                {
                    base.Mission.PlayerTeam = missionPeer.Team;
                    base.OnMissionTick(dt);
                }
                // else do nothing
            }
        }
    }
}
