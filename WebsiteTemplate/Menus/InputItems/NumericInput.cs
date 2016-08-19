using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace WebsiteTemplate.Menus.InputItems
{
    public class NumericInput<T> : InputField where T : struct,
          IComparable,
          IComparable<T>,
          IConvertible,
          IEquatable<T>,
          IFormattable
    {
        public override InputType InputType
        {
            get
            {
                return InputType.Numeric;
            }
        }

        public int DecimalPlaces { get; set; }

        public double Step
        {
            get
            {
                var steps = Math.Pow(10, DecimalPlaces);
                steps = 1 / steps;
                return steps;
            }
        }

        public NumericInput(string name, string label, object defaultValue = null, string tabName = null, bool mandatory = false, int decimalPlaces = 0)
            : base(name, label, defaultValue, tabName, mandatory)
        {
            DecimalPlaces = decimalPlaces;
        }

        public override object GetValue(JToken jsonToken)
        {
            var value = jsonToken?.ToString();
            if (typeof(T) == typeof(int))
            {
                int intValue;
                if (int.TryParse(value, out intValue))
                {
                    return intValue;
                }
                else
                {
                    return 0;
                }
            }
            else if (typeof(T) == typeof(decimal))
            {
                decimal decimalValue;
                if (decimal.TryParse(value, out decimalValue))
                {
                    return decimalValue;
                }
                else
                {
                    return 0m;
                }
            }
            else if (typeof(T) == typeof(float))
            {
                float floatValue;
                if (float.TryParse(value, out floatValue))
                {
                    return floatValue;
                }
                else
                {
                    return 0f;
                }
            }
            else if (typeof(T) == typeof(long))
            {
                long longValue;
                if (long.TryParse(value, out longValue))
                {
                    return longValue;
                }
                else
                {
                    return 0L;
                }
            }
            else
            {
                throw new Exception("Unhandled numeric type: " + typeof(T).ToString().Split(".".ToCharArray()).Last());
            }
        }
    }
}