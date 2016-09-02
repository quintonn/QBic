﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace WebsiteTemplate.Utilities
{
    public class JsonHelper
    {
        private JObject Data { get; set; }

        public JsonHelper()
        {
            Data = new JObject();
        }

        private JsonHelper(JObject data)
        {
            Data = data;
        }

        public override string ToString()
        {
            if (Data.ToString() == "{}")
            {
                return String.Empty;
            }
            return Data.ToString();
        }

        public void Add(string propertyName, JToken value)
        {
            Data.Add(propertyName, value);
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
                else if (typeof(T) == typeof(JsonHelper))
                {
                    value = Parse(item.ToString());
                }
                else if (typeof(T) == typeof(JsonArray))
                {
                    //var arr = item as JArray;
                    //value = new JsonArray(arr.Select(a => FromObject(a)).ToList());
                    value = JsonArray.FromObject(item);
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

        public object this[string key]
        {
            get
            {
                return Data[key];
            }
            set
            {
                Data[key] = JToken.FromObject(value);
            }
        }

        public static JsonHelper Parse(string json)
        {
            JObject data;
            if (!String.IsNullOrWhiteSpace(json))
            {
                try
                {
                    data = JObject.Parse(json);
                }
                catch (Exception e)
                {
                    data = new JObject();
                }
            }
            else
            {
                data = new JObject();
            }
            return new JsonHelper(data);
        }

        public static JsonHelper FromObject(object item)
        {
            if (String.IsNullOrWhiteSpace(item.ToString()))
            {
                return new JsonHelper();
            }
            return new JsonHelper(JObject.FromObject(item));
        }

        public static T DeserializeObject<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }

        public static string SerializeObject(object value)
        {
            return JsonConvert.SerializeObject(value);
        }
    }
}