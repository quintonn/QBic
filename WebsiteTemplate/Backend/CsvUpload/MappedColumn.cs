namespace WebsiteTemplate.Backend.CsvUpload
{
    public class MappedColumn
    {
        public string ColumnName { get; set; }

        /// <summary>
        /// Starts at 1
        /// </summary>
        public int ColumnIndex { get; set; }

        public string Data { get; set; }

        public MappedColumn(string columnName, int columnIndex, string data)
        {
            ColumnName = columnName;
            ColumnIndex = columnIndex;
            Data = data;
        }
    }
}