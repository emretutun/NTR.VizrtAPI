using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NTR.Core.Interfaces;

namespace NTR.Infrastructure.Tcp
{
    /// <summary>
    /// Vizrt Engine ile TCP/IP bağlantısını yönetir.
    ///
    /// DEĞİŞİKLİKLER (Öncelik 2 — TCP Yanıt Okuma):
    ///   - ReceiveCallback artık gelen "CODE OK / CODE ERR" yanıtlarını parse eder.
    ///   - Pending ACK'ler için ConcurrentDictionary<int, TCS> tutulur.
    ///   - WaitForAckAsync(code, timeout) metodu eklendi: komutun motordan onayını bekler.
    ///   - Bağlantı düşünce tüm bekleyen TCS'ler iptal edilir.
    /// </summary>
    public class VizrtTcpConnection : IVizrtConnection
    {
        private TcpClient? _tcpClient;
        private NetworkStream? _stream;
        private readonly byte[] _buffer = new byte[30000];

        // Anahtar: responseCode (int), Değer: TCS<bool> (true=OK, false=ERR, iptal=timeout)
        private readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> _pendingAcks = new();

        public string IP { get; set; } = "";
        public int Port { get; set; } = 6100;
        public string ParentName { get; set; } = "";

        public bool IsConnected
        {
            get
            {
                if (_tcpClient == null || _tcpClient.Client == null) return false;
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

                if (!success) { _tcpClient.Close(); return false; }

                _tcpClient.EndConnect(result);
                _stream = _tcpClient.GetStream();
                _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, _stream);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ParentName}] Connect Error: {ex.Message}");
                return false;
            }
        }

        public bool Disconnect()
        {
            try
            {
                CancelAllPending();
                _stream?.Close();
                _tcpClient?.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{ParentName}] Disconnect Error: {ex.Message}");
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
                    byte[] ba = Encoding.UTF8.GetBytes(commandText);
                    _tcpClient!.GetStream().Write(ba, 0, ba.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[{ParentName}] Send Error: {ex.Message}");
                    Disconnect();
                    return false;
                }
            }
        }

        /// <summary>
        /// Belirli bir responseCode için Vizrt'ten "CODE OK" yanıtı bekler.
        /// Timeout dolunca iyimser davranır (true döner) — motor meşgulse bloke etmez.
        /// -1 kodlu komutlara Vizrt yanıt vermez, bunlar için bu metodu çağırmayın.
        /// </summary>
        public async Task<bool> WaitForAckAsync(int responseCode, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _pendingAcks[responseCode] = tcs;

            using var cts = new CancellationTokenSource(timeout);
            cts.Token.Register(() =>
            {
                if (_pendingAcks.TryRemove(responseCode, out _))
                    tcs.TrySetResult(true); // Timeout → devam et (iyimser)
            });

            return await tcs.Task;
        }

        // ─── YANIT PARSE ──────────────────────────────────────────────────────
        // Vizrt yanıt formatı: "<responseCode> OK" veya "<responseCode> ERR <reason>"
        // Örnek: "42 OK" veya "42 ERR OBJECT_NOT_FOUND"
        // -1 ile gönderilen fire-and-forget komutlara Vizrt yanıt vermez.

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (_stream == null) return;
                int iRx = _stream.EndRead(ar);

                if (iRx <= 0) { HandleConnectionDrop(); return; }

                string data = Encoding.UTF8.GetString(_buffer, 0, iRx - 1);
                ParseResponse(data);

                _stream.BeginRead(_buffer, 0, _buffer.Length, ReceiveCallback, _stream);
            }
            catch
            {
                HandleConnectionDrop();
            }
        }

        private void ParseResponse(string raw)
        {
            // Birden fazla yanıt tek pakette gelebilir (\n ile ayrılmış)
            foreach (string line in raw.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                int spaceIdx = trimmed.IndexOf(' ');
                if (spaceIdx <= 0) continue;

                if (!int.TryParse(trimmed[..spaceIdx], out int code)) continue;

                string rest = trimmed[(spaceIdx + 1)..];
                bool isOk = rest.StartsWith("OK", StringComparison.OrdinalIgnoreCase);

                if (_pendingAcks.TryRemove(code, out var tcs))
                    tcs.TrySetResult(isOk);
            }
        }

        private void HandleConnectionDrop()
        {
            Console.WriteLine($"[{ParentName}] Bağlantı koptu. 5 saniye sonra yeniden denenecek...");
            CancelAllPending();
            _ = Task.Run(async () =>
            {
                await Task.Delay(5000);
                if (!IsConnected)
                {
                    Console.WriteLine($"[{ParentName}] Yeniden bağlanılıyor...");
                    Connect();
                }
            });
        }

        private void CancelAllPending()
        {
            foreach (var kv in _pendingAcks)
                kv.Value.TrySetCanceled();
            _pendingAcks.Clear();
        }
    }
}