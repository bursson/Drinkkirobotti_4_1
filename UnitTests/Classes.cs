using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Common;
using LogicLayer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotService;
using LogicTest;


namespace UnitTests
{
    [TestClass]
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    public class UnitTest1
    {
        private List<Bottle> _bottleshelf;
        private List<Drink> _drinkdb;
        private Bottleshelf _bs;
        private OrderQueue _queue;
        private ActivityQueue _activity;
             
        private int shelfsize = 10;

        [TestInitialize]
        public void InitClasses()
        {
            _bottleshelf = new List<Bottle>(); //TODO: replace with actual bottleshelf
            _drinkdb = new List<Drink>();
            _bs = new Bottleshelf(shelfsize);
            _queue = new OrderQueue();
            _activity = new ActivityQueue(new Activity(ActivityType.Idle));
        }

        [TestMethod]
        public void TestBottleshelf()
        {
            Assert.AreEqual(_bs.Size(), shelfsize, "Bottleshelf size");
            Assert.AreEqual(_bs.AvailableSlots(), shelfsize, "Bottleshelf available shelfsize");
            for (int i = 0; i < shelfsize; i++)
            {
                _bs.AddBottle(new Bottle("Testbottle" + i));
                Assert.AreEqual(_bs.AvailableSlots(), shelfsize-1-i);
            }
            Assert.IsFalse(_bs.AddBottle(new Bottle("fail")), "Bottleshelf overflow");
            Assert.IsTrue(_bs.RemoveBottle("Testbottle5"));
            Assert.AreEqual(_bs.AvailableSlots(), 1);
            Assert.IsTrue(_bs.RemoveBottle("Testbottle0"));
            Assert.IsTrue(_bs.RemoveBottle("Testbottle9"));
            Assert.AreEqual(_bs.AvailableSlots(), 3);
            Assert.IsNull(_bs.Find("Testbottle"));
            var checkBottle = _bs.Find("Testbottle3");
            Assert.AreEqual(100, checkBottle.MaxVolume);
            Assert.AreEqual(checkBottle.Name,"Testbottle3");

            var shelf = _bs.Shelf;
            Assert.AreEqual(shelf.Length, shelfsize, "Shelf length");
            Assert.AreEqual(shelf[0], null, "Shelf0");
            Assert.AreEqual(shelf[1].Name, "Testbottle1", "Shelf1");
            Assert.AreEqual(shelf[2].Name, "Testbottle2", "Shelf2");
            Assert.AreEqual(shelf[3].Name, "Testbottle3", "Shelf3");
            Assert.AreEqual(shelf[4].Name, "Testbottle4", "Shelf4");
            Assert.AreEqual(shelf[5], null, "Shelf5");
            Assert.AreEqual(shelf[6].Name, "Testbottle6", "Shelf6");
            Assert.AreEqual(shelf[7].Name, "Testbottle7", "Shelf7");
            Assert.AreEqual(shelf[8].Name, "Testbottle8", "Shelf8");
            Assert.AreEqual(shelf[9], null, "Shelf9");

        }
        [TestMethod]
        public void TestPortion()
        {
            Bottle justABottle = new Bottle("something");
            Portion invalidPortion = new Portion("something", 0);
            Assert.IsFalse(invalidPortion.IsValid(), "Invalid portion 1");
            justABottle.Name = "something";
            Portion validPortion = new Portion("something", 2);
            Assert.IsTrue(validPortion.IsValid(), "Valid portion 1");


        }
        [TestMethod]
        public void TestOrder()
        {
            var justADrink = new Drink("some");
            var justAOrder = new Order(OrderType.Drink, 1, 1, justADrink);
            Assert.AreEqual(justAOrder.OrderId, 1, "OrderID");
            Assert.AreEqual(justAOrder.GetOrderType(), OrderType.Drink, "Ordertype");
            justADrink.AddPortion(new Portion("a", 3));
            justAOrder = new Order(OrderType.Drink, 2, 1, justADrink);
            Assert.AreEqual(justAOrder.GetRecipe(), justADrink, "Getrecipe");

            justADrink.AddPortion(new Portion("b", 10));
            Assert.IsFalse(justADrink.RemovePortion("c"));
            Assert.AreEqual(justADrink.Portions().Count, 2);
            Assert.IsTrue(justADrink.RemovePortion("b"));
            Assert.AreEqual(justADrink.Portions().Count, 1);
            Assert.IsTrue(justADrink.RemovePortion("a"));
            Assert.AreEqual(justADrink.Portions().Count, 0);
        }
        [TestMethod]
        public void TestClasses()
        {
            for (int i = 0; i < shelfsize; i++)
            {
                _bottleshelf.Add(new Bottle("Bottle" + i));
                Assert.AreEqual(_bottleshelf.Count, i + 1, "Bottle" + i);
            }
            for (int i = 0; i < shelfsize; i++)
            {
                _drinkdb.Add(new Drink("Testdrink" + i));
                for (var index = 0; index < _bottleshelf.Count; index++)
                {
                    if (index < 3)
                    {
                        Bottle t = _bottleshelf[index];
                        _drinkdb[i].AddPortion(t.Name, i);
                    }
                    else
                    {
                        Portion newPortion = new Portion(_bottleshelf[index].Name, i);
                        _drinkdb[i].AddPortion(newPortion);
                    }

                }
            }

            for (var i = 0; i < _bottleshelf.Count; ++i)
            {
                Assert.AreEqual(_drinkdb[i].Name, "Testdrink" + i, $"Drinkname {_drinkdb[i].Name}");
                for (int j = 0; j < _bottleshelf.Count; j++)
                {
                    Assert.AreEqual(_drinkdb[i].Portions()[j].Name, _bottleshelf[j].Name, $"Bottlename {_bottleshelf[j].Name}");
                    Assert.AreEqual(_drinkdb[i].Portions()[j].Amount, i, $"BottleAmount {_bottleshelf[j].Name}");
                }


            }

        }

        [TestMethod]
        public void Orderqueue()
        {
            Assert.AreEqual(_queue.Count, 0, "Queuecount1");
            Assert.IsTrue(_queue.Add(new Order(OrderType.Sparkling, 1, 1), 10));
            Assert.AreEqual(_queue.Count, 1, "Queuecount2");
            Assert.IsTrue(_queue.Add(new Order(OrderType.Beer, 2, 1), 88));
            Assert.AreEqual(_queue.Count, 2, "Queuecount3");
            Assert.IsTrue(_queue.Add(new Order(OrderType.Drink, 3, 1, new Drink("A")), 10));
            Assert.IsTrue(_queue.Add(new Order(OrderType.Drink, 0, 1, new Drink("B")), 10));

            Assert.AreEqual(_queue.Pop().GetOrderType(), OrderType.Beer, "Queue1" );
            Assert.AreEqual(_queue.Pop().GetRecipe().Name, "B", "Queue2");
            Assert.AreEqual(_queue.Pop().GetOrderType(), OrderType.Sparkling, "Queue3");
            Assert.AreEqual(_queue.Pop().GetRecipe().Name, "A", "Queue4");
            for (int i = 0; i < 20; i++)
            {
                Assert.IsTrue(_queue.Add(new Order(OrderType.Sparkling, i, 1), 10));
            }
            for (int i = 0; i < 20; i++)
            {
                Assert.AreEqual(i, _queue.Pop().OrderId);
            }
            Assert.IsNull(_queue.Pop());

            try
            {
                _queue.Add(new Order(OrderType.Beer, 2, 1), -1);
                Assert.Fail("Expected a ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {

            }
        }


        [TestMethod]
        public void Activityqueue()
        {
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 10));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 11));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 12));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.Idle), 10));
            Assert.IsFalse(_activity.Add(new Activity(ActivityType.ProcessOrders), 10));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.Macro, "asd"), 10));
        
            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual(ActivityType.Idle, _activity.Pop().Type);
            Assert.AreEqual(ActivityType.Macro, _activity.Pop().Type);
            Assert.AreEqual(0, _activity.Count);

            _activity = new ActivityQueue(new Activity(ActivityType.Macro, "testing"));
            Assert.AreEqual("testing", _activity.Pop().Data);
            Assert.AreEqual("testing", _activity.Pop().Data);
            Assert.AreEqual(ActivityType.Macro, _activity.Pop().Type);

            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 10));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 20));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 1000));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.ProcessOrders), 0));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.Macro, "first"), 15));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.Macro, "second"), 100));
            Assert.IsTrue(_activity.Add(new Activity(ActivityType.Idle), 100));

            Assert.AreEqual(7, _activity.Count);

            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual("second", _activity.Pop().Data);
            Assert.AreEqual(ActivityType.Idle, _activity.Pop().Type);
            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual("first", _activity.Pop().Data);
            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual(ActivityType.ProcessOrders, _activity.Pop().Type);
            Assert.AreEqual(0, _activity.Count);
            Assert.AreEqual("testing", _activity.Pop().Data);
            Assert.AreEqual("testing", _activity.Pop().Data);
            Assert.AreEqual("testing", _activity.Pop().Data);

            try
            {
                _activity.Add(new Activity(ActivityType.Idle), -5);
                Assert.Fail("Expected a ArgumentOutOfRangeException");
            }
            catch (ArgumentOutOfRangeException)
            {
                
            }




        }
    }
}
