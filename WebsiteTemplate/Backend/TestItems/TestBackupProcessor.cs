using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestBackupProcessor : DoSomething
    {
        public override bool AllowInMenu
        {
            get
            {
                return true;
            }
        }

        public override string Description
        {
            get
            {
                return "Test Backup Web Call";
            }
        }

        public override EventNumber GetId()
        {
            return 89356;
        }

        public override async Task<IList<IEvent>> ProcessAction()
        {
            return new List<IEvent>()
            {
                new ShowMessage("Test done")
            };
        }
    }
}