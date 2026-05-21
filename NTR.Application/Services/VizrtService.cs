using NTR.Application.DTOs;
using NTR.Core.Entities;
using NTR.Core.Enums;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Vizrt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NTR.Application.Services
{
    /// <summary>
    /// DEĞİŞİKLİKLER:
    ///
    /// ── Öncelik 1 (Thread-Safety) ──────────────────────────────────────────
    ///   - Her VizrtEngineType için bağımsız SemaphoreSlim(1,1) eklendi.
    ///   - Tüm state okuyan/yazan public metotlar ilgili engine'in semaphore'unu alır.
    ///   - async metotlar: await semaphore.WaitAsync() + finally Release() deseni.
    ///   - sync metotlar: semaphore.Wait() + finally Release() deseni.
    ///   - İki rejisör aynı engine'e eş zamanlı komut gönderirse biri sırada bekler,
    ///     state bozulması olmaz.
    ///
    /// ── Öncelik 2 (Task.Delay → PlayAndWaitAsync) ─────────────────────────
    ///   - VizrtEngineClient.PlayAndWaitAsync(scene, anim, fallbackMs) kullanıldı.
    ///   - Motor ACK döndürürse fallback bekleme atlanır.
    ///   - Motor ACK döndürmezse (Vizrt bazı DIRECTOR START komutlarına yanıt vermez)
    ///     fallbackMs kadar beklenir — davranış eskiyle aynı ama altyapı hazır.
    ///   - Tüm Task.Delay çağrıları PlayAndWaitAsync ile değiştirildi.
    ///   - IVizrtEngine.Play hâlâ fire-and-forget; sadece geçiş/out animasyonlarda
    ///     PlayAndWaitAsync kullanılır.
    /// </summary>
    public class VizrtService : IVizrtService
    {
        private readonly Dictionary<VizrtEngineType, VizrtEngineClient> _engines;
        private readonly Dictionary<VizrtEngineType, bool> _kjTekOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _kjCiftOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _kjUzunOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _yerOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _sosyalMedyaOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _isimlikOnAir;
        private readonly Dictionary<VizrtEngineType, int> _nextTextAnimIndex;
        private readonly Dictionary<VizrtEngineType, string> _kjScenePath;
        private readonly VizrtSettings _settings;
        private readonly Dictionary<VizrtEngineType, bool> _telefonIsimOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _muhabirKameraOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _canliOnAir;
        private readonly Dictionary<VizrtEngineType, bool> _canliYerOnAir;
        private readonly Dictionary<VizrtEngineType, RozetType?> _aktifRozet;
        private readonly Dictionary<VizrtEngineType, bool> _whatsappOnAir;
        private readonly LogService _log;

        // ── Öncelik 1: Per-engine mutex ────────────────────────────────────
        // SemaphoreSlim(1,1) = async-uyumlu mutex. Her engine bağımsız kilitlenir;
        // Reji'ye gelen istek Grafik1'i bloke etmez.
        private readonly Dictionary<VizrtEngineType, SemaphoreSlim> _locks;

        public VizrtService(VizrtSettings vizrtSettings, LogService logService)
        {
            _settings = vizrtSettings;
            _log = logService;

            _engines = new Dictionary<VizrtEngineType, VizrtEngineClient>
            {
                { VizrtEngineType.Reji,    new VizrtEngineClient(1, "Reji") },
                { VizrtEngineType.Grafik1, new VizrtEngineClient(2, "viz-Grafik1") },
                { VizrtEngineType.Grafik2, new VizrtEngineClient(3, "viz-Grafik2") }
            };

            _locks = new Dictionary<VizrtEngineType, SemaphoreSlim>
            {
                { VizrtEngineType.Reji,    new SemaphoreSlim(1, 1) },
                { VizrtEngineType.Grafik1, new SemaphoreSlim(1, 1) },
                { VizrtEngineType.Grafik2, new SemaphoreSlim(1, 1) }
            };

            _aktifRozet = new Dictionary<VizrtEngineType, RozetType?>
            {
                { VizrtEngineType.Reji,    null },
                { VizrtEngineType.Grafik1, null },
                { VizrtEngineType.Grafik2, null }
            };

            _kjTekOnAir = InitBoolDict();
            _kjCiftOnAir = InitBoolDict();
            _kjUzunOnAir = InitBoolDict();
            _yerOnAir = InitBoolDict();
            _sosyalMedyaOnAir = InitBoolDict();
            _isimlikOnAir = InitBoolDict();
            _telefonIsimOnAir = InitBoolDict();
            _muhabirKameraOnAir = InitBoolDict();
            _canliOnAir = InitBoolDict();
            _canliYerOnAir = InitBoolDict();
            _whatsappOnAir = InitBoolDict();

            _nextTextAnimIndex = new Dictionary<VizrtEngineType, int>
            {
                { VizrtEngineType.Reji,    1 },
                { VizrtEngineType.Grafik1, 1 },
                { VizrtEngineType.Grafik2, 1 }
            };

            _kjScenePath = new Dictionary<VizrtEngineType, string>
            {
                { VizrtEngineType.Reji,    vizrtSettings.Scenes.KJScene },
                { VizrtEngineType.Grafik1, vizrtSettings.Scenes.KJScene },
                { VizrtEngineType.Grafik2, vizrtSettings.Scenes.KJScene }
            };
        }

        private Dictionary<VizrtEngineType, bool> InitBoolDict() =>
            new Dictionary<VizrtEngineType, bool>
            {
                { VizrtEngineType.Reji,    false },
                { VizrtEngineType.Grafik1, false },
                { VizrtEngineType.Grafik2, false }
            };

        private VizrtEngineClient GetEngine(VizrtEngineType engineType) => _engines[engineType];

        // ─── CONNECTION ───────────────────────────────────────────────────

        public CommandResult Connect(VizrtEngineType engineType, string ip)
        {
            var engine = GetEngine(engineType);
            bool result = engine.Connect(ip);
            if (result) _log.Log("Engine", $"{engineType} bağlantısı kuruldu.", $"IP: {ip}");
            else _log.Error("Engine", $"{engineType} bağlantısı kurulamadı.", $"IP: {ip}");
            return result
                ? CommandResult.Ok($"{engineType} bağlantısı kuruldu. IP: {ip}")
                : CommandResult.Fail($"{engineType} bağlantısı kurulamadı. IP: {ip}");
        }

        public CommandResult Disconnect(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            bool result = engine.Disconnect();
            _log.Log("Engine", $"{engineType} bağlantısı kesildi.");
            return result
                ? CommandResult.Ok($"{engineType} bağlantısı kesildi.")
                : CommandResult.Fail($"{engineType} bağlantısı kesilemedi.");
        }

        public VizrtEngine GetEngineStatus(VizrtEngineType engineType) => GetEngine(engineType).GetStatus();
        public List<VizrtEngine> GetAllEngineStatus() => _engines.Values.Select(e => e.GetStatus()).ToList();

        // ─── SCENE ───────────────────────────────────────────────────────

        public CommandResult LoadScene(VizrtEngineType engineType, string scenePath)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected)
                {
                    _log.Warning("Scene", $"{engineType} bağlı değil.", $"Scene: {scenePath}");
                    return CommandResult.Fail($"{engineType} bağlı değil.");
                }
                _kjScenePath[engineType] = scenePath;
                engine.LoadScene(scenePath);
                _log.Log("Scene", "Scene yüklendi.", $"{engineType} | {scenePath}");
                return CommandResult.Ok($"Scene yüklendi: {scenePath}");
            }
            finally { sem.Release(); }
        }

        // ─── KJ ──────────────────────────────────────────────────────────

        public async Task<CommandResult> SendKjAsync(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "", RozetType? rozet = null)
        {
            _log.Log("KJ", $"{kjType} KJ yayına verildi.",
                $"{engineType} | {text1}" + (string.IsNullOrEmpty(text2) ? "" : $" | {text2}") +
                (rozet.HasValue ? $" | Rozet: {rozet}" : ""));

            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                if (string.IsNullOrEmpty(scene)) return CommandResult.Fail("Scene path tanımlı değil. Önce LoadScene çağırın.");
                if (string.IsNullOrWhiteSpace(text1)) return CommandResult.Fail("Text1 boş olamaz.");

                CommandResult kjResult = kjType switch
                {
                    KjType.Tekli => await SendKjTekliAsync(engine, scene, engineType, text1),
                    KjType.Ciftli => string.IsNullOrWhiteSpace(text2)
                        ? CommandResult.Fail("Çift satır KJ için Text2 gereklidir.")
                        : await SendKjCiftliAsync(engine, scene, engineType, text1, text2),
                    KjType.Uzun => string.IsNullOrWhiteSpace(text2)
                        ? CommandResult.Fail("Uzun KJ için Text2 gereklidir.")
                        : await SendKjUzunAsync(engine, scene, engineType, text1, text2),
                    _ => CommandResult.Fail("Geçersiz KJ tipi.")
                };

                if (rozet.HasValue)
                {
                    if (_aktifRozet[engineType].HasValue && _aktifRozet[engineType] != rozet)
                    {
                        engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                        _aktifRozet[engineType] = null;
                    }
                    if (_aktifRozet[engineType] != rozet)
                    {
                        engine.Play(scene, GetRozetInAnim(engineType, rozet.Value));
                        _aktifRozet[engineType] = rozet;
                    }
                }
                else if (_aktifRozet[engineType].HasValue)
                {
                    engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }

                return kjResult;
            }
            finally { sem.Release(); }
        }

        private async Task<CommandResult> SendKjTekliAsync(VizrtEngineClient engine, string scene, VizrtEngineType engineType, string text1)
        {
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT";

            // ── Öncelik 2: PlayAndWaitAsync, Task.Delay yerine ─────────────
            if (_kjCiftOnAir[engineType])
            {
                await engine.PlayAndWaitAsync(scene, ciftOutAnim, 1500);
                _kjCiftOnAir[engineType] = false;
            }
            if (_kjUzunOnAir[engineType])
            {
                await engine.PlayAndWaitAsync(scene, uzunOutAnim, 1500);
                _kjUzunOnAir[engineType] = false;
            }

            string textYolu1 = isCumartesi ? "TEK_KJ_TEXT$TEK_KJ_TEXT" : "KJ_TEK$SATIR_1$TEXT1";
            string textYolu2 = isCumartesi ? "TEK_KJ_TEXT$TEK_KJ_TEXT" : "KJ_TEK$SATIR_2$TEXT2";
            string inAnimasyonu = isCumartesi ? "KJ$TEK_KJ$IN" : "KJ_TUM$KJ_TEK$IN";
            string updateAnim1 = isCumartesi ? "KJ$TEK_KJ$IN" : "KJ_TUM$KJ_TEK$TEXT1";
            string updateAnim2 = isCumartesi ? "KJ$TEK_KJ$IN" : "KJ_TUM$KJ_TEK$TEXT2";

            if (!_kjTekOnAir[engineType])
            {
                engine.SetObjectText(scene, textYolu1, text1);
                engine.Play(scene, inAnimasyonu);
                _kjTekOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, textYolu1, text1);
                    engine.Play(scene, updateAnim1);
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, textYolu2, text1);
                    engine.Play(scene, updateAnim2);
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Tekli KJ yayına verildi.");
        }

        private async Task<CommandResult> SendKjCiftliAsync(VizrtEngineClient engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT";

            if (_kjTekOnAir[engineType])
            {
                await engine.PlayAndWaitAsync(scene, tekOutAnim, 1500);
                _kjTekOnAir[engineType] = false;
            }
            if (_kjUzunOnAir[engineType])
            {
                await engine.PlayAndWaitAsync(scene, uzunOutAnim, 1500);
                _kjUzunOnAir[engineType] = false;
            }

            string textUstYolu1 = isCumartesi ? "CIFT_KJ_TEXT_UST$CIFT_KJ_TEXT_UST" : "KJ_CIFT$SATIR_1$TEXT_UST_1";
            string textAltYolu1 = isCumartesi ? "CIFT_KJ_TEXT_ALT$CIFT_KJ_TEXT_ALT" : "KJ_CIFT$SATIR_1$TEXT_ALT_1";
            string textUstYolu2 = isCumartesi ? "CIFT_KJ_TEXT_UST$CIFT_KJ_TEXT_UST" : "KJ_CIFT$SATIR_2$TEXT_UST_2";
            string textAltYolu2 = isCumartesi ? "CIFT_KJ_TEXT_ALT$CIFT_KJ_TEXT_ALT" : "KJ_CIFT$SATIR_2$TEXT_ALT_2";
            string inAnimasyonu = isCumartesi ? "KJ$CIFT_KJ$IN" : "KJ_TUM$KJ_CIFT$IN";
            string updateUstAnim1 = isCumartesi ? "KJ$CIFT_KJ$IN" : "KJ_TUM$KJ_CIFT$TEXT_UST_1";
            string updateAltAnim1 = isCumartesi ? "" : "KJ_TUM$KJ_CIFT$TEXT_ALT_1";
            string updateUstAnim2 = isCumartesi ? "KJ$CIFT_KJ$IN" : "KJ_TUM$KJ_CIFT$TEXT_UST_2";
            string updateAltAnim2 = isCumartesi ? "" : "KJ_TUM$KJ_CIFT$TEXT_ALT_2";

            if (!_kjCiftOnAir[engineType])
            {
                engine.SetObjectText(scene, textUstYolu1, text1);
                engine.SetObjectText(scene, textAltYolu1, text2);
                engine.Play(scene, inAnimasyonu);
                _kjCiftOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, textUstYolu1, text1);
                    engine.SetObjectText(scene, textAltYolu1, text2);
                    engine.Play(scene, updateUstAnim1);
                    if (!string.IsNullOrEmpty(updateAltAnim1)) engine.Play(scene, updateAltAnim1);
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, textUstYolu2, text1);
                    engine.SetObjectText(scene, textAltYolu2, text2);
                    engine.Play(scene, updateUstAnim2);
                    if (!string.IsNullOrEmpty(updateAltAnim2)) engine.Play(scene, updateAltAnim2);
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Çift satır KJ yayına verildi.");
        }

        private async Task<CommandResult> SendKjUzunAsync(VizrtEngineClient engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";

            if (_kjTekOnAir[engineType])
            {
                await engine.PlayAndWaitAsync(scene, tekOutAnim, 1500);
                _kjTekOnAir[engineType] = false;
            }
            if (_kjCiftOnAir[engineType])
            {
                await engine.PlayAndWaitAsync(scene, ciftOutAnim, 1500);
                _kjCiftOnAir[engineType] = false;
            }

            string textUstYolu1 = isCumartesi ? "UZUN_KJ_TEXT_UST$UZUN_KJ_TEXT_UST" : "KJ_UZUN$SATIR_1$TEXT_UZUN_UST_1";
            string textAltYolu1 = isCumartesi ? "UZUN_KJ_TEXT_ALT$UZUN_KJ_TEXT_ALT" : "KJ_UZUN$SATIR_1$TEXT_UZUN_ALT_1";
            string textUstYolu2 = isCumartesi ? "UZUN_KJ_TEXT_UST$UZUN_KJ_TEXT_UST" : "KJ_UZUN$SATIR_2$TEXT_UZUN_UST_2";
            string textAltYolu2 = isCumartesi ? "UZUN_KJ_TEXT_ALT$UZUN_KJ_TEXT_ALT" : "KJ_UZUN$SATIR_2$TEXT_UZUN_ALT_2";
            string inAnimasyonu = isCumartesi ? "KJ$UZUN_KJ$IN" : "KJ_TUM$KJ_UZUN$IN";
            string updateAnim1 = isCumartesi ? "KJ$UZUN_KJ$IN" : "KJ_TUM$KJ_UZUN$KJ_UZUN_TEXT1";
            string updateAnim2 = isCumartesi ? "KJ$UZUN_KJ$IN" : "KJ_TUM$KJ_UZUN$KJ_UZUN_TEXT2";

            if (!_kjUzunOnAir[engineType])
            {
                engine.SetObjectText(scene, textUstYolu1, text1);
                engine.SetObjectText(scene, textAltYolu1, text2);
                engine.Play(scene, inAnimasyonu);
                _kjUzunOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, textUstYolu1, text1);
                    engine.SetObjectText(scene, textAltYolu1, text2);
                    engine.Play(scene, updateAnim1);
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, textUstYolu2, text1);
                    engine.SetObjectText(scene, textAltYolu2, text2);
                    engine.Play(scene, updateAnim2);
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Uzun KJ yayına verildi.");
        }

        public async Task<CommandResult> TakeKjAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");
                string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
                string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";
                string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT";

                if (_kjTekOnAir[engineType]) { engine.Play(scene, tekOutAnim); _kjTekOnAir[engineType] = false; }
                if (_kjCiftOnAir[engineType]) { engine.Play(scene, ciftOutAnim); _kjCiftOnAir[engineType] = false; }
                if (_kjUzunOnAir[engineType]) { engine.Play(scene, uzunOutAnim); _kjUzunOnAir[engineType] = false; }

                if (_aktifRozet[engineType].HasValue)
                {
                    engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }

                _nextTextAnimIndex[engineType] = 1;
                _log.Log("KJ", "KJ yayından alındı.", engineType.ToString());
                return CommandResult.Ok("KJ yayından alındı.");
            }
            finally { sem.Release(); }
        }

        public async Task<CommandResult> TakeAllAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                return await TakeAllInternalAsync(engineType);
            }
            finally { sem.Release(); }
        }

        // Lock dışarıdan alındığında (örn. SendRollAsync) kullanılır
        private async Task<CommandResult> TakeAllInternalAsync(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT";

            if (_kjTekOnAir[engineType]) { engine.Play(scene, tekOutAnim); _kjTekOnAir[engineType] = false; }
            if (_kjCiftOnAir[engineType]) { engine.Play(scene, ciftOutAnim); _kjCiftOnAir[engineType] = false; }
            if (_kjUzunOnAir[engineType]) { engine.Play(scene, uzunOutAnim); _kjUzunOnAir[engineType] = false; }

            if (_aktifRozet[engineType].HasValue)
            {
                engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                _aktifRozet[engineType] = null;
            }
            if (_sosyalMedyaOnAir[engineType]) { engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT"); _sosyalMedyaOnAir[engineType] = false; }
            if (_whatsappOnAir[engineType]) { engine.Play(scene, "KJ_TUM$TELEFON_WHATSAPP$OUT"); _whatsappOnAir[engineType] = false; }
            if (_yerOnAir[engineType]) { engine.Play(scene, "YER_KOSE_OUT"); _yerOnAir[engineType] = false; }
            if (_isimlikOnAir[engineType]) { engine.Play(scene, "KJ_TUM$ISIMLIK$OUT"); _isimlikOnAir[engineType] = false; }
            if (_telefonIsimOnAir[engineType]) { engine.Play(scene, "KJ_TUM$TELEFON$OUT"); engine.Play(scene, "KJ_TUM$ISIMLIK_2$OUT"); _telefonIsimOnAir[engineType] = false; }
            if (_muhabirKameraOnAir[engineType])
            {
                string muhabirOutAnim = isCumartesi ? "KJ$MUHABIR_KAMERA$OUT" : "KJ_TUM$ISIMLIK_3$OUT";
                engine.Play(scene, muhabirOutAnim);
                _muhabirKameraOnAir[engineType] = false;
            }
            if (_canliOnAir[engineType]) { engine.Play(scene, "KJ_TUM$CANLI_OUT"); _canliOnAir[engineType] = false; }
            if (_canliYerOnAir[engineType]) { engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT"); _canliYerOnAir[engineType] = false; }

            _nextTextAnimIndex[engineType] = 1;

            // Çıkış animasyonlarının bitmesi için bekle (ACK desteklenmediğinden fallback)
            await Task.Delay(1000);
            engine.StageToStart("RENDERER*MAIN_LAYER");
            _log.Log("KJ", "Tüm grafikler yayından alındı.", engineType.ToString());
            return CommandResult.Ok("Tüm grafikler yayından alındı.");
        }

        // ─── YER ─────────────────────────────────────────────────────────

        public async Task<CommandResult> SendYerAsync(VizrtEngineType engineType, string text)
        {
            _log.Log("Yer", "Yer KJ yayına verildi.", $"{engineType} | {text}");

            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                if (_yerOnAir[engineType])
                    await engine.PlayAndWaitAsync(scene, "YER_KOSE_OUT", 800);

                engine.SetObjectText(scene, "YER_KOSE$group$yer_text", text);
                engine.Play(scene, "YER_KOSE_IN");
                _yerOnAir[engineType] = true;
                return CommandResult.Ok("Yer KJ yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeYer(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                if (_yerOnAir[engineType]) { engine.Play(_kjScenePath[engineType], "YER_KOSE_OUT"); _yerOnAir[engineType] = false; }
                return CommandResult.Ok("Yer KJ yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── SOSYAL MEDYA ─────────────────────────────────────────────────

        public async Task<CommandResult> SendSosyalMedyaAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                engine.Play(scene, "SOSYAL_MEDYA_DONUSUMLU$OUT");
                await Task.Delay(500);
                engine.Play(scene, "SOSYAL_MEDYA_DONUSUMLU$IN");
                _sosyalMedyaOnAir[engineType] = true;
                return CommandResult.Ok("Sosyal medya yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeSosyalMedya(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                if (_sosyalMedyaOnAir[engineType]) { engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT"); _sosyalMedyaOnAir[engineType] = false; }
                if (_whatsappOnAir[engineType]) { engine.Play(scene, "KJ_TUM$TELEFON_WHATSAPP$OUT"); _whatsappOnAir[engineType] = false; }
                return CommandResult.Ok("Sosyal medya yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── ISİMLİK ─────────────────────────────────────────────────────

        public CommandResult SendIsimlik(VizrtEngineType engineType, string isim)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
                string textPath = isCumartesi ? "ISIMLIK$ISIMLIK$SUNUCU_ISIM" : "ISIMLIK$isim";
                string inAnim = isCumartesi ? "KJ$ISIMLIK$IN" : "ISIMLIK$IN";

                if (!string.IsNullOrWhiteSpace(isim))
                    engine.SetObjectText(scene, textPath, isim.ToUpper(new System.Globalization.CultureInfo("tr-TR")));

                engine.Play(scene, inAnim);
                _isimlikOnAir[engineType] = true;
                _log.Log("İsimlik", "İsimlik yayına verildi.", $"{engineType} | {(string.IsNullOrWhiteSpace(isim) ? "Sahnede Sabit" : isim)}");
                return CommandResult.Ok("İsimlik yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeIsimlik(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
                string outAnim = isCumartesi ? "KJ$ISIMLIK$OUT" : "ISIMLIK$OUT";
                if (_isimlikOnAir[engineType]) { engine.Play(scene, outAnim); _isimlikOnAir[engineType] = false; }
                return CommandResult.Ok("İsimlik yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── RAW COMMAND ──────────────────────────────────────────────────

        public CommandResult SendRawCommand(VizrtEngineType engineType, string command)
        {
            _log.Log("Raw", "Ham komut gönderildi.", $"{engineType} | {command}");
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
            return engine.Send(command);
        }

        // ─── TELEFON İSİMLİK ──────────────────────────────────────────────

        public async Task<CommandResult> SendTelefonIsimlikAsync(VizrtEngineType engineType, string isim, string title, bool telefonMu)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                if (_isimlikOnAir[engineType])
                {
                    await engine.PlayAndWaitAsync(scene, "KJ_TUM$ISIMLIK$OUT", 400);
                    _isimlikOnAir[engineType] = false;
                }
                if (_telefonIsimOnAir[engineType])
                {
                    engine.Play(scene, "KJ_TUM$TELEFON$OUT");
                    engine.Play(scene, "KJ_TUM$ISIMLIK_2$OUT");
                    await Task.Delay(400);
                    _telefonIsimOnAir[engineType] = false;
                }

                if (telefonMu)
                {
                    engine.Visibility(scene, "ISIMLIK_2", false);
                    engine.Visibility(scene, "TELEFON", true);
                    engine.SetObjectText(scene, "TELEFON$ISIM", isim);
                    engine.SetObjectText(scene, "TELEFON$TITLE", title);
                    engine.Play(scene, "KJ_TUM$TELEFON$IN");
                }
                else
                {
                    engine.Visibility(scene, "TELEFON", false);
                    engine.Visibility(scene, "ISIMLIK_2", true);
                    engine.SetObjectText(scene, "ISIMLIK_2$ISIM", isim);
                    engine.SetObjectText(scene, "ISIMLIK_2$TITLE", title);
                    engine.Play(scene, "KJ_TUM$ISIMLIK_2$IN");
                }
                _telefonIsimOnAir[engineType] = true;
                return CommandResult.Ok($"{(telefonMu ? "Telefon" : "İsimlik")} yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeTelefonIsimlik(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                engine.Play(scene, "KJ_TUM$TELEFON$OUT");
                engine.Play(scene, "KJ_TUM$ISIMLIK_2$OUT");
                _telefonIsimOnAir[engineType] = false;
                return CommandResult.Ok("Telefon/İsimlik yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── MUHABİR KAMERA ───────────────────────────────────────────────

        public CommandResult SendMuhabirKamera(VizrtEngineType engineType, string muhabir, string kameraman)
        {
            _log.Log("MuhabirKamera", "Muhabir/Kamera yayına verildi.", $"{engineType} | Muhabir: {muhabir} | Kamera: {kameraman}");
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                if (string.IsNullOrWhiteSpace(muhabir) && string.IsNullOrWhiteSpace(kameraman))
                    return CommandResult.Fail("Muhabir ve kameraman ikisi birden boş olamaz.");

                string scene = _kjScenePath[engineType];
                bool isHaftaSonu = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");

                string muhabirTextPath = isHaftaSonu ? "MUHABIR_KAMERA$MUHABIR_TEXT" : "ISIMLIK_3$noname$HABER$HABER_TEXT";
                string muhabirGrupPath = isHaftaSonu ? "MUHABIR_KAMERA$MUHABIR_GRUP" : "ISIMLIK_3$noname$HABER";
                string kameraTextPath = isHaftaSonu ? "MUHABIR_KAMERA$KAMERA_TEXT" : "ISIMLIK_3$noname$KAMERA$KAMERA_TEXT";
                string kameraGrupPath = isHaftaSonu ? "MUHABIR_KAMERA$KAMERA_GRUP" : "ISIMLIK_3$noname$KAMERA";
                string inAnim = isHaftaSonu ? "KJ$MUHABIR_KAMERA$IN" : "KJ_TUM$ISIMLIK_3$IN";

                var trCulture = new System.Globalization.CultureInfo("tr-TR");
                if (!string.IsNullOrWhiteSpace(muhabir)) { engine.SetObjectText(scene, muhabirTextPath, muhabir.ToUpper(trCulture)); engine.Visibility(scene, muhabirGrupPath, true); }
                else engine.Visibility(scene, muhabirGrupPath, false);
                if (!string.IsNullOrWhiteSpace(kameraman)) { engine.SetObjectText(scene, kameraTextPath, kameraman.ToUpper(trCulture)); engine.Visibility(scene, kameraGrupPath, true); }
                else engine.Visibility(scene, kameraGrupPath, false);

                engine.Play(scene, inAnim);
                _muhabirKameraOnAir[engineType] = true;
                return CommandResult.Ok($"Muhabir/Kamera yayına verildi. Muhabir: {muhabir} / Kamera: {kameraman}");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeMuhabirKamera(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                bool isHaftaSonu = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
                string outAnim = isHaftaSonu ? "KJ$MUHABIR_KAMERA$OUT" : "KJ_TUM$ISIMLIK_3$OUT";
                if (_muhabirKameraOnAir[engineType]) { engine.Play(scene, outAnim); _muhabirKameraOnAir[engineType] = false; }
                return CommandResult.Ok("Muhabir/Kamera yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── CANLI ───────────────────────────────────────────────────────

        public CommandResult SendCanli(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                if (_canliYerOnAir[engineType]) { engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT"); _canliYerOnAir[engineType] = false; }
                engine.Play(scene, "KJ_TUM$CANLI_IN");
                _canliOnAir[engineType] = true;
                return CommandResult.Ok("Canlı yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeCanli(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                if (_canliOnAir[engineType]) { engine.Play(scene, "KJ_TUM$CANLI_OUT"); _canliOnAir[engineType] = false; }
                if (_canliYerOnAir[engineType]) { engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT"); _canliYerOnAir[engineType] = false; }
                return CommandResult.Ok("Canlı yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── CANLI YER ───────────────────────────────────────────────────

        public CommandResult SendCanliYer(VizrtEngineType engineType, string text)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                if (_canliOnAir[engineType]) { engine.Play(scene, "KJ_TUM$CANLI_OUT"); _canliOnAir[engineType] = false; }
                if (_yerOnAir[engineType]) { engine.Play(scene, "KJ_TUM$YER_KOSE$YER_KOSE_OUT"); _yerOnAir[engineType] = false; }
                engine.SetObjectText(scene, "CANLI_YER_KOSE$group$canli_yer_text", text);
                engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_IN");
                _canliYerOnAir[engineType] = true;
                return CommandResult.Ok("Canlı yer yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeCanliYer(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                if (_canliYerOnAir[engineType]) { engine.Play(_kjScenePath[engineType], "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT"); _canliYerOnAir[engineType] = false; }
                return CommandResult.Ok("Canlı yer yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── ROZETLER ────────────────────────────────────────────────────
         
        private string GetRozetInAnim(VizrtEngineType engineType, RozetType rozetType)
        {
            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
            if (isCumartesi) return rozetType switch
            {
                RozetType.AzSonra => "KJ$AZ_SONRA$IN",
                RozetType.AzSonraDsf => "KJ$DSF_AZ_SONRA$IN",
                RozetType.SicakGelisme => "KJ$CORNER$IN",
                _ => ""
            };
            return rozetType switch
            {
                RozetType.AzSonra => "KJ_TUM$KJ_AZ_SONRA$IN",
                RozetType.AzSonraDsf => "KJ_TUM$KJ_AZ_SONRA_DSF$IN",
                RozetType.AzSonraDsf2 => "KJ_TUM$KJ_AZ_SONRA_DSF_2$IN",
                RozetType.SonDakika => "KJ_TUM$KJ_SON_DAKIKA$IN",
                RozetType.OzelHaber => "KJ_TUM$OZEL_HABER$IN",
                RozetType.WhatsappIhbar => "KJ_TUM$KJ_WHATSAPP_IHBAR$IN",
                _ => ""
            };
        }

        private string GetRozetOutAnim(VizrtEngineType engineType, RozetType rozetType)
        {
            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
            if (isCumartesi) return rozetType switch
            {
                RozetType.AzSonra => "KJ$AZ_SONRA$OUT",
                RozetType.AzSonraDsf => "KJ$DSF_AZ_SONRA$OUT",
                RozetType.SicakGelisme => "KJ$CORNER$OUT",
                _ => ""
            };
            return rozetType switch
            {
                RozetType.AzSonra => "KJ_TUM$KJ_AZ_SONRA$OUT",
                RozetType.AzSonraDsf => "KJ_TUM$KJ_AZ_SONRA_DSF$OUT",
                RozetType.AzSonraDsf2 => "KJ_TUM$KJ_AZ_SONRA_DSF_2$OUT",
                RozetType.SonDakika => "KJ_TUM$KJ_SON_DAKIKA$OUT",
                RozetType.OzelHaber => "KJ_TUM$OZEL_HABER$OUT",
                RozetType.WhatsappIhbar => "KJ_TUM$KJ_WHATSAPP_IHBAR$OUT",
                _ => ""
            };
        }

        public CommandResult SendRozet(VizrtEngineType engineType, RozetType rozetType)
        {
            _log.Log("Rozet", $"{rozetType} rozeti yayına verildi.", engineType.ToString());
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                if (_aktifRozet[engineType].HasValue && _aktifRozet[engineType] != rozetType)
                {
                    engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }
                if (_aktifRozet[engineType] == rozetType) return CommandResult.Ok($"{rozetType} rozeti zaten yayında.");
                if (_sosyalMedyaOnAir[engineType]) { engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT"); _sosyalMedyaOnAir[engineType] = false; }
                engine.Play(scene, GetRozetInAnim(engineType, rozetType));
                _aktifRozet[engineType] = rozetType;
                return CommandResult.Ok($"{rozetType} rozeti yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeRozet(VizrtEngineType engineType, RozetType rozetType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                if (_aktifRozet[engineType] == rozetType) { engine.Play(_kjScenePath[engineType], GetRozetOutAnim(engineType, rozetType)); _aktifRozet[engineType] = null; }
                return CommandResult.Ok($"{rozetType} rozeti yayından alındı.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeAllRozet(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                if (_aktifRozet[engineType].HasValue) { engine.Play(_kjScenePath[engineType], GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value)); _aktifRozet[engineType] = null; }
                return CommandResult.Ok("Tüm rozetler yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── WHATSAPP ─────────────────────────────────────────────────────

        public async Task<CommandResult> SendWhatsappAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                if (_sosyalMedyaOnAir[engineType])
                {
                    await engine.PlayAndWaitAsync(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT", 500);
                    _sosyalMedyaOnAir[engineType] = false;
                }
                engine.Play(scene, "KJ_TUM$TELEFON_WHATSAPP$IN");
                _whatsappOnAir[engineType] = true;
                return CommandResult.Ok("Whatsapp yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeWhatsapp(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                if (_whatsappOnAir[engineType]) { engine.Play(_kjScenePath[engineType], "KJ_TUM$TELEFON_WHATSAPP$OUT"); _whatsappOnAir[engineType] = false; }
                return CommandResult.Ok("Whatsapp yayından alındı.");
            }
            finally { sem.Release(); }
        }

        // ─── ROLL ─────────────────────────────────────────────────────────

        public async Task<CommandResult> SendRollAsync(VizrtEngineType engineType, string tesekkurYazisi, List<(string Baslik, string Yazi)> satirlar, List<string> sponsorlar)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];

                // Ekranı temizle (lock zaten elimizde, internal metodu kullan)
                await TakeAllInternalAsync(engineType);
                await Task.Delay(1000);

                int doluSatirSayisi = 0;
                int vizrtKapasite = 24;
                var trCulture = new System.Globalization.CultureInfo("tr-TR");

                for (int i = 0; i < vizrtKapasite; i++)
                {
                    int sira = i + 1;
                    string unvan = "", isim = "";
                    if (satirlar != null && i < satirlar.Count)
                    {
                        unvan = (satirlar[i].Baslik ?? "").ToUpper(trCulture);
                        isim = (satirlar[i].Yazi ?? "").ToUpper(trCulture);
                    }
                    engine.SetObjectText(scene, $"baslik{sira}", unvan);
                    engine.SetObjectText(scene, $"yazi{sira}", isim);
                    bool dolu = !string.IsNullOrWhiteSpace(unvan) || !string.IsNullOrWhiteSpace(isim);
                    engine.Visibility(scene, $"baslik{sira}", dolu);
                    engine.Visibility(scene, $"yazi{sira}", dolu);
                    if (dolu) doluSatirSayisi++;
                }

                engine.SetObjectText(scene, "tesekkur", (tesekkurYazisi ?? "").ToUpper(trCulture));

                string klasorYolu = @"D:\SHOWTV_REJI_DATA\ROLL\";
                for (int k = 1; k <= 5; k++)
                {
                    if (sponsorlar != null && (k - 1) < sponsorlar.Count)
                    {
                        engine.Send($"SCENE*{scene}*TREE*$reklam_image_{k}*IMAGE SET {klasorYolu}{sponsorlar[k - 1]}");
                        engine.Visibility(scene, $"reklam_image_{k}", true);
                    }
                    else engine.Visibility(scene, $"reklam_image_{k}", false);
                }

                int tesekkurVal = string.IsNullOrWhiteSpace(tesekkurYazisi) ? 0 : 3;
                int reklamVal = (sponsorlar?.Count ?? 0) * 2;
                int toplamSanal = Math.Max(doluSatirSayisi + tesekkurVal + reklamVal, 1);
                double targetY = 490.0 + ((toplamSanal - 12) * 48.0);
                string strY = targetY.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
                engine.Send($"SCENE*{scene}*TREE*$TEXT*ANIMATION*Position*KEY*$roll_text_pos*XYZ SET 0.0 {strY} 0.0");

                engine.Play(scene, "KJ_TUM$ROLL$IN");
                _log.Log("Roll", "Roll yayına verildi.", $"Dolu Satır: {doluSatirSayisi}, Sponsor: {sponsorlar?.Count ?? 0}");
                return CommandResult.Ok("Roll yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public CommandResult TakeRoll(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            sem.Wait();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];
                bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
                engine.Play(scene, isCumartesi ? "KJ$ROLL$OUT" : "KJ_TUM$ROLL$OUT");
                _log.Log("Roll", "Roll yayından alındı.", engineType.ToString());
                return CommandResult.Ok("Roll yayından alındı.");
            }
            finally { sem.Release(); }
        }

        public async Task<CommandResult> SendRollTekMetinAsync(VizrtEngineType engineType, string rollMetni, List<string> sponsorlar)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
                string scene = _kjScenePath[engineType];

                await TakeAllInternalAsync(engineType);
                await Task.Delay(500);

                var trCulture = new System.Globalization.CultureInfo("tr-TR");
                string gonderilecekMetin = (rollMetni ?? "").ToUpper(trCulture);
                engine.SetObjectText(scene, "ROLL_TEXT", gonderilecekMetin);

                string klasorYolu = @"D:\SHOWTV_REJI_DATA\ROLL\";
                for (int k = 1; k <= 5; k++)
                {
                    if (sponsorlar != null && (k - 1) < sponsorlar.Count)
                    {
                        engine.Send($"SCENE*{scene}*TREE*$reklam_image_{k}*IMAGE SET {klasorYolu}{sponsorlar[k - 1]}");
                        engine.Visibility(scene, $"reklam_image_{k}", true);
                    }
                    else engine.Visibility(scene, $"reklam_image_{k}", false);
                }

                int satirSayisi = gonderilecekMetin.Split('\n').Length;
                int reklamVal = (sponsorlar?.Count ?? 0) * 2;
                int toplamSanal = Math.Max(satirSayisi + reklamVal, 1);
                double targetY = 490.0 + ((toplamSanal - 12) * 48.0);
                string strY = targetY.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);
                engine.Send($"SCENE*{scene}*TREE*$TEXT*ANIMATION*Position*KEY*$roll_text_pos*XYZ SET 0.0 {strY} 0.0");

                engine.Play(scene, "KJ$ROLL$IN");
                _log.Log("Roll", "Roll Tek Metin yayına verildi.", $"Satır: {satirSayisi}, Sponsor: {sponsorlar?.Count ?? 0}");
                return CommandResult.Ok("Roll Tek Metin yayına verildi.");
            }
            finally { sem.Release(); }
        }

        // ─── KELEBEK ─────────────────────────────────────────────────────

        public CommandResult KelebekSahneYukle(VizrtEngineType engineType, string sahneYolu)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
            string cleanPath = sahneYolu.TrimStart('/');
            engine.Send($"-1 RENDERER*BACK_LAYER SET_OBJECT SCENE*{cleanPath}");
            engine.Send("-1 RENDERER*BACK_LAYER*ACTIVE SET 1");
            return CommandResult.Ok($"Kelebek sahnesi yüklendi: {sahneYolu}");
        }

        public CommandResult KelebekIsimGonder(VizrtEngineType engineType, int index, string isim, string title)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
            var trCulture = new System.Globalization.CultureInfo("tr-TR");
            string isimBuyuk = (isim ?? "").Trim().ToUpper(trCulture);
            string titleBuyuk = (title ?? "").Trim().ToUpper(trCulture);

            if (string.IsNullOrWhiteSpace(isimBuyuk))
            {
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$isimlik_bg_{index}*ACTIVE SET 0");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIM{index}*GEOM*TEXT SET ");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$TITLE{index}*GEOM*TEXT SET ");
                return CommandResult.Ok($"Kelebek {index}. kişi ekrandan gizlendi.");
            }
            engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$isimlik_bg_{index}*ACTIVE SET 1");
            engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIM{index}*GEOM*TEXT SET {isimBuyuk}");
            engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$TITLE{index}*GEOM*TEXT SET {titleBuyuk}");
            engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIM{index}*FUNCTION*Maxsize*initialize SET");
            engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIMLIK_{index}*FUNCTION*ControlObject*in SET");
            return CommandResult.Ok($"Kelebek isim gönderildi: {isimBuyuk}");
        }

        public CommandResult KelebekKapat(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");
            engine.Send("RENDERER*BACK_LAYER SET_OBJECT ");
            engine.Send("RENDERER*BACK_LAYER*ACTIVE SET 0");
            _log.Log("Kelebek", "Kelebek kapatıldı.", engineType.ToString());
            return CommandResult.Ok("Kelebek kapatıldı.");
        }


        public CommandResult SendOyuncuDegisiklik(VizrtEngineType engineType, string girenOyuncu, string cikanOyuncu, string takimLogo)
        {
            if (!_engines.TryGetValue(engineType, out var engine) || !engine.IsConnected)
            {
                return new CommandResult { Success = false, Message = "Engine bağlı değil." };
            }

            string logoYolu = @"D:\MATERIALS_HTS\SPOR_OTOMASYON_LOGOLAR\SUPERLIG\" + takimLogo; // Süper Lig takımlarının logolarının bulunduğu klasör yolu

            // 2. Verileri gönderirken IMAGE SET komutunun değerini çift tırnak içine alıyoruz
            engine.Send($"0 RENDERER*TREE*$oyuncu_degisiklik_giren_isim*GEOM*TEXT SET {girenOyuncu}");
            engine.Send($"0 RENDERER*TREE*$oyuncu_degisiklik_cikan_isim*GEOM*TEXT SET {cikanOyuncu}");
            engine.Send($"0 RENDERER*TREE*$oyuncu_degisiklik_takim_logo*TEXTURE*IMAGE SET \"{logoYolu}\"");

            // 3. IN Animasyonunu Başlat
            engine.Send("0 RENDERER*STAGE*DIRECTOR*OYUNCU_DEGISIKLIK_IN START");

            return new CommandResult { Success = true, Message = "Oyuncu değişikliği yayına verildi." };
        }

        public CommandResult TakeOyuncuDegisiklik(VizrtEngineType engineType)
        {
            if (!_engines.TryGetValue(engineType, out var engine) || !engine.IsConnected)
            {
                return new CommandResult { Success = false, Message = "Engine bağlı değil." };
            }

            engine.Send("0 RENDERER*STAGE*DIRECTOR*OYUNCU_DEGISIKLIK_OUT START");

            return new CommandResult { Success = true, Message = "Oyuncu değişikliği ekrandan alındı." };
        }

        public CommandResult SendKartBilgi(VizrtEngineType engineType, string isim, string takimLogo, int kartTipi)
        {
            if (!_engines.TryGetValue(engineType, out var engine) || !engine.IsConnected)
            {
                return new CommandResult { Success = false, Message = "Engine bağlı değil." };
            }

            string logoYolu = @"D:\MATERIALS_HTS\SPOR_OTOMASYON_LOGOLAR\SUPERLIG\" + takimLogo;

            // --- YENİ EKLENEN KISIM: Kart tipine göre başlık yazısını otomatik belirle ---
            string baslikYazisi = "SARI KART"; // Varsayılan
            if (kartTipi == 2) baslikYazisi = "DİREKT KIRMIZI KART";
            else if (kartTipi == 3) baslikYazisi = "KIRMIZI KART"; // İstersen burayı "2. SARI KART" vs. yapabilirsin

            // 1. İsim, Başlık ve Logo Set Etme
            engine.Send($"0 RENDERER*TREE*$kart_bilgi_isim*GEOM*TEXT SET {isim}");
            engine.Send($"0 RENDERER*TREE*$kart_bilgi_baslik*GEOM*TEXT SET {baslikYazisi}"); // YENİ SATIR
            engine.Send($"0 RENDERER*TREE*$kart_bilgi_takim_logo*TEXTURE*IMAGE SET \"{logoYolu}\"");

            // 2. Kart İkonu Gözlerini Aç/Kapat
            engine.Send($"0 RENDERER*TREE*$sari_kart*ACTIVE SET {(kartTipi == 1 ? "1" : "0")}");
            engine.Send($"0 RENDERER*TREE*$kirmizi_kart*ACTIVE SET {(kartTipi == 2 ? "1" : "0")}");
            engine.Send($"0 RENDERER*TREE*$cift_saridan_kirmizi_kart*ACTIVE SET {(kartTipi == 3 ? "1" : "0")}");

            // 3. IN Animasyonunu Başlat
            engine.Send("0 RENDERER*STAGE*DIRECTOR*KART_BILGI_IN START");

            return new CommandResult { Success = true, Message = "Kart bilgisi yayına verildi." };
        }

        public CommandResult TakeKartBilgi(VizrtEngineType engineType)
        {
            if (!_engines.TryGetValue(engineType, out var engine) || !engine.IsConnected)
            {
                return new CommandResult { Success = false, Message = "Engine bağlı değil." };
            }

            // OUT Animasyonunu Başlat
            engine.Send("0 RENDERER*STAGE*DIRECTOR*KART_BILGI_OUT START");

            return new CommandResult { Success = true, Message = "Kart bilgisi ekrandan alındı." };
        }

        public CommandResult SendIstatistik(VizrtEngineType engineType, string evDeger, string depDeger, string baslik, string evLogo, string depLogo)
        {
            if (!_engines.TryGetValue(engineType, out var engine) || !engine.IsConnected)
            {
                return new CommandResult { Success = false, Message = "Engine bağlı değil." };
            }

            // Logo yolları
            string basePath = @"D:\MATERIALS_HTS\SPOR_OTOMASYON_LOGOLAR\SUPERLIG\";
            string evLogoYolu = basePath + evLogo;
            string depLogoYolu = basePath + depLogo;

            // 1. Text Değerlerini Set Etme
            engine.Send($"0 RENDERER*TREE*$istatistik_ev*GEOM*TEXT SET {evDeger}");
            engine.Send($"0 RENDERER*TREE*$istatistik_dep*GEOM*TEXT SET {depDeger}");
            engine.Send($"0 RENDERER*TREE*$istatistik_baslik*GEOM*TEXT SET {baslik}");

            // 2. Logoları Set Etme
            engine.Send($"0 RENDERER*TREE*$istatistik_takim_ev_logo*TEXTURE*IMAGE SET \"{evLogoYolu}\"");
            engine.Send($"0 RENDERER*TREE*$istatistik_takim_dep_logo*TEXTURE*IMAGE SET \"{depLogoYolu}\"");

            // 3. IN Animasyonunu Başlat
            engine.Send("0 RENDERER*STAGE*DIRECTOR*ISTATISTIK_IN START");

            return new CommandResult { Success = true, Message = "İstatistik yayına verildi." };
        }

        public CommandResult TakeIstatistik(VizrtEngineType engineType)
        {
            if (!_engines.TryGetValue(engineType, out var engine) || !engine.IsConnected)
            {
                return new CommandResult { Success = false, Message = "Engine bağlı değil." };
            }

            // OUT Animasyonunu Başlat
            engine.Send("0 RENDERER*STAGE*DIRECTOR*ISTATISTIK_OUT START");

            return new CommandResult { Success = true, Message = "İstatistik ekrandan alındı." };
        }

        // ─── CANLI MAÇ SKORBOARD ───────────────────────────────────────────────

        public async Task<CommandResult> SendSagUstSkorAsync(VizrtEngineType engineType, string evTakim, string depTakim, string evSkor, string depSkor)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync(); // Çakışmaları önlemek için kilitliyoruz
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                if (string.IsNullOrEmpty(scene)) return CommandResult.Fail("Sahne yüklü değil. Lütfen önce Canlı Maç sahnesini yükleyin.");

                // Resimdeki tree yapısı ve eski koddaki container'lar:
                engine.SetObjectText(scene, "ust_takim_ev_isim", evTakim);
                engine.SetObjectText(scene, "ust_takim_dep_isim", depTakim);
                engine.SetObjectText(scene, "skor_ev", evSkor);
                engine.SetObjectText(scene, "skor_dep", depSkor);

                // Eski koddaki IN animasyonunu tetikliyoruz
                engine.Play(scene, "SKOR_IN");

                _log.Log("Skorboard", "Sağ üst skor yayına verildi.", $"{engineType} | {evTakim} {evSkor}-{depSkor} {depTakim}");
                return CommandResult.Ok("Sağ üst skor yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public async Task<CommandResult> TakeSagUstSkorAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];

                // Eski koddaki OUT animasyonunu tetikliyoruz
                engine.Play(scene, "SKOR_OUT");

                _log.Log("Skorboard", "Sağ üst skor yayından alındı.", engineType.ToString());
                return CommandResult.Ok("Sağ üst skor yayından alındı.");
            }
            finally { sem.Release(); }
        }


        // ─── CANLI MAÇ UZATMA VE GOL GRAFİKLERİ ─────────────────────────────────

        public async Task<CommandResult> SendUzatmaAsync(VizrtEngineType engineType, string sure)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                if (string.IsNullOrEmpty(scene)) return CommandResult.Fail("Sahne yüklü değil.");

                // Tabelaya "+5" formatında yazıyı basıyoruz
                engine.SetObjectText(scene, "uzatma_info", "+" + sure);
                engine.Play(scene, "UZATMA_INFO_IN");

                _log.Log("Spor-Uzatma", $"Uzatma tabelası yayına verildi: +{sure}", engineType.ToString());
                return CommandResult.Ok($"Uzatma tabelası (+{sure}) yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public async Task<CommandResult> TakeUzatmaAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                engine.Play(scene, "UZATMA_INFO_OUT");

                return CommandResult.Ok("Uzatma tabelası yayından alındı.");
            }
            finally { sem.Release(); }
        }

        public async Task<CommandResult> SendGolBilgisiAsync(VizrtEngineType engineType, string oyuncuIsim, string dakika, string takimLogo)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                if (string.IsNullOrEmpty(scene)) return CommandResult.Fail("Sahne yüklü değil.");

                string logoYolu = @"D:\MATERIALS_HTS\SPOR_OTOMASYON_LOGOLAR\SUPERLIG\" + takimLogo;

                // Eski kodundaki container yapılarıyla veri set etme
                engine.SetObjectText(scene, "alt_gol_isim", oyuncuIsim.ToUpper(new System.Globalization.CultureInfo("tr-TR")));
                engine.SetObjectText(scene, "alt_gol_dakika", $"GOL {dakika}'");
                engine.Send($"SCENE*{scene}*TREE*$alt_gol_takim_logo*TEXTURE*IMAGE SET \"{logoYolu}\"");

                // Animasyonu başlat
                engine.Play(scene, "GOL_BILGI_IN");

                _log.Log("Spor-Gol", $"Gol bilgisi yayına verildi: {oyuncuIsim} ({dakika}')", engineType.ToString());
                return CommandResult.Ok("Gol bilgisi yayına verildi.");
            }
            finally { sem.Release(); }
        }

        public async Task<CommandResult> TakeGolBilgisiAsync(VizrtEngineType engineType)
        {
            var sem = _locks[engineType];
            await sem.WaitAsync();
            try
            {
                var engine = GetEngine(engineType);
                if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

                string scene = _kjScenePath[engineType];
                engine.Play(scene, "GOL_BILGI_OUT");

                return CommandResult.Ok("Gol bilgisi yayından alındı.");
            }
            finally { sem.Release(); }
        }


        


    }
}