using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RobotCellLayer
{
    public class RobotCellLayer
    {
        private IDeviceComm _robot;

        public RobotCellLayer()
        {
            _robot = null;
        }
        public bool AddRobot(string name, string portname)
        {
            if (portname.Contains("COM"))
            {
                try
                {
                    _robot = new SerialComm(portname, 0);
                }
                catch (Exception)
                {
                    return false;
                }
                return true;
            }else if (portname == "SIM")
            {
                _robot = new DummyComm();
                return true;
            }
            return false;
        }
        public async Task<bool> GrabBottle(int position, int bottletype = 0)
        {
            if (_robot == null)
            {
                throw new NullReferenceException("Robot is not defined");
            }
            return await _robot.SendCommand($"grabBottle({position},{bottletype})");
        }
        public async Task<bool> ReturnBottle(int position, int bottletype = 0)
        {
            if (_robot == null)
            {
                throw new NullReferenceException("Robot is not defined");
            }
            return await _robot.SendCommand($"returnBottle({position},{bottletype})");
        }
        public async Task<bool> GetNewBottle(int position, int bottletype = 0)
        {
            if (_robot == null)
            {
                throw new NullReferenceException("Robot is not defined");
            }
            return await _robot.SendCommand($"getNewBottle({position},{bottletype})");
        }
        public async Task<bool> RemoveBottle(int bottletype = 0)
        {
            if (_robot == null)
            {
                throw new NullReferenceException("Robot is not defined");
            }
            return await _robot.SendCommand($"removeBottle({bottletype})");
        }
        public async Task<bool> PourBottle(int amount, int howMany, int bottletype = 0)
        {
            if (_robot == null)
            {
                throw new NullReferenceException("Robot is not defined");
            }
            return await _robot.SendCommand($"pourDrinks({amount},{howMany},{bottletype})");
        }
    }
}
