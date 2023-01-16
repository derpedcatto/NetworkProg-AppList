using System.Text;
using System;
using System.Windows;

namespace NetworkProg_AppList._1_Client_Server.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Model.NetworkConfiguration _defaultConfig;

        public MainWindow()
        {
            InitializeComponent();
            _defaultConfig = new()
            {
                Ip = IpTextBlock.Text,
                Port = Convert.ToInt32(PortTextBlock.Text),
                Encoding = Encoding.UTF8    // TODO: реализовать анализ
            };
        }


        private void StartServerButton_Click(object sender, RoutedEventArgs e)
        {
            ServerWindow serverWindow = new() { Tag = new Model.NetworkConfiguration(_defaultConfig) };
            serverWindow.Show();
        }

        private void StartClientButton_Click(object sender, RoutedEventArgs e)
        {
            ClientWindow clientWindow = new() { Tag = new Model.NetworkConfiguration(_defaultConfig) };
            clientWindow.Show();
        }
    }
}
