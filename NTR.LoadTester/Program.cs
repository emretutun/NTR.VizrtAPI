using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

const string ApiBaseUrl = "https://localhost:7043";
const string ApiKey = "ntr-vizrt-2026-secret-key";

const int ConcurrentUsers = 100;
const int RequestsPerUser = 250;

using var client = new HttpClient();

client.DefaultRequestHeaders.Add("X-Api-Key", ApiKey);
client.Timeout = TimeSpan.FromSeconds(10);

var rnd = new Random();

var endpointStats = new ConcurrentDictionary<string, List<long>>();
var errors = new ConcurrentBag<string>();

int success = 0;
int failed = 0;

Console.WriteLine("========================================");
Console.WriteLine("NTR VIZRT API CHAOS TEST");
Console.WriteLine("========================================");

var sw = Stopwatch.StartNew();

var tasks = Enumerable.Range(0, ConcurrentUsers)
    .Select(userId => Task.Run(async () =>
    {
        for (int i = 0; i < RequestsPerUser; i++)
        {
            var action = rnd.Next(0, 12);

            try
            {
                switch (action)
                {
                    case 0:
                        await Measure("GET /engine/status", async () =>
                        {
                            await client.GetAsync($"{ApiBaseUrl}/api/engine/status");
                        });
                        break;

                    case 1:
                        await Measure("POST /kj/ver", async () =>
                        {
                            var body = new
                            {
                                type = rnd.Next(0, 3),
                                text1 = $"BREAKING NEWS {Guid.NewGuid()}",
                                text2 = $"ALT YAZI {Guid.NewGuid()}",
                                rozet = rnd.Next(0, 7)
                            };

                            await client.PostAsJsonAsync(
                                $"{ApiBaseUrl}/api/kj/Reji/ver",
                                body);
                        });
                        break;

                    case 2:
                        await Measure("POST /kj/al", async () =>
                        {
                            await client.PostAsync(
                                $"{ApiBaseUrl}/api/kj/Reji/al",
                                null);
                        });
                        break;

                    case 3:
                        await Measure("POST /raw", async () =>
                        {
                            var body = new
                            {
                                command = "RENDERER*STAGE TO_START"
                            };

                            await client.PostAsJsonAsync(
                                $"{ApiBaseUrl}/api/engine/Reji/raw",
                                body);
                        });
                        break;

                    case 4:
                        await Measure("POST /roll/ver", async () =>
                        {
                            var body = new
                            {
                                tesekkurYazisi = "TEST",
                                satirlar = Enumerable.Range(1, 10)
                                    .Select(x => new
                                    {
                                        baslik = $"TITLE {x}",
                                        yazi = $"NAME {Guid.NewGuid()}"
                                    }),
                                sponsorlar = Array.Empty<string>()
                            };

                            await client.PostAsJsonAsync(
                                $"{ApiBaseUrl}/api/kj/Reji/roll/ver",
                                body);
                        });
                        break;

                    case 5:
                        await Measure("POST /kelebek/isim", async () =>
                        {
                            var body = new
                            {
                                index = rnd.Next(1, 6),
                                isim = $"GUEST {Guid.NewGuid()}",
                                title = "EKONOMIST"
                            };

                            await client.PostAsJsonAsync(
                                $"{ApiBaseUrl}/api/kj/Reji/kelebek/isim",
                                body);
                        });
                        break;

                    case 6:
                        await Measure("GET /rundown", async () =>
                        {
                            await client.GetAsync(
                                $"{ApiBaseUrl}/api/rundown");
                        });
                        break;

                    case 7:
                        await Measure("POST /rundown", async () =>
                        {
                            var body = new
                            {
                                ad = $"TEST {Guid.NewGuid()}",
                                tarih = DateTime.Now.ToString("yyyy-MM-dd"),
                                saat = "20:00",
                                kanal = "Show TV"
                            };

                            await client.PostAsJsonAsync(
                                $"{ApiBaseUrl}/api/rundown",
                                body);
                        });
                        break;

                    case 8:
                        await Measure("GET /haber", async () =>
                        {
                            await client.GetAsync(
                                $"{ApiBaseUrl}/api/haber");
                        });
                        break;

                    case 9:
                        await Measure("GET /log/hatalar", async () =>
                        {
                            await client.GetAsync(
                                $"{ApiBaseUrl}/api/log/hatalar");
                        });
                        break;

                    case 10:
                        await Measure("POST /canli/ver", async () =>
                        {
                            await client.PostAsync(
                                $"{ApiBaseUrl}/api/kj/Reji/canli/ver",
                                null);
                        });
                        break;

                    case 11:
                        await Measure("POST /tumunu-al", async () =>
                        {
                            await client.PostAsync(
                                $"{ApiBaseUrl}/api/kj/Reji/tumunu-al",
                                null);
                        });
                        break;
                }

                Interlocked.Increment(ref success);
            }
            catch (Exception ex)
            {
                Interlocked.Increment(ref failed);

                errors.Add(ex.GetType().Name + " => " + ex.Message);
            }
        }
    }));

await Task.WhenAll(tasks);

sw.Stop();

Console.WriteLine();
Console.WriteLine("========================================");
Console.WriteLine("TEST FINISHED");
Console.WriteLine("========================================");

Console.WriteLine($"SUCCESS : {success}");
Console.WriteLine($"FAILED  : {failed}");
Console.WriteLine($"DURATION: {sw.Elapsed}");
Console.WriteLine();

Console.WriteLine("========== ENDPOINT STATS ==========");

foreach (var stat in endpointStats.OrderBy(x => x.Key))
{
    if (stat.Value.Count == 0)
        continue;

    Console.WriteLine(
        $"{stat.Key,-30} " +
        $"AVG: {stat.Value.Average():F2} ms | " +
        $"MAX: {stat.Value.Max()} ms");
}

Console.WriteLine();

if (errors.Count > 0)
{
    Console.WriteLine("========== ERRORS ==========");

    foreach (var err in errors.Take(20))
    {
        Console.WriteLine(err);
    }
}

Console.WriteLine();
Console.WriteLine("Press ENTER...");
Console.ReadLine();

async Task Measure(string name, Func<Task> action)
{
    var s = Stopwatch.StartNew();

    await action();

    s.Stop();

    endpointStats.AddOrUpdate(
        name,
        _ => new List<long> { s.ElapsedMilliseconds },
        (_, list) =>
        {
            lock (list)
            {
                list.Add(s.ElapsedMilliseconds);
            }

            return list;
        });
}