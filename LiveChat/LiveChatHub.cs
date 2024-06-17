using Microsoft.AspNetCore.SignalR;

namespace LiveChat;

public class LiveChatHub : Hub
{
    private static List<(string? user, string message)> _chatMessages = new List<(string?, string)>();
    private static Dictionary<string, string> _userConnections = new Dictionary<string, string>();

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.Identity?.Name;
        var connectionId = Context.ConnectionId;

        lock (_userConnections)
        {
            if (!_userConnections.ContainsKey(userId))
            {
                _userConnections[userId] = connectionId;
            }
        }

        await UpdateConnectedUsersCount();
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User?.Identity?.Name;

        lock (_userConnections)
        {
            if (_userConnections.ContainsKey(userId) && _userConnections[userId] == Context.ConnectionId)
            {
                _userConnections.Remove(userId);
            }
        }

        await UpdateConnectedUsersCount();
        await base.OnDisconnectedAsync(exception);
    }
    public async Task SendMessage(string? user, string message)
    {
        user = Context?.User?.Identity?.Name;
        _chatMessages.Add((user, message));
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task GetChatHistory()
    {
        var serializedChatHistory = _chatMessages.Select(msg => new { user = msg.user, message = msg.message }).ToArray();
        await Clients.Caller.SendAsync("LoadChatHistory", serializedChatHistory);
    }

    private async Task UpdateConnectedUsersCount()
    {
        await Clients.All.SendAsync("ReceiveConnectedUsersCount", _userConnections.Count);
    }
}
