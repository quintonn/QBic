namespace WebsiteTemplate.Menus.ViewItems
{
    public class StringColumn : ViewColumn
    {
        public StringColumn(string columnLabel, string columnName)
            : base(columnLabel, columnName)
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