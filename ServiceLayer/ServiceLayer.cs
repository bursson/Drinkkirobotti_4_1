using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceLayer
{
    public class ServiceLayer
    {
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
