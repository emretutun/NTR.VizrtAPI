using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NTR.Core.Interfaces
{
    public interface IConnectionMonitor
    {
        event Action<string, bool> OnConnectionStateChanged; // engineName, isConnected
    }
}
