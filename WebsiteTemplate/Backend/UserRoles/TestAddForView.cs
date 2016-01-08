using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
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

        private JObject RowData { get; set; }

        private int RowId { get; set; }

        public override IList<InputField> InputFields
        {
            get
            {
                return new List<InputField>()
                {
                    new StringInput("name", "name", RowData?.GetValue("name").ToString(), "", true),
                    new StringInput("age", "age", RowData?.GetValue("age").ToString(), ""),
                    new HiddenInput("rowId", RowId),
                };
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
                var json = JObject.Parse(data);
                var id = json.GetValue("Id").ToString();
                var rowData = json.GetValue("rowData")?.ToString();
                RowId = Convert.ToInt32(json.GetValue("rowId")?.ToString());
                RowData = JObject.Parse(rowData);
            }
            else
            {
                RowData = null;
                RowId = -1;
            }

            return new InitializeResult(true);
        }

        public override async Task<IList<Event>> ProcessAction(string data, int actionNumber)
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
                    new UpdateInputView(data),
                    new CancelInputDialog()
                };
            }
            return null;
        }
    }
}