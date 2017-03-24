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

        private Task _readDataMsgLoop; // Task for read data messages, should be canceled once disposed.

        private ActivityQueue _activityQueue;
        public OrderQueue Queue { get; private set; }
        public Bottleshelf CurrentShelf { get; private set; }
        private Bottleshelf _reservedShelf;
        private Bottle _currentBottle;
        private Task _currentTask;

        private bool _beer;
        private bool _drinks;
        private bool _sparkling;

        private static int REMOVELIMIT = 15;

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
            bool result = true;
            string errorMsg = string.Empty;
            const string fName = nameof(Initialize);
            _systemState = State.Initialize;
            Log.InfoEx(fName, "Initializing LogicLayer");
            // TODO: Set settings from given arguments.
            // And set result based on that.

            // Set the idle-activity
            _activityQueue = args.IdleActivity != null ? new ActivityQueue(args.IdleActivity) : new ActivityQueue(new Activity(ActivityType.Idle));

            // Load bottleshelf backup if provided, otherwise presume it's empty
            CurrentShelf = args.BackupShelf != null
                ? (_reservedShelf = args.BackupShelf)
                : (_reservedShelf = new Bottleshelf());

            Queue = args.BacckupQueue ?? new OrderQueue();

            // What services are available
            _beer = args.Beer;
            _drinks = args.Drinks;
            _sparkling = args.Sparkling;

            _currentTask = Task.Delay(1);

            // Initialize the robot, TODO: read port from cfg and throw if false
            switch (args.Mode)
            {
                case RunMode.Production:
                    _robot.AddRobot("ABB", "COM3");
                    break;
                case RunMode.Simulation:
                    _robot.AddRobot("SIMULATION", "SIM");
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"RunMode argument \"{args.Mode}\" out of range");
            }
            
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
            _readDataMsgLoop = ReadFrontEndMessageLoop(ct); // Start to listen for data messages.
            _systemState = State.Idle;
            var currentTaskCt = new CancellationTokenSource();
            while (!ct.IsCancellationRequested)
            {
                if (!_currentTask.IsCompleted)
                {
                    continue;
                }
                var nextActivity = _activityQueue.Pop();
                currentTaskCt = new CancellationTokenSource();
                switch (nextActivity.Type)
                {
                    case ActivityType.ProcessOrders:
                        _currentTask = ProcessOrders(currentTaskCt.Token);
                        break;
                    case ActivityType.Idle:
                        await Task.Delay(500,ct);
                        break;
                    case ActivityType.IdleDemo:
                        await Task.Delay(500,ct);
                        break;
                    case ActivityType.Macro:
                        await Task.Delay(500,ct);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            currentTaskCt.Cancel();
            await _currentTask;
        }
        public void Dispose()
        {
            const string fName = nameof(Dispose);
            Log.InfoEx(fName, "Disposing LogicLayer");            
            // TODO Dispose stuff here if needed, serialConnections etc.
            // TODO: End read message loop.
        }

        private async Task GrabBottle(string name)
        {
            if (_systemState != State.Idle) throw new StateViolationException();
            _systemState = State.GrabBottle;
            var bottle = CurrentShelf.Find(name);
            var location = CurrentShelf.Find(bottle.BottleId);
            Log.InfoEx("GrabBottle", $"Grabbing bottle {bottle.Name} with ID: {bottle.BottleId} from location {location}");
            await _robot.grabBottle(location);
            _currentBottle = bottle;
        }

        private async Task GetNewBottle(Bottle bottle)
        {
            if (_systemState != State.Idle) throw new StateViolationException();
            if (CurrentShelf.AvailableSlots() < 1) 
            _systemState = State.GetNewBottle;
            CurrentShelf.AddBottle(bottle);
            var location = CurrentShelf.Find(bottle.BottleId);
            Log.InfoEx("GetNewBottle", $"Getting a new bottle {bottle.Name} with ID: {bottle.BottleId} to location {location}");
            await _robot.getNewBottle(location);
            _systemState = State.Idle;

        }

        private async Task ReturnBottle(Bottle bottle)
        {
            if (_systemState != State.PourDrinks) throw new StateViolationException();
            var location = CurrentShelf.Find(bottle.BottleId);
            _systemState = State.ReturnBottle;
            Log.InfoEx("returnBottle", $"Returning {bottle.Name} with ID: {bottle.BottleId} to location {location}");
            await _robot.returnBottle(location);
            _currentBottle = null;
            _systemState = State.Idle;
        }

        private async Task RemoveBottle(Bottle bottle)
        {
            if (!(_systemState == State.PourSparkling || _systemState == State.PourDrinks)) throw new StateViolationException();
            _systemState = State.ReturnBottle;
            Log.InfoEx("removeBottle", $"Removing {bottle.Name} with ID: {bottle.BottleId}");
            await _robot.removeBottle();
            if (bottle.Name != "Sparkling")
            {
                CurrentShelf.RemoveBottle(bottle);
            }
            _currentBottle = null;
            _systemState = State.Idle;
        }

        private async Task PourBottle(Bottle bottle, int amount, int howMany)
        {
            if (_systemState != State.GrabBottle) throw new StateViolationException();
            _systemState = State.PourDrinks;
            // Log
            await _robot.pourBottle(amount / bottle.PourSpeed, howMany);
            bottle.Volume -= amount * howMany;
        }

        private async Task PourDrink(Drink drink, int howMany)
        {
            foreach (var portion in drink.Portions())
            {
                await GrabBottle(portion.Name);
                await PourBottle(_currentBottle, portion.Amount, howMany);
                if (_currentBottle.Volume < REMOVELIMIT)
                {
                    await RemoveBottle(_currentBottle);
                }
                else
                {
                    await ReturnBottle(_currentBottle);
                }
                
            }
        }

        private async Task ProcessOrders(CancellationToken ct)
        {
            if (Queue.Count < 1)
            {
                return;
                await Task.Delay(500, ct);
            }
            if (ct.IsCancellationRequested)
            {
                return;
            }
            while (Queue.Count > 0)
            {
                var currentOrder = Queue.Pop();
                Log.InfoEx("ProcessOrders", $"Started processing order {currentOrder.OrderId}");
                if (currentOrder.GetOrderType() == OrderType.Drink)
                {
                    await PourDrink(currentOrder.GetRecipe(), currentOrder._howMany);
                }
            }
        }

        /// <summary>
        /// Handles reading Data-message reading loop. 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadFrontEndMessageLoop(CancellationToken ct)
        {
            const string funcName = nameof(ReadFrontEndMessageLoop);
            while (!ct.IsCancellationRequested)
            {
                Log.InfoEx(funcName, "Waiting for front-end data messages..");
                var data = await _service.ReadDataAsync(ct);
                Log.InfoEx(funcName, "Received data from front-end.");
                HandleDataMessage(data);
            }
        }

        private void HandleDataMessage(MessageData data)
        {
            const string funcName = nameof(HandleDataMessage);
            // TODO: Handle MessageData object, add to queue etc.
            Log.DebugEx(funcName, $"Retrieved data object: {data}");
        }
    }

    public class StartArguments
    {
        // TODO: Write required starting arguments here.
        public RunMode Mode { get;  set; }
        public bool Beer { get; set; }
        public bool Drinks { get; set; }
        public bool Sparkling { get; set; }
        public Bottleshelf BackupShelf { get; set; }
        public Activity IdleActivity { get; set; }
        public OrderQueue BacckupQueue { get; set; }
    }


}
