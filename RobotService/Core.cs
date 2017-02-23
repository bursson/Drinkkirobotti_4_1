using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using NLog;

namespace RobotService
{
    public static class Core
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Run(CancellationTokenSource cts)
        {
            const string fname = nameof(Run);
            try
            {
                await MainLogic(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.DebugEx(fname, "Canceled");
            }
            catch (Exception e)
            {
                Log.FatalEx(fname, e.ToString());
                cts.Cancel();
                Environment.Exit(1);
            }
        }

        private static async Task MainLogic(CancellationToken ct)
        {
            // Main logic here
            var operatorTask = OperatorConnection.Run(ct);
            var delayTask = Task.Delay(-1, ct);

            var done = await Task.WhenAny(operatorTask, delayTask);
            ct.ThrowIfCancellationRequested();
        }
    }
}
