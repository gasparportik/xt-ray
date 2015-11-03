/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Linq;

namespace Xtv.Parser
{
    public abstract class Parser
    {
        protected IList<Trace> _data;
        public Trace RootTrace
        {
            get
            {
                return _data.First();
            }
        }

        protected Parser()
        {

        }

        public abstract string GetInfo();

        /// <summary>
        /// Tries to parse the specified call trace file
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        /// <exception cref="ParserException">Thrown if the specified file has invalid format and therefore can't be parsed</exception>
        public static Parser ParseFile(string file)
        {
            var parser = new FileParser(file);
            parser.Parse();
            return parser;
        }
    }
}
