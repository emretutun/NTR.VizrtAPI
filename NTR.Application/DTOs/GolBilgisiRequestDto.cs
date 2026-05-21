using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Application.DTOs
{
    public class GolBilgisiRequestDto
    {
        public string OyuncuIsim { get; set; } = string.Empty;
        public string Dakika { get; set; } = string.Empty;
        public string TakimLogo { get; set; } = string.Empty; // Örn: "17378.png"
    }
}