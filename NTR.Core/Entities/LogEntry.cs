using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Entities
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Tarih { get; set; } = DateTime.Now;
        public string Seviye { get; set; } = "INFO"; // INFO, WARNING, ERROR
        public string Kaynak { get; set; } = string.Empty;
        public string Mesaj { get; set; } = string.Empty;
        public string? Detay { get; set; }
    }
}