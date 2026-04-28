using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Application.DTOs
{
    public class IsimlikRequestDto
    {
        public string Isim { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public bool TelefonMu { get; set; } = false;
    }
}