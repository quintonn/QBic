using System.Collections.Generic;
using System.Linq;

namespace WebsiteTemplate.Backend.CsvUpload
{
    public class MappedRow
    {
        /// <summary>
        /// Starts at 1
        /// </summary>
        public int RowIndex { get; set; }

        public List<MappedColumn> Columns { get; set; }

        public MappedRow(int rowIndex)
        {
            RowIndex = rowIndex;
            Columns = new List<MappedColumn>();
        }

        public string this[string columnName]
        {
            get
            {
                return Columns.FirstOrDefault(c => c.ColumnName == columnName)?.Data;
            }
        }
    }
}