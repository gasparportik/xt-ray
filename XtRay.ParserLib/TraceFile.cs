using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtRay.Common
{
    public class TraceFile
    {
        public string Path { get; private set; }

        public TraceFile(string path)
        {
            Path = path;
        }
    }
}
