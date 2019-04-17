using System.Windows;
using System.Windows.Input;

namespace XtRay.Windows
{
    public partial class ParamsDialog : Window
    {
        public ParamsDialog(string[] parameters)
        {
            InitializeComponent();
            ParamTextBox.Text = string.Join("\r\n", parameters);
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
