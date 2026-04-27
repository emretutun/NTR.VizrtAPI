using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Enums
{
    public enum VizrtEngineType
    {
        Reji = 1,
        Grafik1 = 2,
        Grafik2 = 3
    }

    public enum ConnectionStatus
    {
        Connected,
        Disconnected,
        Connecting
    }

    public enum KjType
    {
        Tekli,
        Ciftli,
        Uzun
    }
}