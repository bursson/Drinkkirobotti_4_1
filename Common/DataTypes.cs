using System;
using System.Collections.Generic;
using System.Security.AccessControl;
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
        public Bottle Bottle;
        public int Amount;

        public Portion(Bottle bottle, int amount)
        {
            if (bottle == null) throw new ArgumentNullException(nameof(bottle));
            this.Bottle = bottle;
            this.Amount = amount;
        }
        
        /// <summary>
        /// Check if the portion is valid
        /// </summary>
        /// <returns>True if the bottle has a name and amount is over zero</returns>
        public bool IsValid()
        {
            return  Bottle.Name.Length > 0 && Amount > 0;
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
                if (_portions[i].Bottle.Name != portion.Bottle.Name) continue;
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
                if (_portions[i].Bottle.Name == bottlename)
                {
                    _portions.Remove(_portions[i]);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Insert a new portion to the drink
        /// </summary>
        /// <param name="bottle">The bottle</param>
        /// <param name="amount">The amoount in cl</param>
        /// <returns>Returns true if a new portion was added or edited</returns>
        public bool AddPortion(Bottle bottle, int amount)
        {
            return AddPortion(new Portion(bottle, amount));
        }

        public List<Portion> Portions() { return _portions;}
    }
    public class Order
    {
        private readonly OrderType _orderType;
        private Drink _drink;

        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="orderType">Type of the order</param>
        /// <param name="id">Unique id for the order</param>
        /// <param name="drink">If the order is a drink, pass it here</param>
        public Order(OrderType orderType, int id, Drink drink = null)
        {
            _orderType = orderType;
            _drink = drink;
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

    }

    public class Bottleshelf
    {
        public Bottle[] Shelf { get; }

        /// <summary>
        /// Create a new bottle shelf
        /// </summary>
        /// <param name="size">Maximum amout of bottles in the shelf</param>
        public Bottleshelf(int size)
        {
            Shelf = new Bottle[size];
        }

        // For sql
        public Bottleshelf()
        {
            
        }

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

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
        /// <param name="bottlename">Name of the bottle to be removed</param>
        /// <returns>True if bottle was removed</returns>
        public bool RemoveBottle(string bottlename)
        {
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] != null && Shelf[i].Name == bottlename)
                {
                    Shelf[i] = null;
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
        /// <returns>The Bottle object if found, else null</returns>
        public Bottle Find(string bottlename)
        {
            for (int i = 0; i < Size(); i++)
            {
                if (Shelf[i] != null && Shelf[i].Name == bottlename)
                {
                    return Shelf[i];
                }
            }
            return null;
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


    }
}
