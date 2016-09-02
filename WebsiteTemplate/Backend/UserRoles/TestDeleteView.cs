using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class TestDeleteView : DoSomething
    {
        public override string Description
        {
            get
            {
                return "Delete Test";
            }
        }

        public override EventNumber GetId()
        {
            return 778;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            return new List<IEvent>()
            {
                new UpdateInputView(InputViewUpdateType.Delete)
            };
        }
    }
}