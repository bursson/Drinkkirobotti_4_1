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
        public async Task<Data> ReadDataAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<Config> ReadConfigAsync(CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteAsync(CancellationToken ct, Data data)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> WriteAsync(CancellationToken ct, Config config)
        {
            throw new NotImplementedException();
        }
    }
}
