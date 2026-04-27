using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NTR.Core.Entities;
using NTR.Core.Enums;

namespace NTR.Core.Interfaces
{
    public interface IVizrtService
    {
        // Connection
        CommandResult Connect(VizrtEngineType engineType, string ip);
        CommandResult Disconnect(VizrtEngineType engineType);
        VizrtEngine GetEngineStatus(VizrtEngineType engineType);
        List<VizrtEngine> GetAllEngineStatus();

        // KJ
        CommandResult SendKj(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "");
        CommandResult TakeKj(VizrtEngineType engineType);
        CommandResult TakeAll(VizrtEngineType engineType);

        // Yer
        CommandResult SendYer(VizrtEngineType engineType, string text);
        CommandResult TakeYer(VizrtEngineType engineType);

        // Sosyal Medya
        CommandResult SendSosyalMedya(VizrtEngineType engineType);
        CommandResult TakeSosyalMedya(VizrtEngineType engineType);

        // Isimlik
        CommandResult SendIsimlik(VizrtEngineType engineType, string isim);
        CommandResult TakeIsimlik(VizrtEngineType engineType);

        // Raw Command
        CommandResult SendRawCommand(VizrtEngineType engineType, string command);

        // Scene
        CommandResult LoadScene(VizrtEngineType engineType, string scenePath);
    }
}