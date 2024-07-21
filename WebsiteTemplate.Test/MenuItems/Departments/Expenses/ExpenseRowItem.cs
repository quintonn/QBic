using WebsiteTemplate.Menus.InputItems;

namespace WebsiteTemplate.Test.MenuItems.Departments.Expenses
{
    public class ExpenseRowItem : IViewInputValue
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string Type { get; set; }
        public int Quantity { get; set; }
        public double Amount { get; set; }
        public string Frequency { get; set; }
        public int StartMonth { get; set; }
        public int EndMonth { get; set; }
        public int RollOutPeriod { get; set; }
        public int rowId { get; set; }
    }
}
