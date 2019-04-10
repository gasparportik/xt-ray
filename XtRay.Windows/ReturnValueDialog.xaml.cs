using System.Windows;

namespace XtRay.Windows
{
    public partial class ReturnValueDialog : Window
    {
        public ReturnValueDialog(string returnValue)
        {
            InitializeComponent();
            ReturnValueTextBox.Text = returnValue;
        }
    }
}
