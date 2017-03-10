using System;

namespace Common
{
    public class SerialCommException : Exception
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

    public class StateViolationException : Exception
    {
        
    }
}
