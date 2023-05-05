using AgroSafari.Application.Commands.CreateClient;
using AgroSafari.Application.Commands.DeleteClient;
using AgroSafari.Application.Commands.LoginClient;
using AgroSafari.Application.Commands.UpdateClient;
using AgroSafari.Application.Queries.GetAllClients;
using AgroSafari.Application.Queries.GetClientById;
using AgroSafari.Core.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSafariAPI.Controllers
{
    [Route("api/clients")]

    public class ClientController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IClientRepository _clientRepository;

        public ClientController(IMediator mediator, IClientRepository clientRepository)
        {
            _mediator = mediator;
            _clientRepository = clientRepository;
        }

        [HttpGet]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Get(string query)
        {
            var getAllClientsQuery = new GetAllClientsQuery(query);

            var clients = await _mediator.Send(getAllClientsQuery);

            return Ok(clients);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> GetById(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            return Ok(client);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] CreateClientCommand command)
        {
            var id = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = id }, command);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateClientCommand model)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            client.Update(model.FullName, model.Email, model.BirthDate, model.Cpf);
            await _clientRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = await _clientRepository.GetByIdAsync(id);

            if (client == null)
            {
                return NotFound();
            }

            await _clientRepository.DeleteAsync(client);
            await _clientRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginClientCommand command)
        {
            var loginUserViewModel = await _mediator.Send(command);

            if (loginUserViewModel == null) 
            {
                return BadRequest();
            }
            
            return Ok(loginUserViewModel);
        }
    }
}
