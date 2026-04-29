# NTR.VizrtAPI

Vizrt canlı yayın sistemi entegrasyonu için C# tabanlı ASP.NET Core API kütüphanesi. Reji, Grafik motorları ve broadcast ekipmanı kontrolü için kapsamlı endpoint'ler sunar.

## 📋 İçerik

- [Engine Kontrol](#-engine-kontrol)
- [KJ (Kurucak Jayapım) Yönetimi](#-kj-yönetimi)
- [Rundown Yönetimi](#-rundown-yönetimi)
- [Haber Yönetimi](#-haber-yönetimi)
- [KJ Listesi](#-kj-listesi)
- [Sistem Logları](#-sistem-logları)

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

## 📺 KJ Yönetimi

KJ (Kurucak Jayapım) yönetimi - yazı, görsel ve special efektler kontrol sistemi.

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
- `rozetType`: AzSonra, AzSonraDsf, AzSonraDsf2, SonDakika, OzelHaber, WhatsappIhbar

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

- **Framework:** ASP.NET Core
- **Dil:** C#
- **API Stili:** RESTful

---

## 📝 Lisans

Bu proje özel kullanım için hazırlanmıştır.

---

## 📞 Destek

Sorular ve teknik destekler için iletişime geçin.
