using AgroSafari.Application.Commands.CreateServiceProvider;
using AgroSafari.Application.Commands.DeleteServiceProvider;
using AgroSafari.Application.Commands.LoginClient;
using AgroSafari.Application.Commands.LoginServiceProvider;
using AgroSafari.Application.Commands.UpdateClient;
using AgroSafari.Application.InputModels;
using AgroSafari.Application.Queries.GetAllServiceProviders;
using AgroSafari.Application.Queries.GetServiceProviderById;
using AgroSafari.Core.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSafariAPI.Controllers
{
    [Route("api/serviceProviders")]
    public class ServiceProviderController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IServiceProviderRepository _serviceProviderRepository;

        public ServiceProviderController(IMediator mediator, IServiceProviderRepository serviceProviderRepository)
        {
            _mediator = mediator;
            _serviceProviderRepository = serviceProviderRepository;
        }

        [HttpGet]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> GetAll(string query)
        {
            var command = new GetAllServiceProvidersQuery(query);

            var servicesProviders = await _mediator.Send(command);

            return Ok(servicesProviders);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> GetById(int id)
        {
            var serviceProvider = await _serviceProviderRepository.GetByIdAsync(id);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            return Ok(serviceProvider); 
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Post([FromBody] CreateServiceProviderCommand command)
        {
            var id = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = id }, command);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceProvider model)
        {
            var serviceProvider = await _serviceProviderRepository.GetByIdAsync(id);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            serviceProvider.Update(model.FullName, model.Email, model.Cnpj);
            await _serviceProviderRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Delete(int id)
        {
            var serviceProvider = await _serviceProviderRepository.GetByIdAsync(id);

            if (serviceProvider == null)
            {
                return NotFound();
            }

            await _serviceProviderRepository.DeleteAsync(serviceProvider);
            await _serviceProviderRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginServiceProviderCommand command)
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
