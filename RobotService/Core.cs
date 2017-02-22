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
            Log.DebugEx(fname, "Hello world");

            try
            {
                await MainLogic(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.DebugEx(fname, "Canceled");
            }
            catch (Exception)
            {
                cts.Cancel();
                throw;
            }
        }

        private static async Task MainLogic(CancellationToken ct)
        {
            // Main logic here
            await Task.Delay(-1, ct);
        }
    }
}
