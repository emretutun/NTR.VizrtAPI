using Microsoft.AspNetCore.SignalR;
using NTR.Core.Interfaces;

namespace NTR.API.Hubs
{
    public class VizrtHub : Hub  
    {

        public override async Task OnConnectedAsync()
        {
            var vizrtService = Context.GetHttpContext()!
                .RequestServices.GetRequiredService<IVizrtService>();

            var engines = vizrtService.GetAllEngineStatus();
            foreach (var engine in engines)
            {
                await Clients.Caller.SendAsync("EngineStatusChanged",
                    engine.Name, engine.IsConnected, DateTime.UtcNow.ToString("o"));
            }
            await base.OnConnectedAsync();
        }


    }
}