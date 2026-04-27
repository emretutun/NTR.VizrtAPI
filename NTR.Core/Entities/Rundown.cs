using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Entities
{
    public class Rundown
    {
        public int Id { get; set; }
        public string Ad { get; set; } = string.Empty;
        public string Tarih { get; set; } = string.Empty;
        public string Saat { get; set; } = string.Empty;
        public string Kanal { get; set; } = string.Empty;
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
        public List<Haber> Haberler { get; set; } = new List<Haber>();
    }
}