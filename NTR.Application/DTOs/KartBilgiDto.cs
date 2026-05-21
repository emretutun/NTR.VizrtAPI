using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Application.DTOs
{
    public class KartBilgiDto
    {
        public string Isim { get; set; } = string.Empty;
        public string TakimLogo { get; set; } = string.Empty;

        // 1: Sarı Kart, 2: Kırmızı Kart, 3: Çift Sarıdan Kırmızı
        public int KartTipi { get; set; }
    }
}