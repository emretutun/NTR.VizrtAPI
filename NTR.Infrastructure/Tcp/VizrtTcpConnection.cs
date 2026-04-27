using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Interfaces;
using System.Net.Sockets;

namespace NTR.Infrastructure.Tcp
{
    public class VizrtTcpConnection : IVizrtConnection
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private byte[] _buffer = new byte[30000];
        private string _pool = "";

        public string IP { get; set; } = "";
        public int Port { get; set; } = 6100;
        public string ParentName { get; set; } = "";

        public bool IsConnected
        {
            get
            {
                if (_tcpClient == null) return false;
                if (_tcpClient.Client == null) return false;
                return _tcpClient.Connected;
            }
        }

        public bool Connect()
        {
            try
            {
                _tcpClient = new TcpClient();
                IAsyncResult result = _tcpClient.BeginConnect(IP, Port, null, null);
                bool success = result.AsyncWaitHandle.WaitOne(TimeSpan.FromSeconds(3));

                if (!success)
                {
                    _tcpClient.Close();
                    return false;
                }

                _tcpClient.EndConnect(result);
                _stream = _tcpClient.GetStream();
                _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, _stream);

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VizrtTcpConnection Connect Error: {ex.Message}");
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                _stream?.Close();
                _tcpClient?.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"VizrtTcpConnection Disconnect Error: {ex.Message}");
                return false;
            }
        }

        public bool Send(params object[] prmList)
        {
            lock (this)
            {
                try
                {
                    if (!IsConnected) return false;

                    string commandText = string.Join(" ", prmList) + "\0";
                    Stream stream = _tcpClient!.GetStream();
                    byte[] ba = Encoding.UTF8.GetBytes(commandText);
                    stream.Write(ba, 0, ba.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"VizrtTcpConnection Send Error: {ex.Message}");
                    Disconnect();
                    return false;
                }
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (_stream == null) return;
                int iRx = _stream.EndRead(ar);
                string data = Encoding.UTF8.GetString(_buffer, 0, iRx > 0 ? iRx - 1 : 0);
                Console.WriteLine($"[{ParentName}] Received: {data}");
                _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, _stream);
            }
            catch
            {
                // connection dropped
            }
        }
    }
}