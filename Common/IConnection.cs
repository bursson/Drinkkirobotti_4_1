using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Common
{
    public interface IConnection
    {
        Task<bool> WriteAsync(string msg, CancellationToken ct);
        Task<string> ReadAsync(CancellationToken ct);
        Task Run(CancellationToken ct);
    }
}
