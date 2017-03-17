using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;

namespace Common
{
    /// <summary>
    /// Public static class to extend functionality of the NLog-library.
    /// Simplifies the calling syntax and enables the implemenation for TCP logging in the future.
    /// </summary>
    public static class NLogExtensions
    {
        public static Server Connection { private get; set; }

        public static async Task TraceEx(this Logger log, string funcName, string message)
        {
            log.Trace("{0}|{1}",funcName,message);
            await FormatMessageAndWriteASync("TRACE", funcName, message);
        }

        public static async Task DebugEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Debug("{0}|{1}", funcName, message);

            if(!willBeLoggedToUi)
            {
                return;
            }
            await FormatMessageAndWriteASync("DEBUG", funcName, message);

        }

        public static async Task ErrorEx(this Logger log, string funcName, string message)
        {
            log.Error("{0}|{1}", funcName, message);

            await FormatMessageAndWriteASync("ERROR", funcName, message);

        }

        public static async Task FatalEx(this Logger log, string funcName, string message)
        {
            log.Fatal("{0}|{1}", funcName, message);

            await FormatMessageAndWriteASync("FATAL", funcName, message);
        }
        public static async Task InfoEx(this Logger log, string funcName, string message)
        {
            log.Info("{0}|{1}", funcName, message);
            
            await FormatMessageAndWriteASync("INFO", funcName, message);
        }

        private static async Task FormatMessageAndWriteASync(string level, string functionName, string message)
        {
            string separator = ";;";
            var writeAsync = Connection?.WriteAsync("LOGMESSAGE" + 
                separator + level + 
                separator + functionName + 
                separator + DateTime.Now.ToString() + 
                separator + message, CancellationToken.None);

            if (writeAsync != null)
                await writeAsync;
        }
    }
}
