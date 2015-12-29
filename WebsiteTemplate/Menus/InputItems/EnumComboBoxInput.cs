using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Web;

namespace WebsiteTemplate.Menus.InputItems
{
    public class EnumComboBoxInput<T> : ComboBoxInput where T : struct, IConvertible
    {
        public EnumComboBoxInput(string name, 
                                 string label,
                                 bool addBlankValue = false,
                                 //Func<FieldInfo, string> keyFunc,
                                 //Func<FieldInfo, object> valueFunc,
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

            var enumValues = Enum.GetValues(typeof(T)).Cast<T>().ToList();
            var items = enumValues.ToDictionary(e => (T)e, e => e.ToString()).ToList();

            //var items = fields.ToDictionary(keyFunc, valueFunc);
            //var items = fields.ToDictionary(e => e.GetValue(null).ToString(), e => e.Name);
            
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
    }
}