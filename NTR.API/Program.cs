using NTR.API.Hubs;
using NTR.API.Middleware;
using NTR.Application.Extensions;
using NTR.Core.Entities;
using NTR.API.BackgroundServices;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();
builder.Services.AddHostedService<VizrtConnectionMonitor>();

// VizrtSettings'i oku
var vizrtSettings = builder.Configuration
    .GetSection("VizrtSettings")
    .Get<VizrtSettings>() ?? new VizrtSettings();

builder.Services.AddSingleton(vizrtSettings);

// Data klasörü yolu
string dataPath = Path.Combine(AppContext.BaseDirectory, "Data");
Directory.CreateDirectory(dataPath);

// Servisleri kaydet
builder.Services.AddApplicationServices(dataPath, vizrtSettings);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.MapHub<VizrtHub>("/hubs/vizrt");
// API Key Middleware
app.UseMiddleware<ApiKeyMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();


app.MapControllers();

app.Run();



























// ═══════════════════════════════════════════
// ENGINE
// ═══════════════════════════════════════════

// Tüm engine durumu
// GET https://localhost:7043/api/engine/status

// Tek engine durumu (Reji, Grafik1, Grafik2)
// GET https://localhost:7043/api/engine/status/Reji

// Bağlan
// POST https://localhost:7043/api/engine/Reji/connect
// Body: { "ip": "127.0.0.1", "port": 6100 }

// Bağlantıyı kes
// POST https://localhost:7043/api/engine/Reji/disconnect

// Scene yükle
// POST https://localhost:7043/api/engine/Reji/scene/load
// Body: "SHOW_TV_2025/REJI/YENI_SAYFA/KJ/KJ_TUM_V9"

// Ham komut gönder
// POST https://localhost:7043/api/engine/Reji/raw
// Body: { "command": "RENDERER*STAGE TO_START" }


// ═══════════════════════════════════════════
// KJ
// ═══════════════════════════════════════════

// Tekli KJ ver (rozetsiz)
// POST https://localhost:7043/api/kj/Reji/ver
// Body: { "type": 0, "text1": "Üst yazı", "text2": "", "rozet": null }

// Tekli KJ ver (rozetli)
// POST https://localhost:7043/api/kj/Reji/ver
// Body: { "type": 0, "text1": "Üst yazı", "text2": "", "rozet": 3 }
// rozet: 0=AzSonra, 1=AzSonraDsf, 2=AzSonraDsf2, 3=SonDakika, 4=OzelHaber, 5=WhatsappIhbar

// Çiftli KJ ver
// POST https://localhost:7043/api/kj/Reji/ver
// Body: { "type": 1, "text1": "Üst yazı", "text2": "Alt yazı", "rozet": null }

// Uzun KJ ver
// POST https://localhost:7043/api/kj/Reji/ver
// Body: { "type": 2, "text1": "Üst yazı", "text2": "Alt yazı", "rozet": null }

// KJ al
// POST https://localhost:7043/api/kj/Reji/al

// Tümünü al
// POST https://localhost:7043/api/kj/Reji/tumunu-al

// ─── YER ─────────────────────────────────

// Yer KJ ver
// POST https://localhost:7043/api/kj/Reji/yer/ver
// Body: { "text": "İSTANBUL" }

// Yer KJ al
// POST https://localhost:7043/api/kj/Reji/yer/al

// ─── SOSYAL MEDYA ────────────────────────

// Sosyal medya ver
// POST https://localhost:7043/api/kj/Reji/sosyal-medya/ver

// Sosyal medya al
// POST https://localhost:7043/api/kj/Reji/sosyal-medya/al

// Whatsapp ver
// POST https://localhost:7043/api/kj/Reji/whatsapp/ver

// Whatsapp al
// POST https://localhost:7043/api/kj/Reji/whatsapp/al

// ─── İSİMLİK ─────────────────────────────

// İsimlik ver
// POST https://localhost:7043/api/kj/Reji/isimlik/ver
// Body: { "isim": "Ahmet Yılmaz", "title": "Muhabir", "telefonMu": false }

// İsimlik al
// POST https://localhost:7043/api/kj/Reji/isimlik/al

// ─── TELEFON İSİMLİK ─────────────────────

// Telefon isimlik ver (telefonMu: true = telefon logolu, false = logosuz)
// POST https://localhost:7043/api/kj/Reji/telefon-isimlik/ver
// Body: { "isim": "Ahmet Yılmaz", "title": "Muhabir", "telefonMu": true }

// Telefon isimlik al
// POST https://localhost:7043/api/kj/Reji/telefon-isimlik/al

// ─── MUHABİR KAMERA ──────────────────────

// Muhabir kamera ver
// POST https://localhost:7043/api/kj/Reji/muhabir-kamera/ver
// Body: { "muhabir": "Ahmet Yılmaz", "kameraman": "Mehmet Demir" }

// Muhabir kamera al
// POST https://localhost:7043/api/kj/Reji/muhabir-kamera/al

// ─── CANLI ───────────────────────────────

// Canlı ver
// POST https://localhost:7043/api/kj/Reji/canli/ver

// Canlı al
// POST https://localhost:7043/api/kj/Reji/canli/al

// ─── CANLI YER ───────────────────────────

// Canlı yer ver
// POST https://localhost:7043/api/kj/Reji/canli-yer/ver
// Body: { "text": "İSTANBUL" }

// Canlı yer al
// POST https://localhost:7043/api/kj/Reji/canli-yer/al

// ─── ROZETLER ────────────────────────────

// Rozet ver
// POST https://localhost:7043/api/kj/Reji/rozet/ver?rozetType=SonDakika
// rozetType: AzSonra, AzSonraDsf, AzSonraDsf2, SonDakika, OzelHaber, WhatsappIhbar

// Rozet al
// POST https://localhost:7043/api/kj/Reji/rozet/al?rozetType=SonDakika

// Tüm rozetleri al
// POST https://localhost:7043/api/kj/Reji/rozet/tumunu-al


// ═══════════════════════════════════════════
// RUNDOWN
// ═══════════════════════════════════════════

// Tüm rundownları getir
// GET https://localhost:7043/api/rundown

// Tek rundown getir
// GET https://localhost:7043/api/rundown/1

// Tarihe göre rundown getir
// GET https://localhost:7043/api/rundown/tarih/2026-04-27

// Rundown ekle
// POST https://localhost:7043/api/rundown
// Body: { "ad": "Ana Haber", "tarih": "2026-04-27", "saat": "20:00", "kanal": "Show TV" }

// Rundown güncelle
// PUT https://localhost:7043/api/rundown
// Body: { "id": 1, "ad": "Ana Haber", "tarih": "2026-04-27", "saat": "20:00", "kanal": "Show TV" }

// Rundown sil
// DELETE https://localhost:7043/api/rundown/1


// ═══════════════════════════════════════════
// HABER
// ═══════════════════════════════════════════

// Tüm haberleri getir
// GET https://localhost:7043/api/haber

// Tek haber getir
// GET https://localhost:7043/api/haber/1

// Rundown'a göre haberleri getir
// GET https://localhost:7043/api/haber/rundown/1

// Haber ekle
// POST https://localhost:7043/api/haber
// Body: { "baslik": "Haber başlığı", "icerik": "Haber içeriği", "rundownId": 1, "sira": 1 }

// Haber güncelle
// PUT https://localhost:7043/api/haber
// Body: { "id": 1, "baslik": "Haber başlığı", "icerik": "Haber içeriği", "rundownId": 1, "sira": 1 }

// Haber sil
// DELETE https://localhost:7043/api/haber/1


// ═══════════════════════════════════════════
// KJ LİSTESİ
// ═══════════════════════════════════════════

// Habere göre KJ listesi getir
// GET https://localhost:7043/api/kjlist/haber/1

// Tek KJ getir
// GET https://localhost:7043/api/kjlist/1

// KJ ekle
// POST https://localhost:7043/api/kjlist
// Body: { "haberId": 1, "aciklama": "Tekli KJ", "type": 0, "text1": "Üst yazı", "text2": "" }

// KJ güncelle
// PUT https://localhost:7043/api/kjlist
// Body: { "id": 1, "haberId": 1, "aciklama": "Tekli KJ", "type": 0, "text1": "Üst yazı", "text2": "" }

// KJ sil
// DELETE https://localhost:7043/api/kjlist/1

// KJ sırası değiştir
// POST https://localhost:7043/api/kjlist/swap?id1=1&id2=2