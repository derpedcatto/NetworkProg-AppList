﻿using System.Windows;

namespace NetworkProg_AppList
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        public MenuWindow()
        {
            InitializeComponent();
        }


        private void ClientServerButton_Click(object sender, RoutedEventArgs e)
        {
            new _1_Client_Server.View.MainWindow().ShowDialog();
        }

        private void HTTPButton_Click(object sender, RoutedEventArgs e)
        {
            new _2_HTTP.View.ExchangeRateWindow().ShowDialog();
        }
    }
}
