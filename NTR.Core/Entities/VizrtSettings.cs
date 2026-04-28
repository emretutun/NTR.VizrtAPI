using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Entities
{
    public class VizrtSettings
    {
        public EngineSettings Engines { get; set; } = new();
        public SceneSettings Scenes { get; set; } = new();
    }

    public class EngineSettings
    {
        public EngineConnectionSettings Reji { get; set; } = new();
        public EngineConnectionSettings Grafik1 { get; set; } = new();
        public EngineConnectionSettings Grafik2 { get; set; } = new();
    }

    public class EngineConnectionSettings
    {
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; } = 6100;
    }

    public class SceneSettings
    {
        public string KJScene { get; set; } = string.Empty;
        public Dictionary<string, string> Kelebek { get; set; } = new();
    }
}