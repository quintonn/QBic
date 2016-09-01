using NHibernate;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Backend.Services
{
    /*
        Not sure if i'm happy with this class, in that I am passing session all over the place.
        But maybe this can be a long term upgrade/fix. 
        But this could end up lots of work later???


        Also not sure if handling custom error message inside Processors is nice. Eg, inside ModifyUser class i am override ProcessAction.
        - Quick fix is to have overrideable "SuccessMessage".
        - But now i'm starting to create too many overrideable settings.
        - Idk, think about this.

        Have I added too much with all these processors, services, etc, etc??
    */
    xxxxxxxxx
    public class DataService
    {
        private DataStore Store { get; set; } 
        private AuditService AuditService { get; set; }

        private IList<ISession> Sessions { get; set; } = new List<ISession>();
        public DataService(DataStore store, AuditService auditService)
        {
            Store = store;
            AuditService = auditService;
        }

        public void SaveOrUpdate<T>(ISession session, T item) where T : BaseClass
        {
            var action = String.IsNullOrWhiteSpace(item.Id) ? AuditAction.New : AuditAction.Modify;

            session.SaveOrUpdate(item);
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(item, action, entityName);
        }

        public bool TryDelete<T>(ISession session, T item) where T : BaseClass
        {
            if (item.CanDelete == false)
            {
                return false;
            }
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(item, AuditAction.Delete, entityName);
            session.Delete(item);

            return true;
        }

        public ISession OpenSession()
        {
            return Store.OpenSession();
        }
    }
}