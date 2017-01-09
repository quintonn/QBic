using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using WebsiteTemplate.Menus.InputItems;

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

            var nullableType = Nullable.GetUnderlyingType(typeof(T));

            if (item != null)
            {
                if (typeof(T).IsEnum || (nullableType != null && nullableType.IsEnum))
                {
                    T tempValue;
                    if (InputProcessingMethods.TryParseEnum(item.ToString(), true, out tempValue))
                    {
                        value = tempValue;
                    }
                    //else if (Nullable.GetUnderlyingType(typeof(T)) != null)
                    else if (nullableType.IsEnum)
                    {
                        value = null;
                    }
                    else
                    {
                        throw new Exception(String.Format("Unable to parse non-nullable enum from value '{0}'", value));
                    }
                }
                else if (typeof(T) == typeof(String))
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
            try
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
            catch (JsonReaderException exception)
            {
                Console.WriteLine(exception.Message);
                return default(T);
            }
        }

        public static T DeserializeObject<T>(string value, bool includeTypeInfo = false)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = includeTypeInfo == true ? TypeNameHandling.All : TypeNameHandling.None,
            };
            return JsonConvert.DeserializeObject<T>(value, jsonSettings);
        }

        public static string SerializeObject(object value, bool prettyFormat = false, bool includeTypeInfo = false)
        {
            var jsonSettings = new JsonSerializerSettings()
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                TypeNameHandling = includeTypeInfo == true ? TypeNameHandling.All : TypeNameHandling.None,
            };

            return JsonConvert.SerializeObject(value, prettyFormat == true ? Formatting.Indented : Formatting.None, jsonSettings);
        }
    }
}