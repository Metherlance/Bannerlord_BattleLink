using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.CommonSvMp.NetworkMessages.FromServer
{
    // copy from AdminRequestEndWarmup
    [DefineGameNetworkMessageTypeForMod(GameNetworkMessageSendType.FromClient)]
    public sealed class BLMainHeroRequestEndWarmupMessage : GameNetworkMessage
    {
        protected override bool OnRead()
        {
            return true;
        }

        protected override void OnWrite()
        {
        }

        protected override MultiplayerMessageFilter OnGetLogFilter()
        {
            return MultiplayerMessageFilter.Administration;
        }

        protected override string OnGetLogFormat()
        {
            return "Requested to end warmup";
        }
    }
}
