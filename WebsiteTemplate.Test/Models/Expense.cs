using QBic.Core.Models;

namespace WebsiteTemplate.Test.Models
{
    public class Expense : DynamicClass
    {
        public Expense()
        {
            StartMonth = 1;
            EndMonth = 60;
            Quantity = 1;
        }
        public virtual Department Department { get; set; }

        public virtual string Name { get; set; }

        public virtual ExpenseCategory Category { get; set; }
        public virtual ExpenseType ExpenseType { get; set; }

        public virtual int Quantity { get; set; }

        public virtual double Amount { get; set; }

        public virtual ExpenseFrequency Frequency { get; set; }

        public virtual int StartMonth { get; set; }

        public virtual int EndMonth { get; set; }

        public virtual int RollOutPeriod { get; set; }
    }
}
