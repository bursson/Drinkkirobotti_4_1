using System;
using System.Globalization;
using System.Net;
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
        public static void InitializeLogConnection(CancellationToken ct, IPAddress host, int port)
        {
            Connection = new Client(host, port, "\r\n", true, "Logger");
            Connection.Run(ct);
        }

        private static IConnection Connection { get; set; }

        public static void TraceEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Trace("{0}|{1}",funcName,message);
            Task.WaitAll(FormatMessageAndWriteASync("TRACE", funcName, message, willBeLoggedToUi));
        }

        public static void DebugEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Debug("{0}|{1}", funcName, message);

            Task.WaitAll(FormatMessageAndWriteASync("DEBUG", funcName, message, willBeLoggedToUi));

        }

        public static void WarnEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Warn("{0},{1}",funcName, message);

            Task.WaitAll(FormatMessageAndWriteASync("WARNING", funcName, message, willBeLoggedToUi));
        }

        public static void ErrorEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Error("{0}|{1}", funcName, message);
            Task.WaitAll(FormatMessageAndWriteASync("ERROR", funcName, message, willBeLoggedToUi));
        }

        public static void FatalEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Fatal("{0}|{1}", funcName, message);

            Task.WaitAll(FormatMessageAndWriteASync("FATAL", funcName, message, willBeLoggedToUi));
        }
        public static void InfoEx(this Logger log, string funcName, string message, bool willBeLoggedToUi = true)
        {
            log.Info("{0}|{1}", funcName, message);
            
            Task.WaitAll(FormatMessageAndWriteASync("INFO", funcName, message, willBeLoggedToUi));
        }

        private static async Task FormatMessageAndWriteASync(string level, string functionName, string message, bool willBeLoggedToUi)
        {
            const string separator = ";;";
            
            if (!willBeLoggedToUi)
            {
                return;
            }

            if (Connection != null)
            {
                await Connection.WriteAsync("LOGMESSAGE" +
                separator + level +
                separator + functionName +
                separator + DateTime.Now.ToString("dd.MM.yy hh:mm:ss.fff",CultureInfo.InvariantCulture) +
                separator + message, CancellationToken.None);
            }
        }
    }
}
