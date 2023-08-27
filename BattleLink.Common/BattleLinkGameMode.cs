using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Source.Missions;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Common
{
    public class BattleLinkGameMode : MissionBasedMultiplayerGameMode
    {
        //(Mission missionController) => GetMissionBehaviorsServer()
        private readonly InitializeMissionBehaviorsDelegate behaviors;

        public BattleLinkGameMode(InitializeMissionBehaviorsDelegate _behaviors) : base("BattleLink")
        {
            behaviors = _behaviors;
        }

        public override void StartMultiplayerGame(string _scene)
        {
            MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - "+_scene, 0, DebugColor.Green);

            MissionState.OpenNew("BattleLink", new MissionInitializerRecord(_scene), behaviors);
            //MissionState.OpenNew("BattleLink", new MissionInitializerRecord("battle_terrain_f"), behaviors);
        }

        

    }
}
