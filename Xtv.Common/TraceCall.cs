using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xtv.Common
{
    public class TraceCall
    {
        public string Name { get; private set; }

        public TraceCall(string call)
        {
            Name = call;
        }
    }
}
