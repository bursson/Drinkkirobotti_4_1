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
        public static event Action<string> OnInfo;

        private static readonly Client Connection = new Client(IPAddress.Parse("127.0.0.1"), 7676, "\r\n", true, nameof(RobotServiceConnection));

        public static async Task Run(CancellationToken ct)
        {
            var connTask = Connection.Run(ct);

            while (!ct.IsCancellationRequested)
            {
                await Connection.GetConnectedAsync(ct);
                await HandleConnection(ct);
            }
            ct.ThrowIfCancellationRequested();
        }

        public static async Task Ping(CancellationToken ct)
        {
            await Connection.WriteAsync("PING", ct);
        }

        private static async Task HandleConnection(CancellationToken ct)
        {
            string msg;
            while (!ct.IsCancellationRequested && (msg = await Connection.ReadAsync(ct)) != null)
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

            if (msg.StartsWith("INFO", StringComparison.Ordinal))
            {
                OnInfo?.Invoke(msg);
            }
        }
    }
}
