using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebsiteTemplate.Backend.Services;

namespace WebsiteTemplate.Menus.InputItems
{
    public static class InputProcessingMethods
    {
        private static bool TryParseEnum<TEnum>(string value, bool ignoreCase, out TEnum resultValue)
        {
            resultValue = default(TEnum);
            try
            {
                var result = (TEnum)Enum.Parse(typeof(TEnum), value, ignoreCase);
                resultValue = result;

                return true;
            }
            catch (Exception e)
            {                
                return false;
            }
        }

        public static T GetValue<T>(IDictionary<string, object> inputData, string propertyName, T defaultValue = default(T))
        {
            object value = defaultValue;

            var nullableType = Nullable.GetUnderlyingType(typeof(T));

            if (inputData.ContainsKey(propertyName))
            {
                if (typeof(T).IsEnum || (nullableType != null && nullableType.IsEnum))
                {
                    T tempValue;
                    if (TryParseEnum(inputData[propertyName]?.ToString(), true, out tempValue))
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
                else if (typeof(T) == typeof(bool))
                {
                    value = Convert.ToBoolean(inputData[propertyName]);
                }
                else if (typeof(T) == typeof(DateTime) || typeof(T) == typeof(DateTime?))
                {
                    var tempValue = inputData[propertyName];
                    DateTime date;
                    if (DateTime.TryParse(tempValue?.ToString(), out date))
                    {
                        value = date;
                    }
                    else if (typeof(T) == typeof(DateTime))
                    {
                        throw new Exception(String.Format("Unable to process the value '{0}' as date.", tempValue));
                        //value = new DateTime(1900, 01, 01); ///TODO: What do i do with default dates etc.
                    }
                }
                else if (typeof(T) == typeof(int) || typeof(T) == typeof(int?))
                {
                    var tempValue = inputData[propertyName];
                    int intValue;
                    if (int.TryParse(tempValue?.ToString(), out intValue))
                    {
                        value = intValue;
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                else if (typeof(T) == typeof(decimal))
                {
                    var tempValue = inputData[propertyName];
                    decimal decimalValue;
                    if (decimal.TryParse(tempValue?.ToString(), out decimalValue))
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
                    var tempValue = inputData[propertyName];
                    float floatValue;
                    if (float.TryParse(tempValue?.ToString(), out floatValue))
                    {
                        value = floatValue;
                    }
                    else
                    {
                        value = 0f;
                    }
                }
                else
                {
                    value = (T)inputData[propertyName];
                }
            }
            if (value == null)
            {
                return defaultValue;
            }
            return (T)value;
        }

        public static T GetDataSourceValue<T>(IDictionary<string, object> inputData, DataService dataService, string propertyName)
        {
            var id = InputProcessingMethods.GetValue<string>(inputData, propertyName);
            if (String.IsNullOrWhiteSpace(id))
            {
                return default(T);
            }
            using (var session = dataService.OpenSession())
            {
                var result = session.Get<T>(id);
                return result;
            }
        }
    }
}