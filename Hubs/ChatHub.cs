using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class ChatHub : Hub
{
    public async Task SendOffer(string user, string offer)
    {
        await Clients.Others.SendAsync("ReceiveOffer", user, offer);
    }

    public async Task SendAnswer(string user, string answer)
    {
        await Clients.Others.SendAsync("ReceiveAnswer", user, answer);
    }

    public async Task SendCandidate(string user, string candidate)
    {
        await Clients.Others.SendAsync("ReceiveCandidate", user, candidate);
    }
}
