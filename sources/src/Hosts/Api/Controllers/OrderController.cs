using Application.Commands;
using Application.Queries;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace auth_service.Controllers
{
    [Route("api/v{version:apiVersion}/order")]
    [ApiController]
    [ApiVersion("1.0")]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrderController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{orderId:long}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(long orderId)
        {
            try
            {
                var orderQuery = new OrderQuery()
                {
                    Id = orderId
                };

                var order = await _mediator.Send(orderQuery);
                return Ok(order);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }

        [HttpPost("addProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> AddProduct([FromBody] AddProductCommand command)
        {
            var orderId = await _mediator.Send(command);
            return Ok(new { Id = orderId });
        }

        [HttpPost("deleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DeleteProduct([FromBody] DeleteProductCommand command)
        {
            var orderId = await _mediator.Send(command);
            return Ok(new { Id = orderId });
        }

        [HttpPost("doPayment")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> DoPayment([FromBody] DoPaymentCommand command)
        {
            try
            {
                if (!command.OrderId.HasValue)
                {
                    return BadRequest("Не передан номер заказа.");
                }

                var paymentLink = await _mediator.Send(command);
                return Ok(new { PaymentLink = paymentLink });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)ex.StatusCode, ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex);
            }
        }
    }
}
