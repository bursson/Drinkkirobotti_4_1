using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorUI
{
    public abstract class LogOutput
    {
        protected LogOutput(string message)
        {
            Message = message;
        }

        public string Message { get; set; }

        //TODO: add here log levels, timestamps etc
        
        public abstract string LogLevel { get; }

    }
}
