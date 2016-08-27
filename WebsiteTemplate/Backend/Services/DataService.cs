using NHibernate;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;
using System.Linq;

namespace WebsiteTemplate.Backend.Services
{
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

        public void SaveOrUpdate<T>(T item) where T : BaseClass
        {
            var session = GetSession();
            if (session == null)
            {
                throw new Exception("No session has been opened. Call OpenSession on DataService instance first");
            }

            var action = String.IsNullOrWhiteSpace(item.Id) ? AuditAction.New : AuditAction.Modify;

            session.SaveOrUpdate(item);
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(item, action, entityName);
        }

        public bool TryDelete<T>(T item) where T : BaseClass
        {
            var session = GetSession();
            if (session == null)
            {
                throw new Exception("No session has been opened. Call OpenSession on DataService instance first");
            }
            if (item.CanDelete == false)
            {
                return false;
            }
            var entityName = session.GetEntityName(item);

            AuditService.AuditChange(item, AuditAction.Delete, entityName);
            session.Delete(item);

            return true;
        }

        private ISession GetSession()
        {
            CleanSessions();
            return Sessions.LastOrDefault();
        }

        private static object xLock = new object();

        private void CleanSessions()
        {
            lock (xLock)
            {
                foreach (var item in Sessions.ToList())
                {
                    if (item == null || item.IsOpen == false)
                    {
                        Sessions.Remove(item);
                    }
                }
            }
        }
        public ISession OpenSession()
        {
            CleanSessions();
            var session = Store.OpenSession();
            Sessions.Add(session);
            return session;
        }
    }
}