using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NTR.RejiClient.Models
{
    public class RollRequestDto
    {
        public string TesekkurYazisi { get; set; } = string.Empty;
        public List<RollSatirDto> Satirlar { get; set; } = new List<RollSatirDto>();
        public List<string> Sponsorlar { get; set; } = new List<string>();
    }

    public class RollSatirDto
    {
        public string Baslik { get; set; } = string.Empty;
        public string Yazi { get; set; } = string.Empty;
    }
}