using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SWP391Web.Domain.Constants;
using System.Security.Claims;

namespace SWP391Web.Infrastructure.SignlR
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var dealerId = Context.User?.FindFirst("DealerId")?.Value;
            var roles = Context.User?.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();
            Console.WriteLine($"[Hub] Connected user: {userId}, roles: {string.Join(",", roles)}");
            await base.OnConnectedAsync();
        }

    }
}
