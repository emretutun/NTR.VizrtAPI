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
        CommandResult SendKj(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "", RozetType? rozet = null); CommandResult TakeKj(VizrtEngineType engineType);
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

        // Telefon İsimlik
        CommandResult SendTelefonIsimlik(VizrtEngineType engineType, string isim, string title, bool telefonMu);
        CommandResult TakeTelefonIsimlik(VizrtEngineType engineType);

        // Muhabir Kamera
        CommandResult SendMuhabirKamera(VizrtEngineType engineType, string muhabir, string kameraman);
        CommandResult TakeMuhabirKamera(VizrtEngineType engineType);

        // Canlı
        CommandResult SendCanli(VizrtEngineType engineType);
        CommandResult TakeCanli(VizrtEngineType engineType);

        // Canlı Yer
        CommandResult SendCanliYer(VizrtEngineType engineType, string text);
        CommandResult TakeCanliYer(VizrtEngineType engineType);

        // Rozetler
        CommandResult SendRozet(VizrtEngineType engineType, RozetType rozetType);
        CommandResult TakeRozet(VizrtEngineType engineType, RozetType rozetType);
        CommandResult TakeAllRozet(VizrtEngineType engineType);

        // Whatsapp
        CommandResult SendWhatsapp(VizrtEngineType engineType);
        CommandResult TakeWhatsapp(VizrtEngineType engineType);
    }
}