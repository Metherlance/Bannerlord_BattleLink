using System.Linq;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.Core;
using TaleWorlds.Library;
//using TaleWorlds.LinQuick;

namespace BattleLink.Common.Model
{
    public class BLCharacterObject : BasicCharacterObject
    {
        public Occupation Occupation;
        private CharacterPerks _heroPerks = null;

        public BLCharacterObject() : base()
        {
            //Id = new TaleWorlds.ObjectSystem.MBGUID();
        }

        public bool GetPerkValue(PerkObject perk)
        {
            return IsHero && _heroPerks != null && _heroPerks.GetPropertyValue(perk) != 0;
        }

        public int Tier
        {
            get
            {
                // CharacterStatsModel
                //MissionGameModels.Current.CharacterStatsModel.GetTier(this);
                //GameModel.CharacterStatsModel.GetTier(this);
                //MissionGameModels.Current.CharacterStatsModel.GetTier(this);
                //return Campaign.Current.Models.CharacterStatsModel.GetTier(this);
                return IsHero ? 0 : MathF.Min(MathF.Max(MathF.Ceiling((float)((Level - 5.0) / 5.0)), 0), 6);
            }
        }

        public Equipment FirstBattleEquipment
        {
            get
            {
                return AllEquipments.FirstOrDefault((Equipment e) => !e.IsCivilian);
               // return AllEquipments.FirstOrDefaultQ((Equipment e) => !e.IsCivilian);
            }
        }

        public Equipment FirstCivilianEquipment
        {
            get
            {
                // return AllEquipments.FirstOrDefaultQ((Equipment e) => e.IsCivilian);
                return AllEquipments.FirstOrDefault((Equipment e) => e.IsCivilian);
            }
        }


    }
}
