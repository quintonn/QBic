﻿using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

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

        private DataService DataService { get; set; }

        public int Id { get; set; }

        public string ItemName { get; set; }

        public override EventNumber GetId()
        {
            return Id;
        }

        public BasicCrudDelete(DataService dataService)
        {
            DataService = dataService;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            using (var session = DataService.OpenSession())
            {
                var item = session.Get<T>(id);

                DataService.TryDelete(item);
                
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