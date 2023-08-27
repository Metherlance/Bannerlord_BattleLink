using System;
using System.Collections.Generic;
using System.Text;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;

namespace BattleLink.Common
{
    public class BattleSetting
    {
        public MissionInitializerRecord rec { get; set; }

        public MapEvent mapEvent { get; set; }
        public MapEventSide mesDef { get; set; }
        public MapEventSide mesAtt { get; set; }

        public int Id { get; set; }
        public int SSN { get; set; }
        public string Message { get; set; }
    }
}
