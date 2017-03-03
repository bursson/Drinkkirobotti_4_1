using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Common;
using CommService = ServiceLayer.ServiceLayer; // Set custom names for the classes to make the code less filled with dots.
using RobotCell = RobotCellLayer.RobotCellLayer; //
using DA = DataAccess.DataAccess; //

namespace LogicLayer
{
    public class LogicLayer : IDisposable
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private readonly CommService _service; // ServiceLayer.
        private readonly RobotCell _robot; // RobotCellLayer
        private readonly DA _da; // DataAccess.

        /// <summary>
        /// Creates a new LogicLayer.
        /// </summary>
        /// <param name="sl">ServiceLayer component.</param>
        /// <param name="r">RobotCellLayer component.</param>
        /// <param name="da">DataAccess component.</param>
        public LogicLayer(CommService sl, RobotCell r, DA da)
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
