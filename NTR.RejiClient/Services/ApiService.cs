using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using NTR.RejiClient.Models;
using System.Text;

namespace NTR.RejiClient.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _apiKey;

        public ApiService(string baseUrl, string apiKey)
        {
            _baseUrl = baseUrl.TrimEnd('/');
            _apiKey = apiKey;
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-Api-Key", _apiKey);
            _httpClient.Timeout = TimeSpan.FromSeconds(5);
        }

        private async Task<ApiResult> PostAsync(string endpoint, object? body = null)
        {
            try
            {
                string url = $"{_baseUrl}/{endpoint}";

                StringContent content = body != null
                    ? new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json")
                    : new StringContent("", Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(url, content);
                string json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return new ApiResult
                    {
                        Success = false,
                        Message = $"HTTP {(int)response.StatusCode} - {json}"
                    };
                }

                return JsonConvert.DeserializeObject<ApiResult>(json)
                       ?? new ApiResult { Success = false, Message = "Boş yanıt" };
            }
            catch (TaskCanceledException)
            {
                return new ApiResult { Success = false, Message = "Timeout (5sn geçti)" };
            }
            catch (Exception ex)
            {
                return new ApiResult { Success = false, Message = ex.Message };
            }
        }

        private async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                string url = $"{_baseUrl}/{endpoint}";
                var response = await _httpClient.GetAsync(url);
                string json = await response.Content.ReadAsStringAsync();

                // 1. Ajan: API'den 200 OK dışında bir şey dönerse ekrana basacak
                if (!response.IsSuccessStatusCode)
                {
                    MessageBox.Show($"API Hata Döndü!\nDurum: {response.StatusCode}\nYanıt: {json}", "API Bağlantı Hatası");
                    return default;
                }

                return JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception ex)
            {
                // 2. Ajan: C# tarafında ağ veya JSON çevirme hatası olursa ekrana basacak
                MessageBox.Show($"GetAsync İçinde Patlama Oldu!\nURL: {endpoint}\nHata Mesajı: {ex.Message}", "Sistem Hatası");
                return default;
            }
        }

        private async Task<ApiResult> DeleteAsync(string endpoint)
        {
            try
            {
                string url = $"{_baseUrl}/{endpoint}";
                var response = await _httpClient.DeleteAsync(url);
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResult>(json) ?? new ApiResult { Success = false };
            }
            catch (Exception ex)
            {
                return new ApiResult { Success = false, Message = ex.Message };
            }
        }

        private async Task<ApiResult> PutAsync(string endpoint, object body)
        {
            try
            {
                string url = $"{_baseUrl}/{endpoint}";
                var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, content);
                string json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<ApiResult>(json) ?? new ApiResult { Success = false };
            }
            catch (Exception ex)
            {
                return new ApiResult { Success = false, Message = ex.Message };
            }
        }

        // ─── ENGINE ──────────────────────────────────────────────

        public async Task<List<EngineStatus>> GetAllEngineStatusAsync()
            => await GetAsync<List<EngineStatus>>("api/engine/status") ?? new List<EngineStatus>();

        public async Task<ApiResult> ConnectAsync(string engineType, string ip, int port = 6100)
            => await PostAsync($"api/engine/{engineType}/connect", new { ip, port });

        public async Task<ApiResult> DisconnectAsync(string engineType)
            => await PostAsync($"api/engine/{engineType}/disconnect");

        public async Task<ApiResult> LoadSceneAsync(string engineType, string scenePath)
            => await PostAsync($"api/engine/{engineType}/scene/load", scenePath);

        public async Task<ApiResult> SendRawCommandAsync(string engineType, string command)
            => await PostAsync($"api/engine/{engineType}/raw", new { command });

        // ─── KJ ──────────────────────────────────────────────────

        public async Task<ApiResult> KjVerAsync(string engineType, int type, string text1, string text2 = "", int? rozet = null)
            => await PostAsync($"api/kj/{engineType}/ver", new { type, text1, text2, rozet });

        public async Task<ApiResult> KjAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/al");

        public async Task<ApiResult> TumunuAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/tumunu-al");

        // ─── YER ─────────────────────────────────────────────────

        public async Task<ApiResult> YerVerAsync(string engineType, string text)
            => await PostAsync($"api/kj/{engineType}/yer/ver", new { text });

        public async Task<ApiResult> YerAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/yer/al");

        // ─── SOSYAL MEDYA ─────────────────────────────────────────

        public async Task<ApiResult> SosyalMedyaVerAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/sosyal-medya/ver");

        public async Task<ApiResult> SosyalMedyaAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/sosyal-medya/al");

        public async Task<ApiResult> WhatsappVerAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/whatsapp/ver");

        public async Task<ApiResult> WhatsappAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/whatsapp/al");

        // ─── İSİMLİK ─────────────────────────────────────────────

        public async Task<ApiResult> IsimlikVerAsync(string engineType, string isim)
            => await PostAsync($"api/kj/{engineType}/isimlik/ver", new { isim });

        public async Task<ApiResult> IsimlikAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/isimlik/al");

        public async Task<ApiResult> TelefonIsimlikVerAsync(string engineType, string isim, string title, bool telefonMu)
            => await PostAsync($"api/kj/{engineType}/telefon-isimlik/ver", new { isim, title, telefonMu });

        public async Task<ApiResult> TelefonIsimlikAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/telefon-isimlik/al");

        // ─── MUHABİR KAMERA ──────────────────────────────────────

        public async Task<ApiResult> MuhabirKameraVerAsync(string engineType, string muhabir, string kameraman)
            => await PostAsync($"api/kj/{engineType}/muhabir-kamera/ver", new { muhabir, kameraman });

        public async Task<ApiResult> MuhabirKameraAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/muhabir-kamera/al");

        // ─── CANLI ───────────────────────────────────────────────

        public async Task<ApiResult> CanliVerAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/canli/ver");

        public async Task<ApiResult> CanliAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/canli/al");

        public async Task<ApiResult> CanliYerVerAsync(string engineType, string text)
            => await PostAsync($"api/kj/{engineType}/canli-yer/ver", new { text });

        public async Task<ApiResult> CanliYerAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/canli-yer/al");

        // ─── ROZET ───────────────────────────────────────────────

        public async Task<ApiResult> RozetVerAsync(string engineType, string rozetType)
            => await PostAsync($"api/kj/{engineType}/rozet/ver?rozetType={rozetType}");

        public async Task<ApiResult> RozetAlAsync(string engineType, string rozetType)
            => await PostAsync($"api/kj/{engineType}/rozet/al?rozetType={rozetType}");

        public async Task<ApiResult> RozetTumunuAlAsync(string engineType)
            => await PostAsync($"api/kj/{engineType}/rozet/tumunu-al");

        // ─── RUNDOWN ─────────────────────────────────────────────

        public async Task<List<Rundown>> GetRundownlarAsync()
            => await GetAsync<List<Rundown>>("api/rundown") ?? new List<Rundown>();

        public async Task<List<Rundown>> GetRundownByTarihAsync(string tarih)
            => await GetAsync<List<Rundown>>($"api/rundown/tarih/{tarih}") ?? new List<Rundown>();

        public async Task<ApiResult> RundownEkleAsync(string ad, string tarih, string saat, string kanal)
            => await PostAsync("api/rundown", new { ad, tarih, saat, kanal });

        public async Task<ApiResult> RundownSilAsync(int id)
            => await DeleteAsync($"api/rundown/{id}");

        // ─── HABER ───────────────────────────────────────────────

        public async Task<List<Haber>> GetHaberlerAsync(int rundownId)
            => await GetAsync<List<Haber>>($"api/haber/rundown/{rundownId}") ?? new List<Haber>();

        public async Task<ApiResult> HaberEkleAsync(string baslik, string icerik, int rundownId, int sira)
            => await PostAsync("api/haber", new { baslik, icerik, rundownId, sira });

        public async Task<ApiResult> HaberGuncelleAsync(int id, string baslik, string icerik, int rundownId, int sira)
            => await PutAsync("api/haber", new { id, baslik, icerik, rundownId, sira });

        public async Task<ApiResult> HaberSilAsync(int id)
            => await DeleteAsync($"api/haber/{id}");

        // ─── KJ LİSTESİ ──────────────────────────────────────────

        public async Task<List<KjItem>> GetKjListesiAsync(int haberId)
            => await GetAsync<List<KjItem>>($"api/kjlist/haber/{haberId}") ?? new List<KjItem>();

        public async Task<ApiResult> KjEkleAsync(int haberId, string aciklama, int type, string text1, string text2 = "")
            => await PostAsync("api/kjlist", new { haberId, aciklama, type, text1, text2 });

        public async Task<ApiResult> KjGuncelleAsync(int id, int haberId, string aciklama, int type, string text1, string text2 = "")
            => await PutAsync("api/kjlist", new { id, haberId, aciklama, type, text1, text2 });

        public async Task<ApiResult> KjSilAsync(int id)
            => await DeleteAsync($"api/kjlist/{id}");

        public async Task<ApiResult> KjSwapAsync(int id1, int id2)
            => await PostAsync($"api/kjlist/swap?id1={id1}&id2={id2}");

        public async Task<ApiResult> RollVerAsync(string engineType, RollRequestDto request)
        {
            // Backend API'ye bu adresten post atacağız
            return await PostAsync($"api/kj/{engineType}/roll/ver", request);
        }

        public async Task<ApiResult> RollAlAsync(string engineType)
        {
            // Roll'u yayından alma isteği
            return await PostAsync($"api/kj/{engineType}/roll/al", null);
        }
        // ─── KELEBEK (MULTI-GUEST) ──────────────────────────────────

        public async Task<ApiResult> KelebekSahneYukleAsync(string engineType, string sahneYolu)
        {
            // Backend: [HttpPost("{engineType}/kelebek/sahne")]
            // Not: Sahne yolu body'den string olarak gittiği için objeye sarıyoruz.
            return await PostAsync($"api/kj/{engineType}/kelebek/sahne", new { sahneYolu });
        }

        public async Task<ApiResult> KelebekIsimGonderAsync(string engineType, int index, string isim, string title)
        {
            // Backend: [HttpPost("{engineType}/kelebek/isim")]
            return await PostAsync($"api/kj/{engineType}/kelebek/isim", new { index, isim, title });
        }

        public async Task<ApiResult> KelebekKapatAsync(string engineType)
        {
            // Backend: [HttpPost("{engineType}/kelebek/kapat")]
            return await PostAsync($"api/kj/{engineType}/kelebek/kapat", null);
        }

    }
}