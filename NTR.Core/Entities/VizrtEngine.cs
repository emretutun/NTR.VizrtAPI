using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Entities
{
    public class VizrtEngine
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; } = 6100;
        public bool IsConnected { get; set; }
        public bool HeartbeatEnabled { get; set; }
        public string CurrentScene_Front { get; set; } = string.Empty;
        public string CurrentScene_Middle { get; set; } = string.Empty;
        public string CurrentScene_Back { get; set; } = string.Empty;
    }
}