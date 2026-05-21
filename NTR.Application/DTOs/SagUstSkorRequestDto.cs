using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Application.DTOs
{
    public class SagUstSkorRequestDto
    {
        public string EvTakimIsim { get; set; } = string.Empty;
        public string DepTakimIsim { get; set; } = string.Empty;
        public string EvSkor { get; set; } = "0";
        public string DepSkor { get; set; } = "0";

        // Forma renklerini sonradan ekleyebiliriz, şimdilik sadece yazılar.
    }
}