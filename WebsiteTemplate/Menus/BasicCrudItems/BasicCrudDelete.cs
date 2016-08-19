using System.Collections.Generic;
using System.Threading.Tasks;
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

        public int Id { get; set; }

        public string ItemName { get; set; }

        public override EventNumber GetId()
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