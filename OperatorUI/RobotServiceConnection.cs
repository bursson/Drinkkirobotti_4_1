using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace OperatorUI
{
    public static class RobotServiceConnection
    {
        public static event Action OnPong;
        public static event Action<string> OnLog;

        private static readonly Server LogServer = new Server(9999, "\r\n", true, nameof(RobotServiceConnection));
        private static readonly Client ClientConnection = new Client(IPAddress.Parse("127.0.0.1"), 7676, "\r\n", true, nameof(RobotServiceConnection));

        public static async Task Run(CancellationToken ct)
        {
            var connTask = ClientConnection.Run(ct);
            var serverTask = LogServer.Run(ct);

            var handleClient = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await ClientConnection.GetConnectedAsync(ct);
                    await HandleConnection(ct);
                }
            }, ct);

            var handleLog = Task.Run(async () =>
            {
                while (!ct.IsCancellationRequested)
                {
                    await LogServer.GetConnectedAsync(ct);
                    await HandleLogConnection(ct);
                }
            }, ct);

            await Task.WhenAll(handleClient, handleLog);

            ct.ThrowIfCancellationRequested();
        }

        public static async Task Ping(CancellationToken ct)
        {
            await ClientConnection.WriteAsync("PING", ct);
        }

        private static async Task HandleLogConnection(CancellationToken ct)
        {
            string msg;
            while (!ct.IsCancellationRequested && (msg = await LogServer.ReadAsync(ct)) != null)
            {
                OnLog?.Invoke(msg);
            }
        }

        private static async Task HandleConnection(CancellationToken ct)
        {
            string msg;
            while (!ct.IsCancellationRequested && (msg = await ClientConnection.ReadAsync(ct)) != null)
            {
                HandleMessage(msg);
            }
        }

        private static void HandleMessage(string msg)
        {
            if (msg.Equals("PONG", StringComparison.Ordinal))
            {
                OnPong?.Invoke();
            }
        }
    }
}
