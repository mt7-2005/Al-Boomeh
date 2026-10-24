using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Services;
using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using System.Threading.Tasks;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Al_Boomeh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _order;
        private readonly IConfiguration _configuration;
        private readonly IUserService _usersService;
        private readonly ISendOrderConfirmation _sendOrderConfirmation;
        public OrdersController(IOrderService order, IConfiguration configuration, IUserService usersService, ISendOrderConfirmation sendOrderConfirmation)
        {
            _usersService = usersService;
            _configuration = configuration;
            _order = order;
            _sendOrderConfirmation = sendOrderConfirmation;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet(Name = "GetAllOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderInfoDTO>>> GetAllOrders([FromQuery] int pageNumber, [FromQuery] int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0 || pageSize > 50) return BadRequest("Invalid Data");

            var orderlist = await _order.GetAllOrders(pageNumber, pageSize);
            if (orderlist == null) return NotFound("No Orders Found");
            return Ok(orderlist);
        }

        [HttpGet("{id}", Name = "GetOrderById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderInfoDTO?>> GetOrderById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 0 || !int.TryParse(id.ToString(), out int number))
            {
                return BadRequest("Invalid Data");
            }
            var order = await _order.GetOrderById(id);

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                order.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (order == null) return NotFound($"No order with id {id}");
            return Ok(order);
        }


        [HttpGet("code/{code}", Name = "GetOrderByCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<OrderInfoDTO?>> GetOrderByCode(string code, [FromServices] IAuthorizationService authorizationService)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("Invalid Data");
            }
            var order = await _order.GetOrderByCode(code);


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                order.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (order == null) return NotFound($"No order with code {code}");
            return Ok(order);
        }


        [HttpGet("count", Name = "GetOrdersCountByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderInfoDTO>?>> GetOrdersCountByStatus([FromQuery] int storeId, [FromQuery] enStatus status, [FromServices] IAuthorizationService authorizationService)
        {

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var orderlist = await _order.GetOrdersByStatus(status, storeId);
            if (orderlist == 0) return NotFound("No data found");
            return Ok(orderlist);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("count-today", Name = "GetOrderCount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<long>> GetOrderCount()
        {
            var result = await _order.GetOrderCount();
            if (result == 0) return NotFound("No data found");
            return Ok(result);
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("avg-order-value-last-month", Name = "AvgOrderValueLastMonth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal>> AvgOrderValueLastMonth()
        {
            return await _order.AvgOrderValueLastMonth();
        }

        [Authorize(Roles = "Customer")]
        [HttpPost(Name = "CreateCartLine")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateCartLine([FromBody] CreateOrderCartDTO orderDTO, [FromServices] IAuthorizationService authorizationService, [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
        {


            if (orderDTO == null) return BadRequest("Invalid Data");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                orderDTO.CustomerId,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            int newId = await _order.CreateCart(idempotencyKey, orderDTO);

            if (newId == -1) return BadRequest("failed to create order");
            return CreatedAtAction(nameof(GetOrderById), new { id = newId }, new { id = newId });

        }

        [HttpPut("{id}/time", Name = "UpdateOrderTime")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateOrderTime([FromBody] UpdateOrderTimeDTO orderTimeDTO, int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 0 || orderTimeDTO.EstimatedDeliveryTime <= 0 || orderTimeDTO.EstimatedDeliveryTime <= 0) return BadRequest("Invalid Data");

            var order = await _order.GetOrderById(id);

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                order.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (order == null) return NotFound($"No order with id {id}");

            if (await _order.UpdateOrderTime(id, orderTimeDTO)) return Ok();
            else return BadRequest("Failed To Update Order");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteOrder")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult> DeleteOrder(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            if (await _order.DeleteOrder(id)) return Ok();
            else return BadRequest("Failed To delete");
        }

        [HttpGet("code/{code}/lines", Name = "GetLinesByOrderId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderLineDTO>>> GetLinesByOrderCode(string code, [FromServices] IAuthorizationService authorizationService)
        {
            if (string.IsNullOrEmpty(code)) return BadRequest("Invalid Data");
            var order = await _order.GetOrderByCode(code);

            if (order == null) return NotFound($"No order with code {code}");

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                order.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var authResultCustomer = await authorizationService.AuthorizeAsync(
              User,
              order.CustomerId,
              "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var lineslist = await _order.GetOrderLines(code);

            if (lineslist == null) return NotFound("No data found");
            return Ok(lineslist);
        }

        //owner
        [HttpPut("{id}/status", Name = "UpdateOrderStatus")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDTO statusDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0) return BadRequest("Invalid Data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                statusDTO.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (await _order.UpdateOrderStatus(id, statusDTO)) return Ok();
            return BadRequest("Failed to update");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{id}/lines", Name = "AddLineToOrder")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddLineToOrder([FromBody] CreateOrderLineDTO lineDTO, int id)
        {
            if (id <= 0) return BadRequest("invalid data");
            if (lineDTO == null) return BadRequest("invalid data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            var newlineId = await _order.AddOrderLine(id, lineDTO);
            return Created();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{orderId}/lines/{lineId}", Name = "DeleteLine")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteLineFromOrder(int orderid, int lineid)
        {
            if (orderid <= 0 || lineid <= 0) { return BadRequest("Invalid data"); }
            if (!await _order.IsExist(orderid)) return NotFound($"No Order with id {orderid}");

            if (await _order.DeleteOrderLine(lineid)) return Ok("Deleted Successfully");
            return BadRequest("failed to delete");
        }

        [HttpPost("{id}/feedback", Name = "CreateFeedback")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreateFeedback(int id, [FromBody] OrderFeedbackDTO feedbackDTO)
        {
            if (id <= 0) return BadRequest("Invalid data");
            if (feedbackDTO == null) return BadRequest("Invalid data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            if (await _order.CreateOrderFeedback(feedbackDTO) != -1) return Ok();
            return BadRequest("failed to add");
        }
        [HttpGet("{id}/feedback", Name = "GetOrderFeedback")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderFeedbackDTO>> GetOrderFeedback(int id)
        {
            if (id <= 0) return BadRequest("Invalid data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            return Ok(await _order.GetOrderFeedback(id));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{id}/status-history", Name = "GetOrderHistoryStatus")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<OrderStatusHistoryDTO>> GetOrderHistoryStatus(int id)
        {
            if (id <= 0) return BadRequest("Invalid data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            return Ok(await _order.GetOrderStatusHistories(id));
        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}",Name = "PlaceOrder")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> PlaceOrder(int id, PlaceOrderDTO orderDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0 || orderDTO == null) return BadRequest("Invalid Data");
            if (!await _order.IsExist(id)) return NotFound("No order found");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                orderDTO.CustomerId,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            bool result = false;
            if (orderDTO.Type == enOrderType.Delivery)
            {
                decimal deliveryfees = _configuration.GetValue<decimal>("OrderSettings:DeliveryFees");
                decimal servicefees = _configuration.GetValue<decimal>("OrderSettings:ServiceFees");
                result =await _order.PlaceOrder(id, orderDTO,deliveryfees,servicefees);
                if (result) return Ok();
            }
            result=await _order.PlaceOrder(id,orderDTO);
            if (result) return Ok();
            return BadRequest("failed to place order");
         
        }

        [Authorize(Roles = "Customer")]
        [HttpPost("{id}/line-cart", Name = "AddLineToCart")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult> AddLineToCart(int id,CreateOrderLineDTO lineDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0 || lineDTO == null) return BadRequest("Invalid Data");
            if (!await _order.IsExist(id)) return NotFound("No order found");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                lineDTO.CustomerId,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            int newId = await _order.AddLineToCart(id, lineDTO);
            if (newId != -1)
            {
                return CreatedAtAction(nameof(GetLineFromId), new { id = newId }, new { id = newId });
            }
            return BadRequest("failed to add line");

        }


        [Authorize(Roles = "Customer")]
        [HttpDelete("cart-lines/{lineId}", Name = "DeleteLineCart")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteLineCart(int lineId, [FromServices] IAuthorizationService authorizationService)
        {
            if (lineId < 0) return BadRequest("Invalid Data");
            int? ownerId = await _order.GetCartLineOwnerId(lineId);
            if (ownerId == null) return NotFound(new { message = "line not found" });

            var authResult = await authorizationService.AuthorizeAsync(User, ownerId.Value, "CustomerOwnerOrAdmin");
            if (!authResult.Succeeded) return Forbid();


            if (await _order.DeleteLineFromCart(lineId)) return Ok();
            return BadRequest("failed to delete");
        }

        [HttpGet("lines/{id}", Name = "GetLineFromId")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<OrderLineDTO>> GetLineFromId(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 0) return BadRequest("Invalid data");
            var line=await _order.GetOrderLine(id);

            if (line == null) return NotFound($"No order line with id {id}");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                line.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var authResultCustomer = await authorizationService.AuthorizeAsync(
              User,
              line.CustomerId,
              "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            return Ok(line);
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("send-messages")]
        public async Task<ActionResult> CreateMessages([FromQuery] int count)
        {
            for (int i = 1; i <= count; i++)
            {
                await _sendOrderConfirmation.SendConfirmation(new SendOrderConfirmationDTO
                {
                    SequenceNumber = i
                });
            }
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("place-test-order")]
        public async Task<ActionResult> PlaceTestOrder([FromServices] IConnection connection)
        {
            await using var channel = await connection.CreateChannelAsync();

            var props = new BasicProperties
            {
                MessageId = Guid.NewGuid().ToString(),
                Type = "OrderPlaced",
                Persistent = true,
                Headers = new Dictionary<string, object?>
                {
                    ["version"] = 1
                }
            };

            var testOrder = new OrderPlacedEventDTO
            {
                OrderId = 999999999,
                StoreId = 1,
                CustomerId = 2,
                Total = 54,
                CreatedAtUtc = DateTime.UtcNow
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: "notification-order-placed",
                mandatory: false,
                basicProperties: props,
                body: JsonSerializer.SerializeToUtf8Bytes(testOrder));

            return Ok();
        }

    }
}
