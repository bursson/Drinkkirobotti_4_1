﻿using System;
using System.Threading;
using System.Threading.Tasks;
using NLog;
using Common;
using NLog.Layouts;
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
        private CancellationTokenSource _readLoopCts;
        private CancellationTokenSource _linkedCts;
        private CancellationTokenSource _currentTaskCt;

        private ActivityQueue _activityQueue;
        public OrderQueue Queue { get; private set; }
        public Bottleshelf CurrentShelf { get; private set; }
        private Bottleshelf _reservedShelf;
        private Bottle _currentBottle;
        private Task _currentTask;

        private bool _beer;
        private bool _drinks;
        private bool _sparkling;

        private bool _partialOrderFullfill;

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

        /// <summary>
        /// Initializes the LogicLayer
        /// </summary>
        /// <param name="args">Initialize parameters</param>
        /// <param name="ct">Cancels the initializion</param>
        /// <returns></returns>
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

            // Additional arguments

            _partialOrderFullfill = args.FullfillOrdersPartially;

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

        /// <summary>
        /// Run the logic loop
        /// </summary>
        /// <param name="ct">Cancels the loop, exits when possible</param>
        /// <returns></returns>
        public async Task Run(CancellationToken ct)
        {
            // TODO: Implement logic loop here with tons of private functions.
            _readLoopCts = new CancellationTokenSource();
            _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct,_readLoopCts.Token);
            _readDataMsgLoop = ReadFrontEndMessageLoop(_linkedCts.Token); // Start to listen for data messages.
            _systemState = State.Idle;
            _currentTaskCt = new CancellationTokenSource();
            // Loop while cancellation is not requested
            while (!ct.IsCancellationRequested)
            {
                // If current task is not finished skip the rest of the loop
                if (!_currentTask.IsCompleted)
                {
                    continue;
                }
                // Current activity is finished, get a new one
                var nextActivity = _activityQueue.Pop();
                _currentTaskCt = new CancellationTokenSource();

                switch (nextActivity.Type)
                {
                    // TODO: Idle, Idledemo and Macro
                    case ActivityType.ProcessOrders:
                        _currentTask = ProcessOrders(_currentTaskCt.Token);
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
            // Cancellation requested for the logic, tell current task
            // to exit. Wait for it for controlled exit
            _currentTaskCt.Cancel();
            await _currentTask;
        }
        /// <summary>
        /// Stops the LogicLayer.
        /// </summary>
        /// <returns></returns>
        public async Task Stop()
        {
            const string funcName = nameof(Stop);
            Log.InfoEx(funcName, "Stopping LogicLayer.");
            
            _currentTaskCt.Cancel();
            _readLoopCts.Cancel(); // Stop read data loop.
            await _readDataMsgLoop; // wait until loop has ended.

        }
        public void Dispose()
        {
            const string fName = nameof(Dispose);
            Log.InfoEx(fName, "Disposing LogicLayer");            
            // TODO Dispose stuff here if needed, serialConnections etc.
            // End message read loop if not ended already.
            if(!_readLoopCts.IsCancellationRequested) _readLoopCts.Cancel();
            _readLoopCts.Dispose();
            _linkedCts.Dispose();
        }

        /// <summary>
        /// Grab a bottle
        /// </summary>
        /// <param name="name">Name of the desired bottle</param>
        /// <returns></returns>
        private async Task GrabBottle(string name)
        {
            if (_systemState != State.Idle) throw new StateViolationException();
            _systemState = State.GrabBottle;
            var bottle = CurrentShelf.Find(name)[0];
            var location = CurrentShelf.Find(bottle.BottleId);
            Log.InfoEx("GrabBottle", $"Grabbing bottle {bottle.Name} with ID: {bottle.BottleId} from location {location}");
            await _robot.GrabBottle(location);
            _currentBottle = bottle;
        }

        /// <summary>
        /// Insert a new bottle in to the system
        /// </summary>
        /// <param name="bottle">Bottle information</param>
        /// <returns></returns>
        private async Task GetNewBottle(Bottle bottle)
        {
            if (_systemState != State.Idle) throw new StateViolationException();
            if (CurrentShelf.AvailableSlots() < 1) 
            _systemState = State.GetNewBottle;
            CurrentShelf.AddBottle(bottle);
            var location = CurrentShelf.Find(bottle.BottleId);
            Log.InfoEx("GetNewBottle", $"Getting a new bottle {bottle.Name} with ID: {bottle.BottleId} to location {location}");
            await _robot.GetNewBottle(location);
            _systemState = State.Idle;

        }

        /// <summary>
        /// Return the bottle in hand to the shelf
        /// </summary>
        /// <param name="bottle">Bottle information</param>
        /// <returns></returns>
        private async Task ReturnBottle(Bottle bottle)
        {
            if (_systemState != State.PourDrinks) throw new StateViolationException();
            var location = CurrentShelf.Find(bottle.BottleId);
            _systemState = State.ReturnBottle;
            Log.InfoEx("ReturnBottle", $"Returning {bottle.Name} with ID: {bottle.BottleId} to location {location}");
            await _robot.ReturnBottle(location);
            _currentBottle = null;
            _systemState = State.Idle;
        }

        /// <summary>
        /// Remove the bottle in hand from the system
        /// </summary>
        /// <param name="bottle">Bottle information</param>
        /// <returns></returns>
        private async Task RemoveBottle(Bottle bottle)
        {
            if (!(_systemState == State.PourSparkling || _systemState == State.PourDrinks)) throw new StateViolationException();
            _systemState = State.ReturnBottle;
            Log.InfoEx("RemoveBottle", $"Removing {bottle.Name} with ID: {bottle.BottleId}");
            await _robot.RemoveBottle();
            if (bottle.Name != "Sparkling")
            {
                CurrentShelf.RemoveBottle(bottle);
            }
            _currentBottle = null;
            _systemState = State.Idle;
        }

        /// <summary>
        /// Pour from the bottle in hand
        /// </summary>
        /// <param name="bottle">Bottle information</param>
        /// <param name="amount">Pour amount (cl)</param>
        /// <param name="howMany">How many pours</param>
        /// <returns></returns>
        private async Task PourBottle(Bottle bottle, int amount, int howMany)
        {
            if (_systemState != State.GrabBottle) throw new StateViolationException();
            _systemState = State.PourDrinks;
            // Check how many portions are left in current bottle
            // TODO: Is the safety value OK?
            int leftInBottle = (bottle.Volume - REMOVELIMIT / 2) / amount;
            // Not enough portions in current bottle alone
            if (leftInBottle < howMany)
            {
                Log.InfoEx("PourBottle", $"Not enough {bottle.Name} in ID: {bottle.BottleId}, pouring only {leftInBottle} portions from {howMany}");
                await _robot.PourBottle(amount / bottle.PourSpeed, leftInBottle);
                howMany -= leftInBottle;
                bottle.Volume -= amount * leftInBottle;
                await RemoveBottle(bottle);
                // TODO: Error if no more bottles with same name
                await GrabBottle(bottle.Name);
                // Use recursion to go through bottles until all drinks poured
                await PourBottle(_currentBottle, amount, howMany);
            }
            else
            {
                Log.InfoEx("PourBottle", $"Pouring {howMany} portions {bottle.Name} from ID: {bottle.BottleId}");
                await _robot.PourBottle(amount / bottle.PourSpeed, howMany);
                bottle.Volume -= amount * howMany;
            }
           
        }

        /// <summary>
        /// Pour a drink
        /// </summary>
        /// <param name="drink">Drink information</param>
        /// <param name="howMany">How many drinks</param>
        /// <returns></returns>
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

        /// <summary>
        /// Process orders from the _orderqueue
        /// </summary>
        /// <param name="ct">Token for cancelling the processing</param>
        /// <returns></returns>
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
        /// Loop for reading all data messages from serviceLayer. 
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        private async Task ReadFrontEndMessageLoop(CancellationToken ct)
        {
            const string funcName = nameof(ReadFrontEndMessageLoop);
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    Log.InfoEx(funcName, "Waiting for front-end data messages..");
                    var data = await _service.ReadDataAsync(ct);
                    Log.InfoEx(funcName, "Received data from front-end.");
                    await HandleDataMessage(data);
                }
            }
            catch (OperationCanceledException)
            {
                Log.InfoEx(funcName, "Read message loop was canceled.");
            }
            
        }
        /// <summary>
        /// Handles the read messages from ServiceLayer
        /// </summary>
        /// <param name="data"></param>
        private async Task<bool> HandleDataMessage(MessageData data)
        {
            const string funcName = nameof(HandleDataMessage);
            // TODO: Parallel message handling? (probably not a good idea)

            switch (data.Type)
            {
                case "Order":
                    // Response to front TODO: Response parameters
                    var response = new MessageData {OrderId = data.OrderId, OrderAmount =  data.OrderAmount};

                    // If ordertype = Drink
                    var orderedDrink = ParseDrink(data.Recipe);
                    var newOrder = new Order(OrderType.Drink, data.OrderId, data.OrderAmount, orderedDrink);

                    // Check if enough ingredients
                    var amountAvailable = _reservedShelf.AmountAvailable(orderedDrink);
                    if (amountAvailable < data.OrderAmount)
                    {
                        // If partial order fullfillment is not allowed or no ingredients left
                        // for a drink send a failure and return
                        if (!_partialOrderFullfill || amountAvailable <= 0)
                        {
                            response.Type = "FAILURE";
                            response.ErrorMsg = "Not enough ingredients";
                            return await _service.WriteAsync(new CancellationToken(), response, false);
                        }

                        // Partial order fullfillment is allowed and there are ingredients to atleast 1 drink
                        newOrder._howMany = amountAvailable;
                        response.Type = "ERROR";
                        response.ErrorMsg = $"Only {amountAvailable} drinks added to queue";
                        Log.DebugEx(funcName,
                            $"Adding a partial order {newOrder.OrderId} to queue with {amountAvailable} drinks out of ordered {data.OrderAmount}");
                    }
                    else
                    {
                        // Full amount available, everything is OK
                        response.Type = "OK";
                        Log.DebugEx(funcName, $"Adding order {newOrder.OrderId} to queue with {data.OrderAmount} drinks");
                    }

                    // Reserve the required ingredients and store the bottles to the order
                    newOrder.BottlesToUse = _reservedShelf.Reserve(newOrder);

                    // Check that the recipe was valid
                    if (newOrder.BottlesToUse == null)
                    {
                        response.Type = "FAILURE";
                        response.ErrorMsg = "Invalid drink recipe";
                        return await _service.WriteAsync(new CancellationToken(), response, false);
                    }

                    // Add to queue TODO: How to determine priority
                    if (Queue.Add(newOrder, 100))
                    {
                        return await _service.WriteAsync(new CancellationToken(), response,
                            response.Type == "OK");
                    }

                    // Could not add to queue, send and log error
                    response.Type = "FAILURE";
                    response.ErrorMsg = "Could not add to queue";
                    Log.ErrorEx(funcName, $"Error in adding order {newOrder.OrderId} to queue");
                    return await _service.WriteAsync(new CancellationToken(), response, false);

                case "Config":
                    // TODO: Handle configure messages
                    throw new NotImplementedException();

                default:
                    Log.ErrorEx(funcName, $"Unknown MessageData Type \"{data.Type}\" ");
                    return false;
            }

            
        }

        private Drink ParseDrink(MessageDataPortion[] msg)
        {
            var result = new Drink("Drink");
            foreach (MessageDataPortion p in msg)
            {
                result.AddPortion(p.DrinkName, p.Volume);
            }
            return result;
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
        public bool FullfillOrdersPartially { get; set; }
    }


}
