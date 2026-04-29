using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.RejiClient.Models
{
    public class KjItem
    {
        public int Id { get; set; }
        public int HaberId { get; set; }
        public string Aciklama { get; set; } = string.Empty;
        public int Type { get; set; }
        public string Text1 { get; set; } = string.Empty;
        public string Text2 { get; set; } = string.Empty;
        public int Sira { get; set; }
    }
}