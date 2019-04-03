using System.Windows;

namespace Xtv.Windows
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
