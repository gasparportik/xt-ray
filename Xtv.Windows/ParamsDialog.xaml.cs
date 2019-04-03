using System.Windows;

namespace Xtv.Windows
{
    public partial class ParamsDialog : Window
    {
        public ParamsDialog(string[] parameters)
        {
            InitializeComponent();
            ParamTextBox.Text = string.Join("\r\n", parameters);
        }
    }
}
