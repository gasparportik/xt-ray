/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.IO;

namespace XtRay.ParserLib.Parsers
{
    public class FileParser : StreamParser
    {
        public string Filename { get; private set; }

        public FileParser(string file) : base(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
        {
            if (!File.Exists(file))
            {
                throw new IOException("The specified file does not exist: " + file + "!");
            }
            Filename = file;
        }
    }
}
