using Application.Dtos;
using Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers
{
    [Route("api/v{version:apiVersion}/order")]
    [ApiController]
    [ApiVersion("1.0")]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] CreateOrderCommand command, ApiVersion version)
        {
            var orderId = await _mediator.Send(command);

            return CreatedAtAction("Get", new
            {
                orderId,
                version = version.ToString()
            }, null);

        }

        [HttpGet("{orderId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(long orderId)
        {
            var orderQuery = new OrderQuery()
            {
                Id = orderId
            };

            var order = await _mediator.Send(orderQuery);
            if (order == null) 
            {
                return NotFound();
            }

            return Ok(order);
        }
    }
}
