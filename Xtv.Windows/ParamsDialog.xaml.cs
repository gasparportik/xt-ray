using System.Windows;

namespace Xtv.Windows
{
    public partial class ParamsDialog : Window
    {
        public ParamsDialog(string[] parameters)
        {
            InitializeComponent();
            TextBox.Text = string.Join("\r\n", parameters);
        }
    }
}
