using System.Windows;

namespace NetworkProg_AppList
{
    /// <summary>
    /// Interaction logic for MenuWindow.xaml
    /// </summary>
    public partial class MenuWindow : Window
    {
        #region Constructor

        public MenuWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Events
        
        private void ClientServerButton_Click(object sender, RoutedEventArgs e)
        {
            new _1_Client_Server.View.MainWindow().ShowDialog();
        }

        #endregion
    }
}
