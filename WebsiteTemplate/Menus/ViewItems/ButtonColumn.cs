using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ButtonColumn : ViewColumn
    {
        public ButtonColumn(string columnLabel, string dbColumnName, string columnName)
            : base(columnLabel, columnName)
        {
        }

        public ButtonColumn(string columnLabel, string columnName, ButtonTextSource buttonTextSource, string buttonText)
            : base(columnLabel, columnName)
        {
            ButtonTextSource = buttonTextSource;
            ButtonText = buttonText;
        }

        public override ColumnType ColumnType
        {
            get
            {
                return ViewItems.ColumnType.Button;
            }
        }

        public string ButtonText { get; set; }

        public ButtonTextSource ButtonTextSource { get; set; }

        public Event Event { get; set; }
    }
}