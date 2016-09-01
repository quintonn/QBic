using NHibernate;
using System;
using System.Collections.Generic;
using WebsiteTemplate.Models;
using System.Linq;
using WebsiteTemplate.Backend.Services;
using System.Threading.Tasks;

namespace WebsiteTemplate.Backend.Processing.InputProcessing
{
    public abstract class NHibernateDataItemService<T> : InputProcessingCore<T> where T : BaseClass
    {

        public NHibernateDataItemService(DataService dataService)
            : base(dataService)
        {

        }

        public abstract IQueryOver<T, T> CreateQueryForRetrieval(IQueryOver<T, T> query, string filter, IDictionary<string, object> additionalParameters);

        public override int RetrieveItemCountWithFilter(string filter, IDictionary<string, object> additionalParameters)
        {
            using (var session = DataService.OpenSession())
            {
                var query = session.QueryOver<T>();
                query = CreateQueryForRetrieval(query, filter, additionalParameters);

                return query.RowCount();
            }
        }

        public override IList<T> RetrieveItemsWithFilter(int currentPage, int linesPerPage, string filter, IDictionary<string, object> additionalParameters)
        {
            using (var session = DataService.OpenSession())
            {
                var query = session.QueryOver<T>();
                query = CreateQueryForRetrieval(query, filter, additionalParameters);

                var results = query
                      .Skip((currentPage - 1) * linesPerPage)
                      .Take(linesPerPage)
                      .List<T>()
                      .ToList();
                return results;
            }
        }

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
        /// This method is not called implicitly by the system if the <see cref="SaveOrUpdate" /> method is called.
        /// </summary>
        /// <param name="session">The session to use to retrieve any existing objects. Do not call Save, Update or Flush on session!</param>
        /// <param name="item">The item whose properties is to be updated.</param>
        /// <returns></returns>
        public abstract Task UpdateItem(ISession session, T item);

        /// <summary>
        /// This method will attempt to save or update an item.
        /// This method can be overriden to implement non-default, custom behaviour.
        /// If overriden, use <see cref="DataService.SaveOrUpdate"/> instead of <see cref="ISession.SaveOrUpdate"/>.
        /// </summary>
        /// <param name="itemId">The id of an existing item to update, or empty string if it's a new item.</param>
        public override async Task SaveOrUpdate(string itemId)
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
                
                await UpdateItem(session, item);

                DataService.SaveOrUpdate(session, item);
                session.Flush();
            }
            return;
        }

        public virtual void DeleteItem(ISession session, string itemId)
        {
            var dbItem = session.Get<T>(itemId);
            DataService.TryDelete<T>(session, dbItem);
        }

        public override void DeleteItem(string itemId)
        {
            using (var session = DataService.OpenSession())
            {
                DeleteItem(session, itemId);

                session.Flush();
            }
        }
    }
}