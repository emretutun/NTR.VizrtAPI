using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Application.DTOs
{
    public class KelebekSahneDto
    {
        public string SahneYolu { get; set; } = string.Empty;
    }

    public class KelebekIsimDto
    {
        public int Index { get; set; }
        public string Isim { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
    }
}
