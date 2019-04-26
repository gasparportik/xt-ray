/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using XtRay.ParserLib;

namespace XtRay.Windows
{
    public partial class ParamsDialog : Window
    {
        private List<PHPValue> Params = new List<PHPValue>();
        public ParamsDialog(string[] parameters)
        {
            InitializeComponent();
            foreach (string parameter in parameters)
            {
                Params.Add(new PHPValue(parameter));
            }
            ParamTextBox.Text = string.Join("\n",Params);
            PreviewKeyDown += Dialog_KeyDown;
        }
        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
            // TODO : make another way to parse phpValue
            if (e.Key == Key.F)
            {
                foreach (PHPValue param in Params)
                {
                    param.Parse();
                }
                ParamTextBox.Text = string.Join("\n",Params);
            }
        }
    }
}
