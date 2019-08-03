using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.InputItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.CsvUpload
{
    public class EditCsvMapping : GetInput
    {
        private JsonHelper RowData { get; set; }
        private int RowId { get; set; }

        public override bool AllowInMenu => false;

        public override string Description => "Edit CSV Mapping";

        public override EventNumber GetId()
        {
            return EventNumber.EditCsvColumnMapping;
        }

        public override async Task<InitializeResult> Initialize(string data)
        {
            var json = JsonHelper.Parse(data);

            //if (IsNew == false)
            {
                var rowData = data;
                if (!String.IsNullOrWhiteSpace(json.GetValue("rowData")))
                {
                    rowData = json.GetValue("rowData");
                }

                RowId = json.GetValue<int>("rowid");
                RowData = JsonHelper.Parse(rowData);
            }
            //else
            //{
            //    RowData = new JsonHelper();
            //    RowId = -1;
            //}

            return new InitializeResult(true);
        }

        public override IList<InputField> GetInputFields()
        {
            var result = new List<InputField>();

            result.Add(new LabelInput("Field", "Field", RowData.GetValue("Field")));
            result.Add(new StringInput("Columns", "Columns", RowData.GetValue("Columns"), mandatory: true));
            result.Add(new LabelInput("Lbl2", "", "Note: Column numbers start from 1"));
            result.Add(new LabelInput("Lbl1", "", "Note: Add multiple columns using '&' (e.g. 1&2) or set first non-empty column using ';' (e.g. 2;3)"));

            result.Add(new HiddenInput("rowId", RowId));

            return result;
        }

        public override async Task<IList<IEvent>> ProcessAction(int actionNumber)
        {
            if (actionNumber == 1)
            {
                return new List<IEvent>()
                {
                    new CancelInputDialog(),
                };
            }
            else if (actionNumber == 0)
            {
                return new List<IEvent>()
                {
                    new UpdateInputView(InputViewUpdateType.AddOrUpdate),
                    new CancelInputDialog()
                };
            }
            else
            {
                return null;
            }
        }
    }
}