﻿using Microsoft.Practices.Unity;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.BasicCrudItems;

namespace WebsiteTemplate.Backend.Services
{
    public class EventService
    {
        public static IDictionary<int, IEvent> EventList { get; set; }

        public IUnityContainer Container { get; set; }

        public EventService(IUnityContainer container)
        {
            Container = container;

            if (EventList == null)
            {
                EventList = new Dictionary<int, IEvent>();
            }
            PopulateEventList();
        }

        private void PopulateEventList()
        {
            if (EventList.Count > 0)
            {
                return;
            }

            var curDir = System.Web.HttpRuntime.AppDomainAppPath;
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


                    var eventTypes = assembly.GetTypes()
                                            .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Event)))
                                            .ToList();
                    if (eventTypes.Count > 0)
                    {
                        types.AddRange(eventTypes);
                    }

                }
                catch (ReflectionTypeLoadException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            AppDomain.Unload(appDomain);

            foreach (var type in types)
            {
                if (type.IsClass && !type.IsAbstract && type.IsSubclassOf(typeof(Event)))
                {
                    if (type == typeof(BasicCrudModify<>))
                    {
                        continue;
                    }
                    if (type == typeof(BasicCrudView<>))
                    {
                        continue;
                    }
                    if (type == typeof(BasicCrudDelete<>))
                    {
                        continue;
                    }


                    if (type.GetInterface("IBasicCrudMenuItem") != null)
                    {
                        var subType = (IBasicCrudMenuItem)Container.Resolve(type);

                        var d1 = typeof(BasicCrudView<>);
                        Type[] typeArgs1 = { subType.InnerType };
                        var viewType = d1.MakeGenericType(typeArgs1);

                        var viewInstance = (IBasicCrudView)Container.Resolve(viewType);
                        viewInstance.Id = subType.GetBaseMenuId();
                        viewInstance.ItemName = subType.GetBaseItemName();
                        viewInstance.ColumnsToShowInView = subType.GetColumnsToShowInView();

                        if (!EventList.ContainsKey(viewInstance.GetId()))
                        {
                            EventList.Add(viewInstance.GetId(), viewInstance as Event);
                        }

                        var d2 = typeof(BasicCrudModify<>);
                        Type[] typeArgs2 = { subType.InnerType };
                        var modifyType = d2.MakeGenericType(typeArgs2);
                        var modifyInstance = (IBasicCrudModify)Container.Resolve(modifyType);
                        modifyInstance.Id = subType.GetBaseMenuId() + 1;
                        modifyInstance.ItemName = subType.GetBaseItemName();
                        modifyInstance.InputProperties = subType.GetInputProperties();
                        if (!EventList.ContainsKey(modifyInstance.GetId()))
                        {
                            EventList.Add(modifyInstance.GetId(), modifyInstance as Event);
                        }

                        var d3 = typeof(BasicCrudDelete<>);
                        Type[] typeArgs3 = { subType.InnerType };
                        var deleteType = d3.MakeGenericType(typeArgs2);
                        var deleteInstance = (IBasicCrudDelete)Container.Resolve(deleteType);
                        deleteInstance.Id = subType.GetBaseMenuId() + 2;
                        deleteInstance.ItemName = subType.GetBaseItemName();
                        if (!EventList.ContainsKey(deleteInstance.GetId()))
                        {
                            EventList.Add(deleteInstance.GetId(), deleteInstance as Event);
                        }
                    }
                    else if (type != typeof(BasicCrudMenuItem<>))
                    {
                        var instance = (IEvent)Container.Resolve(type);
                        if (!EventList.ContainsKey(instance.GetId()))
                        {
                            EventList.Add(instance.GetId(), instance);
                        }
                    }
                }
            }
        }
    }
}