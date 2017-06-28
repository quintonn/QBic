using NHibernate;
using System;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    public class DataService
    {
        private DataStore Store { get; set; } 
        private AuditService AuditService { get; set; }

        public DataService(DataStore store, AuditService auditService)
        {
            Store = store;
            AuditService = auditService;
        }

        public void SaveOrUpdate<T>(ISession session, T item, User user = null) where T : BaseClass
        {
            var action = String.IsNullOrWhiteSpace(item.Id) ? AuditAction.New : AuditAction.Modify;

            session.SaveOrUpdate(item);
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(item, action, entityName, user);
        }

        public void TryDelete<T>(ISession session, T item) where T : BaseClass
        {
            if (item.CanDelete == false)
            {
                throw new Exception("Item not allowed to be deleted");
            }
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(item, AuditAction.Delete, entityName);
            session.Delete(item);
        }

        public ISession OpenSession()
        {
            return Store.OpenSession();
        }
    }
}