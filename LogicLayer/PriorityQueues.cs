using System;
using System.Collections.Generic;

namespace LogicLayer
{
    public class OrderQueue : List<Tuple<Order, int>>
    {
        public bool Add(Order neworder, int priority)
        {
            if (neworder == null) throw new ArgumentNullException(nameof(neworder));
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Item1.GetId() == neworder.GetId())
                {
                    return false;
                }
            }
            Add(new Tuple<Order, int>(neworder, priority));
            return true;
        }
        
        public Order Pop()
        {
            var maxpriority = -1;
            Tuple<Order, int> result = null;
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].Item2 > maxpriority)
                {
                    result = this[i];
                    maxpriority = this[i].Item2;
                }else if (this[i].Item2 == maxpriority && result != null && result.Item1.GetId() > this[i].Item1.GetId())
                {
                    result = this[i];
                }
            }
            if (result == null) return null;
            this.Remove(result);
            return result.Item1;
        }
    }
}
