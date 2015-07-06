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
using System.Windows.Navigation;
using System.Windows.Shapes;
using GumShoe.Spider;
using GumShoe.Spider.models;

namespace GumShoe
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<Chatter> ChatterList;
 
        public MainWindow()
        {
            InitializeComponent();
        }

        private void MenuItem_Spider_Settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingsWindow = new SettingsWindow();
            settingsWindow.ShowDialog();
        }

        private void MenuItem_Spider_Go_Click(object sender, RoutedEventArgs e)
        {
            var spider = new Crawl();
            
            //ChatterVolumeDataGrid.ItemsSource = spider.Chatters;
            WebNodesDataGrid.ItemsSource = spider.WebNodes;
            BindingOperations.EnableCollectionSynchronization(spider.WebNodes, WebNodesDataGrid.ItemsSource);

            spider.Seed(Settings.Default.StartingUrl, Settings.Default.MaxAttempts,
                Settings.Default.SecondsDelay, Settings.Default.Steps, Settings.Default.DatabaseFile);
            spider.Start();
        }
    }
}
