﻿using FluentNHibernate.Cfg;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web;
using WebsiteTemplate.Mappings;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Data
{
    public static class DataStoreExtensionMethods
    {
        public static FluentMappingsContainer CustomAddFromAssemblyOf<T>(this FluentMappingsContainer mappings)
        {
            var container = mappings.AddFromAssemblyOf<User>();

            var curDir = HttpRuntime.AppDomainAppPath;
            var dlls = Directory.GetFiles(curDir, "*.dll", SearchOption.AllDirectories);
            var types = new List<Type>();

            var appDomain = AppDomain.CreateDomain("tmpDomainForWebTemplate");
            foreach (var dll in dlls)
            {
                if (dll.Contains("\\roslyn\\"))
                {
                    continue;
                }

                try
                {
                    var assembly = appDomain.Load(File.ReadAllBytes(dll));
                    var dynamicTypes = assembly.GetTypes().Where(myType => myType.IsClass /*&& !myType.IsAbstract */&& myType.IsSubclassOf(typeof(DynamicClass)));
                    types.AddRange(dynamicTypes);
                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine(ex.Message);
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
                    container.Add(makeme);
                }
                else
                {
                    var d1 = typeof(ChildDynamicMap<>);
                    Type[] typeArgs = { type };
                    var makeme = d1.MakeGenericType(typeArgs);
                    container.Add(makeme);
                }
            }

            return container;
        }
    }
}