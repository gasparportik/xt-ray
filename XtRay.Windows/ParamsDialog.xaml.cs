using System.Windows;

namespace XtRay.Windows
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
