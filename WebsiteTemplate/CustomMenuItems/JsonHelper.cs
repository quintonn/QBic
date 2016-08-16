using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebsiteTemplate.CustomMenuItems
{
    public class JsonHelper
    {
        private JObject Data { get; set; }

        public JsonHelper()
        {

        }

        private JsonHelper(JObject data)
        {
            Data = data;
        }

        public string GetValue(string propertyName)
        {
            JToken result = null;
            if (Data.TryGetValue(propertyName, out result))
            {
                return result?.ToString();
            }
            return String.Empty;
        }

        public T GetValue<T>(string propertyName, T defaultValue = default(T))
        {
            object value = defaultValue;

            var item = Data.GetValue(propertyName);

            if (item != null)
            {
                if (typeof(T) == typeof(String))
                {
                    value = item.ToString();
                }
                else if (typeof(T) == typeof(bool))
                {
                    value = Convert.ToBoolean(item.ToString());
                }
                else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                {
                    DateTime date;
                    if (DateTime.TryParse(item.ToString(), out date))
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
                    int intValue;
                    if (int.TryParse(item.ToString(), out intValue))
                    {
                        value = intValue;
                    }
                    else
                    {
                        value = 0;
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    decimal decimalValue;
                    if (decimal.TryParse(item.ToString(), out decimalValue))
                    {
                        value = decimalValue;
                    }
                    else
                    {
                        value = 0m;
                    }
                }
                else if (typeof(T) == typeof(float))
                {
                    float floatValue;
                    if (float.TryParse(item.ToString(), out floatValue))
                    {
                        value = floatValue;
                    }
                    else
                    {
                        value = 0f;
                    }
                }
                else if (typeof(T) == typeof(JObject))
                {
                    value = item as JObject;
                }
                else
                {
                    value = item;
                }
            }
            if (value == null)
            {
                return defaultValue;
            }
            return (T)value;
        }

        public static JsonHelper Parse(string json)
        {
            JObject data;
            if (!String.IsNullOrWhiteSpace(json))
            {
                data = JObject.Parse(json);
            }
            else
            {
                data = new JObject();
            }
            return new JsonHelper(data);
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}