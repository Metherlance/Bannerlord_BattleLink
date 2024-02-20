using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Singleplayer.Patch
{
    [HarmonyPatch(typeof(Agent))]
    class AgentPatch
    {
        //private static Random rand = new Random();

        [HarmonyPrefix]
        [HarmonyPatch("GetAgentFlags")]
        static bool GetAgentFlags(ref AgentFlag __result, Agent __instance)
        {
            if (__instance.Origin is BLAgentOriginSp blOrigin)
            {
                __result = blOrigin.agentFlag;
            }
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("get_State")]
        static bool get_State(ref AgentState __result, Agent __instance)
        {
            if (__instance.Origin is BLAgentOriginSp blOrigin)
            {
                __result = blOrigin.agentState;
            }
            return false;
        }
    }
}
