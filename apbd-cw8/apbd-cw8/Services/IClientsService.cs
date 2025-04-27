using apbd_cw8.Services.DTOs;

namespace apbd_cw8.Services;

public interface IClientsService
{
    Task<List<ClientTripsDTO>> GetClientTrips(int id);
    Task<int> AddClient(ClientDTO client);
    Task<int> RegisterClient(int clientId, int tripId);
    
    Task<int> UnregisterClient(int clientId, int tripId);
}