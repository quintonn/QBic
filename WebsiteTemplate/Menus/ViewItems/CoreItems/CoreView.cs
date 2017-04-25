using NHibernate;
using NHibernate.Criterion;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Models;
using WebsiteTemplate.Utilities;

namespace WebsiteTemplate.Menus.ViewItems.CoreItems
{
    public abstract class CoreView<T> : ShowView where T : DynamicClass
    {
        protected DataService DataService { get; set; }
        public CoreView(DataService dataService)
        {
            DataService = dataService;
            ViewParams = new Dictionary<string, string>();
        }

        protected Dictionary<string, string> ViewParams { get; set; }

        public override bool AllowInMenu
        {
            get
            {
                return false;
            }
        }

        public override IEnumerable GetData(GetDataSettings settings)
        {
            using (var session = DataService.OpenSession())
            {
                var data = CreateQuery(session, settings).Skip((settings.CurrentPage - 1) * settings.LinesPerPage)
                                                   .Take(settings.LinesPerPage)
                                                   .List<T>()
                                                   .ToList();
                var results = TransformData(data);
                return results;
            }
        }

        public virtual IEnumerable TransformData(IList<T> data)
        {
            return data;
        }

        public override int GetDataCount(GetDataSettings settings)
        {
            ViewParams = GetViewParameters(settings);

            using (var session = DataService.OpenSession())
            {
                var result = CreateQuery(session, settings).RowCount();
                return result;
            }
        }

        public virtual IQueryOver<T> CreateQuery(ISession session, GetDataSettings settings)
        {
            var query = session.QueryOver<T>();

            if (!String.IsNullOrWhiteSpace(settings.Filter))
            {
                var filterItems = GetFilterItems();
                foreach (var item in filterItems)
                {
                    query = query.WhereRestrictionOn(item).IsLike(settings.Filter, MatchMode.Anywhere);
                }
            }

            OrderQuery(query);

            return query;
        }

        public virtual Dictionary<string, string> GetViewParameters(GetDataSettings settings)
        {
            return new Dictionary<string, string>();
        }

        public override Dictionary<string, object> GetEventParameters()
        {
            return ViewParams.ToDictionary(x => x.Key, z => (object)z.Value);
        }

        public override Dictionary<string, string> DataForGettingMenu
        {
            get
            {
                return ViewParams;
            }
        }

        protected string GetParameter(string parameterName, string alternateName, GetDataSettings settings, bool allowNull = true)
        {
            var item = GetParameter(parameterName, settings, true);
            if (String.IsNullOrWhiteSpace(item))
            {
                item = GetParameter(alternateName, settings, allowNull);
            }
            return item;
        }

        protected string GetParameter(string parameterName, GetDataSettings settings, bool allowNull = true)
        {
            var data = settings.ViewData;
            var json = JsonHelper.Parse(data);
            var tmpData = json.GetValue("data");
            var result = String.Empty;
            if (!String.IsNullOrWhiteSpace(tmpData))
            {
                var x = JsonHelper.Parse(tmpData);
                result = x.GetValue(parameterName);
                if (String.IsNullOrWhiteSpace(result))
                {
                    result = tmpData;
                }
            }
            else
            {
                var eventParams = json.GetValue("eventParameters");
                if (!String.IsNullOrWhiteSpace(eventParams))
                {
                    var x = JsonHelper.Parse(eventParams);
                    result = x.GetValue(parameterName);
                }
                else if (!String.IsNullOrWhiteSpace(data))
                {
                    result = data;
                }
            }

            if (!allowNull && String.IsNullOrWhiteSpace(result))
            {
                throw new Exception("Parameter " + parameterName + " should not be null");
            }

            return result;
        }

        public virtual IQueryOver<T> OrderQuery(IQueryOver<T, T> query)
        {
            return query;
        }
        /* E.G.
             public override IQueryOver<LogItem> OrderQuery(IQueryOver<LogItem, LogItem> query)
             {
                return query.OrderBy(x => x.Position).Asc();
             }
         * */

        public abstract List<Expression<Func<T, object>>> GetFilterItems();
        /* E.G.
         * return new List<Expression<Func<EventFixture, object>>>()
            {
                //x => x.AwayTeam.Name,
                //x => x.HomeTeam.Name,
                //x => x.DateTimeUTC
            };
            */
    }
}