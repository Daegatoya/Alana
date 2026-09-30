using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Classes.Interpreter
{
    public enum ExecFlow
    {
        Return,
        Break,
        Continue
    }

    public class ExecResult
    {
        public ExecFlow Flow { get; }
        public object? Value { get; }

        public ExecResult(ExecFlow flow, object? value = null)
        {
            Flow = flow;
            Value = value;
        }
    }
}
