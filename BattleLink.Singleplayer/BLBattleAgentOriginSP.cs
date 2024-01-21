using BattleLink.Common.DtoSpSv;
using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.AgentOrigins;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.CampaignSystem.Actions.KillCharacterAction;

namespace BattleLink.Singleplayer
{
    // in SP it is PartyGroupAgentOrigin (unless for horses null)
    public class BLAgentOriginSp : IAgentOriginBase
    {
        public AgentFlag agentFlag = AgentFlag.None;
        public AgentState agentState = AgentState.None;
        public Agent mount=null;
        public Agent rider=null;
        public int partyIndex=-1;
        //public int troopElementIndex=-1;
        public FlattenedTroopRosterElement troopElement;
        internal MapEventParty mapEventParty;

        public BLAgentOriginSp()
        {
        }

        public BLAgentOriginSp(BasicCharacterObject troop)
        {
        }

        public BLAgentOriginSp(BasicCharacterObject troop, AgentFlag agentFlag, AgentState agentState)
        {
            this.agentFlag = agentFlag;
            this.agentState = agentState;
        }

        BasicCharacterObject IAgentOriginBase.Troop => troopElement.Troop;

        void IAgentOriginBase.OnScoreHit(BasicCharacterObject victim, BasicCharacterObject captain, int damage, bool isFatal, bool isTeamKill, WeaponComponentData attackerWeapon)
        {
            mapEventParty.OnTroopScoreHit(troopElement.Descriptor, (CharacterObject)victim, damage, isFatal, isTeamKill, attackerWeapon, false);
        }
        public bool IsUnderPlayersCommand
        {
            get
            {
                return troopElement.Troop == Hero.MainHero.CharacterObject || PartyGroupAgentOrigin.IsPartyUnderPlayerCommand(mapEventParty.Party);
            }
        }

        uint IAgentOriginBase.FactionColor => throw new NotImplementedException();

        uint IAgentOriginBase.FactionColor2 => throw new NotImplementedException();

        public IBattleCombatant BattleCombatant
        {
            get
            {
                return mapEventParty.Party;
            }
        }

        int IAgentOriginBase.UniqueSeed => throw new NotImplementedException();

        int IAgentOriginBase.Seed => throw new NotImplementedException();

        Banner IAgentOriginBase.Banner => throw new NotImplementedException();


        
        void IAgentOriginBase.SetWounded()
        {
            mapEventParty.OnTroopWounded(troopElement.Descriptor);
            if (troopElement.Troop.IsHero)
            {
                troopElement.Troop.HeroObject.MakeWounded();// dont put Hero or KillCharacterActionDetail.WoundedInBattle here, it is for death...
            }
        }

        public void SetKilled(Hero strikerHero)
        {
            mapEventParty.OnTroopKilled(troopElement.Descriptor);
            if (troopElement.Troop.IsHero)
            {
                // use KillCharacterAction.ApplyByBattle() instead like PartyGroupAgentOrigin TODO
                troopElement.Troop.HeroObject.AddDeathMark(strikerHero, KillCharacterActionDetail.DiedInBattle);
            }
        }

        void IAgentOriginBase.SetKilled()
        {
            SetKilled(null);
        }

        void IAgentOriginBase.SetRouted()
        {
            mapEventParty.OnTroopRouted(troopElement.Descriptor);
        }

        void IAgentOriginBase.OnAgentRemoved(float agentHealth)
        {
            if (!troopElement.Troop.IsHero)
                return;
            troopElement.Troop.HeroObject.HitPoints = MathF.Max(1, MathF.Round(agentHealth));
        }

        void IAgentOriginBase.SetBanner(Banner banner)
        {
            throw new NotImplementedException();
        }
       
    }
}
