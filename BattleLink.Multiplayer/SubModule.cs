using TaleWorlds.MountAndBlade;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Library;
using System;
using System.IO;
using TaleWorlds.Engine;
using BattleLink.Common;
using TaleWorlds.MountAndBlade.Source.Missions;
using static TaleWorlds.Library.Debug;
using TaleWorlds.MountAndBlade.View.MissionViews.Singleplayer;

namespace BattleLink.Multiplayer
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly Color green = Color.FromUint(0x008000);

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnSubModuleLoad", green));
        }
        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            base.OnBeforeInitialModuleScreenSetAsRoot();

            InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnBeforeInitialModuleScreenSetAsRoot", green));
        }

        protected override void InitializeGameStarter(Game game, IGameStarter starterObject)
        {
            MBDebug.Print("BattleLink - InitializeGameStarter");
            base.InitializeGameStarter(game, starterObject);
            game.GameTextManager.LoadGameTexts();
            MBDebug.Print("BattleLink - InitializeGameStarter - End", 0, DebugColor.Green);
        }
    }

}


