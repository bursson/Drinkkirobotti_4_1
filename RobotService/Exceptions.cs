using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace RobotService
{
    internal class SerialCommException : Exception
    {
        private readonly string _portname;
        private readonly string _message;

        public SerialCommException(string portname, string message)
        {
            _message = message;
            _portname = portname;
        }

        public override string Message => $"Portname {_portname}: {_message}";
    }
}
