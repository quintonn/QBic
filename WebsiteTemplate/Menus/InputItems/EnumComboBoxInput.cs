using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using NHibernate.Util;
using System.Linq.Expressions;

namespace WebsiteTemplate.Menus.InputItems
{
    public class EnumComboBoxInput<T> : ComboBoxInput where T : struct, IConvertible
    {
        public EnumComboBoxInput(string name, 
                                 string label,
                                 bool addBlankValue = false,
                                 Func<KeyValuePair<T, string>, bool> whereClause = null,
                                 Func<KeyValuePair<T, string>, object> orderByClause = null,
                                 object defaultValue = null, 
                                 string tabName = null)
            : base(name, label, defaultValue, tabName)
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type (enum)");
            }

            UpdateList(whereClause, orderByClause, addBlankValue);
        }

        public void UpdateList(Func<KeyValuePair<T, string>, bool> whereClause = null,
                               Func<KeyValuePair<T, string>, object> orderByClause = null,
                               bool addBlankValue = false)
        {
            var enumValues = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            var items = enumValues.ToDictionary(e => (T)e, e => e.ToString()).ToList();

            if (whereClause != null)
            {
                items = items.Where(whereClause).ToList();
            }
            if (orderByClause != null)
            {
                items = items.OrderBy(orderByClause).ToList();
            }

            var listItems = items.ToDictionary(e => e.Key.ToString(), e => (object)e.Value).ToList();
            if (addBlankValue)
            {
                listItems.Insert(0, new KeyValuePair<string, object>("", ""));
            }

            ListItems = listItems.ToDictionary(e => e.Key, e => e.Value);
        }

        public override object GetValue(JToken jsonToken)
        {
            if (jsonToken == null || string.IsNullOrWhiteSpace(jsonToken?.ToString()))
            {
                return null;// Enum.GetValues(typeof(T)).First();
            }
            var result = (T)Enum.Parse(typeof(T), jsonToken?.ToString());
            return result;
        }
    }
}