using NHibernate;
using System;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.BasicCrudItems
{
    public interface IBasicCrudDelete : IEvent
    {
        int Id { get; set; }
        string ItemName { get; set; }

        Action<ISession, object> OnDeleteInternal { get; set; }
    }
}