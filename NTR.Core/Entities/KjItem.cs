using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Enums;

namespace NTR.Core.Entities
{
    public class KjItem
    {
        public int Id { get; set; }
        public int HaberId { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public KjType Type { get; set; } = KjType.Tekli;
        public string Text1 { get; set; } = string.Empty;
        public string Text2 { get; set; } = string.Empty;
        public int Sira { get; set; }
        public DateTime OlusturmaTarihi { get; set; } = DateTime.Now;
    }
}