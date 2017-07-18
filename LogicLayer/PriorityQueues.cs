using System;
using System.Collections.Generic;
using Common;

namespace LogicLayer
{
    public class OrderQueue : List<Tuple<Order, int>>
    {
        public bool Add(Order neworder, int priority)
        {
            if (neworder == null) throw new ArgumentNullException(nameof(neworder));
            if (priority < 0) throw new ArgumentOutOfRangeException(nameof(priority));
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Item1.OrderId == neworder.OrderId)
                {
                    return false;
                }
            }
            Add(new Tuple<Order, int>(neworder, priority));
            return true;
        }

        /// <summary>
        /// Remove order with the given id from the queue
        /// </summary>
        /// <param name="id">Id of the order to remove</param>
        /// <returns>The removed order. If not found returns null</returns>
        public Order Remove(int id)
        {
            foreach (var i in this)
            {
                if (i.Item1.OrderId == id)
                {
                    if (Remove(i))
                    {
                        return i.Item1;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Remove the given order from the queue
        /// </summary>
        /// <param name="order"></param>
        /// <returns>True if successfull</returns>
        public bool Remove(Order order)
        {
            foreach (var i in this)
            {
                if (i.Item1 == order)
                {
                    return Remove(i);
                }
            }
            return false;
        }


        /// <summary>
        /// Find all orders cointaining the given bottle
        /// </summary>
        /// <param name="bottle"></param>
        /// <returns>List of Tuples containing orders and their priority</returns>
        public List<Tuple<Order, int>> Find(Bottle bottle)
        {
            var result = new List<Tuple<Order, int>>();
            foreach (var i in this)
            {
                foreach (var p in i.Item1.BottlesToUse)
                {
                    if (p.Item1 == bottle)
                    {
                        result.Add(i);
                        break;
                    }
                }
            }
            return result;
        }
        
        public Order Pop()
        {
            var maxpriority = -1;
            Tuple<Order, int> result = null;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Item2 > maxpriority)
                {
                    result = this[i];
                    maxpriority = this[i].Item2;
                }else if (this[i].Item2 == maxpriority && result != null && result.Item1.OrderId > this[i].Item1.OrderId)
                {
                    result = this[i];
                }
            }
            if (result == null) return null;
            Remove(result);
            return result.Item1;
        }
    }

    /// <summary>
    /// Class for storing what kind of activities should be done
    /// </summary>
    public class ActivityQueue : List<Tuple<Activity, int>>
    {
        public Activity DefaultActivity { get; private set; }
        /// <summary>
        /// Construct a new Activity Queue
        /// </summary>
        /// <param name="defaultActivity">What should be done if the queue is empty</param>
        public ActivityQueue(Activity defaultActivity)
        {
            DefaultActivity = defaultActivity;
        }
        public bool Add(Activity newaction, int priority)
        {
            if (priority < 0) throw new ArgumentOutOfRangeException(nameof(priority));
            if (newaction == null) throw new ArgumentNullException(nameof(newaction));
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Item1.Type != newaction.Type || this[i].Item1.Data != newaction.Data) continue;
                if (this[i].Item2 == priority) return false;
            }
            Add(new Tuple<Activity, int>(newaction, priority));
            return true;
        }

        public Activity Pop()
        {
            var maxpriority = -1;
            Tuple<Activity,int> result = null;
            for (int i = 0; i < Count; i++)
            {
                if (this[i].Item2 <= maxpriority) continue;
                result = this[i];
                maxpriority = this[i].Item2;
            }

            if (result == null) return DefaultActivity;
            Remove(result);
            return result.Item1;
        }
    }
}
