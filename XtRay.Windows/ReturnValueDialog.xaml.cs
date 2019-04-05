using System.Windows;

namespace XtRay.Windows
{
    public partial class ReturnValueDialog : Window
    {
        public ReturnValueDialog(string parameters)
        {
            InitializeComponent();
            ReturnValueTextBox.Text = parameters;
        }
    }
}
