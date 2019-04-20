using NHibernate;
using NHibernate.Criterion;
using QBic.Core.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using WebsiteTemplate.Backend.Services;

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

        public virtual List<KeyValuePair<Expression<Func<T, object>>, Expression<Func<object>>>> GetAliases()
        {
            return new List<KeyValuePair<Expression<Func<T, object>>, Expression<Func<object>>>>();
        }

        public virtual IQueryOver<T> CreateQuery(ISession session, GetDataSettings settings, Expression<Func<T, bool>> additionalCriteria = null)
        {
            var query = session.QueryOver<T>();

            var aliases = GetAliases();
            foreach (var alias in aliases)
            {
                query = query.Left.JoinAlias(alias.Key, alias.Value);
            }

            var or = Restrictions.Disjunction();

            if (!String.IsNullOrWhiteSpace(settings.Filter))
            {
                var filterItems = GetFilterItems();
                
                foreach (var item in filterItems)
                {
                    var x = Restrictions.InsensitiveLike(Projections.Property(item), settings.Filter, MatchMode.Anywhere);
                    or.Add(x);
                }

                var tempDict = new Dictionary<string, object>()
                {
                    { "X", settings.Filter }
                };

                var nonStringFilterItems = GetNonStringFilterItems();

                var method = Type.GetType("WebsiteTemplate.Menus.InputItems.InputProcessingMethods").GetMethod("GetValue");
                
                foreach (var item in nonStringFilterItems)
                {
                    var defaultValue = Activator.CreateInstance(item.Key);

                    MethodInfo generic = method.MakeGenericMethod(item.Key);

                    var theValue = generic.Invoke(null, new List<object>() { tempDict, "X", defaultValue }.ToArray());
                    if ((item.Key.IsEnum && theValue.ToString().ToLower() == settings.Filter.ToLower() ) || !theValue.Equals(defaultValue))
                    {
                        var x = Restrictions.Eq(Projections.Property(item.Value), theValue);
                        or.Add(x);
                    }
                }
                query.Where(or);
            }

            if (additionalCriteria != null)
            {
                query.And(additionalCriteria);
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

        public virtual IQueryOver<T> OrderQuery(IQueryOver<T, T> query)
        {
            return query;
        }
        /* E.G.
             public override IQueryOver<LogItem> OrderQuery(IQueryOver<LogItem, LogItem> query)
             {
                return query.OrderBy(x => x.Position).Asc;
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

        public virtual List<KeyValuePair<Type, Expression<Func<T, object>>>> GetNonStringFilterItems()
        {
            return new List<KeyValuePair<Type, Expression<Func<T, object>>>>();
        }
    }
}