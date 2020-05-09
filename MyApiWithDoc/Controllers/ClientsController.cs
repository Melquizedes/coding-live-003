using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MyApiWithDoc.Entities;
using MyApiWithDoc.MockData;
using MyApiWithDoc.Requests;
using MyApiWithDoc.Responses;
using static Bogus.DataSets.Name;

namespace MyApiWithDoc.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly ILogger<ClientsController> _logger;
        private static ClientMock mockClients;

        public ClientsController(ILogger<ClientsController> logger)
        {
            _logger = logger;
            if (mockClients == null)
            {
                _logger.LogInformation("Creating mock data");
                mockClients = new ClientMock();
            }
        }

        [HttpGet("{id}")]
        public ActionResult<ClientResponse> GetById([FromRoute] int id)
        {
            if (id <= 0)
                return BadRequest();

            var clientResponse = FilterById(id);
            if (clientResponse == null)
                return NotFound();

            return Ok(clientResponse);
        }

        [HttpGet("search")]
        public ActionResult<IEnumerable<ClientResponse>> GetByFilter(
            [FromQuery] string name = null,
            [FromQuery] Gender? gender = null)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            if (!string.IsNullOrEmpty(name) && gender == null)
            {
                var clientsResponse = FilterByName(name);
                if (clientsResponse == null)
                    NotFound();

                return Ok(clientsResponse);
            }

            else if (string.IsNullOrEmpty(name) && gender != null)
            {
                var clientsResponse = FilterByGender(gender);
                if (clientsResponse == null)
                    NotFound();

                return Ok(clientsResponse);
            }

            else if (!string.IsNullOrEmpty(name) && gender != null)
            {
                var clientsResponse = FilterByNameAndGender(name, gender);
                if (clientsResponse == null)
                    NotFound();

                return Ok(clientsResponse);
            }
            else
                return BadRequest();
        }

        [HttpGet]
        public ActionResult<ClientResponse> GetAll()
        {
            try
            {
                var clientsResponse = NoFilter();
                if (clientsResponse == null)
                    return NoContent();

                return Ok(clientsResponse);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public ActionResult<int> Post([FromBody] CreateClientRequest clientRequest)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var nextId = mockClients.Data.Max(c => c.Id) + 1;
            var client = new Client(
                id: nextId,
                name: clientRequest.Name,
                email: clientRequest.Email,
                gender: clientRequest.Gender,
                phone: clientRequest.Phone);

            mockClients.Add(client);

            return CreatedAtAction(nameof(Post), new { Id = client.Id });
        }

        [HttpPut("{id}")]
        public ActionResult<int> Put(
            [FromRoute] int id,
            [FromBody] UpdateClientRequest clientRequest)
        {
            if (!ModelState.IsValid || id != clientRequest.Id)
                return BadRequest();

            var client = mockClients.GetById(id);
            if (client == null)
                return NotFound();

            client.Name = clientRequest.Name;
            client.Email = clientRequest.Email;
            client.Gender = clientRequest.Gender;
            client.Phone = clientRequest.Phone;

            mockClients.Update(client);

            return Ok(new { Id = client.Id });
        }

        [HttpPatch("{id}")]
        public ActionResult<int> Patch(
            [FromRoute] int id,
            [FromQuery] bool enabled)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var client = mockClients.GetById(id);
            if (client == null)
                return NotFound();

            client.Enabled = enabled;

            mockClients.Update(client);

            return Ok(new { Id = client.Id });
        }

        [HttpDelete("{id}")]
        public ActionResult<int> Delete([FromRoute] int id)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var client = mockClients.GetById(id);
            if (client == null)
                return NotFound();

            mockClients.Remove(id);

            return NoContent();
        }

        private IEnumerable<ClientResponse> NoFilter()
        {
            return from client in mockClients.Data
                   select new ClientResponse(
                       id: client.Id,
                       name: client.Name,
                       email: client.Email,
                       gender: client.Gender,
                       phone: client.Phone,
                       enabled: client.Enabled
                   );
        }
        private ClientResponse FilterById(int id)
        {
            return (from client in mockClients.Data
                    where client.Id.Equals(id)
                    select new ClientResponse(
                        id: client.Id,
                        name: client.Name,
                        email: client.Email,
                        gender: client.Gender,
                        phone: client.Phone,
                        enabled: client.Enabled
                    )).FirstOrDefault();
        }
        private IEnumerable<ClientResponse> FilterByName(string name)
        {
            return from client in mockClients.Data
                   where client.Name.Contains(name)
                   select new ClientResponse(
                       id: client.Id,
                       name: client.Name,
                       email: client.Email,
                       gender: client.Gender,
                       phone: client.Phone,
                       enabled: client.Enabled
                   );
        }

        private IEnumerable<ClientResponse> FilterByGender(Gender? gender)
        {
            return from client in mockClients.Data
                   where client.Gender.Equals(gender)
                   select new ClientResponse(
                       id: client.Id,
                       name: client.Name,
                       email: client.Email,
                       gender: client.Gender,
                       phone: client.Phone,
                       enabled: client.Enabled
                   );
        }

        private object FilterByNameAndGender(string name, Gender? gender)
        {
            return from client in mockClients.Data
                   where client.Name.Contains(name) && client.Gender.Equals(gender)
                   select new ClientResponse(
                       id: client.Id,
                       name: client.Name,
                       email: client.Email,
                       gender: client.Gender,
                       phone: client.Phone,
                       enabled: client.Enabled
                   );
        }
    }
}