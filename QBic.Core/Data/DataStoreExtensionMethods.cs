using QBic.Core.Mappings;
using QBic.Core.Models;
using FluentNHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.Linq;

namespace QBic.Core.Data
{
    public static class DataStoreExtensionMethods
    {
        public static FluentMappingsContainer AddAuditModels(this FluentMappingsContainer mappings)
        {
            var auditType = Type.GetType("WebsiteTemplate.Mappings.AuditEventMapping, WebsiteTemplate");

            mappings.Add(auditType);

            return mappings;

        }

        public static FluentMappingsContainer AddFromRunningAssemblies(this FluentMappingsContainer mappings)
        {
            var types = new List<Type>();
            var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in allAssemblies)
            {
                var assemblyTypes = assembly.GetTypes();
                var tmpBaseTypes = assemblyTypes
                                           .Where(myType => myType.IsClass /*&& !myType.IsAbstract */&& myType.IsSubclassOf(typeof(DynamicClass)))
                                           .ToList();
                types.AddRange(tmpBaseTypes);

                var refreshTokenMapper = assemblyTypes.Where(t => t.Name.EndsWith("RefreshTokenMap")).FirstOrDefault();
                if (refreshTokenMapper != null)
                {
                    mappings.Add(refreshTokenMapper);
                }

                var baseMappingTypes = assemblyTypes.Where(t => t.BaseType != null && 
                                                                t.BaseType.Name.Contains("BaseClassMap") && 
                                                                !t.Name.Contains("AuditEventMapping")) // Don't map audit table to main data store.
                                                    .ToList();
                if (baseMappingTypes.Count > 0)
                {
                    baseMappingTypes.ForEach(b =>
                    {
                        mappings.Add(b);
                    });
                }
            }

            var list = new List<string>();
            foreach (var type in types)
            {
                var typeString = type.ToString();
                if (list.Contains(typeString))
                {
                    continue;
                }

                list.Add(typeString);
                if (type.BaseType == typeof(DynamicClass))
                {
                    var d1 = typeof(DynamicMap<>);
                    Type[] typeArgs = { type };
                    var makeme = d1.MakeGenericType(typeArgs);
                    mappings.Add(makeme);
                }
                else
                {
                    var d1 = typeof(ChildDynamicMap<>);
                    Type[] typeArgs = { type };
                    var makeme = d1.MakeGenericType(typeArgs);
                    mappings.Add(makeme);
                }
            }

            return mappings;
        }
    }
}