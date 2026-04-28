using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Enums;
using NTR.Core.Interfaces;
using NTR.Infrastructure.Vizrt;

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


        public VizrtService(VizrtSettings vizrtSettings)
        {
            _settings = vizrtSettings;
            _telefonIsimOnAir = InitBoolDict();
            _muhabirKameraOnAir = InitBoolDict();
            _canliOnAir = InitBoolDict();
            _canliYerOnAir = InitBoolDict();
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
            return result
                ? CommandResult.Ok($"{engineType} bağlantısı kuruldu. IP: {ip}")
                : CommandResult.Fail($"{engineType} bağlantısı kurulamadı. IP: {ip}");
        }

        public CommandResult Disconnect(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            bool result = engine.Disconnect();
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
                return CommandResult.Fail($"{engineType} bağlı değil.");

            _kjScenePath[engineType] = scenePath;
            engine.LoadScene(scenePath);
            return CommandResult.Ok($"Scene yüklendi: {scenePath}");
        }

        // ─── KJ ──────────────────────────────────────────────────

        public CommandResult SendKj(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "", RozetType? rozet = null)
        {
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
                    engine.Play(scene, GetRozetOutAnim(_aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }

                // Yeni rozeti aç (zaten açık değilse)
                if (_aktifRozet[engineType] != rozet)
                {
                    engine.Play(scene, GetRozetInAnim(rozet.Value));
                    _aktifRozet[engineType] = rozet;
                }
            }
            else
            {
                // Rozet null → aktif rozet varsa kapat
                if (_aktifRozet[engineType].HasValue)
                {
                    engine.Play(scene, GetRozetOutAnim(_aktifRozet[engineType]!.Value));
                    _aktifRozet[engineType] = null;
                }
            }

            return kjResult;
        }

        private CommandResult SendKjTekli(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1)
        {
            // Farklı KJ türü açıksa kapat
            if (_kjCiftOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$KJ_CIFT$OUT");
                _kjCiftOnAir[engineType] = false;
                Thread.Sleep(1500);
            }
            if (_kjUzunOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$KJ_UZUN$OUT");
                _kjUzunOnAir[engineType] = false;
                Thread.Sleep(1500);
            }

            if (!_kjTekOnAir[engineType])
            {
                // İlk kez açılıyor → metni set et, IN direktörünü çalıştır
                engine.SetObjectText(scene, "KJ_TEK$SATIR_1$TEXT1", text1);
                engine.Play(scene, "KJ_TUM$KJ_TEK$IN");
                _kjTekOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                // Zaten açık → sadece yazı değişecek
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, "KJ_TEK$SATIR_1$TEXT1", text1);
                    engine.Play(scene, "KJ_TUM$KJ_TEK$TEXT1");
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, "KJ_TEK$SATIR_2$TEXT2", text1);
                    engine.Play(scene, "KJ_TUM$KJ_TEK$TEXT2");
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Tekli KJ yayına verildi.");
        }

        private CommandResult SendKjCiftli(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            // Farklı KJ türü açıksa kapat
            if (_kjTekOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$KJ_TEK$OUT");
                _kjTekOnAir[engineType] = false;
                Thread.Sleep(1500);
            }
            if (_kjUzunOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$KJ_UZUN$OUT");
                _kjUzunOnAir[engineType] = false;
                Thread.Sleep(1500);
            }

            if (!_kjCiftOnAir[engineType])
            {
                // İlk kez açılıyor → metni set et, IN direktörünü çalıştır
                engine.SetObjectText(scene, "KJ_CIFT$SATIR_1$TEXT_UST_1", text1);
                engine.SetObjectText(scene, "KJ_CIFT$SATIR_1$TEXT_ALT_1", text2);
                engine.Play(scene, "KJ_TUM$KJ_CIFT$IN");
                _kjCiftOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                // Zaten açık → sadece yazı değişecek
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, "KJ_CIFT$SATIR_1$TEXT_UST_1", text1);
                    engine.SetObjectText(scene, "KJ_CIFT$SATIR_1$TEXT_ALT_1", text2);
                    engine.Play(scene, "KJ_TUM$KJ_CIFT$TEXT_UST_1");
                    engine.Play(scene, "KJ_TUM$KJ_CIFT$TEXT_ALT_1");
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, "KJ_CIFT$SATIR_2$TEXT_UST_2", text1);
                    engine.SetObjectText(scene, "KJ_CIFT$SATIR_2$TEXT_ALT_2", text2);
                    engine.Play(scene, "KJ_TUM$KJ_CIFT$TEXT_UST_2");
                    engine.Play(scene, "KJ_TUM$KJ_CIFT$TEXT_ALT_2");
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Çift satır KJ yayına verildi.");
        }

        private CommandResult SendKjUzun(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            // Farklı KJ türü açıksa kapat
            if (_kjTekOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$KJ_TEK$OUT");
                _kjTekOnAir[engineType] = false;
                Thread.Sleep(1500);
            }
            if (_kjCiftOnAir[engineType])
            {
                engine.Play(scene, "KJ_TUM$KJ_CIFT$OUT");
                _kjCiftOnAir[engineType] = false;
                Thread.Sleep(1500);
            }

            if (!_kjUzunOnAir[engineType])
            {
                // İlk kez açılıyor → SATIR_1'e yaz, IN direktörünü çalıştır
                engine.SetObjectText(scene, "KJ_UZUN$SATIR_1$TEXT_UZUN_UST_1", text1);
                engine.SetObjectText(scene, "KJ_UZUN$SATIR_1$TEXT_UZUN_ALT_1", text2);
                engine.Play(scene, "KJ_TUM$KJ_UZUN$IN");
                _kjUzunOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                // Zaten açık → SATIR_1 ve SATIR_2 arasında geçiş yap
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    // SATIR_1'e yaz, TEXT1 direktörünü çalıştır
                    engine.SetObjectText(scene, "KJ_UZUN$SATIR_1$TEXT_UZUN_UST_1", text1);
                    engine.SetObjectText(scene, "KJ_UZUN$SATIR_1$TEXT_UZUN_ALT_1", text2);
                    engine.Play(scene, "KJ_TUM$KJ_UZUN$KJ_UZUN_TEXT1");
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    // SATIR_2'ye yaz, TEXT2 direktörünü çalıştır
                    engine.SetObjectText(scene, "KJ_UZUN$SATIR_2$TEXT_UZUN_UST_2", text1);
                    engine.SetObjectText(scene, "KJ_UZUN$SATIR_2$TEXT_UZUN_ALT_2", text2);
                    engine.Play(scene, "KJ_TUM$KJ_UZUN$KJ_UZUN_TEXT2");
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

            if (_kjTekOnAir[engineType]) { engine.Play(scene, "KJ_TEK$OUT"); _kjTekOnAir[engineType] = false; }
            if (_kjCiftOnAir[engineType]) { engine.Play(scene, "KJ_CIFT$OUT"); _kjCiftOnAir[engineType] = false; }
            if (_kjUzunOnAir[engineType]) { engine.Play(scene, "KJ_UZUN$OUT"); _kjUzunOnAir[engineType] = false; }

            _nextTextAnimIndex[engineType] = 1;
            return CommandResult.Ok("KJ yayından alındı.");
        }

        public CommandResult TakeAll(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            TakeKj(engineType);
            TakeSosyalMedya(engineType);
            TakeYer(engineType);
            TakeIsimlik(engineType);

            engine.StageToStart("RENDERER*MAIN_LAYER");
            return CommandResult.Ok("Tümü yayından alındı.");
        }

        // ─── YER ─────────────────────────────────────────────────

        public CommandResult SendYer(VizrtEngineType engineType, string text)
        {
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

            if (_sosyalMedyaOnAir[engineType])
            {
                engine.Play(_kjScenePath[engineType], "SOSYAL_MEDYA_DONUSUMLU$OUT");
                _sosyalMedyaOnAir[engineType] = false;
            }
            return CommandResult.Ok("Sosyal medya yayından alındı.");
        }

        // ─── ISİMLİK ─────────────────────────────────────────────

        public CommandResult SendIsimlik(VizrtEngineType engineType, string isim)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            engine.SetObjectText(scene, "ISIMLIK$isim", isim);
            engine.Play(scene, "ISIMLIK$IN");
            _isimlikOnAir[engineType] = true;
            return CommandResult.Ok("İsimlik yayına verildi.");
        }

        public CommandResult TakeIsimlik(VizrtEngineType engineType)
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            if (_isimlikOnAir[engineType])
            {
                engine.Play(_kjScenePath[engineType], "ISIMLIK$OUT");
                _isimlikOnAir[engineType] = false;
            }
            return CommandResult.Ok("İsimlik yayından alındı.");
        }

        // ─── RAW COMMAND ──────────────────────────────────────────

        public CommandResult SendRawCommand(VizrtEngineType engineType, string command)
        {
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

        private string GetRozetInAnim(RozetType rozetType)
        {
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

        private string GetRozetOutAnim(RozetType rozetType)
        {
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
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];

            // Farklı bir rozet açıksa kapat
            if (_aktifRozet[engineType].HasValue && _aktifRozet[engineType] != rozetType)
            {
                string outAnim = GetRozetOutAnim(_aktifRozet[engineType]!.Value);
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

            string inAnim = GetRozetInAnim(rozetType);
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
                engine.Play(scene, GetRozetOutAnim(rozetType));
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
                engine.Play(scene, GetRozetOutAnim(_aktifRozet[engineType]!.Value));
                _aktifRozet[engineType] = null;
            }

            return CommandResult.Ok("Tüm rozetler yayından alındı.");
        }

    }
}