using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Application.DTOs
{
    public class ConnectRequestDto
    {
        public string IP { get; set; } = string.Empty;
        public int Port { get; set; } = 6100;
    }
}