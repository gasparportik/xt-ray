/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System;
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
