using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Interfaces
{
    public interface IVizrtConnection
    {
        bool IsConnected { get; }
        string IP { get; set; }
        int Port { get; set; }
        string ParentName { get; set; }

        bool Connect();
        bool Disconnect();
        bool Send(params object[] prmList);
    }
}