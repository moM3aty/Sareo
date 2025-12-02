using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sareoo.Trackers
{
    public class PresenceTracker
    {
        private static readonly Dictionary<string, string> OnlineUsers = new Dictionary<string, string>();

        public Task UserConnected(string userId, string connectionId)
        {
            lock (OnlineUsers)
            {
                if (!OnlineUsers.ContainsKey(userId))
                {
                    OnlineUsers.Add(userId, connectionId);
                }
            }
            return Task.CompletedTask;
        }

        public Task UserDisconnected(string userId, string connectionId)
        {
            lock (OnlineUsers)
            {
                if (OnlineUsers.ContainsKey(userId))
                {
                    OnlineUsers.Remove(userId);
                }
            }
            return Task.CompletedTask;
        }

        public Task<string[]> GetOnlineUsers()
        {
            string[] onlineUsers;
            lock (OnlineUsers)
            {
                onlineUsers = OnlineUsers.Keys.ToArray();
            }
            return Task.FromResult(onlineUsers);
        }
    }
}