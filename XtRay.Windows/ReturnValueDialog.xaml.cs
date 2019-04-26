/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Input;
using XtRay.ParserLib;

namespace XtRay.Windows
{
    public partial class ReturnValueDialog : Window
    {
        private PHPValue returnValue;
        public ReturnValueDialog(string parameters)
        {
            InitializeComponent();
            returnValue = new PHPValue(parameters);
            ReturnValueTextBox.Text = returnValue.ToString();
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
                returnValue.Parse();
                ReturnValueTextBox.Text = returnValue.ToString();
            }
        }
    }
}
