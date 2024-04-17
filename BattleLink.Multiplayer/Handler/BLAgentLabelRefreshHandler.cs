using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using System.Reflection;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.ObjectSystem;

namespace BattleLink.Handler
{
    public class BLAgentLabelRefreshHandler
    {
        public static void HandleServerEventAgentLabelRefresh(GameNetworkMessage baseMessage)
        {
            BLAgentLabelRefreshMessage agentLabelRefreshMessage = (BLAgentLabelRefreshMessage)baseMessage;
            //Agent agent = Mission.Current.AllAgents[agentLabelRefreshMessage.id];
            // ...
            Agent agent = Mission.Current.FindAgentWithIndex(agentLabelRefreshMessage.id);
            var view = Mission.Current.GetMissionBehavior<MissionAgentLabelView>();

            
           MethodInfo agentUpdate = typeof(MissionAgentLabelView).GetMethod("BannerBearerLogic_OnBannerBearerAgentUpdated", BindingFlags.Instance | BindingFlags.NonPublic);

           agentUpdate.Invoke(view, new object[] { agent , true});

        }

    }
}