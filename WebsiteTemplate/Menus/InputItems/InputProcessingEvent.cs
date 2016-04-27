using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Menus.BaseItems;

namespace WebsiteTemplate.Menus.InputItems
{
    public abstract class InputProcessingEvent : Event
    {
        public Dictionary<string, object> InputData { get; set; } = new Dictionary<string, object>();

        /// <summary>
        /// This method assumes you know the return type.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            object value = default(T);

            if (InputData.ContainsKey(propertyName))
            {
                if (typeof(T) == typeof(bool))
                {
                    value = Convert.ToBoolean(InputData[propertyName]);
                }
                else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                {
                    var tempValue = InputData[propertyName];
                    DateTime date;
                    if (DateTime.TryParse(tempValue?.ToString(), out date))
                    {
                        value = date;
                    }
                    else if (typeof(T) == typeof(DateTime))
                    {
                        value = new DateTime(1900, 01, 01); ///TODO: What do i do with default dates etc.
                    }
                }
                else if (typeof(T) == typeof(int))
                {
                    var tempValue = InputData[propertyName];
                    int intValue;
                    if (int.TryParse(tempValue?.ToString(), out intValue))
                    {
                        value = intValue;
                    }
                    else
                    {
                        value = 0;
                    }
                }
                else
                {
                    value = (T)InputData[propertyName];
                }
            }
            if (value == null)
            {
                return defaultValue;
            }
            return (T)value;
        }

        public DateTime? GetDateFromString(string dateString, DateTime? defaultValue = null)
        {
            DateTime result;
            if (DateTime.TryParse(dateString, out result))
            {
                return result;
            }
            return defaultValue;
        }

        public decimal GetDecimalValue(string decimalString, decimal defaultValue = 0)
        {
            decimal result;
            if (Decimal.TryParse(decimalString, out result))
            {
                return result;
            }
            return defaultValue;
        }

        public string GetValue(string propertyName)
        {
            return GetValue<string>(propertyName);
        }

        public T GetDataSourceValue<T>(string propertyName)
        {
            using (var session = Store.OpenSession())
            {
                var id = GetValue(propertyName);
                var result = session.Get<T>(id);
                return result;
            }
        }
    }
}