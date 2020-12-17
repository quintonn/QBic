using QBic.Core.Data;
using QBic.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using WebsiteTemplate.Backend.Services;
using WebsiteTemplate.Data;
using WebsiteTemplate.Models;

namespace WebsiteTemplate.Menus.InputItems
{
    public class DataSourceComboBoxInput<T> : ComboBoxInput where T : BaseClass
    {
        /// <summary>
        /// This creates a <see cref="WebsiteTemplate.Menus.InputItems.ComboBoxInput"> item where the source is obtained
        /// from the database for the type T.
        /// </summary>
        /// <param name="name">Input name to be used by process method.</param>
        /// <param name="label">Label to be displayed on user interface to user.</param>
        /// <param name="keyFunc">Function to return a property of T or a value to use as the key in the dictionary to use for the list source.</param>
        /// <param name="valueFunc">Function to return a property of T or a value to use as the value in the dictionary.</param>
        /// <param name="defaultValue">Default value to use for this field. Should match a value from returned by keyFunc or be null.</param>
        /// <param name="tabName">The name of the tab to appear on the user interface, or null if only 1 tab is required.</param>
        /// <param name="whereClause">An optional where clause to pass into the NHibernate query.</param>
        /// <param name="orderByClause">An optional Order by clause to pass into the NHibernate query.</param>
        /// <param name="orderByAsc">If an order by clause is provided, this will set order direction.</param>
        /// <param name="addBlankValue">If set to true will add an empty option to the combo box.</param>
        public DataSourceComboBoxInput(string name,
                                       string label,
                                       Func<T, string> keyFunc,
                                       Func<T, object> valueFunc,
                                       object defaultValue = null,
                                       string tabName = null,
                                       Expression<Func<T, bool>> whereClause = null,
                                       Expression<Func<T, object>> orderByClause = null,
                                       bool orderByAsc = true,
                                       bool addBlankValue = false)
            : base(name, label, defaultValue, tabName)
        {
            KeyFunc = keyFunc;
            ValueFunc = valueFunc;
            UpdateList(whereClause, orderByClause, orderByAsc, addBlankValue);   
        }

        private Func<T, string> KeyFunc { get; set; }
        private Func<T, object> ValueFunc { get; set; }

        public void UpdateList(Expression<Func<T, bool>> whereClause = null,
                                       Expression<Func<T, object>> orderByClause = null,
                                       bool orderByAsc = true,
                                       bool addBlankValue = false)
        {
            var store = DataStore.GetInstance(false, null);
            using (var session = store.OpenSession())
            {
                var queryOver = session.QueryOver<T>();
                if (whereClause != null)
                {
                    ///  Leave this for references - #LambdaExpression
                    ///  var expression = Expression.Lambda<Func<bool>>(Expression.Call(whereClause.Method));
                    ///  queryOver = queryOver.Where(expression);
                    queryOver = queryOver.Where(whereClause);
                }
                if (orderByClause != null)
                {
                    if (orderByAsc)
                    {
                        queryOver = queryOver.OrderBy(orderByClause).Asc;
                    }
                    else
                    {
                        queryOver = queryOver.OrderBy(orderByClause).Desc;
                    }
                }
                var list = queryOver.List<T>();

                var result = list.ToDictionary(KeyFunc, ValueFunc);
                if (orderByClause == null)
                {
                    result = result.OrderBy(o => o.Value).ToDictionary(o => o.Key, o => o.Value);
                }
                if (addBlankValue)
                {
                    var tmpList = result.Select(r => new KeyValuePair<string, object>(r.Key, r.Value)).ToList();
                    tmpList.Insert(0, new KeyValuePair<string, object>("", ""));
                    result = tmpList.ToDictionary(t => t.Key, t => t.Value);
                }
                ListItems = result;
            }
        }
    }
}