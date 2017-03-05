using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;
using DataAccess;
using LogicLayer;
using NLog;
using ServiceLayer;
using BusinessLogic = LogicLayer.LogicLayer;
using DA = DataAccess.DataAccess;
using RobotCell = RobotCellLayer.RobotCellLayer;
using CommService = ServiceLayer.ServiceLayer;

namespace RobotService
{
    public static class Core
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public static async Task Run(CancellationTokenSource cts)
        {
            const string fname = nameof(Run);
            try
            {
                await MainLogic(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Log.DebugEx(fname, "Canceled");
            }
            catch (Exception e)
            {
                Log.FatalEx(fname, e.ToString());
                cts.Cancel();
                Environment.Exit(1);
            }
        }

        private static async Task MainLogic(CancellationToken ct)
        {
            // Main logic here
            await InitializeDB();
            await AddAnotherJalluToDBTest();

            var operatorTask = OperatorConnection.Run(ct);
            var delayTask = Task.Delay(-1, ct);
            // Test.
            var bll = new BusinessLogic(new CommService(), new RobotCell(), new DA());

            var done = await Task.WhenAny(operatorTask, delayTask);
            ct.ThrowIfCancellationRequested();

        }

        /// <summary>
        /// Check if database and items exist. If not, 
        /// </summary>
        /// <returns></returns>
        private static async Task InitializeDB()
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

        private static async Task AddAnotherJalluToDBTest()
        {
            var db = new SQLiteAsyncConnection(DatabaseInformation.DATABASE_NAME);
            Bottle bottle = new Bottle("Jallu");

            await db.InsertAsync(bottle);
            Log.InfoEx(nameof(AddAnotherJalluToDBTest), "New Bottle added: " + bottle.Name);
        }
    }
}
