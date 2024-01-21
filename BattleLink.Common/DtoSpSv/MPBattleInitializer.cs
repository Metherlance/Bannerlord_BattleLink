using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace BattleLink.Common.DtoSpSv;

public class MPBattleInitializer
{
    public MissionInitializerRecord MissionInitializerRecord;
    public List<BLCharacter> BLCharacters;
    public List<Team> Teams;
    public List<Player> Players;

    public void initCharacter(MBObjectManager instance)
    {
        BLCharacters = new List<BLCharacter>();
        foreach (var team in Teams)
        {
            foreach (var party in team.Parties)
            {
                foreach (var troop in party.Troops)
                {
                    var character = instance.GetObject<BasicCharacterObject>(troop.Id);
                    BLCharacters.Add(new BLCharacter(character));
                }
            }
        }
        BLCharacters = BLCharacters.GroupBy(x => x.Id).Select(x => x.First()).ToList();
    }
}

[XmlRoot(ElementName = "face_key_template")]
public class FaceKeyTemplate
{

    [XmlAttribute(AttributeName = "value")]
    public string Value;
}

[XmlRoot(ElementName = "face")]
public class Face
{

    [XmlElement(ElementName = "face_key_template")]
    public FaceKeyTemplate FaceKeyTemplate;

    [XmlElement(ElementName = "BodyProperties")]
    public BodyProperties BodyProperties;

    [XmlElement(ElementName = "BodyPropertiesMax")]
    public BodyProperties BodyPropertiesMax;

    public Face()
    {
        //for serialization
    }

    public Face(MBBodyProperty bodyPropertyRange)
    {
        BodyProperties = new BodyProperties()
        {
            Version = 4,
            Age = bodyPropertyRange.BodyPropertyMin.Age,
            Build = bodyPropertyRange.BodyPropertyMin.Build,
            Weight = bodyPropertyRange.BodyPropertyMin.Weight,
            Key = bodyPropertyRange.BodyPropertyMin.StaticProperties.ToString().Substring(5, bodyPropertyRange.BodyPropertyMin.StaticProperties.ToString().Length - 7)
        };
        BodyPropertiesMax = new BodyProperties()
        {
            Version = 4,
            Age = bodyPropertyRange.BodyPropertyMax.Age,
            Build = bodyPropertyRange.BodyPropertyMax.Build,
            Weight = bodyPropertyRange.BodyPropertyMax.Weight,
            Key = bodyPropertyRange.BodyPropertyMax.StaticProperties.ToString().Substring(5, bodyPropertyRange.BodyPropertyMax.StaticProperties.ToString().Length - 7)
        };
    }

}

[XmlRoot(ElementName = "equipment")]
public class Equipment
{

    [XmlAttribute(AttributeName = "slot")]
    public string Slot;

    [XmlAttribute(AttributeName = "id")]
    public string Id;
}

[XmlRoot(ElementName = "EquipmentRoster")]
public class EquipmentRoster
{
    enum EquipmentIndexXML
    {
        Item0 = 0,
        Item1 = 1,
        Item2 = 2,
        Item3 = 3,
        ExtraWeaponSlot = 4,
        Head = 5,
        Body = 6,
        Leg = 7,
        Gloves = 8,
        Cape = 9,
        Horse = 10,
        HorseHarness = 11,
    }

    [XmlAttribute(AttributeName = "civilian")]
    public bool IsCivilian;

    [XmlElement(ElementName = "equipment")]
    public List<Equipment> Equipment;


    public EquipmentRoster()
    {
        //for serialization
    }
    public EquipmentRoster(TaleWorlds.Core.Equipment equipment)
    {
        IsCivilian = equipment.IsCivilian;
        Equipment = new List<Equipment>();
        for (int i = 0; i < 12; i += 1)
        {
            EquipmentElement eqEl = equipment[i];
            if (!eqEl.IsEmpty)
            {
                EquipmentIndexXML ei = (EquipmentIndexXML)i;
                Equipment.Add(new Equipment() { Slot = ei.ToString(), Id = "Item." + eqEl.Item.StringId });
            }
        }
    }
}

[XmlRoot(ElementName = "EquipmentSet")]
public class EquipmentSet
{

    [XmlAttribute(AttributeName = "id")]
    public string Id;

    [XmlAttribute(AttributeName = "civilian")]
    public bool Civilian;
}

[XmlRoot(ElementName = "Equipments")]
public class Equipments
{

    [XmlElement(ElementName = "EquipmentRoster")]
    public List<EquipmentRoster> EquipmentRoster;

    [XmlElement(ElementName = "EquipmentSet")]
    public EquipmentSet EquipmentSet;


    public Equipments()
    {
        //for serialization
    }
    public Equipments(MBReadOnlyList<TaleWorlds.Core.Equipment> allEquipments)
    {
        EquipmentRoster = new List<EquipmentRoster>();
        foreach (var equipment in allEquipments)
        {
            EquipmentRoster.Add(new EquipmentRoster(equipment));
        }
    }

}

[XmlRoot(ElementName = "BLCharacter")]
public class BLCharacter
{
    [XmlElement(ElementName = "face")]
    public Face Face;

    [XmlElement(ElementName = "skills")]
    public Skills Skills;

    [XmlElement(ElementName = "Equipments")]
    public Equipments Equipments;

    [XmlElement(ElementName = "Resistances")]
    public Resistances Resistances;

    [XmlAttribute(AttributeName = "id")]
    public string Id;

    [XmlAttribute(AttributeName = "default_group")]
    public string DefaultGroup;

    [XmlAttribute(AttributeName = "level")]
    public int Level;

    [XmlAttribute(AttributeName = "name")]
    public string Name;

    [XmlAttribute(AttributeName = "occupation")]
    public string Occupation;

    [XmlAttribute(AttributeName = "culture")]
    public string Culture;

    [XmlAttribute(AttributeName = "is_hero")]
    public bool IsHero;
    [XmlAttribute(AttributeName = "is_female")]
    public bool IsFemale;


    public BLCharacter()
    {
        //for serialization
    }

    public BLCharacter(BasicCharacterObject bso)
    {
        Id = bso.StringId;
        Level = bso.Level;
        Name = bso.Name.ToString();
        DefaultGroup = bso.DefaultFormationClass.ToString();
        IsHero = bso.IsHero;
        IsFemale = bso.IsFemale;

        Face = new Face(bso.BodyPropertyRange);

        Skills = new Skills();
        Skills.Skill = new List<Skill>
        {
            new Skill() { Id = "Riding", Value =  bso.GetSkillValue(DefaultSkills.Riding) },
            new Skill() { Id = "OneHanded", Value =  bso.GetSkillValue(DefaultSkills.OneHanded) },
            new Skill() { Id = "TwoHanded", Value =  bso.GetSkillValue(DefaultSkills.TwoHanded) },
            new Skill() { Id = "Polearm", Value =  bso.GetSkillValue(DefaultSkills.Polearm) },
            new Skill() { Id = "Crossbow", Value =  bso.GetSkillValue(DefaultSkills.Crossbow) },
            new Skill() { Id = "Bow", Value =  bso.GetSkillValue(DefaultSkills.Bow) },
            new Skill() { Id = "Throwing", Value =  bso.GetSkillValue(DefaultSkills.Throwing) }
        };

        Equipments = new Equipments(bso.AllEquipments);



        //bso.GetSkillValue(ToString());

        //bso.GetSkillValue FaceMeshCache.GetFaceKeys(out var version, out var age, out var build, out var key);
    }

    //public BasicCharacterObject toBasicCharacterObject()
    //{
    //    var basicCharacter = new BasicCharacterObject();
    //    basicCharacter.StringId = Id;
    //    basicCharacter.Level = Level;
    //    basicCharacter.Name = Level;


    //    return basicCharacter;
    //}
}

[XmlRoot(ElementName = "BodyProperties")]
public class BodyProperties
{

    [XmlAttribute(AttributeName = "version")]
    public int Version;

    [XmlAttribute(AttributeName = "age")]
    public double Age;

    [XmlAttribute(AttributeName = "weight")]
    public double Weight;

    [XmlAttribute(AttributeName = "build")]
    public double Build;

    [XmlAttribute(AttributeName = "key")]
    public string Key;
}

[XmlRoot(ElementName = "skill")]
public class Skill
{

    [XmlAttribute(AttributeName = "id")]
    public string Id;

    [XmlAttribute(AttributeName = "value")]
    public int Value;
}

[XmlRoot(ElementName = "skills")]
public class Skills
{

    [XmlElement(ElementName = "skill")]
    public List<Skill> Skill;
}

[XmlRoot(ElementName = "Resistances")]
public class Resistances
{

    [XmlAttribute(AttributeName = "dismount")]
    public int Dismount;
}


[XmlRoot(ElementName = "Troop")]
public class Troop
{

    [XmlAttribute(AttributeName = "id")]
    public string Id;

    [XmlAttribute(AttributeName = "hitPoints")]
    public int HitPoints;

    [XmlAttribute(AttributeName = "number")]
    public int Number;
}

[XmlRoot(ElementName = "Party")]
public class Party
{
    [XmlAttribute(AttributeName = "generalId")]
    public string GeneralId;

    [XmlAttribute(AttributeName = "Id")]
    public string Id;

    [XmlAttribute(AttributeName = "Index")]
    public int Index;

    public List<Troop> Troops;

    public Party()
    {

    }
    public Party(MapEventParty party)
    {
        GeneralId = party.Party.General.StringId;
        Id = party.Party.Id;
        Index = party.Party.Index;

        Troops = new List<Troop>();
        for (int i = 0; i < party.Party.MemberRoster.Count; i += 1)
        {
            var roasterMember = party.Party.MemberRoster.GetElementCopyAtIndex(i);
            Troops.Add(new Troop() { Id = roasterMember.Character.StringId, HitPoints = roasterMember.Character.HitPoints, Number = roasterMember.Number });
        }
    }
}


[XmlRoot(ElementName = "Team")]
public class Team
{
    public List<Party> Parties;


    [XmlAttribute(AttributeName = "battleSide")]
    public string BattleSide;

    [XmlAttribute(AttributeName = "color")]
    public string Color;
    [XmlAttribute(AttributeName = "color2")]
    public string Color2;
    [XmlAttribute(AttributeName = "culture")]
    public string Culture;
    [XmlAttribute(AttributeName = "name")]
    public string Name;
    //[XmlElement(ElementName = "Banner")]
    //public BannerXml Banner;

    //[XmlAttribute(AttributeName = "banner_background_color1")]
    //public uint banner_background_color1;
    //[XmlAttribute(AttributeName = "banner_foreground_color1")]
    //public uint banner_foreground_color1;
    //[XmlAttribute(AttributeName = "banner_background_color2")]
    //public uint banner_background_color2;
    //[XmlAttribute(AttributeName = "banner_foreground_color2")]
    //public uint banner_foreground_color2;
    [XmlAttribute(AttributeName = "faction_banner_key")]
    public string FactionBannerKey;

    [XmlAttribute(AttributeName = "isOpen")]
    public bool IsOpen = true;
    [XmlAttribute(AttributeName = "IsGeneralOpen")]
    public bool IsGeneralOpen = true;

    public Team()
    {
        //for serialization
    }
    public Team(MapEventSide side)
    {
        BattleSide = side.MissionSide.ToString();
        Parties = new List<Party>();
        foreach (var party in side.Parties)
        {
            Parties.Add(new Party(party));
        }
        Color = side.MapFaction.Color.ToHexadecimalString();
        Color2 = side.MapFaction.Color2.ToHexadecimalString();
        Culture = side.MapFaction.StringId;
        Name = side.MapFaction.Name.ToString();

        var faction = side.MapFaction;
        //banner_background_color1 = faction.Banner.Col.BackgroundColor1.ToString();
        //banner_foreground_color1 = faction.Banner.ForegroundColor1.ToString();

        FactionBannerKey = faction.Banner.Serialize();

        // Banner = new BannerXml(side.MapFaction);        
    }
}

[XmlRoot(ElementName = "Banner")]
public class BannerXml
{

    [XmlElement(ElementName = "BannerData")]
    public List<BannerDataXml> BannerData;

    public BannerXml()
    {

    }

    public BannerXml(IFaction faction)
    {
        BannerData = new List<BannerDataXml>();
        foreach (var bannerData in faction.Banner.BannerDataList)
        {
            BannerData.Add(new BannerDataXml(bannerData));
        }
    }
}

[XmlRoot(ElementName = "BannerData")]
public class BannerDataXml
{
    [XmlAttribute(AttributeName = "ColorId")]
    public int ColorId;
    [XmlAttribute(AttributeName = "color2")]
    public int ColorId2;
    [XmlAttribute(AttributeName = "MeshId")]
    public int MeshId;

    public BannerDataXml()
    {
        //for serialization
    }

    public BannerDataXml(BannerData bannerData)
    {
        ColorId = bannerData.ColorId;
        ColorId2 = bannerData.ColorId2;
        MeshId = bannerData.MeshId;
    }
}


[XmlRoot(ElementName = "Player")]
public class Player
{
    [XmlAttribute(AttributeName = "userName")]
    public string UserName;
    [XmlAttribute(AttributeName = "troopId")]
    public string TroopId;
    [XmlAttribute(AttributeName = "partyIndex")]
    public int PartyIndex;

    public Player()
    {
        //for serialization
    }
}
