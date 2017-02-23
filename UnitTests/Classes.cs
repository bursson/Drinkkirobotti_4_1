using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RobotService;


namespace RobotService
{
    [TestClass]
    [SuppressMessage("ReSharper", "SuggestVarOrType_SimpleTypes")]
    public class UnitTest1
    {
        private List<Bottle> _bottleshelf;
        private List<Drink> _drinkdb;
        [TestInitialize]
        public void InitClasses()
        {
            _bottleshelf = new List<Bottle>(); //TODO: replace with actual bottleshelf
            _drinkdb = new List<Drink>();

        }

        [TestMethod]
        public void TestPortion()
        {
            Bottle justABottle = new Bottle("something");
            Portion invalidPortion = new Portion(justABottle, 0);
            Assert.IsFalse(invalidPortion.IsValid());
            invalidPortion.Bottle.Name = "";
            invalidPortion.Amount = 10;
            Assert.IsFalse(invalidPortion.IsValid());

            Portion validPortion = new Portion(justABottle, 2);
            Assert.IsTrue(validPortion.IsValid());


        }
        [TestMethod]
        public void TestOrder()
        {
            var justADrink = new Drink("some");
            var justAOrder = new Order(OrderType.Drink, 1, justADrink);
            Assert.AreEqual(Order.GetId(), 1, "OrderID");
            Assert.AreEqual(Order.GetOrderType(), OrderType.Drink, "Ordertype");
            justADrink.AddPortion(new Portion(new Bottle("a"), 3));
            justAOrder = new Order(OrderType.Drink, 2, justADrink);
            Assert.AreEqual(Order.GetRecipe(), justADrink, "Getrecipe");


        }
        [TestMethod]
        public void TestClasses()
        {
            for (int i = 0; i < 10; i++)
            {
                _bottleshelf.Add(new Bottle("Bottle" + i));
                Assert.AreEqual(_bottleshelf.Count, i + 1, "Bottle" + i);
            }
            for (int i = 0; i < 10; i++)
            {
                _drinkdb.Add(new Drink("Testdrink" + i));
                for (var index = 0; index < _bottleshelf.Count; index++)
                {
                    if (index < 3)
                    {
                        Bottle t = _bottleshelf[index];
                        _drinkdb[i].AddPortion(t, i);
                    }
                    else
                    {
                        Portion newPortion = new Portion(_bottleshelf[index], i);
                        _drinkdb[i].AddPortion(newPortion);
                    }

                }
            }
            for (var i = 0; i < _bottleshelf.Count; ++i)
            {
                Assert.AreEqual(_drinkdb[i].Name, "Testdrink" + i, $"Drinkname {_drinkdb[i].Name}");
                for (int j = 0; j < _bottleshelf.Count; j++)
                {
                    Assert.AreEqual(_drinkdb[i].Portions()[j].Bottle, _bottleshelf[j], $"Bottlename {_bottleshelf[j].Name}");
                    Assert.AreEqual(_drinkdb[i].Portions()[j].Amount, i, $"BottleAmount {_bottleshelf[j].Name}");
                }


            }

        }
    }
}
