using System;
using System.IO.Ports;
using System.Threading.Tasks;
using NLog;
using Common;

namespace RobotCellLayer
{
    interface IDeviceComm
    {
        Task<bool> SendCommand(string command);
    }

    class DummyComm : IDeviceComm
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public DummyComm()
        {
            Log.Info("Using virtual serial port. All actions succeed after a second");
        }
        public async Task<bool> SendCommand(string command)
        {
            Log.Debug($"Virtual serial: Excecuting command: {command}");
            await Task.Delay(1000);
            return true;
        }
   
    }

    class SerialComm : IDeviceComm
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const string Endsign = "!";
        private const string Startedsign = ";STARTED";
        private const string Completionsign = ";COMPLETE";
        private const int Abnormalwait = 60000;

        private static SerialPort _serialPort;

        /// <summary>
        /// Class for simple serial communication
        /// </summary>
        /// <param name="portname">Port for the communication ex. "COM3"</param>
        /// <param name="baud">Baudrate for communication ex. "9600"</param>
        /// <param name="readtimeout">Max time to completion, 0 for infinite wait</param>
        /// <param name="writetimeout">Max time for writing, default 500</param>
        public SerialComm(string portname, int readtimeout, int writetimeout = 500, int baud = 9600)
        {
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = portname;
            _serialPort.BaudRate = baud;
           
            // Set the read/write timeouts
            _serialPort.ReadTimeout = readtimeout > 0 ? readtimeout : SerialPort.InfiniteTimeout;
            _serialPort.WriteTimeout = writetimeout;

            _serialPort.Open();
        }

        ~SerialComm()
        {
            _serialPort.Close();
        }        

        /// <summary>
        /// Send a command to a serial device. Expects a confirmation for starting and
        /// finishing the action.
        /// </summary>
        /// <param name="command"></param>
        /// <returns>True if action was performed correctly, false if could not write to serial</returns>
        public async Task<bool> SendCommand(string command)
        {
            try
            {
                _serialPort.WriteLine(command);
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (!(x is TimeoutException)) return false;
                    Log.Error("Writing " + command + " to serial " + _serialPort.PortName + " timed out");
                    return true;
                });

                return false;
            }

            var readTask = Read();
            var expected = command + Startedsign;
            await readTask;

            if (readTask.Result != expected)
            {
                Log.Fatal(_serialPort.PortName + $" : Got:\"{readTask.Result}\", expected: \" {expected}\"");
                throw new SerialCommException(_serialPort.PortName,$"Got:\"{readTask.Result}\", expected: \" {expected}\"");
            }

            expected = command + Endsign;
            readTask = Read();

            await readTask;

            if (readTask.Result == expected) return true;

            Log.Fatal(_serialPort.PortName + $" : Got:\"{readTask.Result}\", expected: \" {expected}\"");
            throw new SerialCommException(_serialPort.PortName, $"Got:\"{readTask.Result}\", expected: \" {expected}\"");
        }

        /// <summary>
        /// Read serial until the designated Endsign.
        /// </summary>
        /// <returns>String read. Null if timeout occured</returns>
        private static async Task<string> Read()
        {
            try
            {
               var read = Task.Run(() => _serialPort.ReadTo(Endsign));
               await read;
               return read.Result;
            }
            catch (AggregateException e)
            {
                e.Handle((x) =>
                {
                    if (!(x is TimeoutException)) return false;
                    Log.Error("Read from serial" + _serialPort.PortName + "timed out");
                    return true;
                });

                return null;
            }
        }
    }
}
