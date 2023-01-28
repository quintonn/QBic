using System;
using System.Collections.Generic;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Test.Models
{
    public class NewsItem : FilterItemParentBase, ITestItem
    {
        public virtual string Title { get; set; }

        public virtual string Description { get; set; }

        public virtual bool Authorized { get; set; }

        public virtual ISet<NewsFilterItem> Filters { get; set; }

        public virtual DateTime ValidFromDate { get; set; }

        public virtual DateTime ValidUntilDate { get; set; }

        public virtual DateTime ModifiedDateTime { get; set; }

        public virtual User Advisor { get; set; }

        public string Name => Title;

        public NewsItem()
        {
            Filters = new HashSet<NewsFilterItem>();
        }
    }

    internal interface ITestItem
    {
        string Name { get; }
    }
}