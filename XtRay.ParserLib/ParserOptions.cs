/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

namespace XtRay.ParserLib
{
    public class ParserOptions
    {
        public bool Parallel { get; set; } = true;
        public byte Parallelism { get; set; } = 2;
        public bool Experimental { get; set; } = false;
        public bool IgnoreInternal { get; set; } = false;
        public bool ParseParameters { get; set; } = false;
        public bool ParseReturnValue { get; set; } = false;
        public bool ParseAsTree { get; set; } = true;
    }
}
