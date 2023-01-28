using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Test.MenuItems
{
    public class TestInputViewInput : GetInput
    {
        public TestInputViewInput(DataService dataService) : base(dataService)
        {
        }

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

            result.Add(new StringInput("Name", "Name")
            {
                TabName = "Main"
            });

            result.Add(new FileInput("File", "File")
            {
                TabName = "Main"
            });

            result.Add(new FileInput("File2", "File 2")
            {
                TabName = "Main"
            });

            result.Add(new ViewInput("Items", "Items", new TestInputView())
            {
                TabName = "View"
            });

            return result;
        }

        public override EventNumber GetId()
        {
            return new EventNumber(65390);
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            var result = new List<IEvent>();

            var name = GetValue("Name");
            var file = GetValue<FileInfo>("File");

            result.Add(new ShowMessage("Done"));
            return result;
        }
    }
}