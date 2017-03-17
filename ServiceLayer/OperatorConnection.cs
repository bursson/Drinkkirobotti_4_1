using System;
using System.Threading;
using System.Threading.Tasks;
using Common;
using NLog;

namespace ServiceLayer
{
    public static class OperatorConnection
    {
        private static readonly Server Connection = new Server(7676, "\r\n", true, nameof(OperatorConnection));

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Run(CancellationToken ct)
        {
            NLogExtensions.Connection = Connection;

            var connTask = Connection.Run(ct);

            while (!ct.IsCancellationRequested)
            {
                await Connection.GetConnectedAsync(ct);
                await Log.InfoEx(nameof(Run), "Connected");
                await HandleConnection(ct);
            }
            ct.ThrowIfCancellationRequested();
        }

        private static async Task HandleConnection(CancellationToken ct)
        {
            string msg;
            while (!ct.IsCancellationRequested && (msg = await Connection.ReadAsync(ct)) != null)
            {
                await HandleMessage(msg, ct);
            }
        }

        private static async Task HandleMessage(string msg, CancellationToken ct)
        {
            if (msg.Equals("PING", StringComparison.Ordinal))
            {
                await Connection.WriteAsync("PONG", ct);
            }
        }
    }
}
