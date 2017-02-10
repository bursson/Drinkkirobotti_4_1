using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using NLog;

namespace RobotService
{
    class Launcher
    {
        private static Logger log = LogManager.GetCurrentClassLogger();
        static void Main(string[] args)
        {
            const string funcName = nameof(Main); 
            log.InfoEx(funcName,"Launching business logic..");
        }
    }
}
