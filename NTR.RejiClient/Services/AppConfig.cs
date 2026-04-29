using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NTR.RejiClient.Services
{
    public class AppConfig
    {
        public string ApiBaseUrl { get; set; } = "https://localhost:7043";
        public string ApiKey { get; set; } = "ntr-vizrt-2026-secret-key";
        public string EngineType { get; set; } = "Reji";
        public string LastIp { get; set; } = "127.0.0.1";
        public int LastPort { get; set; } = 6100;

        private static readonly string ConfigPath = Path.Combine(
            Application.StartupPath, "config.json");

        public static AppConfig Load()
        {
            try
            {
                if (File.Exists(ConfigPath))
                {
                    string json = File.ReadAllText(ConfigPath);
                    return JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
                }
            }
            catch { }
            return new AppConfig();
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
            }
            catch { }
        }

        public bool IsConfigured =>
            !string.IsNullOrEmpty(ApiBaseUrl) &&
            !string.IsNullOrEmpty(ApiKey);
    }
}