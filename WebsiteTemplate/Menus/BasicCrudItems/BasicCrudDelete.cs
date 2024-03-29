﻿using NHibernate;
using QBic.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudDelete<T> : DoSomething, IBasicCrudDelete where T : BaseClass
    {
        public override string Description
        {
            get
            {
                return "Deletes a " + ItemName;
            }
        }
        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public int Id { get; set; }

        public string ItemName { get; set; }

        public override EventNumber GetId()
        {
            return Id;
        }

        public BasicCrudDelete(DataService dataService) : base(dataService) 
        {
        }

        public Action<ISession, object> OnDeleteInternal { get; set; }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            using (var session = DataService.OpenSession())
            {
                var item = session.Get<T>(id);

                OnDeleteInternal(session, item);

                DataService.TryDelete(session, item);
                
                session.Flush();
            }

            return new List<IEvent>()
            {
                new ShowMessage(ItemName + " deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(Id - 2)
            };
        }
    }
}