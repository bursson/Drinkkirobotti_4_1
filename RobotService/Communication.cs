using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace RobotService
{
    interface IDeviceComm
    {
        Task<bool> SendCommand(string command);
    }

    class DummyComm : IDeviceComm
    {
        public async Task<bool> SendCommand(string command)
        {
            await Task.Delay(1000);
            return true;
        }
    }

    class SerialComm : IDeviceComm
    {
        private const string Endsign = "!";
        private const string Startedsign = ";STARTED";
        private const string Completionsign = ";COMPLETE";
        private const int Abnormalwait = 60000;

        private static SerialPort _serialPort;

        /// <summary>
        /// Class for simple serial communication
        /// </summary>
        /// <param name="portname">Port for the communication ex. "COM3"</param>
        /// <param name="baud">Baudrate for communication "9600"</param>
        /// <param name="readtimeout">Max time to completion, 0 for infinite wait</param>
        public SerialComm(string portname, int baud, int readtimeout, int writetimeout)
        {
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = portname;
            _serialPort.BaudRate = baud;
            _serialPort.Parity = _serialPort.Parity;
            _serialPort.DataBits = _serialPort.DataBits;
            _serialPort.StopBits = _serialPort.StopBits;
            _serialPort.Handshake = _serialPort.Handshake;

            // Set the read/write timeouts
            _serialPort.ReadTimeout = readtimeout > 0 ? readtimeout : SerialPort.InfiniteTimeout;
            _serialPort.WriteTimeout = writetimeout;

            _serialPort.Open();
        }

        ~SerialComm()
        {
            _serialPort.Close();
        }        

        public async Task<bool> SendCommand(string command)
        {
            try
            {
                _serialPort.WriteLine(command);
            }
            catch (TimeoutException)
            {
                // log this
                return false;
            }

            var readTask = Read();
            var expected = command + Startedsign;
            await readTask;

            if (readTask.Result != expected)
            {
                // throw something about robot not responding / false response
            }

            expected = command + Endsign;
            readTask = Read();

            await readTask;
            return readTask.Result == expected;
        }

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
                    if (x is TimeoutException)
                    {
                        // Read timeout occured, log it
                        return true;
                    }
                    return false;
                });

                return null;
            }
        }
    }
}
