using System;
using System.Collections;
using System.Collections.Generic;
using WebsiteTemplate.Menus.BaseItems;
using WebsiteTemplate.Menus.ViewItems;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Backend.CsvUpload
{
    public class MappingView : ViewForInput
    {
        private IList<ColumnSetting> ColumnsToMap { get; set; }

        public MappingView()
        {
            ColumnsToMap = new List<ColumnSetting>();
        }

        public override string Description => "Column Mappings";

        public override void ConfigureColumns(ColumnConfiguration columnConfig)
        {
            columnConfig.AddStringColumn("Field", "Field");
            columnConfig.AddStringColumn("Column Number/s", "Columns");

            columnConfig.AddLinkColumn("", "Id", "Edit", EventNumber.EditCsvColumnMapping);

            //columnConfig.AddButtonColumn("", "Id", "X", new UserConfirmation("Delete selected item?")
            //{
            //    OnConfirmationUIAction = MobileMenuNumber.DeleteCsvColumnMapping
            //});
        }

        //public override IEnumerable GetData(GetDataSettings settings)
        //{
        //    var result = new List<object>();

        //    var rowNumber = 0;
        //    foreach (var column in ColumnsToMap)
        //    {
        //        var item = new
        //        {
        //            column.Field,
        //            column.Columns,
        //            rowId = rowNumber++
        //        };
        //        result.Add(item);
        //    }

        //    return result;
        //}

        //public override int GetDataCount(GetDataSettings settings)
        //{
        //    var jsonString = settings.ViewData;
        //    ColumnsToMap = JsonHelper.DeserializeObject<List<ColumnSetting>>(jsonString);

        //    return ColumnsToMap.Count;
        //}

        public override EventNumber GetId()
        {
            return EventNumber.CsvUploadColumnsView;
        }

        //public override IList<MenuItem> GetViewMenu(Dictionary<string, string> dataForMenu)
        //{
        //    return new List<MenuItem>()
        //    {
        //        new MenuItem("Add", MobileMenuNumber.AddCsvColumnMapping)
        //    };
        //}
    }
}