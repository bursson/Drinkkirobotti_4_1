using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Common;

namespace LogicLayer
{
    public class LogicLayer : IDisposable
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ServiceLayer.ServiceLayer _service;
        private readonly RobotCellLayer.RobotCellLayer _robot;
        private readonly DataAccess.DataAccess _da;

        public LogicLayer(ServiceLayer.ServiceLayer sl, RobotCellLayer.RobotCellLayer r, DataAccess.DataAccess da)
        {
            const string fName = nameof(LogicLayer);
            Log.DebugEx(fName, "Construction BusinessLogic layer..");
            _service = sl;
            _robot = r;
            _da = da;
        }

        public async Task<bool> Initialize(StartArguments args, CancellationToken ct)
        {
            bool result = Convert.ToBoolean(new Random(Convert.ToInt32(DateTime.Now.Second))); // lul.
            string errorMsg = string.Empty;
            const string fName = nameof(Initialize);
            Log.InfoEx(fName, "Initializing LogicLayer");
            // TODO: Set settings from given arguments.
            // And set result based on that.
            if (result)
            {
                Log.InfoEx(fName, "Initialized LogicLayer successfully");
                return true;
            }
            else
            {
                Log.ErrorEx(fName,$"Initializing LogicLayer failed:{Environment.NewLine}{errorMsg}");
                return false;
            }
        }

        public async Task Run(CancellationToken ct)
        {
            // TODO: Implement logic loop here with tons of private functions.
        }
        public void Dispose()
        {
            const string fName = nameof(Dispose);
            Log.InfoEx(fName, "Disposing LogicLayer");
            // TODO Dispose stuff here if needed, serialConnections etc.
        }
    }

    public class StartArguments
    {
        // TODO: Write required starting arguments here.
        public RunMode Mode { get; private set; }
    }


}
