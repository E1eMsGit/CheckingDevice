using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TK158.View
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            KeyDown += ShowHideTestMode;
        }

        private void AddressValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^0-1]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void BitsCountValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex(@"[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void ShowHideTestMode(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.L)
            {
                if (rbTestDevice.Visibility == Visibility.Visible)
                {
                    rbTestDevice.Visibility = Visibility.Hidden;
                }
                else
                {
                    rbTestDevice.Visibility = Visibility.Visible;
                }
            }
        }
    }
}
