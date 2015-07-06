using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GumShoe
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();
            SettingsStartUrl.Text = Settings.Default.StartingUrl;
            SettingsKeyword.Text = Settings.Default.Keyword;
            SettingsAttempts.Text = Settings.Default.MaxAttempts.ToString();
            SettingsSteps.Text = Settings.Default.Steps.ToString();
            SettingsSecondsDelay.Text = Settings.Default.SecondsDelay.ToString();
            DatabaseFile.Text = Settings.Default.DatabaseFile.ToString();
        }

        private void SettingsSave_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.StartingUrl = SettingsStartUrl.Text;
            Settings.Default.Keyword = SettingsKeyword.Text;
            Settings.Default.DatabaseFile = DatabaseFile.Text;
            // Numeric Fields
            Settings.Default.MaxAttempts = int.Parse(SettingsAttempts.Text);
            Settings.Default.Steps = int.Parse(SettingsSteps.Text);
            Settings.Default.SecondsDelay = int.Parse(SettingsSecondsDelay.Text);
            
            Settings.Default.Save();
            this.Close();
        }

        private void SettingsCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void txtNum_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox numericTextBox = (TextBox) sender;
            if (numericTextBox == null)
            {
                return;
            }
            int numValue;

            if (!int.TryParse(numericTextBox.Text, out numValue))
            {
                if (numValue < 0)
                {
                    numValue = Math.Abs(numValue);
                }
                numericTextBox.Text = numValue.ToString();
            }
        }

        private void DatabaseFileBrowse_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.DefaultExt = ".gms";
            dialog.Filter = "Gumshoe Files | *.gms";
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filename = dialog.FileName;
                if (!filename.EndsWith(".gms"))
                {
                    filename += ".gms";
                }
                DatabaseFile.Text = filename;
            }
        }

        
    }
}
