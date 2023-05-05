using AgroSafari.Application.Commands.CreateService;
using AgroSafari.Application.InputModels;
using AgroSafari.Application.Queries.GetAllServices;
using AgroSafari.Application.ViewModels;
using AgroSafari.Core.Repositories;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSafariAPI.Controllers
{
    [Route("api/services")]

    public class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IServiceRepository _serviceRepository;

        public ServiceController(IMediator mediator, IServiceRepository serviceRepository)
        {
            _mediator = mediator;
            _serviceRepository = serviceRepository;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Get(string? query)
        {
            var getAllServicesQuery = new GetAllServicesQuery(query);

            var services = await _mediator.Send(getAllServicesQuery);

            return Ok(services);
        }


        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            return Ok(service);
        }


        [HttpPost]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Post([FromBody] CreateServiceCommand command)
        {
            var id = await _mediator.Send(command);

            return CreatedAtAction(nameof(GetById), new { id = id }, command);
        }


        [HttpPut("{id}")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceInputModel model)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            service.Update(model.Title, model.Description, model.TotalCost);
            await _serviceRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/MakeAvailable")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> MakeAvailable(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            service.MakeAvailable();
            await _serviceRepository.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("{id}/Hire")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Hire(int id, int idClient)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            service.Hire(idClient);
            await _serviceRepository.SaveChangesAsync();

            return NoContent();
        }


        [HttpPut("{id}/finish")]
        [Authorize(Roles = "Client")]
        public async Task<IActionResult> Finish(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            service.Finish();

            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ServiceProvider")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            await _serviceRepository.DeleteAsync(service);
            await _serviceRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("{id}/Status")]
        [Authorize(Roles = "Client, ServiceProvider")]
        public async Task<IActionResult> GetServiceStatus(int id)
        {
            var service = await _serviceRepository.GetByIdAsync(id);

            if (service == null)
            {
                return NotFound();
            }

            var serviceViewModel = new ServiceStatusViewModel(service.Id, service.Title, service.ServiceStatus);

            return Ok(serviceViewModel);
        }
    }
}
