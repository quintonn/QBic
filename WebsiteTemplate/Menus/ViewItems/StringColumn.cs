namespace WebsiteTemplate.Menus.ViewItems
{
    public class StringColumn : ViewColumn
    {
        public StringColumn(string columnLabel, string columnName, int columnSpan = 1)
            : base(columnLabel, columnName, columnSpan)
        {
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.String;
            }
        }
    }
}