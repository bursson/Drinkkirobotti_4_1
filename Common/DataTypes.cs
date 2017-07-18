using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using NLog;
using NLog.Layouts;
using SQLite;

namespace Common
{
    public class Activity
    {
        public string Data { get; private set; }
        public ActivityType Type { get; private set; }

        public Activity(ActivityType activityType, string data = null)
        {
            if (data != null)
            {
                Data = data;
            }

            Type = activityType;

        }

       
    }
    public class Bottle
    {
        public Bottle(string bottleName, int maxvolume = 100, int volume = 100, bool isalcoholic = true, int pourspeed = 1) 
        {
            Name = bottleName;
            MaxVolume = maxvolume;
            Volume = volume;
            IsAlcoholic = isalcoholic;
            PourSpeed = pourspeed;
        }

        // For sql
        public Bottle()
        {
        }

        [PrimaryKey, AutoIncrement]
        public int BottleId { get; set; }

        public string Name { get; set; }
        public int MaxVolume { get; private set; }
        public int Volume { get; set; }
        public bool IsAlcoholic { get; }
        public int PourSpeed { get; private set; }

        public bool Pour(int amount)
        {
            if (Volume < amount)
            {
                return false;
            }
            Volume -= amount;
            return true;
        }

    }

    public class Portion
    {
        public string Name;
        public int Amount;

        public Portion(string name, int amount)
        {
            if (name == null) throw new ArgumentNullException(nameof(name));
            this.Name = name;
            this.Amount = amount;
        }

        /// <summary>
        /// Check if the portion is valid
        /// </summary>
        /// <returns>True if the bottle has a name and amount is over zero</returns>
        public bool IsValid()
        {
            return Name.Length > 0 && Amount > 0;
        }
    }

    public class Drink
    {
        private readonly List<Portion> _portions;
        public string Name { get; set; }

        [PrimaryKey, AutoIncrement]
        public int DrinkId { get; set; }

        public Drink(string name)
        {
            Name = name;
            _portions = new List<Portion>();
        }

        /// <summary>
        /// Add a postion to the drink
        /// </summary>
        /// <param name="portion">The portion</param>
        /// <returns>Returns true if a new portion was added or edited</returns>
        public bool AddPortion(Portion portion)
        {
            if (portion == null) throw new ArgumentNullException(nameof(portion));
            for (var i = 0; i < _portions.Count; ++i)
            {
                if (_portions[i].Name != portion.Name) continue;
                if (_portions[i].Amount == portion.Amount)
                {
                    return false;
                }
                _portions[i] = portion;
                return true;
            }

            _portions.Add(portion);
            return true;
        }

        public bool RemovePortion(string bottlename)
        {
            for (int i = 0; i < _portions.Count; i++)
            {
                if (_portions[i].Name == bottlename)
                {
                    _portions.Remove(_portions[i]);
                    return true;
                }
            }
            return false;
        }

        public Portion[] GetPortions()
        {
            return _portions.ToArray();
        }

        /// <summary>
        /// Insert a new portion to the drink
        /// </summary>
        /// <param name="bottle">The bottle</param>
        /// <param name="amount">The amoount in cl</param>
        /// <returns>Returns true if a new portion was added or edited</returns>
        public bool AddPortion(string name, int amount)
        {
            return AddPortion(new Portion(name, amount));
        }

        public List<Portion> Portions() { return _portions;}
    }
    public class Order
    {
        private readonly OrderType _orderType;
        private Drink _drink;
        public int _howMany;
        

        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="orderType">Type of the order</param>
        /// <param name="id">Unique id for the order</param>
        /// <param name="howMany"></param>
        /// <param name="drink">If the order is a drink, pass it here</param>
        public Order(OrderType orderType, int id, int howMany, Drink drink = null)
        {
            _orderType = orderType;
            OrderId = id;
            _drink = drink;
            _howMany = howMany;
        }

        [PrimaryKey, AutoIncrement]
        public int OrderId { get; set; }

        public int DrinkId { get; set; }

        public OrderType GetOrderType()
        {
            return _orderType;
        }

        public Drink GetRecipe()
        {
            return _drink;
        }

        public List<Tuple<Bottle, int>> BottlesToUse { get; set; }


    }

    public class Bottleshelf
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        public Bottle[] Shelf { get; }

        /// <summary>
        /// Create a new bottle shelf
        /// </summary>
        /// <param name="size">Maximum amout of bottles in the shelf</param>
        public Bottleshelf(int size, int removelimit)
        {
            Shelf = new Bottle[size];
            _removelimit = removelimit;
        }

        // For sql
        public Bottleshelf()
        {

        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        private int _removelimit;

        /// <summary>
        /// Add a bottle to the shelf
        /// </summary>
        /// <param name="newbottle">Bottle to be added</param>
        /// <returns>True if bottle was added</returns>
        public bool AddBottle(Bottle newbottle)
        {
            if (newbottle == null) throw new ArgumentNullException(nameof(newbottle));
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] == null)
                {
                    Shelf[i] = newbottle;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Remove a bottle from the shelf
        /// </summary>
        /// <param name="bottle">Bottle to be removed</param>
        /// <returns>True if bottle was removed</returns>
        public bool RemoveBottle(Bottle bottle)
        {
            if (bottle == null) throw new ArgumentNullException(nameof(bottle));
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] == bottle)
                {
                    Shelf[i] = null;
                    return true;
                }

            }
            return false;
        }

        /// <summary>
        /// Search for a bottle
        /// </summary>
        /// <param name="bottlename">Name of the bottle</param>
        /// <returns>List of bottles with that name</returns>
        public List<Bottle> Find(string bottlename)
        {
            List<Bottle> res = new List<Bottle>();
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] != null && Shelf[i].Name == bottlename)
                {
                    res.Add(Shelf[i]);
                }
            }
            return res;
        }

        public int Find(int id)
        {
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] != null && Shelf[i].BottleId == id)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// Get the size of the shelf
        /// </summary>
        /// <returns>Size of the shelf</returns>
        public int Size(){ return Shelf.Length; }

      

        /// <summary>
        /// Amount of empty slots in the shelf
        /// </summary>
        /// <returns>Amount of empty slots in the shelf</returns>
        public int AvailableSlots()
        {
            int res = 0;
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] == null)
                {
                    ++res;
                }
            }
            return res;
        }

        /// <summary>
        /// How many drinks can be prepared with current shelf
        /// </summary>
        /// <param name="drink">Drink-object</param>
        /// <returns>Amount of drinks that can be produced</returns>
        public int AmountAvailable(Drink drink)
        {
            int result = 1000;
            foreach (Portion p in drink.GetPortions())
            {
                int portionAvailable = 0;
                foreach (Bottle b in Find(p.Name))
                {
                    // TODO: how do we handle removelimit?
                    int availableInThisBottle = (b.Volume - (_removelimit / 2)) / p.Amount;
                    portionAvailable += availableInThisBottle;
                }

                if (portionAvailable < result)
                {
                    result = portionAvailable;
                }
            }
            return result;
        }

        public List<Tuple<Bottle, int>> Reserve(Order order)
        {
            const string funcName = nameof(Reserve);

            // Should never happen, exception maybe?
            if (AmountAvailable(order.GetRecipe()) < order._howMany)
            {
                Log.ErrorEx(funcName, $"Not enough ingredients available for the drink being reserved in order {order.OrderId}");
                return null;
            }
            var result = new List<Tuple<Bottle, int>>();
            
            // Iterate through all portions in order
            foreach (Portion p in order.GetRecipe().Portions())
            {
                // Get all bottles with that name and iteratre through them until required amount is reached
                var amountToPour = p.Amount * order._howMany;
                var bottles = Find(p.Name);
                foreach (Bottle b in bottles)
                {
                    // Check how much liquid left in bottle TODO: How to handle removelimit
                    var available = (b.Volume - _removelimit / 2) / p.Amount;
                    if (available < order._howMany)
                    {
                        b.Volume -= available * p.Amount;
                        amountToPour -= available * p.Amount;
                        result.Add(new Tuple<Bottle, int>(b, available));
                    }
                    else
                    {
                        // Last bottle to consider, add to result and return
                        b.Volume -= amountToPour;
                        result.Add(new Tuple<Bottle, int>(b, order._howMany));
                        return result;
                    }
                }
                Log.FatalEx(funcName, $"No bottles or not enough bottles to reserve the order {order.OrderId}");
                throw new NotImplementedException();
            }
            Log.ErrorEx(funcName, $"No portions in the drink being reserved in order {order.OrderId}");
            return null;
        }

        /// <summary>
        /// Dereserve ingredients reserved by an order. Throws, if order bottles include a bottle which is not present
        /// in the recipe, or if the recipe includes multiple portions with same name.
        /// </summary>
        /// <param name="order"></param>
        /// <returns>True if successfull</returns>
        public bool Dereserve(Order order)
        {
            // Add the reserved amount back to the bottles reserved originaly by the order
            foreach (var b in order.BottlesToUse)
            {
                // Amount added is the amount of portions multiplied by the portion amount specified in the recipe
                b.Item1.Volume += b.Item2 * order.GetRecipe().Portions().Single(p => p.Name == b.Item1.Name).Amount;
            }
            return true;
        }
    }
}
