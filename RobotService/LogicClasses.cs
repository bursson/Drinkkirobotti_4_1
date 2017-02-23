using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotService
{
    public struct Bottle
    {
        public string Name;

        public Bottle(string bottleName)
        {
            Name = bottleName;
        }
    }

    public struct Portion
    {
        public Bottle Bottle;
        public int Amount;

        public Portion(Bottle bottle, int amount)
        {
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
        private List<Portion> _portions;
        public string Name { get; set; }

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
        private static OrderType _orderType;
        private static int _id;
        private static Drink _drink;

        /// <summary>
        /// Create new order
        /// </summary>
        /// <param name="orderType">Type of the order</param>
        /// <param name="id">Unique id for the order</param>
        /// <param name="drink">If the order is a drink, pass it here</param>
        public Order(OrderType orderType, int id, Drink drink = null)
        {
            _orderType = orderType;
            _id = id;
            _drink = drink;
        }
        
        public static int GetId()
        {
            return _id;
        }

        public static OrderType GetOrderType()
        {
            return _orderType;
        }

        public static Drink GetRecipe()
        {
            return _drink;
        }

    }
}
