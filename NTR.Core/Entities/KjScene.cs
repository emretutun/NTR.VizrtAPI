using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Enums;

namespace NTR.Core.Entities
{
    public class KjScene
    {
        public string ScenePath { get; set; } = string.Empty;
        public KjType Type { get; set; }
        public string Text1 { get; set; } = string.Empty;
        public string Text2 { get; set; } = string.Empty;
        public bool IsOnAir { get; set; }
    }
}