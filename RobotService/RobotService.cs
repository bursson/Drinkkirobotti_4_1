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
    public class RobotService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();
        private Task _task;

        public void Start()
        {
            _task = Core.Run(_cts);
        }

        public void Stop()
        {
            const string fname = nameof(Stop);
            try
            {
                _cts.Cancel();
                _task.GetAwaiter().GetResult();
                Log.DebugEx(fname, "Service stopped");
            }
            catch (Exception e)
            {
                Log.FatalEx(fname, e.ToString());
            }
        }
    }
}
