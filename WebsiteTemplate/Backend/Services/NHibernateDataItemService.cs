using NHibernate;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Models;
using System.Linq;

namespace WebsiteTemplate.Backend.Services
{
    public abstract class NHibernateDataItemService<T> : DataItemServiceCore<T> where T : BaseClass
    {
        
        public NHibernateDataItemService(DataService dataService)
            :base(dataService)
        {

        }

        public abstract IQueryOver<T> CreateQueryForRetrieval(ISession session, string filter);

        public override int RetrieveItemCountWithFilter(string filter)
        {
            using (var session = DataService.OpenSession())
            {
                var query = CreateQueryForRetrieval(session, filter);

                return query.RowCount();
            }
        }

        public override IList<T> RetrieveItemsWithFilter(int currentPage, int linesPerPage, string filter)
        {
            using (var session = DataService.OpenSession())
            {
                var query = CreateQueryForRetrieval(session, filter);

                var results = query
                      .Skip((currentPage - 1) * linesPerPage)
                      .Take(linesPerPage)
                      .List<T>()
                      .ToList();
                return results;
            }
        }

        public abstract T RetrieveItem(ISession session, string itemId);

        public override T RetrieveItem(string itemId)
        {
            using (var session = DataService.OpenSession())
            {
                return session.Get<T>(itemId);
            }
        }

        public abstract T RetrieveExistingItem(ISession session);

        public override T RetrieveExistingItem()
        {
            using (var session = DataService.OpenSession())
            {
                return RetrieveExistingItem(session);
            }
        }

        /// <summary>
        /// This method is called to update the item with relevant properties which can be obtained by calling GetValue().
        /// Do not save, update or flush the session!
        /// </summary>
        /// <param name="session">The session to use to retrieve any existing objects. Do not call Save, Update or Flush on session!</param>
        /// <param name="item">The item whose properties can be updated.</param>
        /// <returns></returns>
        public abstract void UpdateItem(ISession session, T item);
        public override void SaveOrUpdate(string itemId)
        {
            using (var session = DataService.OpenSession())
            {
                T item;
                if (!String.IsNullOrWhiteSpace(itemId))
                {
                    item = session.Get<T>(itemId);
                }
                else
                {
                    item = Activator.CreateInstance<T>();
                }

                UpdateItem(session, item);

                DataService.SaveOrUpdate(item);
                session.Flush();
            }
        }
    }
}