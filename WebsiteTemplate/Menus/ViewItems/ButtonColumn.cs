using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class ButtonColumn : ClickableColumn
    {
        public ButtonColumn(string columnLabel, string keyColumn, string buttonText, int eventNumber, string parametersToPass = null)
            : base(columnLabel, keyColumn, buttonText, eventNumber, ClickableType.Button, parametersToPass)
        {
            
        }

        public ButtonColumn(string columnLabel, string keyColumn, string linkText, Event eventItem, string parametersToPass = null)
            : base(columnLabel, keyColumn, linkText, eventItem, ClickableType.Button, parametersToPass)
        {

        }
    }
}