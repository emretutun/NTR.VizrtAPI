# NTR.VizrtAPI

Vizrt canlı yayın sistemi entegrasyonu için C# tabanlı ASP.NET Core API kütüphanesi. Reji, Grafik motorları ve broadcast ekipmanı kontrolü için kapsamlı endpoint'ler sunar.

---

## 📋 İçerik

- [Proje Yapısı](#-proje-yapısı-mimarisi)
- [Dosya Dizini](#-dosya-dizini)
- [DTO Nedir?](#-dto-nedir-data-transfer-object)
- [Veri Akışı](#-bir-isteğin-akışı)
- [API Endpoint'leri](#-api-endpointleri)
  - [Engine Kontrol](#-engine-kontrol)
  - - [KJ Yönetimi](#-kj-kurucak-jayapım-yönetimi)
  - [Roll (Akan Yazı)](#-roll-akan-yazı)
  - [Kelebek (Çoklu Konuk)](#-kelebek-çoklu-konuk)
  - [Rundown Yönetimi](#-rundown-yönetimi)
  - [Haber Yönetimi](#-haber-yönetimi)
  - [KJ Listesi](#-kj-listesi)
  - [Sistem Logları](#-sistem-logları)
- [NTR.RejiClient — Masaüstü Arayüzü](#-ntrrejiclient--masaüstü-arayüzü)

---

## 🏗️ Proje Yapısı (Mimarisi)

Bu proje **Clean Architecture** prensiplerine göre 5 ana katmandan oluşmaktadır:

### **1️⃣ NTR.API (Presentation Layer - Sunum Katmanı)**

**"Dış dünya ile iletişim kuran kapı"**

- HTTP isteklerini alır
- Client'ten JSON verisi alır ve DTO'ya dönüştürür
- Business logic'i çağırır
- JSON yanıt gönderir

```
NTR.API/
├── Controllers/          ← API endpoint'leri (HTTP interface)
│   ├── EngineController.cs      → Engine durumu ve bağlantı
│   ├── KjController.cs          → KJ, rozet, yer, isimlik vb.
│   ├── RundownController.cs     → Rundown yönetimi
│   ├── HaberController.cs       → Haber yönetimi
│   ├── KjListController.cs      → KJ listesi
│   └── LogController.cs         → Sistem logları
├── Middleware/          ← İstek öncesi/sonrası işlemler (Auth, validation)
├── Program.cs           ← Uygulamanın başlangıç noktası (DI setup)
├── appsettings.json     ← Ayarlar
└── appsettings.Development.json ← Geliştirme ayarları
```

**Örnek Controller:**

```csharp
[ApiController]
[Route("api/[controller]")]
public class EngineController : ControllerBase
{
    private readonly IVizrtService _vizrtService;
    
    [HttpPost("{engineType}/connect")]
    public IActionResult Connect(VizrtEngineType engineType, 
                                  [FromBody] ConnectRequestDto dto)
    {
        // Client gönderir: { "ip": "127.0.0.1", "port": 6100 }
        // DTO otomatik deserialize olur
        var result = _vizrtService.Connect(engineType, dto.IP);
        return Ok(result);
    }
}
```

---

### **2️⃣ NTR.Application (Business Logic Layer - İş Mantığı Katmanı)**

**"İş mantığı ve kurallar burada yaşıyor"**

- Controllers'dan gelen DTOs'u alır
- Validasyonlar yapar
- İş kurallarını uygular
- Infrastructure'u çağırır

```
NTR.Application/
├── DTOs/                ← Veri Transfer Nesneleri (Client ↔ API)
│   ├── KjRequestDto.cs
│   │   ├── Type (Tekli, Çiftli, Uzun)
│   │   ├── Text1 (Üst yazı)
│   │   ├── Text2 (Alt yazı)
│   │   └── Rozet (Opsiyonel: AzSonra, SonDakika, vb.)
│   ├── RollRequestDto.cs          ← YENİ
│   │   ├── TesekkurYazisi
│   │   ├── Satirlar (List<RollSatirDto> — Baslik + Yazi, maks. 24)
│   │   └── Sponsorlar (List<string> — dosya adları, maks. 5)
│   ├── KelebekRequestDto.cs       ← YENİ
│   │   ├── KelebekSahneDto (SahneYolu)
│   │   └── KelebekIsimDto (Index, Isim, Title)
│   ├── ConnectRequestDto.cs
│   ├── EngineStatusDto.cs
│   ├── IsimlikRequestDto.cs
│   ├── MuhabirKameraRequestDto.cs
│   ├── RawCommandDto.cs
│   └── YerRequestDto.cs
│
├── Services/            ← İş Mantığı (Business Rules)
│   ├── VizrtService.cs  ← ANA SERVİS (KJ, rozet, scene yönetimi)
│   ├── KjService.cs     ← KJ veritabanı işlemleri
│   ├── HaberService.cs  ← Haber yönetimi
│   ├── RundownService.cs ← Rundown yönetimi
│   └── LogService.cs    ← Sistem logging
│
└── Extensions/          ← Helper fonksiyonları ve DI setup
```

**VizrtService Görevi:**

```csharp
public class VizrtService : IVizrtService
{
    // State tracking dictionaries (UI'da hangi grafik açık?)
    private readonly Dictionary<VizrtEngineType, bool> _kjTekOnAir;
    private readonly Dictionary<VizrtEngineType, bool> _kjCiftOnAir;
    private readonly Dictionary<VizrtEngineType, bool> _kjUzunOnAir;
    private readonly Dictionary<VizrtEngineType, bool> _yerOnAir;
    private readonly Dictionary<VizrtEngineType, bool> _canliOnAir;
    private readonly Dictionary<VizrtEngineType, bool> _whatsappOnAir;
    private readonly Dictionary<VizrtEngineType, RozetType?> _aktifRozet;
    
    // Vizrt engine client'leri
    private readonly Dictionary<VizrtEngineType, IVizrtEngine> _engines;
    
    public CommandResult SendKj(VizrtEngineType engineType, 
                                KjType kjType, 
                                string text1, 
                                string text2 = "", 
                                RozetType? rozet = null)
    {
        // 1. Başka KJ açık mı kontrol et
        // 2. Yazıyı set et (SetObjectText)
        // 3. IN animasyonunu çalıştır (Play)
        // 4. Rozeti açarsa onu da yönet
    }
}
```

---

### **3️⃣ NTR.Core (Domain/Entities Layer - Kural Kitabı)**

**"İşletme kuralları ve veri yapıları"**

- Veritabanı varlıkları (Entities)
- Sabit değerler (Enums)
- Hizmet kontratları (Interfaces)

```
NTR.Core/
├── Entities/            ← Veritabanı Modelleri
│   ├── VizrtEngine.cs
│   │   ├── Id
│   │   ├── Name (Reji, Grafik1, Grafik2)
│   │   ├── IP
│   │   ├── Port
│   │   ├── IsConnected
│   │   ├── CurrentScene_Front, Middle, Back
│   │   └── HeartbeatEnabled
│   ├── Rundown.cs
│   ├── Haber.cs
│   ├── KjItem.cs
│   ├── LogEntry.cs
│   ├── KjScene.cs
│   ├── VizrtSettings.cs
│   └── CommandResult.cs ← Operasyon sonucu
│
├── Enums/               ← Sabit Değerler
│   ├── VizrtEngineType (Reji, Grafik1, Grafik2)
│   ├── KjType (Tekli, Çiftli, Uzun)
│   └── RozetType (AzSonra, AzSonraDsf, SonDakika, OzelHaber, WhatsappIhbar)
│
└── Interfaces/          ← Kontratlar (Hizmet Tanımları)
    ├── IVizrtService
    └── IVizrtEngine
```

**Örnek Enum:**

```csharp
public enum KjType
{
    Tekli = 0,   // Single line
    Ciftli = 1,  // Double line
    Uzun = 2     // Long format
}

public enum RozetType
{
    AzSonra = 0,
    AzSonraDsf = 1,
    AzSonraDsf2 = 2,
    SonDakika = 3,
    OzelHaber = 4,
    WhatsappIhbar = 5,
    SicakGelisme = 6   // YENİ: Sadece tekli KJ ile çalışan Corner (Sıcak Gelişme) rozeti
}
```

---

### **4️⃣ NTR.Infrastructure (Data Access Layer - Depo/Bağlantı)**

**"Vizrt motoru ve veritabanı ile konuşuyor"**

- Vizrt motoruna TCP üzerinden komut gönderir
- Veritabanı erişimi
- Socket/connection yönetimi

```
NTR.Infrastructure/
├── Tcp/                 ← TCP Bağlantısı
│   └── TcpClient yönetimi
├── Vizrt/               ← Vizrt Motor İstemcisi
│   └── VizrtEngineClient.cs
│       ├── Connect(ip)
│       ├── Disconnect()
│       ├── SendCommand(command)
│       ├── SetObjectText(scene, object, text)
│       ├── Play(scene, animation)
│       └── GetStatus()
└── Repository/          ← Veritabanı Sorguları
    └── Rundown, Haber, KjItem vb. CRUD işlemleri
```

**VizrtEngineClient Görevi:**

```csharp
public class VizrtEngineClient : IVizrtEngine
{
    private TcpClient _tcpClient;
    
    public bool Connect(string ipAddress)
    {
        // TCP bağlantı kur
        _tcpClient = new TcpClient();
        _tcpClient.Connect(ipAddress, 6100);
    }
    
    public CommandResult Send(string command)
    {
        // Vizrt'e ham TCP komut gönder
        // Örn: "CONTAINER*xxx SET_TEXT 'Metnin içeriği'"
    }
    
    public void SetObjectText(string scene, string objectPath, string text)
    {
        // Vizrt scene'deki bir objenin metnini değiştir
        Send($"CONTAINER*{objectPath} SET_TEXT '{text}'");
    }
    
    public void Play(string scene, string animation)
    {
        // Vizrt scene'deki bir animasyonu çalıştır
        Send($"DIRECTOR*{animation} PLAY");
    }
}
```

---

### **🌟 Akıllı Sahne Yönlendirmesi (Smart Scene Routing)**

API, `VizrtService.cs` içerisinde motora yüklü olan sahnenin adını (`_kjScenePath`) kontrol ederek yolları dinamik olarak değiştirir. Örneğin; eğer sahne adında `"CUMARTESI_SURPRIZI"` veya `"PAZAR"` geçiyorsa, API dışarıdan aynı isteği alsa bile Vizrt motoruna gönderdiği obje ve animasyon yollarını otomatik olarak o sahneye (Örn: `KJ$TEK_KJ$IN`) uyarlar. Eski projede ise eski yolları (`KJ_TUM$KJ_TEK$IN`) kullanmaya devam eder. Bu sayede **tek bir endpoint ile birden fazla farklı Vizrt projesi** yönetilebilir.

---

## 📁 Dosya Dizini

```
emretutun/NTR.VizrtAPI/
│
├─── NTR.API/                          (HTTP API Endpoint'leri)
│    ├─ Controllers/
│    │  ├─ EngineController.cs         → /api/engine (bağlantı, status)
│    │  ├─ KjController.cs             → /api/kj (grafik yönetimi, roll, kelebek)
│    │  ├─ RundownController.cs        → /api/rundown (yayın planı)
│    │  ├─ HaberController.cs          → /api/haber (haberler)
│    │  ├─ KjListController.cs         → /api/kjlist (KJ listesi)
│    │  └─ LogController.cs            → /api/log (loglar)
│    ├─ Middleware/
│    │  └─ ApiKeyMiddleware.cs         → API key validation
│    ├─ Program.cs                     → Uygulamanın giriş noktası
│    ├─ appsettings.json               → Production ayarları
│    └─ appsettings.Development.json   → Development ayarları
│
├─── NTR.Application/                  (İş Mantığı)
│    ├─ DTOs/                          ← Veri Transfer Nesneleri
│    │  ├─ KjRequestDto.cs             → POST /api/kj/{engine}/ver
│    │  ├─ RollRequestDto.cs           → POST /api/kj/{engine}/roll/ver  ← YENİ
│    │  ├─ RollTekMetinRequestDto.cs   → POST /api/kj/{engine}/roll-tek-metin/ver  ← YENİ
│    │  ├─ KelebekRequestDto.cs        → POST /api/kj/{engine}/kelebek/* ← YENİ
│    │  ├─ ConnectRequestDto.cs        → POST /api/engine/{engine}/connect
│    │  ├─ EngineStatusDto.cs          → GET /api/engine/status
│    │  ├─ IsimlikRequestDto.cs
│    │  ├─ MuhabirKameraRequestDto.cs
│    │  ├─ RawCommandDto.cs
│    │  └─ YerRequestDto.cs
│    │
│    ├─ Services/                      ← İş Mantığı (Business Logic)
│    │  ├─ VizrtService.cs             → Vizrt motor komutları (1000+ satır)
│    │  ├─ KjService.cs                → KJ veritabanı işlemleri
│    │  ├─ HaberService.cs             → Haber işlemleri
│    │  ├─ RundownService.cs           → Rundown işlemleri
│    │  └─ LogService.cs               → Logging işlemleri
│    │
│    └─ Extensions/
│       └─ ServiceCollectionExtensions.cs → DI (Dependency Injection) setup
│
├─── NTR.Core/                         (Domain/Kural Kitabı)
│    ├─ Entities/                      ← Veritabanı Modelleri
│    │  ├─ VizrtEngine.cs              → Engine tanımı
│    │  ├─ Rundown.cs                  → Yayın planı
│    │  ├─ Haber.cs                    → Haber
│    │  ├─ KjItem.cs                   → KJ öğesi
│    │  ├─ KjScene.cs                  → KJ scene
│    │  ├─ LogEntry.cs                 → Log kaydı
│    │  ├─ VizrtSettings.cs            → Vizrt ayarları
│    │  └─ CommandResult.cs            → İşlem sonucu
│    │
│    ├─ Enums/                         ← Sabit Değerler
│    │  └─ VizrtEngineType.cs          → VizrtEngineType, KjType, RozetType
│    │
│    └─ Interfaces/                    ← Hizmet Kontratları
│       ├─ IVizrtService.cs            → Vizrt service arayüzü
│       ├─ IVizrtEngine.cs             → Engine client arayüzü
│       ├─ IRundownRepository.cs
│       ├─ IHaberRepository.cs
│       ├─ IKjRepository.cs
│       └─ ILogRepository.cs
│
├─── NTR.Infrastructure/               (Veri Erişimi ve Bağlantılar)
│    ├─ Tcp/
│    │  └─ VizrtTcpConnection.cs       → Raw TCP soket yönetimi
│    │
│    ├─ Vizrt/
│    │  └─ VizrtEngineClient.cs        → TCP üzerinden komut gönderme
│    │
│    └─ Repository/
│       ├─ JsonRundownRepository.cs
│       ├─ JsonHaberRepository.cs
│       ├─ JsonKjRepository.cs
│       └─ TxtLogRepository.cs
│
├─── NTR.RejiClient/                   (Windows Forms UI - Reji Kumandası) ← YENİ
│    ├─ MainForm.cs                    → Ana reji arayüzü
│    ├─ MainForm.Designer.cs
│    ├─ MainForm.resx
│    ├─ Program.cs
│    ├─ Forms/
│    │  ├─ RollForm.cs                 → Roll (akan yazı) editörü
│    │  ├─ RollForm.Designer.cs
│    │  ├─ Kelebek.cs                  → Kelebek (çoklu konuk) editörü
│    │  └─ Kelebek.Designer.cs
│    ├─ Models/
│    │  ├─ ApiResult.cs
│    │  ├─ EngineStatus.cs
│    │  ├─ Haber.cs
│    │  ├─ KjItem.cs
│    │  ├─ Rundown.cs
│    │  └─ RollRequestDto.cs
│    └─ Services/
│       ├─ ApiService.cs               → Tüm API çağrıları (HttpClient)
│       └─ AppConfig.cs                → config.json okuma/yazma
│
├─── NTR.VizrtAPI.slnx                 ← Solution dosyası (tüm projeleri içerir)
├─── README.md                         ← Bu dosya
└─── .gitignore                        ← Git exclude kuralları
```

---

## 📦 DTO Nedir? (Data Transfer Object)

**DTO**, API client'i ile server arasında veri taşıyan ara nesne. Veritabanı Entity'si yerine kullanılır.

### **Neden DTO kullanırız?**

```
❌ KÖTÜ (Entity'i direkt göndermek):
Client → POST /api/rundown
         { "id": 1, "ad": "Ana Haber", "tarih": "2026-04-27", 
           "saat": "20:00", "kanal": "Show TV", 
           "internalField1": "hidden", "internalField2": "secret" }
         ← INTERNAL ALANLAR SİZE GÖRÜNTÜLENİYOR!

✅ İYİ (DTO kullanmak):
Client → POST /api/rundown
         { "ad": "Ana Haber", "tarih": "2026-04-27", 
           "saat": "20:00", "kanal": "Show TV" }
         ← SADECE GEREKLI ALANLAR!
```

### **Örnek DTO:**

```csharp
// NTR.Application/DTOs/KjRequestDto.cs
public class KjRequestDto
{
    public KjType Type { get; set; } = KjType.Tekli;      // 0=Tekli, 1=Çiftli, 2=Uzun
    public string Text1 { get; set; } = string.Empty;     // Üst yazı
    public string Text2 { get; set; } = string.Empty;     // Alt yazı
    public RozetType? Rozet { get; set; } = null;         // Opsiyonel rozet
}
```

### **Veri Akışı:**

```
1. CLIENT (Postman/Frontend)
   POST /api/kj/Reji/ver
   Content-Type: application/json
   
   {
     "type": 0,
     "text1": "Son Dakika Haberi",
     "text2": "",
     "rozet": 3
   }

2. API (EngineController)
   [HttpPost("{engineType}/ver")]
   public IActionResult SendKj(VizrtEngineType engineType, 
                                [FromBody] KjRequestDto dto)
   ↓ ASP.NET Core otomatik deserialize eder:
   dto.Type = KjType.Tekli (0)
   dto.Text1 = "Son Dakika Haberi"
   dto.Text2 = ""
   dto.Rozet = RozetType.SonDakika (3)

3. SERVICE (VizrtService)
   _vizrtService.SendKj(engineType, 
                        dto.Type, 
                        dto.Text1, 
                        dto.Text2, 
                        dto.Rozet)

4. İŞLEMLER
   ├─ Başka KJ açık mı? Kapat
   ├─ Metni set et: SetObjectText("KJ_TEK$SATIR_1$TEXT1", "Son Dakika Haberi")
   ├─ IN animasyonunu çalıştır: Play("KJ_TUM$KJ_TEK$IN")
   └─ Rozeti aç: Play("KJ_TUM$KJ_SON_DAKIKA$IN")

5. INFRASTRUCTURE (VizrtEngineClient)
   TCP bağlantısı üzerinden Vizrt'e komut gönder:
   "CONTAINER*KJ_TEK$SATIR_1$TEXT1 SET_TEXT 'Son Dakika Haberi'"
   "DIRECTOR*KJ_TUM$KJ_TEK$IN PLAY"
   "DIRECTOR*KJ_TUM$KJ_SON_DAKIKA$IN PLAY"

6. VIZRT MOTOR
   ✅ Yayında "Son Dakika Haberi" yazısı görülür!
```

---

## 🔄 Bir İsteğin Akışı

### **Örnek: KJ Yayına Ver (Step-by-Step)**

```
┌─────────────────────────────────────────────────────────────┐
│ CLIENT LAYER (Postman/Frontend)                              │
├─────────────────────────────────────────────────────────────┤
│ POST /api/kj/Reji/ver                                        │
│ {                                                             │
│   "type": 0,                  ← Tekli KJ                     │
│   "text1": "Son Dakika",      ← Üst yazı                     │
│   "text2": "",                                                │
│   "rozet": 3                  ← SonDakika rozeti             │
│ }                                                             │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ API LAYER (NTR.API/Controllers/KjController.cs)             │
├─────────────────────────────────────────────────────────────┤
│ [HttpPost("{engineType}/ver")]                              │
│ public IActionResult SendKj(VizrtEngineType engineType,     │
│                              [FromBody] KjRequestDto dto)   │
│                                                              │
│ ✓ DTO'yu deserialize et: KjRequestDto nesnesine dönüştür  │
│ ✓ Parametreleri doğrula: engineType = Reji                │
│ ✓ Service'i çağır: _vjService.SendKj(...)                  │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ SERVICE LAYER (NTR.Application/Services/VizrtService.cs)    │
├─────────────────────────────────────────────────────────────┤
│ public CommandResult SendKj(VizrtEngineType engineType,     │
│                              KjType kjType,                 │
│                              string text1,                  │
│                              string text2 = "",             │
│                              RozetType? rozet = null)       │
│                                                              │
│ 1. Engine bağlı mı kontrol et                              │
│ 2. Başka KJ açık mı kontrol et → Kapat                     │
│ 3. Metni set et (Vizrt objesine yaz):                      │
│    engine.SetObjectText(scene, "KJ_TEK$SATIR_1$TEXT1",     │
│                         "Son Dakika")                       │
│ 4. IN animasyonunu çalıştır:                               │
│    engine.Play(scene, "KJ_TUM$KJ_TEK$IN")                  │
│ 5. Rozeti yönet:                                            │
│    if (rozet == RozetType.SonDakika)                        │
│      engine.Play(scene, "KJ_TUM$KJ_SON_DAKIKA$IN")         │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ INFRASTRUCTURE LAYER                                         │
│ (NTR.Infrastructure/Vizrt/VizrtEngineClient.cs)             │
├─────────────────────────────────────────────────────────────┤
│ public void SetObjectText(string scene,                     │
│                           string objectPath,               │
│                           string text)                      │
│ {                                                            │
│   string cmd =                                              │
│     $"CONTAINER*{objectPath} SET_TEXT '{text}'";           │
│   Send(cmd);  ← TCP üzerinden Vizrt'e gönder               │
│ }                                                            │
│                                                              │
│ public void Play(string scene, string animation)           │
│ {                                                            │
│   string cmd = $"DIRECTOR*{animation} PLAY";               │
│   Send(cmd);  ← TCP üzerinden Vizrt'e gönder               │
│ }                                                            │
│                                                              │
│ private CommandResult Send(string command)                 │
│ {                                                            │
│   // TCP bağlantısı üzerinden komut gönder                 │
│   _tcpClient.GetStream().Write(Encoding.UTF8.GetBytes(...))│
│ }                                                            │
└─────────────────────────────────────────────────────────────┘
                           ↓
┌─────────────────────────────────────────────────────────────┐
│ VIZRT MOTOR (Hardware)                                       │
├─────────────────────────────────────────────────────────────┤
│ TCP paketler alınır:                                        │
│ • CONTAINER*KJ_TEK$SATIR_1$TEXT1 SET_TEXT 'Son Dakika'    │
│ • DIRECTOR*KJ_TUM$KJ_TEK$IN PLAY                           │
│ • DIRECTOR*KJ_TUM$KJ_SON_DAKIKA$IN PLAY                    │
│                                                              │
│ ✅ YAYINDA GÖRÜLÜR: "Son Dakika" yazısı + SonDakika rozeti│
└─────────────────────────────────────────────────────────────┘
```

---

## 🎬 Engine Kontrol

Vizrt motorlarının (Reji, Grafik1, Grafik2) durumunu kontrol etmek, bağlantı yönetimi ve scene yükleme işlemleri.

### Tüm Engine Durumunu Getir
```http
GET https://localhost:7043/api/engine/status
```
**Açıklama:** Tüm motor durumlarını görüntüler (Reji, Grafik1, Grafik2)

**Yanıt Örneği:**
```json
{
  "reji": "Connected",
  "grafik1": "Disconnected",
  "grafik2": "Connected"
}
```

---

### Tek Engine Durumunu Getir
```http
GET https://localhost:7043/api/engine/status/{engineType}
```
**Parametreler:**
- `engineType`: `Reji`, `Grafik1`, veya `Grafik2`

**Örnek:**
```http
GET https://localhost:7043/api/engine/status/Reji
```

**Yanıt:**
```json
{
  "status": "Connected",
  "engineType": "Reji"
}
```

---

### Engine'ye Bağlan
```http
POST https://localhost:7043/api/engine/{engineType}/connect
Content-Type: application/json

{
  "ip": "127.0.0.1",
  "port": 6100
}
```
**Parametreler:**
- `engineType`: `Reji`, `Grafik1`, veya `Grafik2`
- `ip` (Body): Engine'nin IP adresi
- `port` (Body): Port numarası (opsiyonel, varsayılan 6100)

**Örnek:**
```http
POST https://localhost:7043/api/engine/Grafik1/connect
Content-Type: application/json

{
  "ip": "192.168.1.100",
  "port": 6100
}
```

---

### Engine Bağlantısını Kes
```http
POST https://localhost:7043/api/engine/{engineType}/disconnect
```
**Örnek:**
```http
POST https://localhost:7043/api/engine/Reji/disconnect
```

---

### Scene Yükle
```http
POST https://localhost:7043/api/engine/{engineType}/scene/load
Content-Type: application/json

"SHOW_TV_2025/REJI/YENI_SAYFA/KJ/KJ_TUM_V9"
```
**Açıklama:** Belirtilen scene dosyasını motora yükler

**Örnek:**
```http
POST https://localhost:7043/api/engine/Reji/scene/load
Content-Type: application/json

"SHOW_TV_2025/REJI/YENI_SAYFA/KJ/KJ_TUM_V9"
```

---

### Ham Komut Gönder
```http
POST https://localhost:7043/api/engine/{engineType}/raw
Content-Type: application/json

{
  "command": "RENDERER*STAGE TO_START"
}
```
**Açıklama:** Doğrudan Vizrt komutları göndermek için kullanılır (ileri seviye)

**Örnek:**
```http
POST https://localhost:7043/api/engine/Reji/raw
Content-Type: application/json

{
  "command": "RENDERER*STAGE TO_START"
}
```

---

## 📺 KJ (Kurucak Jayapım) Yönetimi

KJ yönetimi - yazı, görsel ve special efektler kontrol sistemi.

### Tekli KJ Gönder (Rozetsiz)
```http
POST https://localhost:7043/api/kj/{engineType}/ver
Content-Type: application/json

{
  "type": 0,
  "text1": "Üst yazı",
  "text2": "",
  "rozet": null
}
```
**Parametreler:**
- `type`: 0 = Tekli, 1 = Çiftli, 2 = Uzun
- `text1`: Üst yazı
- `text2`: Alt yazı (çiftli ve uzun için)
- `rozet`: Opsiyonel rozet türü

**Örnek:**
```http
POST https://localhost:7043/api/kj/Reji/ver
Content-Type: application/json

{
  "type": 0,
  "text1": "Son Dakika Haberi",
  "text2": "",
  "rozet": null
}
```

---

### Çiftli KJ Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/ver
Content-Type: application/json

{
  "type": 1,
  "text1": "Üst yazı",
  "text2": "Alt yazı",
  "rozet": null
}
```

**Örnek:**
```http
POST https://localhost:7043/api/kj/Grafik1/ver
Content-Type: application/json

{
  "type": 1,
  "text1": "Ahmet Yılmaz",
  "text2": "Muhabir",
  "rozet": null
}
```

---

### Uzun KJ Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/ver
Content-Type: application/json

{
  "type": 2,
  "text1": "Ana yazı",
  "text2": "Detaylı açıklama",
  "rozet": null
}
```

---

### Rozetli KJ Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/ver
Content-Type: application/json

{
  "type": 0,
  "text1": "Haber metni",
  "text2": "",
  "rozet": 3
}
```
**Rozet Türleri:**
- `0`: AzSonra
- `1`: AzSonraDsf
- `2`: AzSonraDsf2
- `3`: SonDakika
- `4`: OzelHaber
- `5`: WhatsappIhbar
- `6`: SicakGelisme *(Sadece `type: 0` Tekli KJ ile kullanılabilir — Corner rozeti)*

---

### KJ Al (Kaldır)
```http
POST https://localhost:7043/api/kj/{engineType}/al
```
**Açıklama:** Ekrandan KJ'yi kaldırır

**Örnek:**
```http
POST https://localhost:7043/api/kj/Reji/al
```

---

### Tüm KJ'leri Al
```http
POST https://localhost:7043/api/kj/{engineType}/tumunu-al
```
**Açıklama:** Ekrandaki tüm KJ'leri kaldırır

---

### Yer KJ Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/yer/ver
Content-Type: application/json

{
  "text": "İSTANBUL"
}
```

---

### Yer KJ Al
```http
POST https://localhost:7043/api/kj/{engineType}/yer/al
```

---

### Sosyal Medya Göster
```http
POST https://localhost:7043/api/kj/{engineType}/sosyal-medya/ver
```

---

### Sosyal Medya Gizle
```http
POST https://localhost:7043/api/kj/{engineType}/sosyal-medya/al
```

---

### WhatsApp Göster
```http
POST https://localhost:7043/api/kj/{engineType}/whatsapp/ver
```

---

### WhatsApp Gizle
```http
POST https://localhost:7043/api/kj/{engineType}/whatsapp/al
```

---

### İsimlik Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/isimlik/ver
Content-Type: application/json

{
  "isim": "Ahmet Yılmaz",
  "title": "Muhabir",
  "telefonMu": false
}
```

> **💡 Akıllı İsimlik Notu:** `isim` parametresi boş (`""`) gönderilirse, API Vizrt motorundaki mevcut metni (SetObjectText) ezmez. Sahnede sabit yazan ismi (Örn: Sabit Sunucu İsmi) koruyarak sadece IN animasyonunu tetikler.

---

### İsimlik Al
```http
POST https://localhost:7043/api/kj/{engineType}/isimlik/al
```

---

### Telefon İsimlik Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/telefon-isimlik/ver
Content-Type: application/json

{
  "isim": "Mehmet Demir",
  "title": "Muhabir",
  "telefonMu": true
}
```
**Açıklama:** `telefonMu: true` telefon logolu, `false` logosuz

---

### Telefon İsimlik Al
```http
POST https://localhost:7043/api/kj/{engineType}/telefon-isimlik/al
```

---

### Muhabir-Kamera Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/muhabir-kamera/ver
Content-Type: application/json

{
  "muhabir": "Ahmet Yılmaz",
  "kameraman": "Mehmet Demir"
}
```

---

### Muhabir-Kamera Al
```http
POST https://localhost:7043/api/kj/{engineType}/muhabir-kamera/al
```

---

### Canlı Yayın Göster
```http
POST https://localhost:7043/api/kj/{engineType}/canli/ver
```

---

### Canlı Yayın Gizle
```http
POST https://localhost:7043/api/kj/{engineType}/canli/al
```

---

### Canlı Yer Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/canli-yer/ver
Content-Type: application/json

{
  "text": "İSTANBUL"
}
```

---

### Canlı Yer Al
```http
POST https://localhost:7043/api/kj/{engineType}/canli-yer/al
```

---

### Rozet Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/rozet/ver?rozetType=SonDakika
```
**Query Parametreleri:**
- `rozetType`: AzSonra, AzSonraDsf, AzSonraDsf2, SonDakika, OzelHaber, WhatsappIhbar, SicakGelisme

**Örnek:**
```http
POST https://localhost:7043/api/kj/Reji/rozet/ver?rozetType=SonDakika
```

---

### Rozet Al
```http
POST https://localhost:7043/api/kj/{engineType}/rozet/al?rozetType=SonDakika
```

---

### Tüm Rozetleri Al
```http
POST https://localhost:7043/api/kj/{engineType}/rozet/tumunu-al
```

---

## 📜 Roll (Akan Yazı)

Jenerik veya katkıda bulunanlar listesi olarak akan yazı grafiği. Teşekkür metni, isim-unvan satırları ve sponsor logolarını destekler.

> **Not:** Roll başlamadan önce `TakeAll()` otomatik çağrılır, ekrandaki tüm grafikler temizlenir (1 saniyelik bekleme süresi dahil).

---

### Roll Yayına Ver
```http
POST https://localhost:7043/api/kj/{engineType}/roll/ver
Content-Type: application/json

{
  "tesekkurYazisi": "SHOW TV HABER MERKEZİNE TEŞEKKÜR EDERİZ",
  "satirlar": [
    { "baslik": "YAPIMCI", "yazi": "UFUK COŞKUN" },
    { "baslik": "EDİTÖR", "yazi": "ESRA DOĞAN" },
    { "baslik": "MUHABİR", "yazi": "EMEL KILIÇ" }
  ],
  "sponsorlar": ["logo1.jpg", "logo2.png"]
}
```

**Parametreler:**
- `tesekkurYazisi`: Teşekkür metni. Çok satırlı olabilir.
- `satirlar`: Maks. **24 satır**. Her satırda `baslik` (unvan) ve `yazi` (isim) alanı.
- `sponsorlar`: Maks. **5 adet** sponsor görseli. Sadece dosya adı yazılır (ör. `logo.jpg`). Görseller `D:\SHOWTV_REJI_DATA\ROLL\` klasöründen okunur.

**Örnek (Tekli konuk, sponsorsuz):**
```http
POST https://localhost:7043/api/kj/Reji/roll/ver
Content-Type: application/json

{
  "tesekkurYazisi": "TEŞEKKÜR EDERİZ",
  "satirlar": [
    { "baslik": "YAPIMCI", "yazi": "ALİ KURT" }
  ],
  "sponsorlar": []
}
```

---

### Roll Al (Durdur)
```http
POST https://localhost:7043/api/kj/{engineType}/roll/al
```
**Açıklama:** Akan yazıyı durdurur ve ekrandan kaldırır.

**Örnek:**
```http
POST https://localhost:7043/api/kj/Reji/roll/al
```

---

### Roll Yayına Ver (Tek Metinli Yeni Sistem)

Bu sistem 24 satırlık grid mantığı yerine, `\n` ile ayrılmış tek bir uzun metin bloğunu kabul eder. API, metindeki satır sayısını sayarak Vizrt Y ekseni animasyon (Position) bitiş noktasını otomatik hesaplar.

```http
POST https://localhost:7043/api/kj/{engineType}/roll-tek-metin/ver
Content-Type: application/json

{
  "rollMetni": "MAGAZİN PROGRAMLARI GENEL KOORDİNATÖRÜ\nREŞAT BALOOĞLU\n\nEDİTÖRLER\nSELÇUK ÖZDEMİR",
  "sponsorlar": ["logo1.jpg", "logo2.png"]
}
```

**Parametreler:**
- `rollMetni`: Tüm akış metni. Satır aralıkları `\n` (Enter) ile belirlenir.
- `sponsorlar`: Maks. **5 adet** sponsor görseli. Sadece dosya adı yazılır (ör. `logo.jpg`). Görseller `D:\SHOWTV_REJI_DATA\ROLL\` klasöründen okunur.

> ⚡ **Performans Notu:** Eski sistemdeki çıkış animasyonlarının birbirine girmesini engellemek için kullanılan `TakeAll` bekleme süresi, gecikmeyi önlemek adına **500ms (0.5 sn)**'ye optimize edilmiştir. Roll komutları artık anında tepki verir.

---

## 🦋 Kelebek (Çoklu Konuk)

Kelebek sistemi; aynı anda 1–5 konuğun isim ve unvan bilgisini ekranda gösterir. Sahne **Back Layer**'a yüklenir ve her konuk bağımsız olarak yönetilebilir.

**Kullanım sırası:**
1. Önce sahneyi yükle (`kelebek/sahne`)
2. İstediğin konukları gönder (`kelebek/isim`)
3. Bitince kapat (`kelebek/kapat`)

---

### Kelebek Sahnesi Yükle
```http
POST https://localhost:7043/api/kj/{engineType}/kelebek/sahne
Content-Type: application/json

{
  "sahneYolu": "SHOW_TV_2025/REJI/YENI_SAYFA/KELEBEK/3KISI"
}
```
**Açıklama:** Belirtilen sahneyi Back Layer'a yükler ve aktive eder. Sahne adı `KelebekGorselleri/` klasöründeki PNG dosyasının adıyla eşleşmelidir.

**Örnek:**
```http
POST https://localhost:7043/api/kj/Reji/kelebek/sahne
Content-Type: application/json

{
  "sahneYolu": "SHOW_TV_2025/REJI/YENI_SAYFA/KELEBEK/5KISI"
}
```

---

### Kelebek İsim Gönder
```http
POST https://localhost:7043/api/kj/{engineType}/kelebek/isim
Content-Type: application/json

{
  "index": 1,
  "isim": "Ahmet Yılmaz",
  "title": "Ekonomist"
}
```

**Parametreler:**
- `index`: Konuk sırası. `1` ile `5` arasında bir değer.
- `isim`: Kişinin adı. Boş bırakılırsa o konuk ekrandan **gizlenir**.
- `title`: Unvan veya görev tanımı.

> **Not:** İsim ve title otomatik olarak Türkçe büyük harfe (`ToUpper`) dönüştürülür.

**Örnek — 3 konuk göndermek:**
```http
POST https://localhost:7043/api/kj/Reji/kelebek/isim
Content-Type: application/json
{ "index": 1, "isim": "Ahmet Yılmaz", "title": "Ekonomist" }

POST https://localhost:7043/api/kj/Reji/kelebek/isim
Content-Type: application/json
{ "index": 2, "isim": "Ayşe Kaya", "title": "Siyaset Uzmanı" }

POST https://localhost:7043/api/kj/Reji/kelebek/isim
Content-Type: application/json
{ "index": 3, "isim": "Mehmet Demir", "title": "Gazeteci" }
```

**Örnek — Bir konuğu ekrandan gizlemek:**
```http
POST https://localhost:7043/api/kj/Reji/kelebek/isim
Content-Type: application/json
{ "index": 2, "isim": "", "title": "" }
```

---

### Kelebek Kapat
```http
POST https://localhost:7043/api/kj/{engineType}/kelebek/kapat
```
**Açıklama:** Back Layer'ı tamamen kapatır, sahneyi bellekten boşaltır.

**Örnek:**
```http
POST https://localhost:7043/api/kj/Reji/kelebek/kapat
```

---

## 📋 Rundown Yönetimi

Yayın planı (Rundown) yönetimi - gün bazlı yayın takvimi.

### Tüm Rundownları Getir
```http
GET https://localhost:7043/api/rundown
```

**Yanıt:**
```json
[
  {
    "id": 1,
    "ad": "Ana Haber",
    "tarih": "2026-04-27",
    "saat": "20:00",
    "kanal": "Show TV"
  }
]
```

---

### Tek Rundown Getir
```http
GET https://localhost:7043/api/rundown/{id}
```

**Örnek:**
```http
GET https://localhost:7043/api/rundown/1
```

---

### Tarihe Göre Rundown Getir
```http
GET https://localhost:7043/api/rundown/tarih/{tarih}
```

**Örnek:**
```http
GET https://localhost:7043/api/rundown/tarih/2026-04-27
```

---

### Rundown Ekle
```http
POST https://localhost:7043/api/rundown
Content-Type: application/json

{
  "ad": "Ana Haber",
  "tarih": "2026-04-27",
  "saat": "20:00",
  "kanal": "Show TV"
}
```

---

### Rundown Güncelle
```http
PUT https://localhost:7043/api/rundown
Content-Type: application/json

{
  "id": 1,
  "ad": "Ana Haber",
  "tarih": "2026-04-27",
  "saat": "20:00",
  "kanal": "Show TV"
}
```

---

### Rundown Sil
```http
DELETE https://localhost:7043/api/rundown/{id}
```

**Örnek:**
```http
DELETE https://localhost:7043/api/rundown/1
```

---

## 📰 Haber Yönetimi

Rundown içerisindeki haberlerin yönetimi.

### Tüm Haberleri Getir
```http
GET https://localhost:7043/api/haber
```

---

### Tek Haber Getir
```http
GET https://localhost:7043/api/haber/{id}
```

**Örnek:**
```http
GET https://localhost:7043/api/haber/1
```

---

### Rundown'a Göre Haberleri Getir
```http
GET https://localhost:7043/api/haber/rundown/{rundownId}
```

**Örnek:**
```http
GET https://localhost:7043/api/haber/rundown/1
```

---

### Haber Ekle
```http
POST https://localhost:7043/api/haber
Content-Type: application/json

{
  "baslik": "Haber başlığı",
  "icerik": "Haber içeriği",
  "rundownId": 1,
  "sira": 1
}
```

---

### Haber Güncelle
```http
PUT https://localhost:7043/api/haber
Content-Type: application/json

{
  "id": 1,
  "baslik": "Haber başlığı",
  "icerik": "Haber içeriği",
  "rundownId": 1,
  "sira": 1
}
```

---

### Haber Sil
```http
DELETE https://localhost:7043/api/haber/{id}
```

**Örnek:**
```http
DELETE https://localhost:7043/api/haber/1
```

---

## 📑 KJ Listesi

Haberlere bağlı KJ öğelerinin yönetimi.

### Habere Göre KJ Listesi Getir
```http
GET https://localhost:7043/api/kjlist/haber/{haberId}
```

**Örnek:**
```http
GET https://localhost:7043/api/kjlist/haber/1
```

---

### Tek KJ Getir
```http
GET https://localhost:7043/api/kjlist/{id}
```

**Örnek:**
```http
GET https://localhost:7043/api/kjlist/5
```

---

### KJ Ekle
```http
POST https://localhost:7043/api/kjlist
Content-Type: application/json

{
  "haberId": 1,
  "aciklama": "Tekli KJ",
  "type": 0,
  "text1": "Üst yazı",
  "text2": ""
}
```

---

### KJ Güncelle
```http
PUT https://localhost:7043/api/kjlist
Content-Type: application/json

{
  "id": 1,
  "haberId": 1,
  "aciklama": "Tekli KJ",
  "type": 0,
  "text1": "Üst yazı",
  "text2": ""
}
```

---

### KJ Sil
```http
DELETE https://localhost:7043/api/kjlist/{id}
```

**Örnek:**
```http
DELETE https://localhost:7043/api/kjlist/1
```

---

### KJ Sırası Değiştir
```http
POST https://localhost:7043/api/kjlist/swap?id1={id1}&id2={id2}
```

**Örnek:**
```http
POST https://localhost:7043/api/kjlist/swap?id1=1&id2=2
```
**Açıklama:** İki KJ öğesinin sırasını değiştirir.

---

## 📊 Sistem Logları

API işlemlerinin sistem loglarını görüntüleme ve yönetme.

### Bugünün Loglarını Getir
```http
GET https://localhost:7043/api/log/bugun
```

---

### Belirli Tarihin Loglarını Getir
```http
GET https://localhost:7043/api/log/tarih/{tarih}
```

**Örnek:**
```http
GET https://localhost:7043/api/log/tarih/2026-04-28
```
**Format:** YYYY-MM-DD

---

### Hata Loglarını Getir
```http
GET https://localhost:7043/api/log/hatalar
```

---

### Logları Temizle
```http
DELETE https://localhost:7043/api/log/temizle
```
**Açıklama:** Günlük logları temizler.

---

## 🔧 Teknoloji

- **Framework:** ASP.NET Core (.NET 8)
- **Dil:** C#
- **API Stili:** RESTful
- **Veritabanı:** Lokal JSON dosya tabanlı (yapılandırılabilir)
- **İletişim:** TCP/IP (Vizrt motor ile, port 6100)

---

## 🖥️ NTR.RejiClient — Masaüstü Arayüzü

Windows Forms (.NET 8) tabanlı masaüstü reji kumanda uygulaması. API üzerinden tüm Vizrt komutlarını göndermek için grafiksel arayüz sağlar.

### Yapılandırma (config.json)

Uygulama, çalıştığı dizinde `config.json` dosyasını arar. Bulunamazsa varsayılan değerlerle oluşturur.

```json
{
  "ApiBaseUrl": "https://localhost:7043",
  "ApiKey": "ntr-vizrt-2026-secret-key",
  "EngineType": "Reji",
  "LastIp": "127.0.0.1",
  "LastPort": 6100,
  "ScenePath": "SHOW_TV_2025/REJI/YENI_SAYFA/KJ/KJ_TUM_V9"
}
```

| Alan | Açıklama |
|---|---|
| `ApiBaseUrl` | NTR.API sunucusunun adresi |
| `ApiKey` | `X-Api-Key` header değeri |
| `EngineType` | Varsayılan motor: `Reji`, `Grafik1` veya `Grafik2` |
| `LastIp` | Son bağlanılan IP (otomatik kaydedilir) |
| `ScenePath` | Varsayılan Vizrt scene yolu |

---

### Ana Ekran (MainForm)

- **Bağlan:** IP adresi girip Vizrt motoruna TCP bağlantısı kurar.
- **Kanal Seçici:** Show TV / HaberTurk kanal bazlı rundown filtreleme.
- **Tarih Seçici:** Seçilen güne ait rundownları listeler.
- **Haberler:** Seçili rundowndaki haberleri gösterir.
- **KJ Listesi:** Seçili habere ait KJ öğelerini listeler; üzerine çift tıklayınca KJ otomatik olarak yayına verilir.
- **KJ Kontrol Butonları:** Tekli / Çiftli / Uzun KJ gönder, KJ Al, Tümünü Al.
- **Yer / Sosyal Medya / WhatsApp / Canlı:** İlgili grafikleri aç/kapat.
- **İsimlik / Sunucu İsimliği / Muhabir-Kameraman:** İsimlik grafikleri.
- **Roll Butonu:** RollForm penceresini açar.
- **Kelebek Butonu:** Kelebek formunu açar.

---

### RollForm — Akan Yazı Editörü

Roll içeriğini düzenlemek için grid tabanlı editör. Veriler `roll_data.txt` dosyasına otomatik kaydedilir, bir sonraki açılışta yüklenir.

**Kullanım:**

1. Teşekkür metnini `Teşekkür Yazısı` kutusuna yaz.
2. Grid'e `Başlık` (unvan) ve `Yazı` (isim) kolonlarını doldur. Maks. 24 satır.
3. Sponsor görseli eklemek için sol listeden seçip `Ekle` butonuna bas. Maks. 5 görsel.
4. `ROLL VER` butonuna bas.
5. Roll'u durdurmak için `ROLL AL` butonuna bas.

> Sponsor görselleri `D:\SHOWTV_REJI_DATA\ROLL\` klasöründen okunur.

---

### Kelebek Formu — Çoklu Konuk İsimliği

1–5 konuğun aynı anda isimliğini yönetmek için form.

**Kullanım:**

1. Sol listeden sahne seçildiğinde sağda PNG önizlemesi görünür.
2. `Sahne Geç` butonuna basıldığında sahne Back Layer'a yüklenir.
3. İsim ve unvan kutularını doldur, `İsimlikeri Ver` butonuna bas — dolu olan her konuk için animasyon otomatik tetiklenir.
4. Tek bir konuğu kaldırmak için o satırdaki `Temizle` butonuna bas.
5. Tüm konukları kaldırmak için `Tümünü Temizle` butonuna bas.
6. Sahneyi tamamen kapatmak için `Kelebek Al` butonuna bas.

---

## 📝 Lisans

Bu proje özel kullanım için hazırlanmıştır.

---

## 📞 Destek

Sorular ve teknik destekler için iletişime geçin.
