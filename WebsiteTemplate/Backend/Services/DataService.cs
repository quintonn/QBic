using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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

        public void SaveOrUpdate<T>(T item) where T : BaseClass
        {
            System.Diagnostics.Trace.TraceInformation("saving an object : " + item.GetType().ToString());
            
            using (var session = Store.OpenSession())
            {
                AuditService.AuditChange(item, String.IsNullOrWhiteSpace(item.Id) ? AuditAction.New : AuditAction.Modify);

                session.SaveOrUpdate(item);
                session.Flush();
            }
            System.Diagnostics.Trace.TraceInformation("object saved: " + item.GetType().ToString());
        }

        public bool TryDelete<T>(T item) where T : BaseClass
        {
            if (item.CanDelete == false)
            {
                return false;
            }
            using (var session = Store.OpenSession())
            {
                AuditService.AuditChange(item, AuditAction.Delete);
                session.Delete(item);
                session.Flush();
            }
            return true;
        }
    }
}