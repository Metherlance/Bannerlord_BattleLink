using BattleLink.Common;
using Helpers;
using Newtonsoft.Json;
using Replay;
using System;
using System.IO;
using System.Reflection;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem.Encounters;
using TaleWorlds.CampaignSystem.GameMenus;
using TaleWorlds.CampaignSystem.MapEvents;
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

           

            Vec2 pos = Vec2.Forward;
            A a = new A();
            a.v1 = pos;
            a.v2 = pos;
            B b = new B();
            b.v1 = a;
            //b.v2 = a;
            //b.V3 = a;
            //b.v5 = new A[]{ a, a };

            //string json = JsonUtils.SerializeObject(b, 10);
            //Console.WriteLine(json);
            //string json2 = JsonConvert.SerializeObject(b);

            TroopRosterElementDto troopRosterElementDto = new TroopRosterElementDto();
            troopRosterElementDto.characterStringId = "test";
            troopRosterElementDto.number = 1;
            troopRosterElementDto.woundedNumber = 0;
            troopRosterElementDto.xp = 0;

            PartyDto partyDto = new PartyDto();
            partyDto.data = new TroopRosterElementDto[] { troopRosterElementDto };

            SideDto sideDto = new SideDto();
            sideDto.party = partyDto;
            sideDto.winner = true;

            BattleResult battleResult = new BattleResult();
            battleResult.attacker = sideDto;
            battleResult.defender = sideDto;

            string json = JsonUtils.SerializeObject(battleResult, 10);
            // private string GetDirectoryFullPath(PlatformDirectoryPath directoryPath);
            //string path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "MB2_BL_2185888_res.json");
            //File.WriteAllText(path, json);


            CampaignBattleResult campaignBattleResult = new CampaignBattleResult();
            //campaignBattleResult.PlayerVictory = true;

            //Type t = typeof(CampaignBattleResult);
            //var f = t.GetField("<PlayerVictory>k__BackingField");
            //var p = t.GetProperty("PlayerVictory", BindingFlags.Public | BindingFlags.Instance);
            //p.SetValue(campaignBattleResult, true);
            //f.SetValue(campaignBattleResult, true);

            Console.WriteLine("finn");

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
    
    class B{
        public A v1;
        // public A v2;
        // public A V3 {get; set; }
        // public A V4 { get {
        //         return v1;
        //     }
        // }
        // public A[] v5;
        private A _v6 = new A();

        public A getV6()
        {
            return _v6;
        }

    }
    class A{
        public Vec2 v1;
        public Vec2 v2;
    }
}