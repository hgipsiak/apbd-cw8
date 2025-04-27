using apbd_cw8.Services.DTOs;
using Microsoft.Data.SqlClient;

namespace apbd_cw8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString =
        "Data Source=db-mssql;Initial Catalog=2019SBD;Integrated Security=True;TrustServerCertificate=True;";

    public async Task<List<ClientTripsDTO>> GetClientTrips(int id)
    {
        var clientTrips = new Dictionary<int, ClientTripsDTO>();

        string command = @"SELECT trip.IdTrip, trip.Name, trip.Description, trip.DateFrom, trip.DateTo, trip.MaxPeople,
                            country.IdCountry AS CountryId, country.Name AS CountryName, CAST(CAST(Client_Trip.RegisteredAt AS nvarchar(120)) AS datetime) AS RegisteredAt, 
                            CAST(CAST(Client_Trip.PaymentDate AS nvarchar(120)) AS datetime) AS PaymentDate
                            FROM Trip
                            JOIN Country_Trip ON trip.IdTrip = Country_Trip.IdTrip
                            JOIN Country ON Country.IdCountry = Country_Trip.IdCountry
                            JOIN Client_Trip ON trip.IdTrip = Client_Trip.IdTrip
                            JOIN Client ON Client.IdClient = Client_Trip.IdClient
							WHERE Client.IdClient = @ClientId
							ORDER BY trip.IdTrip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("ClientId", id);

            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    int tripId = reader.GetInt32(reader.GetOrdinal("IdTrip"));
                    int tripNameOrdinal = reader.GetOrdinal("Name");
                    int descriptionOrdinal = reader.GetOrdinal("Description");
                    int dateFromOrdinal = reader.GetOrdinal("DateFrom");
                    int dateToOrdinal = reader.GetOrdinal("DateTo");
                    int maxPeopleOrdinal = reader.GetOrdinal("MaxPeople");
                    int idCountryOrdinal = reader.GetOrdinal("CountryId");
                    int countryNameOrdinal = reader.GetOrdinal("CountryName");
                    int registeredAtOrdinal = reader.GetOrdinal("RegisteredAt");
                    int paymentDateOrdinal = reader.GetOrdinal("PaymentDate");
                    if (!clientTrips.ContainsKey(tripId))
                    {
                        clientTrips[tripId] = new ClientTripsDTO(){
                            Id = tripId,
                            Name = reader.GetString(tripNameOrdinal),
                            Description = reader.GetString(descriptionOrdinal),
                            DateFrom = reader.GetDateTime(dateFromOrdinal),
                            DateTo = reader.GetDateTime(dateToOrdinal),
                            MaxPeople = reader.GetInt32(maxPeopleOrdinal),
                            Countries = new List<CountryDTO>(),
                            RegisteredAt = reader.GetDateTime(registeredAtOrdinal),
                            PaymentDate = reader.IsDBNull(paymentDateOrdinal) ? null : reader.GetDateTime(paymentDateOrdinal)
                        };
                    }

                    clientTrips[tripId].Countries.Add(new CountryDTO()
                    {
                        IdCountry = reader.GetInt32(idCountryOrdinal),
                        Name = reader.GetString(countryNameOrdinal)
                    });
                }
            }
        }

        return clientTrips.Values.ToList();
    }

    public async Task<int> AddClient(ClientDTO client)
    {
        string command = @"INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) VALUES 
                                                                      (@FirstName, @LastName, @Email, @Telephone, @Pesel);
                            SELECT SCOPE_IDENTITY();";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(command, conn))
        {
            cmd.Parameters.AddWithValue("FirstName", client.FirstName);
            cmd.Parameters.AddWithValue("LastName", client.LastName);
            cmd.Parameters.AddWithValue("Email", client.Email);
            cmd.Parameters.AddWithValue("Telephone", client.Telephone);
            cmd.Parameters.AddWithValue("Pesel", client.Pesel);
            
            await conn.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            client.Id = Convert.ToInt32(result);
            
            return Convert.ToInt32(result);
        }
    }

    public async Task<int> RegisterClient(int clientId, int tripId)
    {
        string command1 = @"INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt)
                            VALUES (@IdClient, @IdTrip, @RegisteredAt);";
        string command2 = "SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient";
        string command3 = "SELECT COUNT(*) FROM Trip WHERE IdTrip = @IdTrip";
        string command4 = "SELECT Trip.MaxPeople FROM Trip WHERE IdTrip = @IdTrip";
        string command5 = "SELECT COUNT(*) FROM Client_Trip WHERE IdTrip = @IdTrip";
            
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd1 = new SqlCommand(command1, conn))
        using (SqlCommand cmd2 = new SqlCommand(command2, conn))
        using (SqlCommand cmd3 = new SqlCommand(command3, conn))
        using (SqlCommand cmd4 = new SqlCommand(command4, conn))
        using (SqlCommand cmd5 = new SqlCommand(command5, conn))
        {
            cmd2.Parameters.AddWithValue("IdClient", clientId);
            cmd3.Parameters.AddWithValue("IdTrip", tripId);
            cmd4.Parameters.AddWithValue("IdTrip", tripId);
            cmd5.Parameters.AddWithValue("IdTrip", tripId);
            
            cmd1.Parameters.AddWithValue("IdClient", clientId);
            cmd1.Parameters.AddWithValue("IdTrip", tripId);
            cmd1.Parameters.AddWithValue("RegisteredAt", int.Parse(DateTime.Now.ToString("yyyyMMdd")));
            
            await conn.OpenAsync();
            
            var client = (int)await cmd2.ExecuteScalarAsync();
            var trip = (int)await cmd3.ExecuteScalarAsync();
            if (client == 0 || trip == 0) return -1;
            var maxCount = (int)await cmd4.ExecuteScalarAsync();
            var registered = (int)await cmd5.ExecuteScalarAsync();
            if (registered == maxCount) return -2;
            
            await cmd1.ExecuteNonQueryAsync();
            
            return 0;
        }
    }

    public async Task<int> UnregisterClient(int clientId, int tripId)
    {
        string command1 = @"DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";
        string command2 = @"SELECT COUNT(*) FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd1 = new SqlCommand(command1, conn))
        using (SqlCommand cmd2 = new SqlCommand(command2, conn))
        {
            cmd2.Parameters.AddWithValue("IdClient", clientId);
            cmd2.Parameters.AddWithValue("IdTrip", tripId);
            cmd1.Parameters.AddWithValue("IdClient", clientId);
            cmd1.Parameters.AddWithValue("IdTrip", tripId);
            
            await conn.OpenAsync();
            var registered = (int)await cmd2.ExecuteScalarAsync();
            if (registered == 0) return -1;
            int rowsAffected = await cmd1.ExecuteNonQueryAsync();
            
            return rowsAffected;
        }
    }
}