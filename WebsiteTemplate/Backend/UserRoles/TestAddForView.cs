using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using WebsiteTemplate.CustomMenuItems;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.UserRoles
{
    public class TestAddForView : GetInput
    {
        public override string Description
        {
            get
            {
                return "testing";
            }
        }

        private JsonHelper RowData { get; set; }

        public override IList<InputField> InputFields
        {
            get
            {
                var result = new List<InputField>()
                {
                    new StringInput("name", "name", RowData.GetValue("name"), "", true),
                    new StringInput("age", "age", RowData.GetValue("age"), ""),
                };
                return result;
            }
        }

        public override EventNumber GetId()
        {
            return 777;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            if (!String.IsNullOrWhiteSpace(data))
            {
                var json = JsonHelper.Parse(data);
                RowData = JsonHelper.Parse(data);
            }
            else
            {
                RowData = null;
            }

            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<Event>()
                {
                    new CancelInputDialog(),
                };
            }
            else if (actionNumber == 0)
            {
                return new List<Event>()
                {
                    new UpdateInputView(InputViewUpdateType.AddOrUpdate),
                    new CancelInputDialog()
                };
            }
            return null;
        }
    }
}