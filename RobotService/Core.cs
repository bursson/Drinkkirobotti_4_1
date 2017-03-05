using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using NLog;
using ServiceLayer;
using BusinessLogic = LogicLayer.LogicLayer;
using DA = DataAccess.DataAccess;
using RobotCell = RobotCellLayer.RobotCellLayer;
using CommService = ServiceLayer.ServiceLayer;

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
            await DataAccess.DataAccess.InitializeDB();

            var operatorTask = OperatorConnection.Run(ct);
            var delayTask = Task.Delay(-1, ct);
            // Test.
            var bll = new BusinessLogic(new CommService(), new RobotCell(), new DA());

            var done = await Task.WhenAny(operatorTask, delayTask);
            ct.ThrowIfCancellationRequested();

        }
    }
}
