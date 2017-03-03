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
            for (int i = 0; i < Count; i++)
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
