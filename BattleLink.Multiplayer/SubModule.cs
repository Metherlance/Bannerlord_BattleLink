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
using TaleWorlds.InputSystem;
using TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.Lobby.CustomGame;
using static TaleWorlds.MountAndBlade.ViewModelCollection.Multiplayer.Lobby.CustomGame.MPCustomGameVM;
using Messages.FromLobbyServer.ToClient;
using TaleWorlds.MountAndBlade.Diamond;
using System.Collections.Generic;
using TaleWorlds.PlayerServices;
using Messages.FromClient.ToLobbyServer;
using TaleWorlds.Diamond;
using System.Runtime;
using System.Reflection;

namespace BattleLink.Multiplayer
{
    public class SubModule : MBSubModuleBase
    {
        private static readonly Color green = Color.FromUint(0x008000);

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            InformationManager.DisplayMessage(new InformationMessage("BattleLink - OnSubModuleLoad", green));

                // patch methods
                var harmony = new Harmony("bannerlord.mplocal");
                harmony.PatchAll();


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

        protected override void OnApplicationTick(float dt)
        {
            //if (InputKey.Numpad1.IsPressed())
            //{
            //    InformationManager.DisplayMessage(new InformationMessage("MPLocal - Join", green));
            //    var lobbyClient = NetworkMain.GameClient;
            //    //lobbyClient.Game

            //   // MPCustomGameVM a = new MPCustomGameVM(null, CustomGameMode.CustomServer);

            //   // new RequestCustomGameServerListMessage();
            //   // Client<LobbyClient> a;

            //   // var lobbyClient = new LobbyClient(null, null);

            //   //CustomGameServerListResponse customGameServerListResponse = lobbyClient.CallFunction<CustomGameServerListResponse>(new RequestCustomGameServerListMessage());
            //   // AvailableCustomGames avai = customGameServerListResponse.AvailableCustomGames;
            //   // a.RefreshCustomGameServerList(avai);

            //   // var ba = a.GameList;

            //   // var bb = a.GameList.Count;
            //    var lobbyState = new LobbyState();
            //    //var gameVM = new MPCustomGameVM(lobbyState, CustomGameMode.CustomServer);

            //    //var lobbyClient = new LobbyClient(gameVM, LobbyClient.LobbyServerPort);


            //    string name = "";
            //    string address = "127.0.0.1";
            //    int port = 7896;
            //    string region = "";
            //    string gameModule = "";
            //    string gameType = "";
            //    string map = "";
            //    string uniqueMapId = "";
            //    string gamePassword = "";
            //    string adminPassword = "";
            //    int maxPlayerCount = 4;
            //    bool isOfficial = false;
            //    bool byOfficialProvider = false;
            //    bool crossplayEnabled = true;
            //    PlayerId hostId = PlayerId.Empty;
            //    string hostName = "";
            //    List<ModuleInfoModel> loadedModules = new List<ModuleInfoModel>();
            //    bool allowsOptionalModules = true;
            //    int permission = 0;

            //    GameServerProperties gameServerProperties = new GameServerProperties(name, address, port, region, gameModule, gameType, map, uniqueMapId, gamePassword, adminPassword, maxPlayerCount, isOfficial, byOfficialProvider, crossplayEnabled, hostId, hostName, loadedModules, allowsOptionalModules, permission);
            //    JoinGameData joinGameData = new JoinGameData(gameServerProperties, 0, 0);
            //    TaleWorlds.MountAndBlade.Module.CurrentModule.GetMultiplayerGameMode(joinGameData.GameServerProperties.GameType).JoinCustomGame(joinGameData);
            //}
        }
    }


    //[HarmonyPatch(typeof(MissionBasedMultiplayerGameMode))]
    //class MissionBasedMultiplayerGameModePatch
    //{
    //    [HarmonyPrefix]
    //    [HarmonyPatch("JoinCustomGame")]
    //    static bool JoinCustomGame(MissionBasedMultiplayerGameMode __instance, ref JoinGameData joinGameData)
    //    {
    //            InformationManager.DisplayMessage(new InformationMessage("MPLocal - JoinCustomGame", Color.FromUint(0x008000)));
    //             return true;
    //    }
    //}

    //[HarmonyPatch(typeof(MPCustomGameVM))]
    //class MPCustomGameVMPatch
    //{
    //    [HarmonyPrefix]
    //    [HarmonyPatch("JoinCustomGame")]
    //    static bool JoinCustomGame(MPCustomGameVMPatch __instance, ref GameServerEntry selectedServer, ref string passwordInput)
    //    {
    //        InformationManager.DisplayMessage(new InformationMessage("MPLocal - JoinCustomGame", Color.FromUint(0x008000)));
    //        return true;
    //    }
    //}

    //[HarmonyPatch(typeof(LobbyClient))]
    //class MLobbyClientPatch
    //{
    //    [HarmonyPrefix]
    //    [HarmonyPatch("OnJoinCustomGameResultMessage")]
    //    static bool OnJoinCustomGameResultMessage(MPCustomGameVMPatch __instance, ref JoinCustomGameResultMessage message)
    //    {
    //        //InformationManager.DisplayMessage(new InformationMessage("MPLocal - OnJoinCustomGameResultMessage", Color.FromUint(0x008000)));
    //        if (message.Success)
    //        {
    //            GameServerProperties gameServerProperties = message.JoinGameData.GameServerProperties;

    //            //easyier that add address in DiamondClientApplication.ProxyAddressMap
    //            FieldInfo fieldInfo = typeof(GameServerProperties).GetField("<Address>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
    //            fieldInfo.SetValue(gameServerProperties, "127.0.0.1");
    //        }
    //        return true;
    //    }
    //}
    

}


