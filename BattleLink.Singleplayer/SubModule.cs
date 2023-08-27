using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Singleplayer
{
    // see https://github.com/LesserScholar/ArtisanBeer/blob/master/SubModule.cs
    // https://www.youtube.com/watch?v=WIsGqcGOeZQ
    public class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            MBDebug.Print("OnSubModuleLoad", 0, DebugColor.Cyan);

        }

        protected override void OnSubModuleUnloaded()
        {
            base.OnSubModuleUnloaded();
            MBDebug.Print("OnSubModuleUnloaded", 0, DebugColor.Cyan);

        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            MBDebug.Print("OnBeforeInitialModuleScreenSetAsRoot", 0, DebugColor.Cyan);
            InformationManager.DisplayMessage(new InformationMessage("OnBeforeInitialModuleScreenSetAsRoot"));

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