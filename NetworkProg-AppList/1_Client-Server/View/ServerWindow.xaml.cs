using System;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Windows;

namespace NetworkProg_AppList._1_Client_Server.View
{
    /// <summary>
    /// Interaction logic for ServerWindow.xaml
    /// </summary>
    public partial class ServerWindow : Window
    {
        private Model.NetworkConfiguration? _networkConfig;  // из стартового окна
        private Socket? _listenSocket;   // постоянно активный (слушающий)
        private Thread? _listenThread;   // поток с сервером


        public ServerWindow()
        {
            InitializeComponent();
        }


        private void StartServer()
        {
            if (_listenSocket == null || _networkConfig == null) return;

            Socket? requestSocket = null;   // обменный сокет - новый для каждого клиента
            try
            {
                _listenSocket.Bind(_networkConfig.EndPoint);    // привязка сокета к EndPoint
                _listenSocket.Listen(10);   // 10 - допустимая очередь

                Dispatcher.Invoke(() => { TextBlockLog.Text += "Сервер запущен\n"; });

                /*Приём и обработка запросов*/
                Model.RequestMessage message = new();
                Model.ServerResponseData serverResponse = new();
                string receivedMessage;
                while (true)
                {
                    requestSocket = _listenSocket.Accept(); // ожидание запроса и открытие сокета обмена данными
                    message.String = String.Empty;  // Новый сеанс приема
                    serverResponse.Status = String.Empty;
                    receivedMessage = String.Empty;

                    try
                    {
                        do
                        {
                            // получаем данные в буфер, AcceptedBytes - кол-во реально полученных байт
                            message.AcceptedBytes = requestSocket.Receive(message.Buffer);

                            // Переводим байты в строку согласно принятой кодировке, ограничивам работу 'AcceptedBytes' байтами...
                            message.String += _networkConfig.Encoding.GetString(message.Buffer, 0, message.AcceptedBytes);
                        } while (requestSocket.Available > 0);  //...пока есть данные для приема

                        var requestData = JsonSerializer.Deserialize<Model.ClientRequestData>(message.String);
                        receivedMessage = (requestData?.Command) switch
                        {
                            "CREATE" => "[CLIENT] Новое сообщение: " + requestData.Data,
                            _ => "[CLIENT] Команда не распознана",
                        };

                        serverResponse.Status = "SUCCESS";
                        serverResponse.Data = "\n[SERVER] Сообщение получено\n";
                    }
                    catch
                    {
                        serverResponse.Status = "FAILURE";
                        serverResponse.Data = "\n[SERVER] Сообщение не получено\n";
                        receivedMessage = "[SERVER] Ошибка получения сообщения" + "\n";
                    }

                    // Ответ клиенту (строка->байты->отправка)
                    message.Buffer = _networkConfig.Encoding.GetBytes(JsonSerializer.Serialize(serverResponse));
                    requestSocket.Send(message.Buffer);

                    // Закрытие соединения (обменного сокета)
                    requestSocket.Shutdown(SocketShutdown.Both);
                    requestSocket.Close();

                    Dispatcher.Invoke(() => { TextBlockLog.Text += receivedMessage + "\n"; });
                }
            }
            catch (Exception ex) 
            {
                Dispatcher.Invoke(() => { TextBlockLog.Text += "[SERVER] Ошибка! " + ex.Message + "\nСервер остановлен\n"; });
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.Tag is Model.NetworkConfiguration config)
            {
                _networkConfig = config;                                // сохраняем полученную конфигурацию
                _listenSocket = new Socket(AddressFamily.InterNetwork,  // один на окно | IPv4 адресация
                                           SocketType.Stream,           // двусторонний (чтение/запись)
                                           ProtocolType.Tcp);

                // запуск сервера - обязательно в отдельном потоке
                _listenThread = new Thread(StartServer);
                _listenThread.Start();
            }
            else
            {
                MessageBox.Show("Configuration error!");
                this.Close();
            }
        }

        private void CloseServerButton_Click(object sender, RoutedEventArgs e)
        {
            if (_listenThread != null)  // сервер запущен - надо останавливать
            {
                _listenSocket?.Close(); // закрываем сокет - это создаст исключение, разрушающее поток
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_listenThread != null)  // сервер запущен - надо останавливать
            {
                _listenSocket?.Close(); // закрываем сокет - это создаст исключение, разрушающее поток
            }
        }
    }
}

/* Сервер находится в постоянной активности - "слушает порт"
 * Поэтому серверная активность всегда запускается в 
 * отдельном потоке (иначе он полностью заблокирует интерфейс)
 * ! В серверной части существуют два типа сокетов
 *   - слушающий сокет: постоянно активная компонента
 *   - сокет обмена данными: создается для каждого запроса
 *       от клиента, обеспечивает обмен данными и закрывается
 *       после этого
 */