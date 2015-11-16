namespace WebsiteTemplate.Menus.ViewItems
{
    public class BooleanColumn : ViewColumn
    {
        public BooleanColumn(string columnLabel, string columnName)
            : this(columnLabel, columnName, "True", "False")
        {
        }

        public BooleanColumn(string columnLabel, string columnName, string trueValue, string falseValue)
            : base(columnLabel, columnName)
        {
            TrueValueDisplay = trueValue;
            FalseValueDisplay = falseValue;
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.Boolean;
            }
        }

        public string TrueValueDisplay { get; set; }

        public string FalseValueDisplay { get; set; }
    }
}