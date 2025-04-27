using apbd_cw8.Services;
using apbd_cw8.Services.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace apbd_cw8.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientsService _clientsService;

        public ClientsController(IClientsService clientsService)
        {
            _clientsService = clientsService;
        }

        
        [HttpGet("{id}/trips")]
        public async Task<IActionResult> GetClientTrips(int id)
        {
            var clientTrips = await _clientsService.GetClientTrips(id);
            if (clientTrips.Count == 0) return NotFound("Client not found or there are no trips");
            return Ok(clientTrips);
        }

        [HttpPost]
        public async Task<IActionResult> AddClient(ClientDTO client)
        {
            var newClient = await _clientsService.AddClient(client);
            return Ok(newClient);
        }

        [HttpPut("{id}/trips/{tripId}")]
        public async Task<IActionResult> RegisterClient(int id, int tripId)
        {
            var registerClient = await _clientsService.RegisterClient(id, tripId);
            switch (registerClient)
            {
                case -2: return BadRequest("Trip already filled");
                case -1: return NotFound("Client or Trip not found");
            }
            return Ok("Client successfully registered");
        }

        [HttpDelete("{id}/trips/{tripId}")]
        public async Task<IActionResult> UnregisterClient(int id, int tripId)
        {
            var unregisterClient = await _clientsService.UnregisterClient(id, tripId);
            if (unregisterClient == -1) return NotFound("Registration not found");
            return Ok("Client successfully unregistered");
        }
    }
}
