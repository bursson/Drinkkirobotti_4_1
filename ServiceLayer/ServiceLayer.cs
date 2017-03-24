using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Common;

namespace ServiceLayer
{
    public class ServiceLayer
    {
        // TODO: Set to appconfig or smth
        private const int FrontEndPort = 10001; // Change.
        private const int OpUIport = 7676;
        private const string EndOfMessage = "\r\n"; // TODO End of message-string
        private readonly Server _frontEndListener;
        private readonly Server _opUiListener;

        private Task _frontEndTask;
        private Task _opUiTask;

        public ServiceLayer()
        {
            _frontEndListener = new Server(FrontEndPort,EndOfMessage,true,"FrontEndServer");
            _opUiListener = new Server(OpUIport, EndOfMessage, true,"OperatorUIListener");
            
        }

        public void Run(CancellationToken ct)
        {
            _frontEndTask = _frontEndListener.Run(ct);
            _opUiTask = _opUiListener.Run(ct);
        }


        public async Task<MessageData> ReadDataAsync(CancellationToken ct)
        {
            // TODO ACtual implementation, await data messages and return them:
            await Task.Delay(10000, ct);
            return new MessageData
            {
                DummyDataElement = "DummyDataMessage"
            }; 

        }

        public async Task<MessageConfig> ReadConfigAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteAsync(CancellationToken ct, MessageData data)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteAsync(CancellationToken ct, MessageConfig config)
        {
            throw new NotImplementedException();
        }
    }
}
