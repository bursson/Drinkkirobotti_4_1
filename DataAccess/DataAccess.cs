using System;
using System.Collections.Generic;
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
            var connection = new SQLiteAsyncConnection("test.db");

            await connection.CreateTableAsync<Stock>();

            Log.InfoEx(nameof(InitializeDB), "Table created");

            Stock stock = new Stock()
            {
                Symbol = "NOK"
            };

            await connection.InsertAsync(stock);
            Log.InfoEx(nameof(InitializeDB), "New customer ID: " + stock.Id);
        }
    }



    public class Stock
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [MaxLength(8)]
        public string Symbol { get; set; }
    }

    public class Valuation
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Indexed]
        public int StockId { get; set; }
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
    }
}
