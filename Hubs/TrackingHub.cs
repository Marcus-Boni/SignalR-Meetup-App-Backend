// Hubs/TrackingHub.cs
using Microsoft.AspNetCore.SignalR;

namespace CarTrackingApi.Hubs
{
    // Este Hub não precisa de métodos chamados pelo cliente
    // O servidor é quem vai iniciar a comunicação
    public class TrackingHub : Hub
    {
        // Poderíamos ter métodos aqui para o cliente chamar, ex:
        // public async Task SendMessage(string user, string message)
        // Mas para este demo, só o servidor fala.
    }
}