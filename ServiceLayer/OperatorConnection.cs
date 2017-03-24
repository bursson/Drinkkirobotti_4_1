using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Common;
using NLog;

namespace ServiceLayer
{
    public static class OperatorConnection
    {
        private static readonly Server ServerConnection = new Server(7676, "\r\n", true, nameof(OperatorConnection));

        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Run(CancellationToken ct)
        {
            var connTask = ServerConnection.Run(ct);

            while (!ct.IsCancellationRequested)
            {
                await ServerConnection.GetConnectedAsync(ct);
                await Log.DebugEx(nameof(Run), "Connected");
                await HandleConnection(ct);
            }
            ct.ThrowIfCancellationRequested();
        }

        private static async Task HandleConnection(CancellationToken ct)
        {
            string msg;
            while (!ct.IsCancellationRequested && (msg = await ServerConnection.ReadAsync(ct)) != null)
            {
                await HandleMessage(msg, ct);
            }
        }

        private static async Task HandleMessage(string msg, CancellationToken ct)
        {
            if (msg.Equals("PING", StringComparison.Ordinal))
            {
                await ServerConnection.WriteAsync("PONG", ct);
            }
        }
    }
}
