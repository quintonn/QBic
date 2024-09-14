namespace WebsiteTemplate.Backend.CsvUpload
{
    public class ColumnSetting
    {
        public string Field { get; set; }

        public string Columns { get; set; }

        public ColumnSetting(string field, string columns = null)
        {
            Field = field;
            Columns = columns;
        }
    }
}