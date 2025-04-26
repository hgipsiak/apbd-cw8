using apbd_cw8.Services.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;TrustServerCertificate=True;";

    public async Task<List<TripDTO>> GetTrips()
    {
        var trips = new Dictionary<int, TripDTO>();
        
        string command = @"SELECT trip.IdTrip, trip.Name, trip.Description, trip.DateFrom, trip.DateTo, trip.MaxPeople, country.IdCountry AS CountryId, country.Name AS CountryName
            FROM Trip 
            JOIN Country_Trip ON trip.IdTrip = Country_Trip.IdTrip
            JOIN Country ON Country_Trip.IdCountry = Country.IdCountry
            ORDER BY trip.IdTrip ASC";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int idTrip = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    int nameOrdinal = reader.GetOrdinal("Name");
                    int descriptionOrdinal = reader.GetOrdinal("Description");
                    int dateFromOrdinal = reader.GetOrdinal("DateFrom");
                    int dateToOrdinal = reader.GetOrdinal("DateTo");
                    int maxPeopleOrdinal = reader.GetOrdinal("MaxPeople");
                    int idCountryOrdinal = reader.GetOrdinal("CountryId");
                    int countryNameOrdinal = reader.GetOrdinal("CountryName");
                    if (!trips.ContainsKey(idTrip))
                    {
                        trips[idTrip] = new TripDTO()
                        {
                            Id = idTrip,
                            Name = reader.GetString(nameOrdinal),
                            Description = reader.GetString(descriptionOrdinal),
                            DateFrom = reader.GetDateTime(dateFromOrdinal),
                            DateTo = reader.GetDateTime(dateToOrdinal),
                            MaxPeople = reader.GetInt32(maxPeopleOrdinal),
                            Countries = new List<CountryDTO>()
                        };
                    }

                    trips[idTrip].Countries.Add(new CountryDTO()
                    {
                        IdCountry = reader.GetInt32(idCountryOrdinal),
                        Name = reader.GetString(countryNameOrdinal)
                    });

                }
            }
        }
        
        return trips.Values.ToList();
    }
}