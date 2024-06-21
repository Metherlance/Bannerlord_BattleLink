using BattleLink.Common.Behavior;
using BattleLink.Common.Spawn;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using System.Linq;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;

namespace BattleLink.Server.Handler
{
    public class BLMainHeroRequestEndWarmupHandler
    {
        // ClientMessageHandlerDelegate
        public static bool HandleClientEventMainHeroRequestEndWarmupMessage(NetworkCommunicator peer, BLMainHeroRequestEndWarmupMessage _)
        {
            //BLMainHeroRequestEndWarmupMessage message = (BLMainHeroRequestEndWarmupMessage)mes;

            if (peer==null || Mission.Current == null)
            {
                return false;
            }

            if(!peer.IsAdmin && BLReferentialHolder.listPlayer.Where(x => peer.UserName.Equals(x.UserName) && "main_hero".Equals(x.TroopId)).IsEmpty())
            {
                return false;
            }

            var warmupComp = Mission.Current.GetMissionBehavior<BLMultiplayerWarmupComponent>();
            if (warmupComp!=null)
            {
                warmupComp.EndingWarmupByMainHero();
                return true;
            }
            return false;
        }
    }
}
