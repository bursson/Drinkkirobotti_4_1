using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorUI
{
    public class Trace : LogOutput
    {
        public Trace(string message) : base(message)
        {
        }

        public override string LogLevel => "Trace";
    }

    public class Debug : LogOutput
    {
        public Debug(string message) : base(message)
        {
        }

        public override string LogLevel => "Debug";
    }

    public class Error : LogOutput
    {
        public Error(string message) : base(message)
        {
        }

        public override string LogLevel => "Error";
    }

    public class Fatal : LogOutput
    {
        public Fatal(string message) : base(message)
        {
        }

        public override string LogLevel => "Fatal";
    }

    public class Info : LogOutput
    {
        public Info(string message) : base(message)
        {
        }

        public override string LogLevel => "Info";
    }
}
