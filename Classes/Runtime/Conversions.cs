using Classes.Handler;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    internal static class Conversions
    {
        public static object ToNum(Interpreter runtime, object value)
        {
            if (value is not string && value is not char && value is not int)
            {
                throw runtime.Error($"Unexpected type for parsenum(). Expected schar, num, or str but received {value.GetType()}");
            }
            if (int.TryParse(value.ToString(), out int result))
            {
                return result;
            }
            else
            {
                throw runtime.Error($"Cannot parse expected value to num");
            }
        }

        public static object ToDecimal(Interpreter runtime, object value)
        {
            if (value is not string && value is not char && value is not decimal && value is not int)
            {
                throw runtime.Error($"Unexpected type for parsedecimal(). Expected schar, num, decimal, or str but received {value.GetType()}");
            }
            if (value is decimal)
            {
                return value;
            }
            else if (value is int)
            {
                return Convert.ToDecimal(value);
            }
            else if (value is string || value is char)
            {
                decimal.TryParse(value.ToString(), CultureInfo.InvariantCulture, out decimal result);
                return result;
            }
            else
            {
                throw runtime.Error($"Cannot parse expected value to decimal");
            }
        }

        public static object ToBool(Interpreter runtime, object value)
        {
            if (value is not string && value is not bool)
            {
                throw runtime.Error($"Unexpected type for parseboolean(). Expected str or bool but received {value.GetType()}");
            }
            if (bool.TryParse(value.ToString(), out bool result))
            {
                return result;
            }
            else
            {
                throw runtime.Error($"Cannot parse expected value to boolean");
            }
        }

        public static object ToChar(Interpreter runtime, object value)
        {
            if (value is not string && value is not int && value is not char)
            {
                throw runtime.Error($"Unexpected type for parseschar(). Expected num, schar, or str but received {value.GetType()}");
            }
            if (char.TryParse(value.ToString(), out char result))
            {
                return result;
            }
            else
            {
                throw runtime.Error($"Cannot parse expected value to schar");
            }
        }

        public static object ToStr(object value)
        {
            return value.ToString()!;
        }
    }
}
