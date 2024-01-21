using BattleLink.Common.Model;
using BattleLink.Common.Utils;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade.Network.Messages;
using TaleWorlds.ObjectSystem;

namespace BattleLink.Handler
{
    public class BLInitCultureHandler
    {

        public static void HandleServerEventInitCultureMessage(GameNetworkMessage mes)
        {
            //<BasicCultures>
            //	<Culture
            //		id="empire"
            //		name="{=empirefaction}Empire"
            //		is_main_culture="true"
            //		default_face_key="000fa92e90004202aced5d976886573d5d679585a376fdd605877a7764b8987c00000000000007520000037f0000000f00000037049140010000000000000000"
            //		color="FF802463"
            //		color2="FFF1C232"
            //		cloth_alternative_color1="FFDE9953"
            //		cloth_alternative_color2="FF4E3A55"
            //		banner_background_color1="FF7739a7"
            //		banner_foreground_color1="FFf1c232"
            //		banner_background_color2="FFf1c232"
            //		banner_foreground_color2="FF7739a7"
            //		faction_banner_key="11.4.124.4345.4345.764.764.1.0.0.163.0.5.512.512.764.764.1.0.0" />
            BLInitCultureMessage message = (BLInitCultureMessage)mes;

            XmlDocument doc = new XmlDocument();
            XmlElement culture = doc.CreateElement("Culture");

            culture.SetAttribute("id", message.id);
            culture.SetAttribute("name", message.name);

            culture.SetAttribute("is_main_culture", "true");
            culture.SetAttribute("default_face_key", "000fa92e90004202aced5d976886573d5d679585a376fdd605877a7764b8987c00000000000007520000037f0000000f00000037049140010000000000000000");
            culture.SetAttribute("color", message.Color.ToHexadecimalString());
            culture.SetAttribute("color2", message.Color2.ToHexadecimalString());

            culture.SetAttribute("cloth_alternative_color1", message.ClothAlternativeColor1.ToHexadecimalString());
            culture.SetAttribute("cloth_alternative_color2", message.ClothAlternativeColor2.ToHexadecimalString());

            culture.SetAttribute("banner_background_color1", message.BannerBackgroundColor1.ToHexadecimalString());
            culture.SetAttribute("banner_background_color2", message.BannerBackgroundColor2.ToHexadecimalString());

            culture.SetAttribute("banner_foreground_color1", message.BannerForegroundColor1.ToHexadecimalString());
            culture.SetAttribute("banner_foreground_color2", message.BannerForegroundColor2.ToHexadecimalString());

            culture.SetAttribute("faction_banner_key", message.FactionBannerKey);

            MBGUID guid = new MBGUID(message.mbguid);

            var basicCultureObject = BLMBObjectManagerUtils.CreateCultureFromXmlNode(culture, guid);

            // var basicCultureObject = (BasicCultureObject)MBObjectManager.Instance.CreateObjectFromXmlNode(culture);

        }

    }
}