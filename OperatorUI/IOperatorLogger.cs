using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OperatorUI
{
    interface IOperatorLogger
    {
        void WriteLine(LogOutput newOutput);
    }
}
