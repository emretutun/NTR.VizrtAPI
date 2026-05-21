using System;
using System.Collections.Generic;
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

        // KJ — Sleep içerdiği için async
        Task<CommandResult> SendKjAsync(VizrtEngineType engineType, KjType kjType, string text1, string text2 = "", RozetType? rozet = null);
        Task<CommandResult> TakeKjAsync(VizrtEngineType engineType);
        Task<CommandResult> TakeAllAsync(VizrtEngineType engineType);

        // Yer — Sleep içerdiği için async
        Task<CommandResult> SendYerAsync(VizrtEngineType engineType, string text);
        CommandResult TakeYer(VizrtEngineType engineType);

        // Sosyal Medya — Sleep içerdiği için async
        Task<CommandResult> SendSosyalMedyaAsync(VizrtEngineType engineType);
        CommandResult TakeSosyalMedya(VizrtEngineType engineType);

        // Isimlik
        CommandResult SendIsimlik(VizrtEngineType engineType, string isim);
        CommandResult TakeIsimlik(VizrtEngineType engineType);

        // Raw Command
        CommandResult SendRawCommand(VizrtEngineType engineType, string command);

        // Scene
        CommandResult LoadScene(VizrtEngineType engineType, string scenePath);

        // Telefon İsimlik — Sleep içerdiği için async
        Task<CommandResult> SendTelefonIsimlikAsync(VizrtEngineType engineType, string isim, string title, bool telefonMu);
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

        // Whatsapp — Sleep içerdiği için async
        Task<CommandResult> SendWhatsappAsync(VizrtEngineType engineType);
        CommandResult TakeWhatsapp(VizrtEngineType engineType);

        // Roll — Sleep içerdiği için async
        Task<CommandResult> SendRollAsync(VizrtEngineType engineType, string tesekkurYazisi, List<(string Baslik, string Yazi)> satirlar, List<string> sponsorlar);
        CommandResult TakeRoll(VizrtEngineType engineType);
        Task<CommandResult> SendRollTekMetinAsync(VizrtEngineType engineType, string rollMetni, List<string> sponsorlar);

        // Kelebek
        CommandResult KelebekSahneYukle(VizrtEngineType engineType, string sahneYolu);
        CommandResult KelebekIsimGonder(VizrtEngineType engineType, int index, string isim, string title);
        CommandResult KelebekKapat(VizrtEngineType engineType);

        CommandResult SendOyuncuDegisiklik(VizrtEngineType engineType, string girenOyuncu, string cikanOyuncu, string takimLogo);
        CommandResult TakeOyuncuDegisiklik(VizrtEngineType engineType);

        CommandResult SendKartBilgi(VizrtEngineType engineType, string isim, string takimLogo, int kartTipi);
        CommandResult TakeKartBilgi(VizrtEngineType engineType);

        CommandResult SendIstatistik(VizrtEngineType engineType, string evDeger, string depDeger, string baslik, string evLogo, string depLogo);
        CommandResult TakeIstatistik(VizrtEngineType engineType);

        Task<CommandResult> SendSagUstSkorAsync(VizrtEngineType engineType, string evTakim, string depTakim, string evSkor, string depSkor);
        Task<CommandResult> TakeSagUstSkorAsync(VizrtEngineType engineType);

        Task<CommandResult> SendUzatmaAsync(VizrtEngineType engineType, string sure);
        Task<CommandResult> TakeUzatmaAsync(VizrtEngineType engineType);
        Task<CommandResult> SendGolBilgisiAsync(VizrtEngineType engineType, string oyuncuIsim, string dakika, string takimLogo);
        Task<CommandResult> TakeGolBilgisiAsync(VizrtEngineType engineType);


    }
}