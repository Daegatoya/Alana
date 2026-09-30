using Classes.AST;
using Classes.Handler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Runtime
{
    internal static class TypeChecker
    {
        public static bool CheckType(Interpreter runtime, object value, VarType? type, bool isArray = false)
        {
            if (isArray)
            {
                if (value is not List<object> values)
                {
                    throw runtime.Error($"Type error: expected an array of {type}");
                }

                foreach (object element in values)
                {
                    bool elementIsRight = type switch
                    {
                        VarType.NUM => element is int,
                        VarType.STR => element is string,
                        VarType.CHAR => element is char,
                        VarType.DECIMAL => element is decimal,
                        VarType.BOOL => element is bool,
                        _ => false
                    };

                    if (!elementIsRight)
                    {
                        throw runtime.Error($"Type error: expected array of {type} but got element of type {element.GetType()}");
                    }
                }

                return true;
            }

            bool isRight = type switch
            {
                VarType.NUM => value is int,
                VarType.STR => value is string,
                VarType.CHAR => value is char,
                VarType.DECIMAL => value is decimal,
                VarType.BOOL => value is bool,
                _ => false
            };

            if (isRight) return true;

            throw runtime.Error($"Type error: expected {type} but got {value.GetType()}");
        }
    }
}
