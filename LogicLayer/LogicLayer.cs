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

        private State _systemState;

        private readonly CommService _service; // ServiceLayer.
        private readonly RobotCell _robot; // RobotCellLayer
        private readonly DA _da; // DataAccess.

        private ActivityQueue _activityQueue;
        private OrderQueue _orderQueue;
        private Bottleshelf _currentShelf;
        private Bottleshelf _reservedShelf;

        private bool _beer;
        private bool _drinks;
        private bool _sparkling;

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
            _systemState = State.Initialize;
            Log.InfoEx(fName, "Initializing LogicLayer");
            // TODO: Set settings from given arguments.
            // And set result based on that.

            // Set the idle-activity
            _activityQueue = args.IdleActivity != null ? new ActivityQueue(args.IdleActivity) : new ActivityQueue(new Activity(ActivityType.Idle));

            // Load bottleshelf backup if provided, otherwise presume it's empty
            _currentShelf = args.BackupShelf != null
                ? (_reservedShelf = args.BackupShelf)
                : (_reservedShelf = new Bottleshelf());

            _orderQueue = new OrderQueue();

            // What services are available
            _beer = args.Beer;
            _drinks = args.Drinks;
            _sparkling = args.Sparkling;

            if (result)
            {
                Log.InfoEx(fName, "Initialized LogicLayer successfully");
                return true;
            }
            else
            {
                _systemState = State.InitFailure;
                Log.ErrorEx(fName,$"Initializing LogicLayer failed:{Environment.NewLine}{errorMsg}");
                return false;
            }
        }

        public async Task Run(CancellationToken ct)
        {
            // TODO: Implement logic loop here with tons of private functions.
            _systemState = State.Idle;
        }
        public void Dispose()
        {
            const string fName = nameof(Dispose);
            Log.InfoEx(fName, "Disposing LogicLayer");
            // TODO Dispose stuff here if needed, serialConnections etc.
        }

        private async Task GrabBottle(string name)
        {
            if (_systemState != State.Idle) throw new StateViolationException();
            _systemState = State.GrabBottle;
            var bottle = _currentShelf.Find(name);
            var location = _currentShelf.Find(bottle.BottleId);
            Log.InfoEx("GrabBottle", $"Grabbing bottle {bottle.Name} with ID: {bottle.BottleId} from location {location}");
            // Run IComm task
        }

        private async Task GetNewBottle(Bottle bottle)
        {
            if (_systemState != State.Idle) throw new StateViolationException();
            if (_currentShelf.AvailableSlots() < 1) 
            _systemState = State.GetNewBottle;
            Log.InfoEx("GetNewBottle", $"Getting a new bottle {bottle.Name} with ID: {bottle.BottleId}");
            // Run IComm task
            _currentShelf.AddBottle(bottle);
        }

        private async Task returnBottle(Bottle bottle)
        {
            if (_systemState != State.PourDrinks) throw new StateViolationException();
            var location = _currentShelf.Find(bottle.BottleId);
            _systemState = State.ReturnBottle;
            Log.InfoEx("returnBottle", $"Returning {bottle.Name} with ID: {bottle.BottleId} to location {location}");
            // Run IComm task
            _systemState = State.Idle;
        }

        private async Task removeBottle(Bottle bottle)
        {
            if (_systemState == State.PourSparkling || _systemState == State.PourDrinks) throw new StateViolationException();
            _systemState = State.ReturnBottle;
            Log.InfoEx("removeBottle", $"Removing {bottle.Name} with ID: {bottle.BottleId}");
            // Run IComm task
            if (bottle.Name != "Sparkling")
            {
                _currentShelf.RemoveBottle(bottle);
            }
            _systemState = State.Idle;
        }

    }

    public class StartArguments
    {
        // TODO: Write required starting arguments here.
        public RunMode Mode { get; private set; }
        public bool Beer { get; private set; }
        public bool Drinks { get; private set; }
        public bool Sparkling { get; private set; }
        public Bottleshelf BackupShelf { get; private set; }
        public Activity IdleActivity { get; private set; }
    }


}
