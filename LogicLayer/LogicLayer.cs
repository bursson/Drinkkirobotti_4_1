using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using Common;
using System.Threading.Tasks;
using ServiceLayer;

namespace LogicLayer
{
    public class LogicLayer : IDisposable
    {
        private static Logger Log = LogManager.GetCurrentClassLogger();

        private readonly ServiceLayer.ServiceLayer _service;
        private readonly RobotCellLayer.RobotCellLayer _robot;
        private readonly DataAccess.DataAccess _da;

        public LogicLayer(ServiceLayer.ServiceLayer sl, RobotCellLayer.RobotCellLayer r, DataAccess.DataAccess da)
        {
            const string fName = nameof(LogicLayer);
            Log.DebugEx(fName, "Construction BusinessLogic layer..");
            _service = sl;
            _robot = r;
            _da = da;
        }

        public bool Initialize(StartArguments args)
        {
            // TODO: Set settings from given arguments.
            // And return based on that.
            return true;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class StartArguments
    {
        // TODO:
        public RunMode Mode { get; private set; }
        
    }


}
