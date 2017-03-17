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
            var writeAsync = Connection?.WriteAsync($"TRACE {funcName}|{message}", CancellationToken.None);
            if (writeAsync != null)
                await writeAsync;
        }

        public static void DebugEx(this Logger log, string funcName, string message)
        {
            log.Debug("{0}|{1}",funcName,message);
        }

        public static void ErrorEx(this Logger log, string funcName, string message)
        {
            log.Error("{0}|{1}", funcName, message);
        }

        public static void FatalEx(this Logger log, string funcName, string message)
        {
            log.Fatal("{0}|{1}", funcName, message);
        }
        public static async Task InfoEx(this Logger log, string funcName, string message)
        {
            try
            {
                log.Info("{0}|{1}", funcName, message);
                var writeAsync = Connection?.WriteAsync($"INFO {funcName}|{message}", CancellationToken.None);
                if (writeAsync != null)
                    await writeAsync;
            }
            catch (Exception e)
            {
                log.Error(e, "asd");
            }
        }
    }
}
