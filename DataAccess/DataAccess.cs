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
