using System.Net;
using System.Text;
using System;

namespace NetworkProg_AppList._1_Client_Server.Model
{
    public class NetworkConfiguration
    {
        public String Ip { get; set; }
        public int Port { get; set; }
        public Encoding Encoding { get; set; }

        private IPEndPoint _endPoint;
        public IPEndPoint EndPoint
        {
            get
            {
                _endPoint ??= new IPEndPoint(IPAddress.Parse(Ip), Port);
                return _endPoint;
            }
        }

        public NetworkConfiguration() { }

        public NetworkConfiguration(NetworkConfiguration config)
        {
            this.Ip = config.Ip;
            this.Port = config.Port;
            this.Encoding = config.Encoding;
            this._endPoint = config.EndPoint;
        }
    }
}
