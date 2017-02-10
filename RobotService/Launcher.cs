using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace RobotService
{
    class Launcher
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            const string funcName = nameof(Main);
            // Use the following notation to save compilation time incase logmessage is not written. (nlog tutorial). 
            log.Debug("{0}Launching business logic", funcName);
        }
    }
}
