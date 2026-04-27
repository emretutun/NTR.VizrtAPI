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

        public VizrtService()
        {
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

            _kjScenePath = new Dictionary<VizrtEngineType, string>
            {
                { VizrtEngineType.Reji,    "" },
                { VizrtEngineType.Grafik1, "" },
                { VizrtEngineType.Grafik2, "" }
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

        public CommandResult SendKj(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "")
        {
            var engine = GetEngine(engineType);
            if (!engine.IsConnected)
                return CommandResult.Fail($"{engineType} bağlı değil.");

            string scene = _kjScenePath[engineType];
            if (string.IsNullOrEmpty(scene))
                return CommandResult.Fail("Scene path tanımlı değil. Önce LoadScene çağırın.");

            if (string.IsNullOrWhiteSpace(text1))
                return CommandResult.Fail("Text1 boş olamaz.");

            switch (kjType)
            {
                case KjType.Tekli:
                    return SendKjTekli(engine, scene, engineType, text1);

                case KjType.Ciftli:
                    if (string.IsNullOrWhiteSpace(text2))
                        return CommandResult.Fail("Çift satır KJ için Text2 gereklidir.");
                    return SendKjCiftli(engine, scene, engineType, text1, text2);

                case KjType.Uzun:
                    if (string.IsNullOrWhiteSpace(text2))
                        return CommandResult.Fail("Uzun KJ için Text2 gereklidir.");
                    return SendKjUzun(engine, scene, engineType, text1, text2);

                default:
                    return CommandResult.Fail("Geçersiz KJ tipi.");
            }
        }

        private CommandResult SendKjTekli(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1)
        {
            if (_kjCiftOnAir[engineType]) { engine.Play(scene, "KJ_CIFT$OUT"); _kjCiftOnAir[engineType] = false; Thread.Sleep(1500); }
            if (_kjUzunOnAir[engineType]) { engine.Play(scene, "KJ_UZUN$OUT"); _kjUzunOnAir[engineType] = false; Thread.Sleep(1500); }

            if (!_kjTekOnAir[engineType])
            {
                engine.SetObjectText(scene, "KJ_TEK$TEXT1", text1);
                engine.Play(scene, "KJ_BANT");
                engine.Play(scene, "TEXT1");
                _kjTekOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, "KJ_TEK$TEXT1", text1);
                    engine.Play(scene, "TEXT1");
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, "KJ_TEK$TEXT2", text1);
                    engine.Play(scene, "TEXT2");
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Tekli KJ yayına verildi.");
        }

        private CommandResult SendKjCiftli(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            if (_kjTekOnAir[engineType]) { engine.Play(scene, "KJ_TEK$OUT"); _kjTekOnAir[engineType] = false; Thread.Sleep(1500); }
            if (_kjUzunOnAir[engineType]) { engine.Play(scene, "KJ_UZUN$OUT"); _kjUzunOnAir[engineType] = false; Thread.Sleep(1500); }

            if (!_kjCiftOnAir[engineType])
            {
                engine.SetObjectText(scene, "TEXT_UST_1", text1);
                engine.SetObjectText(scene, "TEXT_ALT_1", text2);
                engine.Play(scene, "KJ_CIFT_BANT");
                engine.Play(scene, "TEXT_UST_1");
                engine.Play(scene, "TEXT_ALT_1");
                _kjCiftOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, "TEXT_UST_1", text1);
                    engine.SetObjectText(scene, "TEXT_ALT_1", text2);
                    engine.Play(scene, "TEXT_UST_1");
                    engine.Play(scene, "TEXT_ALT_1");
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, "TEXT_UST_2", text1);
                    engine.SetObjectText(scene, "TEXT_ALT_2", text2);
                    engine.Play(scene, "TEXT_UST_2");
                    engine.Play(scene, "TEXT_ALT_2");
                    _nextTextAnimIndex[engineType] = 1;
                }
            }
            return CommandResult.Ok("Çift satır KJ yayına verildi.");
        }

        private CommandResult SendKjUzun(IVizrtEngine engine, string scene, VizrtEngineType engineType, string text1, string text2)
        {
            if (_kjTekOnAir[engineType]) { engine.Play(scene, "KJ_TEK$OUT"); _kjTekOnAir[engineType] = false; Thread.Sleep(1500); }
            if (_kjCiftOnAir[engineType]) { engine.Play(scene, "KJ_CIFT$OUT"); _kjCiftOnAir[engineType] = false; Thread.Sleep(1500); }

            if (!_kjUzunOnAir[engineType])
            {
                engine.SetObjectText(scene, "TEXT_UZUN_UST_1", text1);
                engine.SetObjectText(scene, "TEXT_UZUN_ALT_1", text2);
                engine.Play(scene, "KJ_UZUN_BANT");
                engine.Play(scene, "KJ_UZUN_TEXT1");
                _kjUzunOnAir[engineType] = true;
                _nextTextAnimIndex[engineType] = 2;
            }
            else
            {
                if (_nextTextAnimIndex[engineType] == 1)
                {
                    engine.SetObjectText(scene, "TEXT_UZUN_UST_1", text1);
                    engine.SetObjectText(scene, "TEXT_UZUN_ALT_1", text2);
                    engine.Play(scene, "KJ_UZUN_TEXT1");
                    _nextTextAnimIndex[engineType] = 2;
                }
                else
                {
                    engine.SetObjectText(scene, "TEXT_UZUN_UST_2", text1);
                    engine.SetObjectText(scene, "TEXT_UZUN_ALT_2", text2);
                    engine.Play(scene, "KJ_UZUN_TEXT2");
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

            engine.SetObjectText(scene, "yer_text", text);
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
    }
}