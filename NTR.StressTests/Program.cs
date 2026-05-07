using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// Ngrok adresin ve API anahtarın tanımlandı
const string ApiBaseUrl = "https://localhost:7043";
const string ApiKey = "ntr-vizrt-2026-secret-key";
const int ToplamIstekSayisi = 1000;

Console.WriteLine("==================================================");
Console.WriteLine(" NTR VIZRT API - STRES VE YÜK TESTİ BAŞLIYOR...");
Console.WriteLine("==================================================");
Console.WriteLine($"Hedef URL: {ApiBaseUrl}/api/rundown");
Console.WriteLine($"Gönderilecek İstek Sayısı: {ToplamIstekSayisi}\n");

// HttpClient'ı bir kere oluşturuyoruz (Soketleri tüketmemek için çok önemli)
using var httpClient = new HttpClient();
httpClient.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);

// Ngrok tarafında "Too Many Requests" (429) yememek için timeout'u uzun tutuyoruz
httpClient.Timeout = TimeSpan.FromSeconds(30);

var gecikmeler = new ConcurrentBag<long>();
int basariliIstekler = 0;
int hataliIstekler = 0;

var kronometre = Stopwatch.StartNew();

// 1000 isteği aynı anda asenkron olarak fırlatıyoruz
var gorevler = Enumerable.Range(0, ToplamIstekSayisi).Select(async i =>
{
    var istekKronometresi = Stopwatch.StartNew();
    try
    {
        var response = await httpClient.GetAsync($"{ApiBaseUrl}/api/rundown");
        istekKronometresi.Stop();

        gecikmeler.Add(istekKronometresi.ElapsedMilliseconds);

        // 200 OK döndüyse başarılı say
        if (response.IsSuccessStatusCode)
            Interlocked.Increment(ref basariliIstekler);
        else
            Interlocked.Increment(ref hataliIstekler);
    }
    catch
    {
        istekKronometresi.Stop();
        Interlocked.Increment(ref hataliIstekler);
    }
});

// Tüm mermilerin (isteklerin) hedefe varıp dönmesini bekle
await Task.WhenAll(gorevler);

kronometre.Stop();

// Sonuçları Hesapla
double ortalamaGecikme = gecikmeler.Any() ? gecikmeler.Average() : 0;
long enHizli = gecikmeler.Any() ? gecikmeler.Min() : 0;
long enYavas = gecikmeler.Any() ? gecikmeler.Max() : 0;

Console.WriteLine("TEST TAMAMLANDI!\n");
Console.WriteLine("=== SONUÇLAR ===");
Console.WriteLine($"Toplam Geçen Süre   : {kronometre.ElapsedMilliseconds} ms");
Console.WriteLine($"Başarılı İstekler   : {basariliIstekler}");
Console.WriteLine($"Hatalı İstekler     : {hataliIstekler}");
Console.WriteLine("--------------------------------------------------");
Console.WriteLine($"Ortalama Yanıt Süresi: {ortalamaGecikme:F2} ms");
Console.WriteLine($"En Hızlı Yanıt       : {enHizli} ms");
Console.WriteLine($"En Yavaş Yanıt       : {enYavas} ms");
Console.WriteLine("==================================================");

if (hataliIstekler > 0)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("\nDİKKAT: Hatalı istekler var! API veya Ngrok yük altında boğuluyor olabilir.");
    Console.ResetColor();
}
else if (ortalamaGecikme > 250) // Ngrok internet tüneli olduğu için eşiği yüksek tuttuk
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\nUYARI: Sıfır hata var ama yanıt süreleri yüksek. (Bu durum Ngrok tünelinden kaynaklı olabilir)");
    Console.ResetColor();
}
else
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.WriteLine("\nHARİKA: API ngrok üzerinden bile kaya gibi sağlam çalışıyor!");
    Console.ResetColor();
}

Console.WriteLine("\nÇıkmak için Enter'a basın...");
Console.ReadLine();