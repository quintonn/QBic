using QBic.Core.Models;
using System;

namespace WebsiteTemplate.Test.Models
{
    public class Department : DynamicClass
    {
        public Department()
        {
        }

        public virtual string Name { get; set; }
        public virtual DateTime Date { get; set; }

    }
}
