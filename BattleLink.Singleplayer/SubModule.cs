using BattleLink.Common.Debug;
using BattleLink.Singleplayer.Patch;
using HarmonyLib;
using Helpers;
using SandBox.GameComponents;
using System;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.CustomBattle;
using TaleWorlds.MountAndBlade.View.MissionViews;
using TaleWorlds.MountAndBlade.ViewModelCollection;
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

        protected override void OnSubModuleLoad()
        {
            //Agent a22 = (Agent)FormatterServices.GetUninitializedObject(typeof(Agent));
            //Scene a22 = (Scene)FormatterServices.GetUninitializedObject(typeof(Scene));
            //var a = a22.GetSunDirection();
            //var a2 = FormatterServices.GetUninitializedObject(typeof(TeamAISiegeDefender));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(TeamAISiegeAttacker));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(UsableMachine));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(TacticDefendCastle));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(SiegeLane));



            //var a2 = FormatterServices.GetUninitializedObject(typeof(TeamAISiegeComponent));
            //var a2 = FormatterServices.GetUninitializedObject(typeof(CharacterObject));
            //new SandboxBattleBannerBearersModel();
            //new BattlePowerCalculationLogic();
            //var a = FormatterServices.GetUninitializedObject(typeof(AgentBuildData));
            //var a = FormatterServices.GetUninitializedObject(typeof(MissionCombatantsLogic));
            //var a = FormatterServices.GetUninitializedObject(typeof(TacticComponent));

            //var a2 = (Mission)FormatterServices.GetUninitializedObject(typeof(MissionAgentSpawnLogic));

            //var a2 = (PlayerEncounter)FormatterServices.GetUninitializedObject(typeof(PlayerEncounter));
            //a2.SetupFields(null,null);

            base.OnSubModuleLoad();

            var harmony = new Harmony("BattleLink.Singleplayer.Patch.Permanent");
            var OpenNew = AccessTools.Method(typeof(MissionState), "OpenNew");
            var OpenNewPrefix = AccessTools.Method(typeof(MissionStatePatch), "OpenNew");
            harmony.Patch(OpenNew, prefix: new HarmonyMethod(OpenNewPrefix));


            // custom battle MP
            //Module.CurrentModule.AddInitialStateOption(new InitialStateOption("CustomBattleMP", new TextObject("{}Custom Battle MP"), 5001, () =>
            //{
            //    InformationManager.DisplayMessage(new InformationMessage("Custom Battle MP - Click"));
            //    MissionStatePatch.CampaignTimeSecBattleMP = DateTime.UtcNow.ToBinary();
            //    MBGameManager.StartNewGame(new CustomGameManager());
            //},
            //() => (false,null)
            //));


        }

        //protected override void OnSubModuleUnloaded()
        //{
        //    base.OnSubModuleUnloaded();
        //    MBDebug.Print("OnSubModuleUnloaded", 0, DebugColor.Cyan);

        //}

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(new InformationMessage("BattleLink | loaded", new Color(0, 1, 0)));

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

        public override void OnBeforeMissionBehaviorInitialize(Mission mission)
        {
            //var a = mission.Scene.GetSunDirection();

            mission.AddMissionBehavior(new BLDebugMissionLogic());
        }

        //public virtual void OnCampaignStart(Game game, object starterObject)
        //{
        //    MBDebug.Print("OnCampaignStart", 0, DebugColor.Cyan);
        //    InformationManager.DisplayMessage(new InformationMessage("OnCampaignStart"));
        //}
    }

}