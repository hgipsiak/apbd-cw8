using apbd_cw8.Services.DTOs;

namespace apbd_cw8.Services;

public interface ITripsService
{
    Task<List<TripDTO>> GetTrips();
}