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
    public enum RozetType
    {
        AzSonra = 0,
        AzSonraDsf = 1,
        AzSonraDsf2 = 2,
        SonDakika = 3,
        OzelHaber = 4,
        WhatsappIhbar = 5,
        SicakGelisme = 6,

    }
}