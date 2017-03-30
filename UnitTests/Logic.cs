using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using LogicLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;



namespace LogicTest
{
    [TestClass]
    public class LogicLayerTest
    {
        private static ServiceLayer.ServiceLayer ser;
        private static RobotCellLayer.RobotCellLayer rob;
        private static DataAccess.DataAccess da;

        private LogicLayer.LogicLayer logic;

        [TestInitialize]
        public void InitClasses()
        {
            ser = new ServiceLayer.ServiceLayer();
            rob = new RobotCellLayer.RobotCellLayer();
            da = new DataAccess.DataAccess();
            rob.AddRobot("SIM", "SIM");
            logic = new LogicLayer.LogicLayer(ser, rob, da);

            var shelf = new Bottleshelf(10);
            shelf.AddBottle(new Bottle("Vodka"));
            shelf.AddBottle(new Bottle("Vesi"));
            shelf.AddBottle(new Bottle("Mehu"));
            var queue = new OrderQueue();

            var kv = new Drink("Kossuvissy");
            Assert.IsTrue(kv.AddPortion("Vodka", 4));
            Assert.IsTrue(kv.AddPortion("Vesi", 10));

            var mehu = new Drink("Mehu");
            Assert.IsTrue(mehu.AddPortion("Mehu", 10));

            queue.Add(new Tuple<Order, int>(new Order(OrderType.Drink, 1, 1, kv), 10));
            queue.Add(new Tuple<Order, int>(new Order(OrderType.Drink, 2, 4, mehu), 10));
            queue.Add(new Tuple<Order, int>(new Order(OrderType.Drink, 3, 3, kv), 10));
            queue.Add(new Tuple<Order, int>(new Order(OrderType.Drink, 4, 1, kv), 10));

            var startarg = new StartArguments();
            startarg.BackupShelf = shelf;
            startarg.Mode = RunMode.Simulation;
            startarg.Beer = false;
            startarg.Drinks = true;
            startarg.Sparkling = false;
            startarg.IdleActivity = new Activity(ActivityType.ProcessOrders);
            startarg.BacckupQueue = queue;

            var init = Task.Run(() => logic.Initialize(startarg, new CancellationToken()));
            init.Wait();
            Assert.IsTrue(init.Result);
        }

        [TestMethod]
        public void LogicTest()
        {   
            var ct = new CancellationTokenSource();
            var runtask = Task.Run(() => logic.Run(ct.Token));
            while (logic.Queue.Count > 0)
            {
                Task.Delay(100).Wait();
            }
            ct.Cancel();
            runtask.Wait();
            
            Assert.AreEqual(0, logic.Queue.Count);
            //Assert.AreEqual(80, logic.CurrentShelf.Find("Mehu").Volume);
            Assert.AreEqual(80, logic.CurrentShelf.Find("Vodka").Volume);
            //Assert.IsNull(logic.CurrentShelf.Find("Mehu"));
            Assert.AreEqual(60, logic.CurrentShelf.Find("Vesi").Volume);
            logic.Dispose();
        }
    }
}
