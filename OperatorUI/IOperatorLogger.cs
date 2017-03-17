using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorUI
{
    public interface IOperatorLogger
    {
        void AddLogOutput(LogOutput newOutput);
    }
}
