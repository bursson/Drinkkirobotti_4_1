using NLog;

namespace Common
{
    /// <summary>
    /// Public static class to extend functionality of the NLog-library.
    /// Simplifies the calling syntax and enables the implemenation for TCP logging in the future.
    /// </summary>
    public static class NLogExtensions
    { 
        public static void TraceEx(this Logger log, string funcName, string message)
        {
            log.Trace("{0}|{1}",funcName,message);
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
        public static void InfoEx(this Logger log, string funcName, string message)
        {
            log.Info("{0}|{1}", funcName, message);
        }
    }
}
