namespace WebsiteTemplate.Models
{
    public abstract class BaseClass
    {
        public virtual string Id { get; set; }

        public virtual bool CanDelete { get; protected set; }

        public BaseClass()
        {
            CanDelete = true;
        }

        public BaseClass(bool canDelete)
        {
            CanDelete = canDelete;
        }
    }
}