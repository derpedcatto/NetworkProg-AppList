using NetworkProg_AppList._1_Client_Server.Model;
using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Windows;

namespace NetworkProg_AppList._1_Client_Server.View
{
    /// <summary>
    /// Interaction logic for ClientWindow.xaml
    /// </summary>
    public partial class ClientWindow : Window
    {
        private Model.NetworkConfiguration _networkConfig;


        public ClientWindow()
        {
            InitializeComponent();
        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Tag is Model.NetworkConfiguration config)
            {
                _networkConfig = config;
            }
            else
            {
                MessageBox.Show("Configuration error!");
                return;
            }
        }

        private void ButtonSendMessage_Click(object sender, RoutedEventArgs e)
        {
            if (_networkConfig is null) return;

            Dispatcher.Invoke(() => { TextBlockLog.Text += "\n[CLIENT] Отправка сообщения...\n"; });

            try
            {
                // Такая же конфигурация, как у сервера
                Socket clientSocket = new(AddressFamily.InterNetwork,
                                          SocketType.Stream,
                                          ProtocolType.Tcp);

                clientSocket.Connect(_networkConfig.EndPoint);


                /*
                 * Отправка
                 */

                // Текст преобразовывается в байты и отправляется
                Model.ClientRequestData request = new()
                {
                    Command = "CREATE",
                    Data = TextBoxMessageField.Text
                };

                clientSocket.Send(_networkConfig.Encoding.GetBytes(JsonSerializer.Serialize(request)));


                /*
                *Получение
                */

                Model.RequestMessage message = new();
                do
                {
                    message.AcceptedBytes = clientSocket.Receive(message.Buffer);
                    message.String += _networkConfig.Encoding.GetString(message.Buffer, 0, message.AcceptedBytes);
                } while (clientSocket.Available > 0);

                var serverResponse = JsonSerializer.Deserialize<ServerResponseData>(message.String);


                Dispatcher.Invoke(() => { TextBlockLog.Text += serverResponse.Data + "\n"; });

                clientSocket.Shutdown(SocketShutdown.Both);
                clientSocket.Close();
            }
            catch (Exception ex)
            {
                Dispatcher.Invoke(() => { TextBlockLog.Text += "[CLIENT] Error: " + ex.Message + "\nОбмен остановлен\n"; });
            }
        }
    }
}
