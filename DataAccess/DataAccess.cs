using System;
using System.CodeDom;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
using NLog;

namespace DataAccess
{
    public class DataAccess
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ServiceLayer.ServiceLayer _service;

        public DataAccess(ServiceLayer.ServiceLayer serviceLayer = null)
        {
           _service = serviceLayer;
        }


        /// <summary>
        /// Check if database and items exist. If not, 
        /// </summary>
        /// <returns></returns>
        public static async Task InitializeDB()
        {
            if (isDBExists()) return;

            Log.InfoEx(nameof(InitializeDB), "No database found, initializing:");

            // If database doesnt exist, create entities etc
            var db = new SQLiteAsyncConnection(DatabaseInformation.DATABASE_NAME);

            // TODO: transform to generic foreach iteration for all DataTypes items
            // For example with reflection
            await db.CreateTableAsync<Bottleshelf>();
            Log.InfoEx(nameof(InitializeDB), "Table " + nameof(Bottleshelf) + " created");

            await db.CreateTableAsync<Bottle>();
            Log.InfoEx(nameof(InitializeDB), "Table " + nameof(Bottle) + " created");
        }

        private static bool isDBExists()
        {
            // File exists
            if (!File.Exists(DatabaseInformation.DATABASE_NAME)) return false;
            var db = new SQLiteConnection(DatabaseInformation.DATABASE_NAME);

            // Entity tables exists
            // TODO: generic foreach
            var tb1 = db.GetTableInfo("BottleShelf");
            var tb2 = db.GetTableInfo("Bottle");
            if (tb1.Count == 0 || tb2.Count == 0) return false;

            return true;
        }

        public static async Task AddAnotherJalluToDBTest()
        {
            var db = new SQLiteAsyncConnection(DatabaseInformation.DATABASE_NAME);
            Bottle bottle = new Bottle("Jallu");

            await db.InsertAsync(bottle);
            Log.InfoEx(nameof(AddAnotherJalluToDBTest), "New Bottle added: " + bottle.Name);
        }
    }

    public static class DatabaseInformation
    {
        /// <summary>
        /// TODO: move to app.config when structure is clear
        /// </summary>
        public const string DATABASE_NAME = "test.db";

    }
}
