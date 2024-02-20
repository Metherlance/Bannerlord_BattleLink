using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View.MissionViews.Order;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using System.Reflection;
using TaleWorlds.MountAndBlade.Multiplayer.GauntletUI.Mission;

namespace BattleLink.Views
{
    [OverrideView(typeof(MultiplayerMissionOrderUIHandler))]
    public class BLMissionGauntletMultiplayerOrderUIHandler : MissionGauntletMultiplayerOrderUIHandler
    {

        private static readonly FieldInfo fieldInit = typeof(MissionGauntletMultiplayerOrderUIHandler).GetField("_isInitialized", BindingFlags.NonPublic | BindingFlags.Instance);
        private static readonly FieldInfo fieldValid = typeof(MissionGauntletMultiplayerOrderUIHandler).GetField("_isValid", BindingFlags.NonPublic | BindingFlags.Instance);

        public override void OnMissionScreenTick(float dt)
        {
            bool _isInitialized = (bool) fieldInit.GetValue(this);
            bool _isValid = (bool) fieldValid.GetValue(this);

            //if (!this._shouldTick || this.MissionScreen.IsRadialMenuActive && !this._dataSource.IsToggleOrderShown)
            //    return;
            if (!_isInitialized)
            {
                Team team = GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>().Team : (Team)null;
                if (team != null && team.Side != BattleSideEnum.None)
                {
                    this.InitializeInADisgustingManner();
                }
            }
            if (!_isValid)
            {
                Team team = GameNetwork.IsMyPeerReady ? GameNetwork.MyPeer.GetComponent<MissionPeer>().Team : (Team)null;
                if (team == null || team.Side == BattleSideEnum.None)
                {
                    return;
                }
                this.ValidateInADisgustingManner();
            }

            base.OnMissionScreenTick(dt);

        }
    }
}
