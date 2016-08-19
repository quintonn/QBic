using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class TestCreate : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Create a new one test";
            }
        }

        public override EventNumber GetId()
        {
            return EventNumber.TestCreate;
        }

        public override async Task<IList<Event>> ProcessAction()
        {
            InputData.Add("name", "Note1.txt");
            InputData.Add("age", "10");

            return new List<Event>()
            {
                new UpdateInputView(InputViewUpdateType.AddOrUpdate),
            };
        }
    }
}