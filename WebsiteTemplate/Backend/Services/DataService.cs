using QBic.Core.Data;
using QBic.Core.Models;
using NHibernate;
using System;
using WebsiteTemplate.Models;
using Qactus.Authorization.Core;

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

        public void SaveOrUpdate<T>(ISession session, T item, IUser user = null) where T : BaseClass
        {
            var action = String.IsNullOrWhiteSpace(item.Id) ? AuditAction.New : AuditAction.Modify;

            var itemId = item.Id; // Get before calling save/update as this creates id

            session.SaveOrUpdate(item);
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(session, itemId, item, action, entityName, user);
        }

        public void TryDelete<T>(ISession session, T item) where T : BaseClass
        {
            if (item.CanDelete == false)
            {
                throw new Exception("Item not allowed to be deleted");
            }
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(session, item.Id, item, AuditAction.Delete, entityName);
            session.Delete(item);
        }

        public ISession OpenSession()
        {
            return Store.OpenSession();
        }

        public IStatelessSession OpenStatelessSession()
        {
            return Store.OpenStatelessSession();
        }

        public ISession OpenAuditSession()
        {
            return Store.OpenAuditSession();
        }

        public IStatelessSession OpenAuditStatelessSession()
        {
            return Store.OpenAuditStatelessSession();
        }
    }
}