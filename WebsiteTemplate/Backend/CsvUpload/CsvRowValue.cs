using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Backend.CsvUpload
{
    public class CsvRowValue : IViewInputValue
    {
        public CsvRowValue(string field, string columns, int rowId)
        {
            Field = field;
            Columns = columns;
            rowId = rowId;
        }

        public string Field { get; set; }
        public string Columns { get; set; }
        public int rowId { get; set; }
    }
}