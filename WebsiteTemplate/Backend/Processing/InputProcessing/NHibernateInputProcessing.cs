using QCumber.Core.Models;
using NHibernate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Menus.ViewItems;

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

        public override IList<T> RetrieveItemsWithFilter(GetDataSettings settings, IDictionary<string, object> additionalParameters)
        {
            using (var session = DataService.OpenSession())
            {
                var query = session.QueryOver<T>();
                query = CreateQueryForRetrieval(query, settings.Filter, additionalParameters);

                var results = query
                      .Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                      .Take(settings.LinesPerPage)
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
        /// This method is not called implicitly by the system if the default <see cref="SaveOrUpdate" /> method is called.
        /// This method is not called if <see cref="SaveOrUpdate"/> is overridden.
        /// </summary>
        /// <param name="item">The item whose properties is to be updated.</param>
        /// <returns></returns>
        public abstract Task<ProcessingResult> UpdateItem(T item);

        /// <summary>
        /// This method will attempt to save or update an item.
        /// This method can be overridden to implement non-default, custom behaviour.
        /// If overridden, use <see cref="DataService.SaveOrUpdate"/> instead of <see cref="ISession.SaveOrUpdate"/>.
        /// </summary>
        /// <param name="itemId">The id of an existing item to update, or empty string if it's a new item.</param>
        public override async Task<ProcessingResult> SaveOrUpdate(string itemId)
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
                
                var result = await UpdateItem(item);
                if (result.Success == false)
                {
                    return result;
                }

                DataService.SaveOrUpdate(session, item);
                session.Flush();
            }
            return new ProcessingResult(true);
        }

        public abstract ProcessingResult PreDeleteActivities(ISession session, string itemId);

        public virtual ProcessingResult DeleteItem(ISession session, string itemId)
        {
            var dbItem = session.Get<T>(itemId);
            DataService.TryDelete<T>(session, dbItem);
            return new ProcessingResult(true);
        }

        public override ProcessingResult DeleteItem(string itemId)
        {
            using (var session = DataService.OpenSession())
            {
                var result = PreDeleteActivities(session, itemId);
                if (result.Success == false)
                {
                    return result;  // Don't flush
                }
                result = DeleteItem(session, itemId);
                if (result.Success == false)
                {
                    return result;  // Don't flush
                }

                session.Flush();
            }
            return new ProcessingResult(true);
        }
    }
}