using BattleLink.Common.Spawn;
using RealmsBattle.Client.Behavior;
using RealmsBattle.Common.Behavior;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.GauntletUI.Mission;
using TaleWorlds.MountAndBlade.Multiplayer;
using TaleWorlds.MountAndBlade.Source.Missions;
using static TaleWorlds.Library.Debug;

namespace RealmsBattle.Multiplayer
{
    public class BattleLinkGameMode : MissionBasedMultiplayerGameMode
    {

        public BattleLinkGameMode() : base("BattleLink")
        {
        }

        public override void StartMultiplayerGame(string _scene)
        {
            MBDebug.Print("BattleLinkGameMode - StartMultiplayerGame - " + _scene, 0, DebugColor.Green);

            //0,256 -> -512 2048
            CompressionMission.AgentOffsetCompressionInfo = new CompressionInfo.Integer(-512, 11);

            InitializeMissionBehaviorsDelegate behaviors = (Mission) => new MissionBehavior[]{

                          (MissionBehavior) MissionLobbyComponent.CreateBehavior(),
                          (MissionBehavior) new MultiplayerAchievementComponent(),
                          (MissionBehavior) new BLMultiplayerWarmupComponent(),
                          //(MissionBehavior) new MultiplayerWarmupComponent(),
                          (MissionBehavior) new MissionMultiplayerGameModeFlagDominationClient(),
                          (MissionBehavior) new MultiplayerRoundComponent(),
                          (MissionBehavior) new MultiplayerTimerComponent(),
                          (MissionBehavior) new MultiplayerMissionAgentVisualSpawnComponent(),
                          new MissionGauntletOptionsUIHandler(),
                          //(MissionBehavior) new ConsoleMatchStartEndHandler(),
                          (MissionBehavior) new MissionLobbyEquipmentNetworkComponent(),
                          (MissionBehavior) new MultiplayerTeamSelectComponent(),
                          (MissionBehavior) new MissionHardBorderPlacer(),
                          (MissionBehavior) new MissionBoundaryPlacer(),
                          (MissionBehavior) new AgentVictoryLogic(),
                          (MissionBehavior) new MissionBoundaryCrossingHandler(),
                          (MissionBehavior) new MultiplayerPollComponent(),
                          (MissionBehavior) new MultiplayerAdminComponent(),
                          (MissionBehavior) new MultiplayerGameNotificationsComponent(),
                          (MissionBehavior) new MissionOptionsComponent(),
                          (MissionBehavior) new MissionScoreboardComponent((IScoreboardData) new CaptainScoreboardData()),
                          (MissionBehavior) MissionMatchHistoryComponent.CreateIfConditionsAreMet(),
                          (MissionBehavior) new EquipmentControllerLeaveLogic(),
                          (MissionBehavior) new MissionRecentPlayersComponent(),
                          (MissionBehavior) new MultiplayerPreloadHelper(),
                            new BLBehaviorClient()
                          //,new RBDebugMissionLogic()
                          //,new RBDebugMissionBehavior()
                          ,new BLDeploymentMissionView()

                };

            MissionState.OpenNew("BattleLink", new MissionInitializerRecord(_scene), behaviors);

        }
    }
}
