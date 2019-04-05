using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XtRay.Windows
{
    static class Extensions
    {
        public static EventHandler<T> Debounce<T>(this EventHandler<T> handler, int ms = 500)
        {
            var last = 0;
            return (s, e) =>
            {
                var current = Interlocked.Increment(ref last);
                Task.Delay(ms).ContinueWith(task =>
                {
                    if (current == last)
                    {
                        handler(s, e);
                        Interlocked.Exchange(ref last, 0);
                    }
                    task.Dispose();
                });
            };
        }
    }
}
