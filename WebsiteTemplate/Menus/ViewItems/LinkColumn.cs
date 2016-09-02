using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.ViewItems
{
    public class LinkColumn : ClickableColumn
    {
        public LinkColumn(string columnLabel, string keyColumn, string linkText, int eventNumber, string parametersToPass = null)
            : base(columnLabel, keyColumn, linkText, eventNumber, ClickableType.Link, parametersToPass)
        {

        }

        public LinkColumn(string columnLabel, string keyColumn, string linkText, Event eventItem, string parametersToPass = null)
            : base(columnLabel, keyColumn, linkText, eventItem, ClickableType.Link, parametersToPass)
        {

        }
    }
}