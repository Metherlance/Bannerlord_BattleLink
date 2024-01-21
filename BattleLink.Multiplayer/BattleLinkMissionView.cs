using BattleLink.Views;
using System.Collections.Generic;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Multiplayer.View.MissionViews;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.MissionViews;

namespace BattleLink.Multiplayer
{
    public class BattleLinkMissionView
    {
        //see MultiplayerMissionViews
        [ViewCreatorModule]
        public class BattleMissionView
        {
            [ViewMethod("BattleLink")]
            public static MissionView[] OpenBattleLinkMission(Mission mission)
            {
                List<MissionView> missionViewList = new List<MissionView>();
                missionViewList.Add(MultiplayerViewCreator.CreateLobbyEquipmentUIHandler());
                missionViewList.Add(MultiplayerViewCreator.CreateMissionServerStatusUIHandler());
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerFactionBanVoteUIHandler());
                missionViewList.Add(MultiplayerViewCreator.CreateMissionMultiplayerPreloadView(mission));
                missionViewList.Add(MultiplayerViewCreator.CreateMissionKillNotificationUIHandler());
                missionViewList.Add(ViewCreator.CreateMissionAgentStatusUIHandler(mission));
                missionViewList.Add(ViewCreator.CreateMissionMainAgentEquipmentController(mission));
                missionViewList.Add(ViewCreator.CreateMissionMainAgentCheerBarkControllerView(mission));
                missionViewList.Add(MultiplayerViewCreator.CreateMissionMultiplayerEscapeMenu("Captain"));
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerMissionOrderUIHandler(mission));
                missionViewList.Add(ViewCreator.CreateMissionAgentLabelUIHandler(mission));
                missionViewList.Add(ViewCreator.CreateOrderTroopPlacerView(mission));
                // missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerTeamSelectUIHandler());//*
                missionViewList.Add(new BLTeamSelectView());//*
                missionViewList.Add(MultiplayerViewCreator.CreateMissionScoreBoardUIHandler(mission, false));
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerEndOfRoundUIHandler());
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerEndOfBattleUIHandler());
                missionViewList.Add(MultiplayerViewCreator.CreatePollProgressUIHandler());
                missionViewList.Add((MissionView)new MissionItemContourControllerView());
                missionViewList.Add((MissionView)new MissionAgentContourControllerView());
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerMissionHUDExtensionUIHandler());
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerMissionDeathCardUIHandler((Mission)null));
                missionViewList.Add(MultiplayerViewCreator.CreateMissionFlagMarkerUIHandler());
                missionViewList.Add(ViewCreator.CreateOptionsUIHandler());
                missionViewList.Add(ViewCreator.CreateMissionMainAgentEquipDropView(mission));
                missionViewList.Add(MultiplayerViewCreator.CreateMultiplayerAdminPanelUIHandler());
                missionViewList.Add(ViewCreator.CreateMissionBoundaryCrossingView());
                missionViewList.Add((MissionView)new MissionBoundaryWallView());
                missionViewList.Add((MissionView)new SpectatorCameraView());
                return missionViewList.ToArray();
            }
        }

    }
}
