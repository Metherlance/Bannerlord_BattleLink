using HarmonyLib;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace BattleLink.Singleplayer.Patch
{

    [HarmonyPatch(typeof(MissionState))]
    class MissionStatePatch
    {
        public static long CampaignTimeSecBattleMP = -1;

        [HarmonyPrefix]
        [HarmonyPatch("OpenNew")]
        static bool OpenNew(ref Mission __result, string missionName, MissionInitializerRecord rec)
        {
            if ( (Campaign.Current!=null && CampaignTimeSecBattleMP == BattleLinkSingleplayerBehavior.getCampainTimeSec())
                || (Campaign.Current == null && CampaignTimeSecBattleMP>0) )
            {
                BattleLinkSingleplayerBehavior.createAndSendFile(rec, missionName);
                //createAndSendMi0ssionInitializerRecord(rebehaviorsDelegate);
                CampaignTimeSecBattleMP = -1;
                //skip normal path, then later calcul the result
                __result = null;
                return false;
            }
            else
            {
                CampaignTimeSecBattleMP = -1;
                //normal path
                return true;
            }
        }
    }
}
