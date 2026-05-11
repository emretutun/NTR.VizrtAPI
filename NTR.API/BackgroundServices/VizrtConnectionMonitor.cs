using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using NTR.API.Hubs;
using NTR.Core.Interfaces;

namespace NTR.API.BackgroundServices
{
    public class VizrtConnectionMonitor : BackgroundService
    {
        private readonly IVizrtService _vizrtService;
        private readonly IHubContext<VizrtHub> _hub;
        private readonly Dictionary<string, bool> _lastKnownState = new();

        public VizrtConnectionMonitor(IVizrtService vizrtService, IHubContext<VizrtHub> hub)
        {
            _vizrtService = vizrtService;
            _hub = hub;
        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                var engines = _vizrtService.GetAllEngineStatus();
                foreach (var engine in engines)
                {
                    bool current = engine.IsConnected;
                    string key = engine.Name;
                    if (!_lastKnownState.TryGetValue(key, out bool last) || last != current)
                    {
                        _lastKnownState[key] = current;
                        await _hub.Clients.All.SendAsync("EngineStatusChanged", engine.Name, engine.IsConnected, DateTime.UtcNow.ToString("o"), stoppingToken);
                    }
                }
                await Task.Delay(3000, stoppingToken);
            }
        }
    }
}