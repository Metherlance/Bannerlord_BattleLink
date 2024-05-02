using BattleLink.Common.Model;
using System.Xml;
using System.Xml.Linq;
using TaleWorlds.CampaignSystem.Extensions;
using TaleWorlds.Core;

namespace BattleLink.Common
{
    public class BLBasicObject2Xml
    {
        public static XmlElement createClassDivision(BLCharacterObject basicCharacterObject)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement classDivision = doc.CreateElement("MPClassDivision");


            classDivision.SetAttribute("id", basicCharacterObject.StringId + "_class_division");
            classDivision.SetAttribute("hero", basicCharacterObject.StringId);
            classDivision.SetAttribute("troop", basicCharacterObject.StringId);
            // culture will be copy from hero classDivision.SetAttribute("culture", basicCharacterObject.Culture.StringId);
            classDivision.SetAttribute("hero_idle_anim", "act_vlandia_mp_peasant_levy_idle");
            classDivision.SetAttribute("troop_idle_anim", "act_idle_unarmed_1");
            classDivision.SetAttribute("multiplier", "0.92");
            // todo
            classDivision.SetAttribute("cost", "80");
            classDivision.SetAttribute("casual_cost", "90");
            classDivision.SetAttribute("icon", "Infantry_Light");
            classDivision.SetAttribute("melee_ai", "30");
            classDivision.SetAttribute("ranged_ai", "30");
            classDivision.SetAttribute("armor", "7");
            classDivision.SetAttribute("movement_speed", "0.82");
            classDivision.SetAttribute("combat_movement_speed", "0.9");
            classDivision.SetAttribute("acceleration", "1.8");

            var perks = doc.CreateElement("Perks");
            classDivision.AppendChild(perks);


            int equipmentSetIndex = 0;
            basicCharacterObject.AllEquipments.ForEach(equipmentSet =>
            {
                //if (!equipmentSet.IsCivilian)
                {
                    var perk = doc.CreateElement("Perk");
                    perks.AppendChild(perk);

                    perk.SetAttribute("game_mode", "all");
                    perk.SetAttribute("name", "{=*}Set" + equipmentSetIndex);
                    perk.SetAttribute("description", "{=*}Set" + equipmentSetIndex);
                    perk.SetAttribute("icon", "PerkToughness");
                    perk.SetAttribute("perk_list", "1");

                    for(int equipmentIndex = 0; equipmentIndex < (int) EquipmentIndex.NumEquipmentSetSlots; equipmentIndex+=1)
                    {
                        EquipmentIndex eqIndex = (EquipmentIndex)equipmentIndex;
                        EquipmentElement equipmentEl = equipmentSet[eqIndex];
                        ItemObject item = equipmentEl.Item;
                        if(item == null)
                        {
                            continue;
                        }

                        string slot = eqIndex.ToString();
                        slot.Replace("Weapon", "Item");

                        var spawnEffect = doc.CreateElement("OnSpawnEffect");
                        perk.AppendChild(spawnEffect);

                        spawnEffect.SetAttribute("type", "AlternativeEquipmentOnSpawn");
                        spawnEffect.SetAttribute("slot", slot);
                        spawnEffect.SetAttribute("item", item.StringId);
                    }

                    equipmentSetIndex+= 1;

                }
            });



            //            < Perk

            //    game_mode = "all"

            //    name = "{=*}Default"

            //    description = "{=*}Default."

            //    icon = "PerkToughness"

            //    perk_list = "1" >

            //</ Perk >

            return classDivision;

        }

        public static XmlElement createDummyCharacter(BasicCultureObject culture)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement character = doc.CreateElement("BLCharacter");

            character.SetAttribute("id", "bl_character_" + culture.StringId);
            character.SetAttribute("name", "BL Character " + culture.Name);

            character.SetAttribute("culture", "Culture." + culture.StringId);

            character.SetAttribute("default_group", FormationClass.Infantry.GetName());
            character.SetAttribute("is_female", "false");

            var face = doc.CreateElement("face");
            character.AppendChild(face);

            var bodyProperties = doc.CreateElement("BodyProperties");
            //bodyProperties.Value = bprop.ToString();
            bodyProperties.SetAttribute("version", "4");
            bodyProperties.SetAttribute("age", "23");
            bodyProperties.SetAttribute("weight", "0.3333");
            bodyProperties.SetAttribute("build", "0");
            bodyProperties.SetAttribute("key", "00000C07000000010011111211151111000701000010000000111011000101000000500202111110000000000000000000000000000000000000000000A00000");
            face.AppendChild(bodyProperties);

            var bodyPropertiesMax = doc.CreateElement("BodyPropertiesMax");
            //bodyPropertiesMax.Value = bprop.ToString().Replace("BodyProperties", "BodyPropertiesMax");
            bodyPropertiesMax.SetAttribute("version", "4");
            bodyPropertiesMax.SetAttribute("age", "23");
            bodyPropertiesMax.SetAttribute("weight", "0.3333");
            bodyPropertiesMax.SetAttribute("build", "0");
            bodyPropertiesMax.SetAttribute("key", "00000C07000000010011111211151111000701000010000000111011000101000000500202111110000000000000000000000000000000000000000000A00000");
            face.AppendChild(bodyPropertiesMax);


            var skills = doc.CreateElement("skills");
            character.AppendChild(skills);

            skills.AppendChild(createSkillValue(doc, "Riding", 20));
            skills.AppendChild(createSkillValue(doc, "OneHanded", 20));
            skills.AppendChild(createSkillValue(doc, "TwoHanded", 20));
            skills.AppendChild(createSkillValue(doc, "Polearm", 20));
            skills.AppendChild(createSkillValue(doc, "Crossbow", 20));
            skills.AppendChild(createSkillValue(doc, "Bow", 20));
            skills.AppendChild(createSkillValue(doc, "Throwing", 20));

            return character;
        }
        private static XmlNode createSkillValue(XmlDocument doc, string id, int value)
        {
            XmlElement skill = doc.CreateElement("skill");
            skill.SetAttribute("id", id);
            skill.SetAttribute("value", value + "");
            return skill;
        }
    }
}
