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

        public static async Task InitializeDB()
        {
            if(await isDBExists()) return;

            // If database doesnt exist, create entities etc
            var db = new SQLiteAsyncConnection(DatabaseInformation.DATABASE_NAME);
            await db.CreateTableAsync<TestTable>();

            Log.InfoEx(nameof(InitializeDB), "Table created");

            TestTable testTable = new TestTable()
            {
                Content = "Im a test insert. Try to query me!"
            };

            await db.InsertAsync(testTable);
            Log.InfoEx(nameof(InitializeDB), "New customer ID: " + testTable.Id);
        }

        private static async Task<bool> isDBExists()
        {
            // File exists
            if (!File.Exists(DatabaseInformation.DATABASE_NAME)) return false;
            var db = new SQLiteAsyncConnection(DatabaseInformation.DATABASE_NAME);

            // Entity tables exists
            // TODO:
            var tb1 = await db.Table<TestTable>().ToListAsync();
            if (tb1.Count == 0) return false;

            return true;
        }
    }

    public static class DatabaseInformation
    {
        /// <summary>
        /// TODO: move to app.config when structure is clear
        /// </summary>
        public const string DATABASE_NAME = "test.db";

    }


    public class TestTable
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(8)]
        public string Content { get; set; }
    }
}
