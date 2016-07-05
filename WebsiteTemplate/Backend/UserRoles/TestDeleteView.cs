using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

        public override int GetId()
        {
            return 778;
        }

        public override async Task<IList<Event>> ProcessAction()
        {
            int rowId = Convert.ToInt32(GetValue("rowId"));
            return new List<Event>()
            {
                new UpdateInputView(InputViewUpdateType.Delete)
            };
        }
    }
}