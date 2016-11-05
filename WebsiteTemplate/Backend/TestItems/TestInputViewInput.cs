using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.TestItems
{
    public class TestInputViewInput : GetInput
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
                return "Test Input View Input";
            }
        }

        public override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();

            result.Add(new StringInput("Name", "Name"));

            result.Add(new ViewInput("Items", "Items", new TestInputView()));

            return result;
        }

        public override EventNumber GetId()
        {
            return new EventNumber(65390);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            var result = new List<IEvent>();
            result.Add(new ShowMessage("Done"));
            return result;
        }
    }
}