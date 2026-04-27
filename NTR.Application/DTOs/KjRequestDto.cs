using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NTR.Core.Enums;

namespace NTR.Application.DTOs
{
    public class KjRequestDto
    {
        public KjType Type { get; set; } = KjType.Tekli;
        public string Text1 { get; set; } = string.Empty;
        public string Text2 { get; set; } = string.Empty;
    }
}