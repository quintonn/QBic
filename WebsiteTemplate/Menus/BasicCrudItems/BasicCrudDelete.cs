using Newtonsoft.Json.Linq;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public class BasicCrudDelete<T> : DoSomething where T : BaseClass
    {
        public override string Description
        {
            get
            {
                return "Deletes a " + ItemName;
            }
        }

        private int Id { get; set; }

        private string ItemName { get; set; }

        public BasicCrudDelete(int id, string itemName)
        {
            Id = id;
            ItemName = itemName;
        }

        public override int GetId()
        {
            return Id;
        }

        public override async Task<IList<Event>> ProcessAction()
        {
            var id = GetValue<string>("Id");

            using (var session = Store.OpenSession())
            {
                var item = session.Get<T>(id);

                session.Delete(item);
                session.Flush();
            }

            return new List<Event>()
            {
                new ShowMessage(ItemName + " deleted successfully"),
                new CancelInputDialog(),
                new ExecuteAction(Id - 2)
            };
        }
    }
}