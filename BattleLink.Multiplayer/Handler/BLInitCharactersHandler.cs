using BattleLink.Common;
using BattleLink.Common.Utils;
using BattleLink.CommonSvMp.NetworkMessages.FromServer;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace BattleLink.Handler
{
    public class BLInitCharactersHandler
    {

        public static void HandleServerEventInitCharactersMessage(GameNetworkMessage mes)
        {
            BLInitCharactersMessage message = (BLInitCharactersMessage)mes;
            //message.CharacterXml

            XmlDocument doc = new XmlDocument();
            XmlElement character = doc.CreateElement("BLCharacter");

            character.SetAttribute("id", message.id);
            character.SetAttribute("name", message.name);
            character.SetAttribute("default_group", ((FormationClass)message.defaultGroup).GetName());
            character.SetAttribute("is_female", message.isFemale.ToString());


            if (!message.culture.IsEmpty())
            {
                character.SetAttribute("culture", "Culture." + message.culture);
            }


            var face = doc.CreateElement("face");
            character.AppendChild(face);

            var bprop = message.bodyPropertiesValue;
            var bodyProperties = doc.CreateElement("BodyProperties");
            //bodyProperties.Value = bprop.ToString();
            bodyProperties.SetAttribute("version", "4");
            bodyProperties.SetAttribute("age", bprop.Age + "");
            bodyProperties.SetAttribute("weight", bprop.Weight + "");
            bodyProperties.SetAttribute("build", bprop.Build + "");
            var key = bprop.StaticProperties.ToString();
            bodyProperties.SetAttribute("key", key.Substring(5, key.Length - 7));
            face.AppendChild(bodyProperties);

            var bpropMax = message.bodyPropertiesValue;
            var bodyPropertiesMax = doc.CreateElement("BodyPropertiesMax");
            //bodyPropertiesMax.Value = bprop.ToString().Replace("BodyProperties", "BodyPropertiesMax");
            bodyPropertiesMax.SetAttribute("version", "4");
            bodyPropertiesMax.SetAttribute("age", bpropMax.Age + "");
            bodyPropertiesMax.SetAttribute("weight", bpropMax.Weight + "");
            bodyPropertiesMax.SetAttribute("build", bpropMax.Build + "");
            var keyMax = bpropMax.StaticProperties.ToString();
            bodyPropertiesMax.SetAttribute("key", keyMax.Substring(5, keyMax.Length - 7));
            face.AppendChild(bodyPropertiesMax);


            var skills = doc.CreateElement("skills");
            character.AppendChild(skills);

            skills.AppendChild(createSkillValue(doc, "Riding", message.skillRiding));
            skills.AppendChild(createSkillValue(doc, "OneHanded", message.skillOneHanded));
            skills.AppendChild(createSkillValue(doc, "TwoHanded", message.skillTwoHanded));
            skills.AppendChild(createSkillValue(doc, "Polearm", message.skillPolearm));
            skills.AppendChild(createSkillValue(doc, "Crossbow", message.skillCrossbow));
            skills.AppendChild(createSkillValue(doc, "Bow", message.skillBow));
            skills.AppendChild(createSkillValue(doc, "Throwing", message.skillThrowing));
            skills.AppendChild(createSkillValue(doc, "Athletics", message.skillAthletics));



            var equipmentRoasters = doc.CreateElement("Equipments");
            character.AppendChild(equipmentRoasters);

            foreach(Equipment equip in message.equipmentRoasters)
            {
                var equipmentRoster = doc.CreateElement("EquipmentRoster");
                equipmentRoasters.AppendChild(equipmentRoster);
                equipmentRoster.SetAttribute("civilian", "false");




                for (EquipmentIndex eqIndex = EquipmentIndex.Weapon0; eqIndex<EquipmentIndex.NumEquipmentSetSlots;eqIndex+=1)
                {
                    if (equip[eqIndex].Item!=null)
                    {
                        var equipment = doc.CreateElement("equipment");
                        equipmentRoster.AppendChild(equipment);

                        if (EquipmentIndex.Weapon0==eqIndex)
                        {
                            equipment.SetAttribute("slot", "Item0");
                        }
                        else if (EquipmentIndex.ExtraWeaponSlot == eqIndex)
                        {
                            equipment.SetAttribute("slot", "ExtraWeaponSlot");
                        }
                        else if (EquipmentIndex.Head == eqIndex)
                        {
                            equipment.SetAttribute("slot", "Head");
                        }
                        else if (EquipmentIndex.Horse == eqIndex)
                        {
                            equipment.SetAttribute("slot", "Horse");
                        }
                        else
                        {
                            equipment.SetAttribute("slot", eqIndex.ToString().Replace("Weapon", "Item"));
                        }

                        equipment.SetAttribute("id", "Item."+equip[eqIndex].Item.StringId);

                    }
                }
            }

            //      message.bodyPropertiesValue

            //            < face >
            //  < BodyProperties version = "4" age = "25.569999694824219" weight = "0.31639999151229858" build = "0.24850000441074371" key = "00052008000000010070030000001603003300110010200000000001010000000144501304010000000000000000000000000000000000000000000010880000" />
            //  < BodyPropertiesMax version = "4" age = "39" weight = "0.72229999303817749" build = "0.70520001649856567" key = "0010F80FC000370FFFFFEFEFFEEFFF7EEEFFFFFFFEF7EFFEFFFFFFFFFEFFFFFF07CBF0780CFFFFEF000000000000000000000000000000000000000061809105" />
            //</ face >

            //var characterEmpty = MBObjectManager.Instance.CreateObject<BasicCharacterObject>(message.id);
            //characterEmpty.Name = new TextObject(message.name);
            //var bco = MBObjectManager.Instance.RegisterObject<BasicCharacterObject>(new BasicCharacterObject()
            //{
            //    StringId = message.id,
            //    _basicName = new TextObject(message.name),
            //        DefaultFormationClass = (FormationClass)message.defaultGroup,
            //        IsFemale = message.isFemale,
            //            BodyProperties = message.bodyPropertiesValue,
            //            BodyPropertiesMax = message.bodyPropertiesValue

            //});

            var basicCharacterObject = BLMBObjectManagerUtils.CreateCharacterFromXmlNode(character, new MBGUID(message.mbguid));

            //BattleLinkGameMode.basicCharacterObjects.Add(basicCharacterObject);
            // var a = basicCharacterObject.GetBodyPropertiesMax();

            XmlElement classDivisionXml = BLBasicObject2Xml.createClassDivision(basicCharacterObject);

            //Delete previous hero by StringId
            string stringId = classDivisionXml.GetAttribute("id");
            var basicHeroClassByStringId = MBObjectManager.Instance.GetObject<MultiplayerClassDivisions.MPHeroClass>(stringId);
            if (basicHeroClassByStringId != null)
            {
                MBObjectManager.Instance.UnregisterObject(basicHeroClassByStringId);
            }

            MultiplayerClassDivisions.MPHeroClass classDivisionHeroClassNew = (MultiplayerClassDivisions.MPHeroClass)MBObjectManager.Instance.CreateObjectFromXmlNode(classDivisionXml);

            var list = MBObjectManager.Instance.GetObjectTypeList<MultiplayerClassDivisions.MPHeroClass>();

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