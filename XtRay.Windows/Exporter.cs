/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.IO;
using System.Linq;
using System.Reflection;
using XtRay.ParserLib;
using XtRay.ParserLib.Abstractions;

namespace XtRay.Windows
{
    class Exporter
    {
        public static void ExportJson(TraceTree root, string filename)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = ShouldSerializeContractResolver.Instance
            };
            var res = JsonConvert.SerializeObject(root, Formatting.Indented, settings);
            File.WriteAllText(filename, res);
        }

        public static void ExportXml(TraceTree root, string filename)
        {
            //TODO: implement this maybe
        }

        public class ShouldSerializeContractResolver : DefaultContractResolver
        {
            public static ShouldSerializeContractResolver Instance { get; } = new ShouldSerializeContractResolver();
            private static string[] IgnoredProperties = new string[] {
                nameof(ITrace.SelfTime), nameof(ITrace.CumulativeTime), nameof(ITrace.TimePercent), nameof(ITrace.ParentTimePercent)
            };

            protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
            {
                JsonProperty property = base.CreateProperty(member, memberSerialization);
                if (typeof(ITrace).IsAssignableFrom(member.DeclaringType) && IgnoredProperties.Contains(member.Name))
                {
                    property.Ignored = true;
                }
                return property;
            }
        }
    }
}
