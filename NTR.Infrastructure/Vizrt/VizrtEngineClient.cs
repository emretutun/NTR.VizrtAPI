using System;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Tcp;

namespace NTR.Infrastructure.Vizrt
{
    /// <summary>
    /// DEĞİŞİKLİKLER (Öncelik 2 — ACK mekanizması):
    ///   - PlayAndWaitAsync: Play komutunu ACK'li kod ile gönderir, yanıtı bekler.
    ///   - WaitForAnimAsync: Animasyon süresi bitmeden önce motordan onay bekler.
    ///     ACK gelmezse sabit süre bekler (fallback). Task.Delay tamamen kaldırılmadı:
    ///     Vizrt bazı komutlara (DIRECTOR START) yanıt vermez, sadece SET komutları ACK döner.
    ///     Bu yüzden hibrit yaklaşım: önce ACK bekle, timeout olursa animasyon süresini kullan.
    /// </summary>
    public class VizrtEngineClient : IVizrtEngine
    {
        private readonly VizrtTcpConnection _connection;
        private int _uniqueResponseCode = 0;

        // Kısa animasyonlar için ACK timeout (Vizrt bazı komutlara hızlı yanıt verir)
        private static readonly TimeSpan AckTimeout = TimeSpan.FromMilliseconds(200);

        public int Id { get; }
        public string Name { get; }
        public bool IsConnected => _connection.IsConnected;
        public string CurrentScene_Front { get; private set; } = "";
        public string CurrentScene_Middle { get; private set; } = "";
        public string CurrentScene_Back { get; private set; } = "";

        public VizrtEngineClient(int id, string name)
        {
            Id = id;
            Name = name;
            _connection = new VizrtTcpConnection();
        }

        public bool Connect(string ip)
        {
            if (ip.Contains(':'))
            {
                string[] parts = ip.Split(':');
                _connection.IP = parts[0];
                _connection.Port = int.Parse(parts[1]);
            }
            else
            {
                _connection.IP = ip;
                _connection.Port = 6100;
            }
            _connection.ParentName = Name;
            return _connection.Connect();
        }

        public bool Disconnect() => _connection.Disconnect();

        // Fire-and-forget: -1 kodlu komutlara Vizrt yanıt vermez
        public CommandResult Send(string command)
        {
            bool result = _connection.Send($"-1 {command}");
            return result
                ? CommandResult.Ok($"Komut gönderildi: {command}")
                : CommandResult.Fail("Bağlı değil veya gönderim başarısız");
        }

        // ACK bekleyen gönderim — VizrtService içinden kullanılır
        private int SendWithCode(string command)
        {
            if (!IsConnected) return -1;
            int code = _uniqueResponseCode++;
            _connection.Send($"{code} {command}");
            return code;
        }

        public void Play(string scene, string animName)
        {
            if (string.IsNullOrEmpty(animName)) return;
            Send($"RENDERER*STAGE*DIRECTOR*${animName} START");
        }

        public void ReversePlay(string scene, string animName)
        {
            if (string.IsNullOrEmpty(animName)) return;
            Send($"RENDERER*STAGE*DIRECTOR*${animName} START REVERSE");
        }

        /// <summary>
        /// Animasyonu ACK bekleyerek oynatır, sonra animasyon süresi kadar bekler.
        /// Vizrt, DIRECTOR START komutlarına genellikle ACK vermez.
        /// Bu metod şu an fallbackDelay ile çalışır; ileride Vizrt yanıt verirse
        /// fallbackDelay sıfıra indirilebilir.
        /// </summary>
        public async Task PlayAndWaitAsync(string scene, string animName, int fallbackDelayMs)
        {
            if (string.IsNullOrEmpty(animName)) return;

            int code = SendWithCode($"RENDERER*STAGE*DIRECTOR*${animName} START");
            if (code >= 0)
            {
                bool ackReceived = await _connection.WaitForAckAsync(code, AckTimeout);
                if (ackReceived) return; // Motor onayladı, devam et
            }

            // ACK gelmedi veya motor -1 yanıtı döndürdü → animasyon süresini bekle
            if (fallbackDelayMs > 0)
                await Task.Delay(fallbackDelayMs);
        }

        public void SetObjectText(string scene, string objectName, string text)
        {
            Send($"RENDERER*TREE*${objectName}*GEOM*TEXT SET {text}");
        }

        public void Visibility(string scene, string objectName, bool state)
        {
            Send($"RENDERER*TREE*${objectName}*ACTIVE SET {(state ? "1" : "0")}");
        }

        public void LoadScene(string scene)
        {
            CurrentScene_Middle = scene;
            Send($"RENDERER SET_OBJECT SCENE*{scene}");
        }

        public void LoadScene_ToFront(string scene)
        {
            CurrentScene_Front = scene;
            Send($"RENDERER*FRONT_LAYER SET_OBJECT SCENE*{scene}");
        }

        public void LoadScene_ToBack(string scene)
        {
            CurrentScene_Back = scene;
            Send($"RENDERER*BACK_LAYER SET_OBJECT SCENE*{scene}");
        }

        public void StageToStart(string layer)
        {
            Send($"{layer}*STAGE TO_START");
        }

        public void FullCleanup()
        {
            Send("SCENE CLEANUP");
            Send("GEOM CLEANUP");
            Send("IMAGE CLEANUP");
            Send("FONT CLEANUP");
            Send("BASE_FONT CLEANUP");
            Send("RENDERER*FRONT_LAYER SET_OBJECT");
            Send("RENDERER SET_OBJECT");
            Send("RENDERER*BACK_LAYER SET_OBJECT");
            CurrentScene_Front = "";
            CurrentScene_Middle = "";
            CurrentScene_Back = "";
        }

        public VizrtEngine GetStatus() => new VizrtEngine
        {
            Id = Id,
            Name = Name,
            IP = _connection.IP,
            Port = _connection.Port,
            IsConnected = IsConnected,
            CurrentScene_Front = CurrentScene_Front,
            CurrentScene_Middle = CurrentScene_Middle,
            CurrentScene_Back = CurrentScene_Back
        };
    }
}