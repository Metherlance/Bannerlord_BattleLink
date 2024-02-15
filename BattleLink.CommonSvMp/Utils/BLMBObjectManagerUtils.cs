using BattleLink.Common.Model;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using System.Xml;
using TaleWorlds.Core;
using TaleWorlds.ObjectSystem;

namespace BattleLink.Common.Utils
{
    public class BLMBObjectManagerUtils
    {
        private static Assembly assembly = Assembly.Load("TaleWorlds.ObjectSystem");
        private static Type typeIObjectTypeRecord = assembly.GetType("TaleWorlds.ObjectSystem.MBObjectManager+IObjectTypeRecord");
        private static MethodInfo methObjectClass = typeIObjectTypeRecord.GetMethod("get_ObjectClass", BindingFlags.Instance | BindingFlags.Public);
        private static MethodInfo metRegisterMBObjectWithoutInitialization = typeIObjectTypeRecord.GetMethod("RegisterMBObjectWithoutInitialization", BindingFlags.Instance | BindingFlags.Public);
        private static FieldInfo fieldIndexContainer = typeof(MBObjectManager).GetField("ObjectTypeRecords", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo fieldMbguid = typeof(MBObjectBase).GetField("<Id>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

        public static object getContainer(Type type)
        {
            IList objectTypeRecords = (IList)fieldIndexContainer.GetValue(MBObjectManager.Instance);



            if (type.IsSealed)
            {
                foreach (var objectTypeRecord in objectTypeRecords)
                {
                    // parametrized type...
                    Type objectTypeRecordClass = objectTypeRecords[0].GetType();
                    var fieldsObjectClass = objectTypeRecordClass.GetRuntimeProperties();//"ObjectClass", BindingFlags.NonPublic | BindingFlags.Instance);
                    var fieldObjectClass = fieldsObjectClass.ToArray().Where(p => p.Name.Contains("ObjectClass")).First();
                    Type objectClass = (Type)fieldObjectClass.GetValue(objectTypeRecord);
                    if (objectClass == type)
                    {
                        return objectTypeRecord;
                    }
                }
            }
            else
            {
                foreach (var objectTypeRecord in objectTypeRecords)
                {

                    Type objectTypeRecordClass = objectTypeRecords[0].GetType();
                    Type objectClass = (Type)methObjectClass.Invoke(objectTypeRecord, null);

                    // parametrized type...
                    if (type.IsAssignableFrom(objectClass))
                    {
                        return objectTypeRecord;
                    }
                }
            }

            throw new Exception("Type not found");

        }


        public static BasicCultureObject CreateCultureFromXmlNode(XmlElement culture, MBGUID mbguid)
        {
            string stringId = culture.GetAttribute("id");

            //Delete previous culture by StringId
            var basicCultureObjectByStringId = MBObjectManager.Instance.GetObject<BasicCultureObject>(stringId);
            if (basicCultureObjectByStringId != null)
            {
                MBObjectManager.Instance.UnregisterObject(basicCultureObjectByStringId);
            }

            // Create new culture
            var basicCultureObject = (BasicCultureObject)MBObjectManager.Instance.CreateObjectFromXmlNode(culture);
            MBObjectManager.Instance.UnregisterObject(basicCultureObject);

            fieldMbguid.SetValue(basicCultureObject, mbguid);


            var cultureContainer = getContainer(typeof(BasicCultureObject));
            metRegisterMBObjectWithoutInitialization.Invoke(cultureContainer, new object[] { basicCultureObject });

            return basicCultureObject;
        }

        public static BLCharacterObject CreateCharacterFromXmlNode(XmlElement character, MBGUID mbguid)
        {
            string stringId = character.GetAttribute("id");

            //Delete previous culture by StringId
            var basicCharacterObjectByStringId = MBObjectManager.Instance.GetObject<BLCharacterObject>(stringId);
            if (basicCharacterObjectByStringId != null)
            {
                MBObjectManager.Instance.UnregisterObject(basicCharacterObjectByStringId);
            }

            // Create new character
            var blCharacterObject = (BLCharacterObject)MBObjectManager.Instance.CreateObjectFromXmlNode(character);
            MBObjectManager.Instance.UnregisterObject(blCharacterObject);

            fieldMbguid.SetValue(blCharacterObject, mbguid);


            var cultureContainer = getContainer(typeof(BLCharacterObject));
            metRegisterMBObjectWithoutInitialization.Invoke(cultureContainer, new object[] { blCharacterObject });

            return blCharacterObject;
        }

    }
}
