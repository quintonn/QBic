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
        public TestAddForView()
        {
            Console.WriteLine("xx");
        }

        public override string Description
        {
            get
            {
                return "testing";
            }
        }

        private JsonHelper RowData { get; set; }

        private int RowId { get; set; } = -1; //TODO: This should go on the base class or core code. not on every implementation of a GetInput used for InputView's input

        public override IList<InputField> InputFields
        {
            get
            {
                var result = new List<InputField>()
                {
                    new StringInput("name", "name", RowData.GetValue("name"), "", true),
                    new StringInput("age", "age", RowData.GetValue("age"), ""),
                    new HiddenInput("rowId", RowId)
                };
                return result;
            }
        }

        public override int GetId()
        {
            return 777;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            if (!String.IsNullOrWhiteSpace(data))
            {
                var json = JsonHelper.Parse(data);
                var rowData = data;
                if (json.GetValue("rowData") != null)
                {
                    rowData = json.GetValue("rowData")?.ToString();
                }
                
                RowData = JsonHelper.Parse(rowData);
                RowId = RowData.GetValue<int>("rowId");
            }
            else
            {
                RowData = null;
                RowId = -1;
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