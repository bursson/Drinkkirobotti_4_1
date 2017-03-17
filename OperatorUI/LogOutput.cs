using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorUI
{
    public class LogOutput
    {
        /// <summary>
        /// Log message is string with format:
        /// LOGMESSAGE;;
        /// loglevel;;
        /// functionname;;
        /// datetimenow;;
        /// message
        /// </summary>
        /// <param name="message"></param>
        public LogOutput(string message)
        {
            string separator = ";;";
            string[] messageBreakDown = message.Split(new[] { separator }, StringSplitOptions.None);

            if (messageBreakDown.Length != 5)
            {
                return;
            }
            LogLevel = messageBreakDown[1];
            FunctionName = messageBreakDown[2];
            DateTime = messageBreakDown[3];
            Message = messageBreakDown[4];
        }

        public string LogLevel { get; private set; }

        public string FunctionName { get; private set; }

        public string DateTime { get; private set; }
        
        public string Message { get; private set; }


    }
}
