using System;

namespace QBic.Core.Models
{
    public abstract class BaseClass
    {
        public virtual string Id { get; set; }

        public virtual bool CanDelete { get; set; }

        public BaseClass() : this(true)
        {
            
        }

        public BaseClass(bool canDelete)
        {
            Id = Guid.NewGuid().ToString();
            CanDelete = canDelete;
        }
    }
}