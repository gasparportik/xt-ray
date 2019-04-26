/* This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/. */

using System.Windows;
using System.Windows.Input;

namespace XtRay.Windows
{
    public partial class ReturnValueDialog : Window
    {
        public ReturnValueDialog(string parameters)
        {
            InitializeComponent();
            ReturnValueTextBox.Text = parameters;
            PreviewKeyDown += Dialog_KeyDown;
        }
        private void Dialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
            }
        }
    }
}
