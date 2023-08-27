using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.MapEvents;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Party.PartyComponents;
using TaleWorlds.Core;
using TaleWorlds.Engine;

namespace Replay
{
    public class JsonUtils
    {

        public static T Deserialize<T>(string pathfile)
        {
            var serializer = new JsonSerializer();
            using (var streamReader = new StreamReader(pathfile))
            using (var textReader = new JsonTextReader(streamReader))
            {
                T result = serializer.Deserialize<T>(textReader);
                return result;
            }
        }   

        public static string SerializeObject(object obj, int maxDepth)
        {
            using (var strWriter = new System.IO.StringWriter())
            {
                using (var jsonWriter = new CustomJsonTextWriter(strWriter))
                {
                    Func<bool> include = () => jsonWriter.CurrentDepth <= maxDepth;
                    var resolver = new CustomContractResolver(include);
                    //var aaa = new JsonSerializerSettings()
                    //{
                    //    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    //    Error = (serializer, err) => err.ErrorContext.Handled = true,
                    //};

                    var serializer = new JsonSerializer { ContractResolver = resolver,
                                                            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                                                            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                                                            NullValueHandling = NullValueHandling.Ignore,
                        //ObjectCreationHandling = ObjectCreationHandling.Reuse,
                        //NullValueHandling = NullValueHandling.Include,
                        //    _referenceLoopHandling = ReferenceLoopHandling.Error,
                        //_missingMemberHandling = MissingMemberHandling.Ignore,
                        //_nullValueHandling = NullValueHandling.Include;
                        //_defaultValueHandling = DefaultValueHandling.Include;
                        //_objectCreationHandling = ObjectCreationHandling.Auto;
                        //_preserveReferencesHandling = PreserveReferencesHandling.None;
                        //_constructorHandling = ConstructorHandling.Default;
                        //_typeNameHandling = TypeNameHandling.None;
                        //_metadataPropertyHandling = MetadataPropertyHandling.Default;
                        //_context = JsonSerializerSettings.DefaultContext;
                        //_serializationBinder = DefaultSerializationBinder.Instance,
                        //_culture = JsonSerializerSettings.DefaultCulture,
                        //_contractResolver = DefaultContractResolver.Instance,

                    };
                    serializer.Error += (serializer, err) => err.ErrorContext.Handled = true;
                    //serializer.SerializerSettings
               
                    serializer.Serialize(jsonWriter, obj);
                }
                return strWriter.ToString();
            }
        }

        public class CustomContractResolver : DefaultContractResolver
        {
            private readonly Func<bool> _includeProperty;

            public CustomContractResolver(Func<bool> includeProperty)
            {
                _includeProperty = includeProperty;
            }

            protected override List<MemberInfo> GetSerializableMembers(Type objectType)
            {
                //var result = base.GetSerializableMembers(objectType);
                var result = objectType.GetRuntimeFields().Where(fi=>!fi.IsStatic).Select(fi=>(MemberInfo)fi).ToList();
                
                if ("MapEvent".Equals(objectType.Name))
                {
                    result = result.Where(fi => !fi.Name.Contains("MapEventVisual")).ToList();
                }
                if("PartyBase".Equals(objectType.Name))
                {
                    result = result.Where(fi => !fi.Name.Contains("MapEvent") && !fi.Name.Contains("_visual")).ToList();
                }
                if ("MapEventSide".Equals(objectType.Name))
                {
                    result = result.Where(fi => !fi.Name.Contains("_battleParties") && !fi.Name.Contains("_mapFaction") 
                                            && !fi.Name.Contains("_mapEvent")).ToList();
                }
                if ("MobileParty".Equals(objectType.Name))
                {
                    result = result.Where(fi => !fi.Name.Contains("_actualClan") 
                                            && !fi.Name.Contains("<LastVisitedSettlement>k__BackingField")
                                            &&!fi.Name.Contains("_partyComponent") 
                                            &&!fi.Name.Contains("<Ai>k__BackingField")).ToList();
                }
                if ("Hero".Equals(objectType.Name))
                {
                    result = result.Where(fi => !fi.Name.Contains("<LastKnownClosestSettlement>k__BackingField")).ToList();
                }
                if ("Clan".Equals(objectType.Name))
                {
                    result = result.Where(fi => !fi.Name.Contains("_stances")).ToList();
                }

                return result;
            }

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                var property = base.CreateProperty(member, memberSerialization);
                property.ShouldSerialize = obj =>
                {
                    bool show = _includeProperty();
                    return show;
                };
                //property.ShouldSerialize = a=> true;
                property.Readable = true;
                return property;
            }
        }

        public class CustomJsonTextWriter : JsonTextWriter
        {
            public CustomJsonTextWriter(TextWriter textWriter) : base(textWriter) { }

            public int CurrentDepth { get; private set; }

            public override void WriteStartObject()
            {
                CurrentDepth++;
                base.WriteStartObject();
            }

            public override void WriteEndObject()
            {
                CurrentDepth--;
                base.WriteEndObject();
            }
        }
    }
}
