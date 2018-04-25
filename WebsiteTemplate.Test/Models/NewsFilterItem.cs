namespace WebsiteTemplate.Test.Models
{
    public class NewsFilterItem : FilterItem
    {
        public virtual NewsItem NewsItem { get; set; }

        public override FilterItemParentBase Parent
        {
            get
            {
                return NewsItem;
            }
            set
            {
                NewsItem = (NewsItem)value;
            }
        }
    }
}