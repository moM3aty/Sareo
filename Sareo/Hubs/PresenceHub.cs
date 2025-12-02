using Sareoo.Trackers;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Sareoo.Hubs
{
    [Authorize] 
    public class PresenceHub : Hub
    {
        private readonly PresenceTracker _tracker;

        public PresenceHub(PresenceTracker tracker)
        {
            _tracker = tracker;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _tracker.UserConnected(userId, Context.ConnectionId);
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception exception)
        {
            var userId = Context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await _tracker.UserDisconnected(userId, Context.ConnectionId);
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}