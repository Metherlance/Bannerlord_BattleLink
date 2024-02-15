using BattleLink.Common.Behavior;
using BattleLink.Common.Model;
using System;
using System.Collections.Generic;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.MountAndBlade;
using TaleWorlds.ObjectSystem;
using static TaleWorlds.Library.Debug;

namespace BattleLink.Server
{
    public sealed class BLSendReferential2Client : IUdpNetworkHandler
    {
        private static List<BLInitCharactersMessage> listCharacterMessage;
        private static List<BLInitCultureMessage> listCultureMessage;

        //static init singleton / check id already present

        public BLSendReferential2Client()
        {
            listCultureMessage = new List<BLInitCultureMessage>();
            foreach (var team in BLReferentialHolder.listTeam)
            {
                BasicCultureObject cultureObject = MBObjectManager.Instance.GetObject<BasicCultureObject>(team.Culture);
                listCultureMessage.Add(new BLInitCultureMessage()
                {
                    mbguid = cultureObject.Id.InternalValue,
                    id = team.Culture,
                    name = team.Name,
                    Color = Convert.ToUInt32(team.Color, 16),
                    Color2 = Convert.ToUInt32(team.Color2, 16),
                    FactionBannerKey = team.FactionBannerKey,

                });
                // index += 1;
            }


            //prepare message of character
            listCharacterMessage = new List<BLInitCharactersMessage>();
            int index = 0;
            foreach (BLCharacterObject character in BLReferentialHolder.basicCharacterObjects)
            {
                listCharacterMessage.Add(new BLInitCharactersMessage()
                {
                    mbguid = character.Id.InternalValue,
                    id = character.StringId,
                    name = character.Name.ToString(),
                    isFemale = character.IsFemale,
                    defaultGroup = character.DefaultFormationGroup,

                    occupation = (int)character.Occupation,

                    bodyPropertiesValue = character.BodyPropertyRange.BodyPropertyMin,
                    bodyPropertiesValueMax = character.BodyPropertyRange.BodyPropertyMax,

                    indexDic = index,

                    skillRiding = character.GetSkillValue(DefaultSkills.Riding),
                    skillOneHanded = character.GetSkillValue(DefaultSkills.OneHanded),
                    skillTwoHanded = character.GetSkillValue(DefaultSkills.TwoHanded),
                    skillPolearm = character.GetSkillValue(DefaultSkills.Polearm),
                    skillCrossbow = character.GetSkillValue(DefaultSkills.Crossbow),
                    skillBow = character.GetSkillValue(DefaultSkills.Bow),
                    skillThrowing = character.GetSkillValue(DefaultSkills.Throwing),
                    skillAthletics = character.GetSkillValue(DefaultSkills.Athletics),

                    culture = character.Culture?.StringId,

                });
                index += 1;
            }


            //???
            BLReferentialHolder.listCharacterMessage = listCharacterMessage;
            BLReferentialHolder.listCultureMessage = listCultureMessage;

        }


        public void HandleEarlyNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
        }

        public void HandleEarlyPlayerDisconnect(NetworkCommunicator networkPeer)
        {
        }

        public void HandleLateNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
            //do before BaseNetworkComponent (init team)
            foreach (var cultureMessage in listCultureMessage)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(cultureMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            // not necessary before BaseNetworkComponent
            foreach (var characterMessage in listCharacterMessage)
            {
                GameNetwork.BeginModuleEventAsServer(networkPeer);
                GameNetwork.WriteMessage(characterMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            MBDebug.Print("BLSendReferential2Client - HandleLateNewClientAfterLoadingFinished - " + networkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandleLateNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
        }

        public void HandleNewClientAfterLoadingFinished(NetworkCommunicator networkPeer)
        {
        }

        public void HandleNewClientAfterSynchronized(NetworkCommunicator networkPeer)
        {
        }

        public void HandleNewClientConnect(PlayerConnectionInfo clientConnectionInfo)
        {
            //do before BaseNetworkComponent (init team)
            foreach (var cultureMessage in listCultureMessage)
            {
                GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
                GameNetwork.WriteMessage(cultureMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            // not necessary before BaseNetworkComponent
            foreach (var characterMessage in listCharacterMessage)
            {
                GameNetwork.BeginModuleEventAsServer(clientConnectionInfo.NetworkPeer);
                GameNetwork.WriteMessage(characterMessage);
                GameNetwork.EndModuleEventAsServer();
            }

            MBDebug.Print("BLSendReferential2Client - HandleNewMission - " + clientConnectionInfo.NetworkPeer.UserName + " - end ", 0, DebugColor.Green);
        }

        public void HandlePlayerDisconnect(NetworkCommunicator networkPeer)
        {
        }

        public void OnDisconnectedFromServer()
        {
        }

        public void OnEveryoneUnSynchronized()
        {
        }

        public void OnPlayerDisconnectedFromServer(NetworkCommunicator networkPeer)
        {
        }

        public void OnUdpNetworkHandlerClose()
        {
        }

        public void OnUdpNetworkHandlerTick(float dt)
        {
        }
    }
}
