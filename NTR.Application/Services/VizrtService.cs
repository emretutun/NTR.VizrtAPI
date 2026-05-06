using NTR.Application.DTOs;
using NTR.Core.Entities;
using NTR.Core.Enums;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Vizrt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace NTR.Application.Services
{
    public class VizrtService : IVizrtService
    {
        private readonly Dictionary<VizrtEngineType, IVizrtEngine> _engines;
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


        public VizrtService(VizrtSettings vizrtSettings, LogService logService)
        {

            _settings = vizrtSettings;
            _telefonIsimOnAir = InitBoolDict();
            _muhabirKameraOnAir = InitBoolDict();
            _canliOnAir = InitBoolDict();
            _canliYerOnAir = InitBoolDict();
            _whatsappOnAir = InitBoolDict();
            _log = logService;
            _aktifRozet = new Dictionary<VizrtEngineType, RozetType?>
            {
                { VizrtEngineType.Reji,    null },
                { VizrtEngineType.Grafik1, null },
                { VizrtEngineType.Grafik2, null }
            };
            _engines = new Dictionary<VizrtEngineType, IVizrtEngine>

            {
                { VizrtEngineType.Reji,    new VizrtEngineClient(1, "viz-KJ") },
                { VizrtEngineType.Grafik1, new VizrtEngineClient(2, "viz-Grafik1") },
                { VizrtEngineType.Grafik2, new VizrtEngineClient(3, "viz-Grafik2") }
            };

            _kjTekOnAir = InitBoolDict();
            _kjCiftOnAir = InitBoolDict();
            _kjUzunOnAir = InitBoolDict();
            _yerOnAir = InitBoolDict();
            _sosyalMedyaOnAir = InitBoolDict();
            _isimlikOnAir = InitBoolDict();

            _nextTextAnimIndex = new Dictionary<VizrtEngineType, int>
            {
                { VizrtEngineType.Reji,    1 },
                { VizrtEngineType.Grafik1, 1 },
                { VizrtEngineType.Grafik2, 1 }
            };

            // Scene path'leri settings'den al
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

        private IVizrtEngine GetEngine(VizrtEngineType engineType) => _engines[engineType];

        // ─── CONNECTION ───────────────────────────────────────────

        public CommandResult Connect(VizrtEngineType engineType, string ip)
        {
            var engine = GetEngine(engineType);
            bool result = engine.Connect(ip);

            if (result)
                _log.Log("Engine", $"{engineType} bağlantısı kuruldu.", $"IP: {ip}");
            else
                _log.Error("Engine", $"{engineType} bağlantısı kurulamadı.", $"IP: {ip}");

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

        public VizrtEngine GetEngineStatus(VizrtEngineType engineType)
        {
            return GetEngine(engineType).GetStatus();
        }

        public List<VizrtEngine> GetAllEngineStatus()
        {
            return _engines.Values.Select(e => e.GetStatus()).ToList();
        }

        // ─── SCENE ───────────────────────────────────────────────

        public CommandResult LoadScene(VizrtEngineType engineType, string scenePath)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
            {
                _log.Warning("Scene", $"{engineType} bağlı değil.", $"Scene: {scenePath}");
                return CommandResult.Fail($"{engineType} bağlı değil.");
            }

            _kjScenePath[engineType] = scenePath;
            engine.LoadScene(scenePath);
            _log.Log("Scene", $"Scene yüklendi.", $"{engineType} | {scenePath}");
            return CommandResult.Ok($"Scene yüklendi: {scenePath}");
        }

        // ─── KJ ──────────────────────────────────────────────────

        public CommandResult SendKj(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "", RozetType? rozet = null)
        {
            _log.Log("KJ", $"{kjType} KJ yayına verildi.", $"{engineType} | {text1}" + (string.IsNullOrEmpty(text2) ? "" : $" | {text2}") + (rozet.HasValue ? $" | Rozet: {rozet}" : ""));

            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            if (string.IsNullOrEmpty(scene))
                return CommandResult.Fail("Scene path tanımlı değil. Önce LoadScene çağırın.");

            if (string.IsNullOrWhiteSpace(text1))
                return CommandResult.Fail("Text1 boş olamaz.");

            // Önce KJ'yi gönder
            CommandResult kjResult;
            switch (kjType)
            {
                case KjType.Tekli:
                    kjResult = SendKjTekli(engine, scene, engineType, text1);
                    break;
                case KjType.Ciftli:
                    if (string.IsNullOrWhiteSpace(text2))
                        return CommandResult.Fail("Çift satır KJ için Text2 gereklidir.");
                    kjResult = SendKjCiftli(engine, scene, engineType, text1, text2);
                    break;
                case KjType.Uzun:
                    if (string.IsNullOrWhiteSpace(text2))
                        return CommandResult.Fail("Uzun KJ için Text2 gereklidir.");
                    kjResult = SendKjUzun(engine, scene, engineType, text1, text2);
                    break;
                default:
                    return CommandResult.Fail("Geçersiz KJ tipi.");
            }

            // Sonra rozeti bağımsız olarak yönet
            if (rozet.HasValue)
            {
                // Farklı rozet açıksa kapat
                if (_aktifRozet[engineType].HasValue && _aktifRozet[engineType] != rozet)
                {
                    engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }

                // Yeni rozeti aç (zaten açık değilse)
                if (_aktifRozet[engineType] != rozet)
                {
                    engine.Play(scene, GetRozetInAnim(engineType, rozet.Value));
                    _aktifRozet[engineType] = rozet;
                }
            }
            else
            {
                // Rozet null → aktif rozet varsa kapat
                if (_aktifRozet[engineType].HasValue)
                {
                    engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }
            }

            return kjResult;
        }

        private CommandResult SendKjTekli(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1)
        {
            // 🌟 AKILLI YOL SEÇİCİ (SAHNEYE GÖRE) 🌟
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");

            // Çıkış yolları (Geçişler için)
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT"; // Cumarteside uzun yoksa eskiyi denemesi sorun yaratmaz

            // Farklı KJ türü açıksa kapat
            if (_kjCiftOnAir[engineType])
            {
                engine.Play(scene, ciftOutAnim);
                _kjCiftOnAir[engineType] = false;
                Thread.Sleep(1500); // Kapanması için 1.5 saniye bekle
            }
            if (_kjUzunOnAir[engineType])
            {
                engine.Play(scene, uzunOutAnim);
                _kjUzunOnAir[engineType] = false;
                Thread.Sleep(1500);
            }

            // Metin ve Animasyon yolları (Tek KJ için)
            string textYolu1 = isCumartesi ? "TEK_KJ_TEXT$TEK_KJ_TEXT" : "KJ_TEK$SATIR_1$TEXT1";
            string textYolu2 = isCumartesi ? "TEK_KJ_TEXT$TEK_KJ_TEXT" : "KJ_TEK$SATIR_2$TEXT2";
            string inAnimasyonu = isCumartesi ? "KJ$TEK_KJ$IN" : "KJ_TUM$KJ_TEK$IN";
            string updateAnim1 = isCumartesi ? "KJ$TEK_KJ$IN" : "KJ_TUM$KJ_TEK$TEXT1";
            string updateAnim2 = isCumartesi ? "KJ$TEK_KJ$IN" : "KJ_TUM$KJ_TEK$TEXT2";

            if (!_kjTekOnAir[engineType])
            {
                // İlk kez açılıyor
                engine.SetObjectText(scene, textYolu1, text1);
                engine.Play(scene, inAnimasyonu);
                _kjTekOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                // Zaten açık → sadece yazı değişecek
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


        private CommandResult SendKjCiftli(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            // 🌟 AKILLI YOL SEÇİCİ (SAHNEYE GÖRE) 🌟
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");

            // Çıkış yolları (Geçişler için)
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT";

            // Farklı KJ türü açıksa kapat
            if (_kjTekOnAir[engineType])
            {
                engine.Play(scene, tekOutAnim);
                _kjTekOnAir[engineType] = false;
                Thread.Sleep(1500); // Kapanması için 1.5 saniye bekle
            }
            if (_kjUzunOnAir[engineType])
            {
                engine.Play(scene, uzunOutAnim);
                _kjUzunOnAir[engineType] = false;
                Thread.Sleep(1500);
            }

            // Metin ve Animasyon yolları (Çift KJ için)
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
                // İlk kez açılıyor
                engine.SetObjectText(scene, textUstYolu1, text1);
                engine.SetObjectText(scene, textAltYolu1, text2);
                engine.Play(scene, inAnimasyonu);
                _kjCiftOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                // Zaten açık → sadece yazı değişecek
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

        private CommandResult SendKjUzun(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            // 🌟 AKILLI YOL SEÇİCİ (SAHNEYE GÖRE) 🌟
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");

            // Çıkış yolları (Geçişler için diğer KJ'leri kapatırken doğru yolu bulmalı)
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";

            // Farklı KJ türü açıksa kapat
            if (_kjTekOnAir[engineType])
            {
                engine.Play(scene, tekOutAnim);
                _kjTekOnAir[engineType] = false;
                Thread.Sleep(1500);
            }
            if (_kjCiftOnAir[engineType])
            {
                engine.Play(scene, ciftOutAnim);
                _kjCiftOnAir[engineType] = false;
                Thread.Sleep(1500);
            }

            // Metin ve Animasyon yolları (Uzun KJ için)
            // Not: Cumartesi projesinde Uzun KJ tasarlanırsa isimleri buna göre verebilirsin.
            string textUstYolu1 = isCumartesi ? "UZUN_KJ_TEXT_UST$UZUN_KJ_TEXT_UST" : "KJ_UZUN$SATIR_1$TEXT_UZUN_UST_1";
            string textAltYolu1 = isCumartesi ? "UZUN_KJ_TEXT_ALT$UZUN_KJ_TEXT_ALT" : "KJ_UZUN$SATIR_1$TEXT_UZUN_ALT_1";

            string textUstYolu2 = isCumartesi ? "UZUN_KJ_TEXT_UST$UZUN_KJ_TEXT_UST" : "KJ_UZUN$SATIR_2$TEXT_UZUN_UST_2";
            string textAltYolu2 = isCumartesi ? "UZUN_KJ_TEXT_ALT$UZUN_KJ_TEXT_ALT" : "KJ_UZUN$SATIR_2$TEXT_UZUN_ALT_2";

            string inAnimasyonu = isCumartesi ? "KJ$UZUN_KJ$IN" : "KJ_TUM$KJ_UZUN$IN";
            string updateAnim1 = isCumartesi ? "KJ$UZUN_KJ$IN" : "KJ_TUM$KJ_UZUN$KJ_UZUN_TEXT1";
            string updateAnim2 = isCumartesi ? "KJ$UZUN_KJ$IN" : "KJ_TUM$KJ_UZUN$KJ_UZUN_TEXT2";

            if (!_kjUzunOnAir[engineType])
            {
                // İlk kez açılıyor → SATIR_1'e yaz, IN direktörünü çalıştır
                engine.SetObjectText(scene, textUstYolu1, text1);
                engine.SetObjectText(scene, textAltYolu1, text2);
                engine.Play(scene, inAnimasyonu);
                _kjUzunOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                // Zaten açık → SATIR_1 ve SATIR_2 arasında geçiş yap
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    // SATIR_1'e yaz, TEXT1 direktörünü çalıştır
                    engine.SetObjectText(scene, textUstYolu1, text1);
                    engine.SetObjectText(scene, textAltYolu1, text2);
                    engine.Play(scene, updateAnim1);
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    // SATIR_2'ye yaz, TEXT2 direktörünü çalıştır
                    engine.SetObjectText(scene, textUstYolu2, text1);
                    engine.SetObjectText(scene, textAltYolu2, text2);
                    engine.Play(scene, updateAnim2);
                    _nextTextAnimIndex[engineType] = 1;
                }
            }

            return CommandResult.Ok("Uzun KJ yayına verildi.");
        }

        public CommandResult TakeKj(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");

            // 🌟 Akıllı Çıkış Animasyonları 🌟
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT"; // Cumartesi uzun KJ'si gelirse burayı da güncelleyeceğiz

            if (_kjTekOnAir[engineType]) { engine.Play(scene, tekOutAnim); _kjTekOnAir[engineType] = false; }
            if (_kjCiftOnAir[engineType]) { engine.Play(scene, ciftOutAnim); _kjCiftOnAir[engineType] = false; }
            if (_kjUzunOnAir[engineType]) { engine.Play(scene, uzunOutAnim); _kjUzunOnAir[engineType] = false; }

            // Rozeti de kapat
            if (_aktifRozet[engineType].HasValue)
            {
                engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                _aktifRozet[engineType] = null;
            }

            _nextTextAnimIndex[engineType] = 1;
            _log.Log("KJ", "KJ yayından alındı.", engineType.ToString());
            return CommandResult.Ok("KJ yayından alındı.");
        }

        public CommandResult TakeAll(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI");

            // 🌟 Akıllı Çıkış Animasyonları 🌟
            string tekOutAnim = isCumartesi ? "KJ$TEK_KJ$OUT" : "KJ_TUM$KJ_TEK$OUT";
            string ciftOutAnim = isCumartesi ? "KJ$CIFT_KJ$OUT" : "KJ_TUM$KJ_CIFT$OUT";
            string uzunOutAnim = "KJ_TUM$KJ_UZUN$OUT";

            // KJ bantları
            if (_kjTekOnAir[engineType]) { engine.Play(scene, tekOutAnim); _kjTekOnAir[engineType] = false; }
            if (_kjCiftOnAir[engineType]) { engine.Play(scene, ciftOutAnim); _kjCiftOnAir[engineType] = false; }
            if (_kjUzunOnAir[engineType]) { engine.Play(scene, uzunOutAnim); _kjUzunOnAir[engineType] = false; }

            // Rozetler
            if (_aktifRozet[engineType].HasValue)
            {
                engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                _aktifRozet[engineType] = null;
            }

            // Sosyal medya ve Whatsapp
            if (_sosyalMedyaOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT");
                _sosyalMedyaOnAir[engineType] = false;
            }
            if (_whatsappOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$TELEFON_WHATSAPP$OUT");
                _whatsappOnAir[engineType] = false;
            }

            // Yer
            if (_yerOnAir[engineType])
            {
                engine.Play(scene, "YER_KOSE_OUT");
                _yerOnAir[engineType] = false;
            }

            // İsimlik
            if (_isimlikOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$ISIMLIK$OUT");
                _isimlikOnAir[engineType] = false;
            }

            // Telefon İsimlik
            if (_telefonIsimOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$TELEFON$OUT");
                engine.Play(scene, "KJ_TUM$ISIMLIK_2$OUT");
                _telefonIsimOnAir[engineType] = false;
            }

            // Muhabir Kamera
            if (_muhabirKameraOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$ISIMLIK_3$OUT");
                _muhabirKameraOnAir[engineType] = false;
            }

            // Canlı
            if (_canliOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$CANLI_OUT");
                _canliOnAir[engineType] = false;
            }

            // Canlı Yer
            if (_canliYerOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT");
                _canliYerOnAir[engineType] = false;
            }

            // NextTextAnimIndex sıfırla
            _nextTextAnimIndex[engineType] = 1;

            // Stage sıfırla
            Thread.Sleep(1000);
            engine.StageToStart("RENDERER*MAIN_LAYER");
            _log.Log("KJ", "Tüm grafikler yayından alındı.", engineType.ToString());

            return CommandResult.Ok("Tüm grafikler yayından alındı.");
        }

        // ─── YER ─────────────────────────────────────────────────

        public CommandResult SendYer(VizrtEngineType engineType, string text)
        {
            _log.Log("Yer", $"Yer KJ yayına verildi.", $"{engineType} | {text}");
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_yerOnAir[engineType])
            {
                engine.Play(scene, "YER_KOSE_OUT");
                Thread.Sleep(800);
            }

            engine.SetObjectText(scene, "YER_KOSE$group$yer_text", text);
            engine.Play(scene, "YER_KOSE_IN");
            _yerOnAir[engineType] = true;
            return CommandResult.Ok("Yer KJ yayına verildi.");
        }

        public CommandResult TakeYer(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            if (_yerOnAir[engineType])
            {
                engine.Play(_kjScenePath[engineType], "YER_KOSE_OUT");
                _yerOnAir[engineType] = false;
            }
            return CommandResult.Ok("Yer KJ yayından alındı.");
        }

        // ─── SOSYAL MEDYA ─────────────────────────────────────────

        public CommandResult SendSosyalMedya(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            engine.Play(scene, "SOSYAL_MEDYA_DONUSUMLU$OUT");
            Thread.Sleep(500);
            engine.Play(scene, "SOSYAL_MEDYA_DONUSUMLU$IN");
            _sosyalMedyaOnAir[engineType] = true;
            return CommandResult.Ok("Sosyal medya yayına verildi.");
        }

        public CommandResult TakeSosyalMedya(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_sosyalMedyaOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT");
                _sosyalMedyaOnAir[engineType] = false;
            }

            if (_whatsappOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$TELEFON_WHATSAPP$OUT");
                _whatsappOnAir[engineType] = false;
            }

            return CommandResult.Ok("Sosyal medya yayından alındı.");
        }

        // ─── ISİMLİK ─────────────────────────────────────────────

        // ─── ISİMLİK ─────────────────────────────────────────────

        public CommandResult SendIsimlik(VizrtEngineType engineType, string isim)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // 🌟 AKILLI YOL SEÇİCİ 🌟
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");

            // Metin ve Animasyon yolları
            string textPath = isCumartesi ? "ISIMLIK$ISIMLIK$SUNUCU_ISIM" : "ISIMLIK$isim";
            string inAnim = isCumartesi ? "KJ$ISIMLIK$IN" : "ISIMLIK$IN";

            // 🌟 EĞER İSİM BOŞ GELİRSE, SAHNEDEKİ SABİT İSMİ BOZMAMAK İÇİN SET ETME 🌟
            if (!string.IsNullOrWhiteSpace(isim))
            {
                engine.SetObjectText(scene, textPath, isim.ToUpper(new System.Globalization.CultureInfo("tr-TR")));
            }

            // Sadece animasyonu oynat
            engine.Play(scene, inAnim);

            _isimlikOnAir[engineType] = true;
            _log.Log("İsimlik", "İsimlik yayına verildi.", $"{engineType} | İsim: {(string.IsNullOrWhiteSpace(isim) ? "Sahnede Sabit" : isim)}");

            return CommandResult.Ok("İsimlik yayına verildi.");
        }

        public CommandResult TakeIsimlik(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // 🌟 AKILLI YOL SEÇİCİ 🌟
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
            string outAnim = isCumartesi ? "KJ$ISIMLIK$OUT" : "ISIMLIK$OUT";

            if (_isimlikOnAir[engineType])
            {
                engine.Play(scene, outAnim);
                _isimlikOnAir[engineType] = false;
            }

            return CommandResult.Ok("İsimlik yayından alındı.");
        }

        // ─── RAW COMMAND ──────────────────────────────────────────

        public CommandResult SendRawCommand(VizrtEngineType engineType, string command)
        {
            _log.Log("Raw", $"Ham komut gönderildi.", $"{engineType} | {command}");
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            return engine.Send(command);
        }

        // ─── TELEFON İSİMLİK ─────────────────────────────────────

        public CommandResult SendTelefonIsimlik(VizrtEngineType engineType, string isim, string title, bool telefonMu)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_isimlikOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$ISIMLIK$OUT");
                _isimlikOnAir[engineType] = false;
                Thread.Sleep(400);
            }
            if (_telefonIsimOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$TELEFON$OUT");
                engine.Play(scene, "KJ_TUM$ISIMLIK_2$OUT");
                _telefonIsimOnAir[engineType] = false;
                Thread.Sleep(400);
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


        public CommandResult TakeTelefonIsimlik(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            engine.Play(scene, "KJ_TUM$TELEFON$OUT");
            engine.Play(scene, "KJ_TUM$ISIMLIK_2$OUT");
            _telefonIsimOnAir[engineType] = false;
            return CommandResult.Ok("Telefon/İsimlik yayından alındı.");
        }

        // ─── MUHABİR KAMERA ──────────────────────────────────────

        public CommandResult SendMuhabirKamera(VizrtEngineType engineType, string muhabir, string kameraman)
        {
            _log.Log("MuhabirKamera", "Muhabir/Kamera yayına verildi.", $"{engineType} | Muhabir: {muhabir} | Kamera: {kameraman}");
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (string.IsNullOrWhiteSpace(muhabir) && string.IsNullOrWhiteSpace(kameraman))
                return CommandResult.Fail("Muhabir ve kameraman ikisi birden boş olamaz.");

            if (!string.IsNullOrWhiteSpace(muhabir))
            {
                engine.SetObjectText(scene, "ISIMLIK_3$noname$HABER$HABER_TEXT", muhabir.ToUpper());
                engine.Visibility(scene, "ISIMLIK_3$noname$HABER", true);
            }
            else
            {
                engine.Visibility(scene, "ISIMLIK_3$noname$HABER", false);
            }

            if (!string.IsNullOrWhiteSpace(kameraman))
            {
                engine.SetObjectText(scene, "ISIMLIK_3$noname$KAMERA$KAMERA_TEXT", kameraman.ToUpper());
                engine.Visibility(scene, "ISIMLIK_3$noname$KAMERA", true);
            }
            else
            {
                engine.Visibility(scene, "ISIMLIK_3$noname$KAMERA", false);
            }

            engine.Play(scene, "KJ_TUM$ISIMLIK_3$IN");
            _muhabirKameraOnAir[engineType] = true;
            return CommandResult.Ok($"Muhabir/Kamera yayına verildi. Muhabir: {muhabir} / Kamera: {kameraman}");
        }

        public CommandResult TakeMuhabirKamera(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            engine.Play(_kjScenePath[engineType], "KJ_TUM$ISIMLIK_3$OUT");
            _muhabirKameraOnAir[engineType] = false;
            return CommandResult.Ok("Muhabir/Kamera yayından alındı.");
        }

        // ─── CANLI ───────────────────────────────────────────────

        public CommandResult SendCanli(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_canliYerOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT");
                _canliYerOnAir[engineType] = false;
            }

            engine.Play(scene, "KJ_TUM$CANLI_IN");
            _canliOnAir[engineType] = true;
            return CommandResult.Ok("Canlı yayına verildi.");
        }

        public CommandResult TakeCanli(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_canliOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$CANLI_OUT");
                _canliOnAir[engineType] = false;
            }
            if (_canliYerOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT");
                _canliYerOnAir[engineType] = false;
            }
            return CommandResult.Ok("Canlı yayından alındı.");
        }

        // ─── CANLI YER ───────────────────────────────────────────

        public CommandResult SendCanliYer(VizrtEngineType engineType, string text)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_canliOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$CANLI_OUT");
                _canliOnAir[engineType] = false;
            }

            if (_yerOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$YER_KOSE$YER_KOSE_OUT");
                _yerOnAir[engineType] = false;
            }

            engine.SetObjectText(scene, "CANLI_YER_KOSE$group$canli_yer_text", text);
            engine.Play(scene, "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_IN");
            _canliYerOnAir[engineType] = true;
            return CommandResult.Ok("Canlı yer yayına verildi.");
        }

        public CommandResult TakeCanliYer(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            if (_canliYerOnAir[engineType])
            {
                engine.Play(_kjScenePath[engineType], "KJ_TUM$CANLI_YER_KOSE$CANLI_YER_KOSE_OUT");
                _canliYerOnAir[engineType] = false;
            }
            return CommandResult.Ok("Canlı yer yayından alındı.");
        }
        // ─── ROZETLER ────────────────────────────────────────────

        private string GetRozetInAnim(VizrtEngineType engineType, RozetType rozetType)
        {
            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");

            if (isCumartesi)
            {
                return rozetType switch
                {
                    RozetType.AzSonra => "KJ$AZ_SONRA$IN",
                    RozetType.AzSonraDsf => "KJ$DSF_AZ_SONRA$IN",
                    RozetType.SicakGelisme => "KJ$CORNER$IN",
                    _ => ""
                };
            }

            // Eski (Yeni Sayfa) Projesi İçin[cite: 1]
            return rozetType switch
            {
                RozetType.AzSonra => "KJ_TUM$KJ_AZ_SONRA$IN",
                RozetType.AzSonraDsf => "KJ_TUM$KJ_AZ_SONRA_DSF$IN",
                RozetType.AzSonraDsf2 => "KJ_TUM$KJ_AZ_SONRA_DSF_2$IN",
                RozetType.SonDakika => "KJ_TUM$KJ_SON_DAKIKA$IN",
                RozetType.OzelHaber => "KJ_TUM$OZEL_HABER$IN",
                RozetType.WhatsappIhbar => "KJ_TUM$KJ_WHATSAPP_IHBAR$IN",
                RozetType.SicakGelisme => "",
                _ => ""
            };
        }

        private string GetRozetOutAnim(VizrtEngineType engineType, RozetType rozetType)
        {
            string scene = _kjScenePath[engineType];
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");

            if (isCumartesi)
            {
                return rozetType switch
                {
                    RozetType.AzSonra => "KJ$AZ_SONRA$OUT",
                    RozetType.AzSonraDsf => "KJ$DSF_AZ_SONRA$OUT",
                    RozetType.SicakGelisme => "KJ$CORNER$OUT",
                    _ => ""
                };
            }

            // Eski (Yeni Sayfa) Projesi İçin[cite: 1]
            return rozetType switch
            {
                RozetType.AzSonra => "KJ_TUM$KJ_AZ_SONRA$OUT",
                RozetType.AzSonraDsf => "KJ_TUM$KJ_AZ_SONRA_DSF$OUT",
                RozetType.AzSonraDsf2 => "KJ_TUM$KJ_AZ_SONRA_DSF_2$OUT",
                RozetType.SonDakika => "KJ_TUM$KJ_SON_DAKIKA$OUT",
                RozetType.OzelHaber => "KJ_TUM$OZEL_HABER$OUT",
                RozetType.WhatsappIhbar => "KJ_TUM$KJ_WHATSAPP_IHBAR$OUT",
                RozetType.SicakGelisme => "",
                _ => ""
            };
        }

        public CommandResult SendRozet(VizrtEngineType engineType, RozetType rozetType)
        {
            _log.Log("Rozet", $"{rozetType} rozeti yayına verildi.", engineType.ToString());
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // Farklı bir rozet açıksa kapat
            if (_aktifRozet[engineType].HasValue && _aktifRozet[engineType] != rozetType)
            {
                string outAnim = GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value);
                engine.Play(scene, outAnim);
                _aktifRozet[engineType] = null;
            }

            // Aynı rozet zaten açıksa tekrar açma
            if (_aktifRozet[engineType] == rozetType)
                return CommandResult.Ok($"{rozetType} rozeti zaten yayında.");

            // Sosyal medya çakışma kontrolü
            if (_sosyalMedyaOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT");
                _sosyalMedyaOnAir[engineType] = false;
            }

            string inAnim = GetRozetInAnim(engineType, rozetType);
            engine.Play(scene, inAnim);
            _aktifRozet[engineType] = rozetType;

            return CommandResult.Ok($"{rozetType} rozeti yayına verildi.");
        }

        public CommandResult TakeRozet(VizrtEngineType engineType, RozetType rozetType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_aktifRozet[engineType] == rozetType)
            {
                engine.Play(scene, GetRozetOutAnim(engineType, rozetType));
                _aktifRozet[engineType] = null;
            }

            return CommandResult.Ok($"{rozetType} rozeti yayından alındı.");
        }

        public CommandResult TakeAllRozet(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            if (_aktifRozet[engineType].HasValue)
            {
                engine.Play(scene, GetRozetOutAnim(engineType, _aktifRozet[engineType]!.Value));
                _aktifRozet[engineType] = null;
            }

            return CommandResult.Ok("Tüm rozetler yayından alındı.");
        }
        // ─── WHATSAPP ─────────────────────────────────────────────

        public CommandResult SendWhatsapp(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // Sosyal medya açıksa kapat
            if (_sosyalMedyaOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$SOSYAL_MEDYA_DONUSUMLU$OUT");
                _sosyalMedyaOnAir[engineType] = false;
                Thread.Sleep(500);
            }

            engine.Play(scene, "KJ_TUM$TELEFON_WHATSAPP$IN");
            _whatsappOnAir[engineType] = true;
            return CommandResult.Ok("Whatsapp yayına verildi.");
        }

        public CommandResult TakeWhatsapp(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            if (_whatsappOnAir[engineType])
            {
                engine.Play(_kjScenePath[engineType], "KJ_TUM$TELEFON_WHATSAPP$OUT");
                _whatsappOnAir[engineType] = false;
            }
            return CommandResult.Ok("Whatsapp yayından alındı.");
        }
        public CommandResult SendRoll(VizrtEngineType engineType, string tesekkurYazisi, List<(string Baslik, string Yazi)> satirlar, List<string> sponsorlar)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // 0. EKRANI TEMİZLE VE BEKLE
            TakeAll(engineType);
            Thread.Sleep(1000); // Açık olan grafiklerin çıkış animasyonu için 1 saniye bekle

            int doluSatirSayisi = 0;
            int vizrtKapasite = 24;
            var trCulture = new System.Globalization.CultureInfo("tr-TR");

            // 1. İSİM VE ÜNVANLARI GÖNDER
            for (int i = 0; i < vizrtKapasite; i++)
            {
                int sira = i + 1;
                string unvan = "";
                string isim = "";

                if (satirlar != null && i < satirlar.Count)
                {
                    // Verileri alırken Türkçe karakter kurallarına göre büyütüyoruz
                    unvan = (satirlar[i].Baslik ?? "").ToUpper(trCulture);
                    isim = (satirlar[i].Yazi ?? "").ToUpper(trCulture);
                }

                // baslik konteynerlarına Unvan, yazi konteynerlarına İsim atıyoruz
                engine.SetObjectText(scene, $"baslik{sira}", unvan);
                engine.SetObjectText(scene, $"yazi{sira}", isim);

                if (!string.IsNullOrWhiteSpace(unvan) || !string.IsNullOrWhiteSpace(isim))
                {
                    doluSatirSayisi++;
                    engine.Visibility(scene, $"baslik{sira}", true);
                    engine.Visibility(scene, $"yazi{sira}", true);
                }
                else
                {
                    engine.Visibility(scene, $"baslik{sira}", false);
                    engine.Visibility(scene, $"yazi{sira}", false);
                }
            }

            // 2. TEŞEKKÜR METNİNİ GÖNDER
            engine.SetObjectText(scene, "tesekkur", (tesekkurYazisi ?? "").ToUpper(trCulture));

            // 3. SPONSOR/REKLAM LOGOLARINI GÖNDER
            string klasorYolu = @"D:\SHOWTV_REJI_DATA\ROLL\";
            for (int k = 1; k <= 5; k++)
            {
                if (sponsorlar != null && (k - 1) < sponsorlar.Count)
                {
                    string resimAdi = sponsorlar[k - 1];
                    string tamResimYolu = klasorYolu + resimAdi;

                    engine.Send($"SCENE*{scene}*TREE*$reklam_image_{k}*IMAGE SET {tamResimYolu}");
                    engine.Visibility(scene, $"reklam_image_{k}", true);
                }
                else
                {
                    engine.Visibility(scene, $"reklam_image_{k}", false);
                }
            }

            // 4. DİNAMİK Y EKSENİ (BİTİŞ MESAFESİ) HESAPLAMA
            int tesekkurSatirDegeri = string.IsNullOrWhiteSpace(tesekkurYazisi) ? 0 : 3;
            int reklamSatirDegeri = (sponsorlar?.Count ?? 0) * 2;

            int toplamSanalSatir = doluSatirSayisi + tesekkurSatirDegeri + reklamSatirDegeri;
            if (toplamSanalSatir == 0) toplamSanalSatir = 1;

            double targetY = 490.0 + ((toplamSanalSatir - 12) * 48.0);
            string strY = targetY.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            string keyframePosCmd = $"SCENE*{scene}*TREE*$TEXT*ANIMATION*Position*KEY*$roll_text_pos*XYZ SET 0.0 {strY} 0.0";
            engine.Send(keyframePosCmd);

            // 5. ANİMASYONU BAŞLAT
            engine.Play(scene, "KJ_TUM$ROLL$IN");

            _log.Log("Roll", "Roll yayına verildi.", $"Dolu Satır: {doluSatirSayisi}, Sponsor: {sponsorlar?.Count ?? 0}");
            return CommandResult.Ok("Roll yayına verildi.");
        }

        public CommandResult TakeRoll(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // 🌟 AKILLI YOL SEÇİCİ 🌟
            bool isCumartesi = scene.Contains("CUMARTESI_SURPRIZI") || scene.Contains("PAZAR");
            string outAnim = isCumartesi ? "KJ$ROLL$OUT" : "KJ_TUM$ROLL$OUT";

            engine.Play(scene, outAnim);

            _log.Log("Roll", "Roll yayından alındı.", engineType.ToString());
            return CommandResult.Ok("Roll yayından alındı.");
        }

        public CommandResult SendRollTekMetin(VizrtEngineType engineType, string rollMetni, List<string> sponsorlar)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // 0. EKRANI TEMİZLE VE BEKLE
            TakeAll(engineType);

            // 1. METNİ GÖNDER (Büyük harfe çevirerek)
            var trCulture = new System.Globalization.CultureInfo("tr-TR");
            string gonderilecekMetin = (rollMetni ?? "").ToUpper(trCulture);

            engine.SetObjectText(scene, "ROLL_TEXT", gonderilecekMetin);

            // 2. SPONSOR/REKLAM LOGOLARINI GÖNDER (Eski mantığın aynısı)
            string klasorYolu = @"D:\SHOWTV_REJI_DATA\ROLL\";
            for (int k = 1; k <= 5; k++)
            {
                if (sponsorlar != null && (k - 1) < sponsorlar.Count)
                {
                    string resimAdi = sponsorlar[k - 1];
                    string tamResimYolu = klasorYolu + resimAdi;

                    engine.Send($"SCENE*{scene}*TREE*$reklam_image_{k}*IMAGE SET {tamResimYolu}");
                    engine.Visibility(scene, $"reklam_image_{k}", true);
                }
                else
                {
                    engine.Visibility(scene, $"reklam_image_{k}", false);
                }
            }

            // 3. DİNAMİK Y EKSENİ (BİTİŞ MESAFESİ) HESAPLAMA
            // Metindeki enter (\n) sayısını bulup ona göre yüksekliği ayarlıyoruz
            int satirSayisi = gonderilecekMetin.Split('\n').Length;
            int reklamSatirDegeri = (sponsorlar?.Count ?? 0) * 2;
            int toplamSanalSatir = satirSayisi + reklamSatirDegeri;
            if (toplamSanalSatir == 0) toplamSanalSatir = 1;

            double targetY = 490.0 + ((toplamSanalSatir - 12) * 48.0);
            string strY = targetY.ToString("0.000", System.Globalization.CultureInfo.InvariantCulture);

            string keyframePosCmd = $"SCENE*{scene}*TREE*$TEXT*ANIMATION*Position*KEY*$roll_text_pos*XYZ SET 0.0 {strY} 0.0";
            engine.Send(keyframePosCmd);

            // 4. ANİMASYONU BAŞLAT
            engine.Play(scene, "KJ$ROLL$IN");

            _log.Log("Roll", "Roll Tek Metin yayına verildi.", $"Satır: {satirSayisi}, Sponsor: {sponsorlar?.Count ?? 0}");
            return CommandResult.Ok("Roll Tek Metin yayına verildi.");
        }

        // ─── KELEBEK ─────────────────────────────────────────────
        // ─── KELEBEK SAHNE YÜKLE ─────────────────────────────────────────────
        public CommandResult KelebekSahneYukle(VizrtEngineType engineType, string sahneYolu)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

            string cleanPath = sahneYolu.TrimStart('/');

            // Sahneyi yükle ve aktif et (Postman'da çalışan syntax)
            engine.Send($"-1 RENDERER*BACK_LAYER SET_OBJECT SCENE*{cleanPath}");
            engine.Send("-1 RENDERER*BACK_LAYER*ACTIVE SET 1");

            return CommandResult.Ok($"Kelebek sahnesi yüklendi: {sahneYolu}");
        }

        // ─── KELEBEK İSİM GÖNDER ─────────────────────────────────────────────
        public CommandResult KelebekIsimGonder(VizrtEngineType engineType, int index, string isim, string title)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected) return CommandResult.Fail($"{engineType} bağlı değil.");

            var trCulture = new System.Globalization.CultureInfo("tr-TR");
            string isimBuyuk = (isim ?? "").Trim().ToUpper(trCulture);
            string titleBuyuk = (title ?? "").Trim().ToUpper(trCulture);

            if (string.IsNullOrWhiteSpace(isimBuyuk))
            {
                // İSİM BOŞ: Eski kodundaki VisibilityBL(false) mantığı
                // İlgili kişinin arka planını gizle ve yazıları sil
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$isimlik_bg_{index}*ACTIVE SET 0");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIM{index}*GEOM*TEXT SET ");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$TITLE{index}*GEOM*TEXT SET ");

                return CommandResult.Ok($"Kelebek {index}. kişi ekrandan gizlendi.");
            }
            else
            {
                // İSİM DOLU: Arka planı göster ve yazıları bas
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$isimlik_bg_{index}*ACTIVE SET 1");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIM{index}*GEOM*TEXT SET {isimBuyuk}");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$TITLE{index}*GEOM*TEXT SET {titleBuyuk}");

                // Maxsize ve Animasyon tetikleme
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIM{index}*FUNCTION*Maxsize*initialize SET");
                engine.Send($"-1 RENDERER*BACK_LAYER*TREE*$ISIMLIK_{index}*FUNCTION*ControlObject*in SET");

                return CommandResult.Ok($"Kelebek isim gönderildi: {isimBuyuk}");
            }
        }

        public CommandResult KelebekKapat(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            // Eski çalışan kodundaki syntax'ın birebir aynısı
            engine.Send("RENDERER*BACK_LAYER SET_OBJECT ");
            engine.Send("RENDERER*BACK_LAYER*ACTIVE SET 0");

            _log.Log("Kelebek", "Kelebek kapatıldı.", engineType.ToString());
            return CommandResult.Ok("Kelebek kapatıldı.");
        }
        
    }
}