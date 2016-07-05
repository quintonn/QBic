namespace WebsiteTemplate.Menus.ViewItems
{
    public class DateColumn : ViewColumn
    {
        public DateColumn(string columnLabel, string columnName)
            : base(columnLabel, columnName)
        {
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.Date;
            }
        }
    }
}