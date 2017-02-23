using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace RobotService
{
    public static class OperatorConnection
    {
        private static readonly Server Connection = new Server(7676, "\r\n", true, nameof(OperatorConnection));

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
