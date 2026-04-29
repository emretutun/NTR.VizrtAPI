using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.RejiClient.Models
{
    public class Haber
    {
        public int Id { get; set; }
        public string Baslik { get; set; } = string.Empty;
        public string Icerik { get; set; } = string.Empty;
        public int RundownId { get; set; }
        public int Sira { get; set; }
        public List<KjItem> KjListesi { get; set; } = new();
    }
}