namespace NetworkProg_AppList._1_Client_Server.Model
{
    public class RequestMessage
    {
        /// <summary>
        /// Буфер приёма данных
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        /// Перевод буфера в строку
        /// </summary>
        public string String;

        /// <summary>
        /// Кол-во принятых символов
        /// </summary>
        public int AcceptedBytes;


        public RequestMessage()
        {
            Buffer = new byte[2048];
        }
    }
}
