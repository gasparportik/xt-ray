using System.Windows;
using System.Windows.Input;

namespace XtRay.Windows
{
    public partial class ReturnValueDialog : Window
    {
        public ReturnValueDialog(string returnValue)
        {
            InitializeComponent();
            ReturnValueTextBox.Text = returnValue;
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
