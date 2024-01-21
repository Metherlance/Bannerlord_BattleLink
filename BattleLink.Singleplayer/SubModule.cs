using System;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Singleplayer
{
    public class SubModule : MBSubModuleBase
    {

        //protected override void OnApplicationTick(float dt)
        //{
        //    if (Campaign.Current != null)
        //    {
        //        if (PlayerEncounter.Current != null)
        //        {
        //            MapEventParty.Equals(null, null);
        //            //var e = PlayerEncounter.Battle;
        //            var a = PlayerEncounter.Current;
        //            //var s = PlayerEncounter.BattleState;
        //            MBDebug.Print("OnSubModuleLoad", 0, DebugColor.Cyan);
        //        }

        //    }
        //}

        //protected override void OnSubModuleLoad()
        //{
        //    //var a2 = (Mission)FormatterServices.GetUninitializedObject(typeof(MissionAgentSpawnLogic));

        //    //var a2 = (PlayerEncounter)FormatterServices.GetUninitializedObject(typeof(PlayerEncounter));
        //    //a2.SetupFields(null,null);

        //    base.OnSubModuleLoad();


        //}

        //protected override void OnSubModuleUnloaded()
        //{
        //    base.OnSubModuleUnloaded();
        //    MBDebug.Print("OnSubModuleUnloaded", 0, DebugColor.Cyan);

        //}

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(new InformationMessage("BattleLink | loaded", new Color(0,1,0)));

        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            MBDebug.Print("InitializeGameStarter", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("InitializeGameStarter"));
            if (starterObject is CampaignGameStarter campaignstarter)
            {
                campaignstarter.AddBehavior(new BattleLinkSingleplayerBehavior());
            }
        }

        //public virtual void OnCampaignStart(Game game, object starterObject)
        //{
        //    MBDebug.Print("OnCampaignStart", 0, DebugColor.Cyan);
        //    InformationManager.DisplayMessage(new InformationMessage("OnCampaignStart"));
        //}
    }

}