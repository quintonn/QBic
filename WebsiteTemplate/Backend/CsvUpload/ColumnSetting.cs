namespace WebsiteTemplate.Backend.CsvUpload
{
    public class ColumnSetting
    {
        public string ColumnName { get; set; }

        public string ColumnNumbers { get; set; }

        public ColumnSetting(string columnName, string columnNumbers = null)
        {
            ColumnName = columnName;
            ColumnNumbers = columnNumbers;
        }
    }
}