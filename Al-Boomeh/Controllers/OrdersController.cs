using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Services;
using Bogus.DataSets;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Al_BoomehServices.Interfaces;

namespace Al_Boomeh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _order;
        private readonly IConfiguration _configuration;
        private readonly IUsersService _usersService;
        public OrdersController(IOrderService order,IConfiguration configuration, IUsersService usersService)
        {
            _usersService= usersService;
            _configuration = configuration;
            _order = order;
            
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("All",Name ="GetAllOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderInfoDTO>>> GetAllOrders([FromQuery] int pagenumber,[FromQuery] int pagesize)
        {
           var orderlist=await _order.GetAllOrders(pagenumber,pagesize);
            if (orderlist == null) return NotFound("No Orders Found");
            return Ok(orderlist);
        }

        [HttpGet("{id}/order",Name ="GetOrderById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<OrderInfoDTO?>> GetOrderById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if(id<0||!int.TryParse(id.ToString(), out int number))
            {
                return BadRequest("Invalid Data");
            }
            var order=await _order.GetOrderById(id);

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                order.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (order == null) return NotFound($"No order with id {id}");
            return Ok(order);
        }


        [HttpGet("{code}", Name = "GetOrderByCode")]
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


        [HttpGet("{status}/get-count-by-status",Name ="GetOrdersCountByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<OrderInfoDTO>?>> GetOrdersCountByStatus(int storeId,enStatus status, [FromServices] IAuthorizationService authorizationService)
        {

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var orderlist =await _order.GetOrdersByStatus(status,storeId);
            if (orderlist ==0) return NotFound("No data found");
            return Ok(orderlist);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet(Name = "GetOrderCount")]
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

            int newId = await _order.CreateCart(idempotencyKey,orderDTO);

            if (newId == -1) return BadRequest("failed to create order");
            return CreatedAtAction(nameof(GetOrderById), new { id = newId }, new { id = newId });

        }

        [HttpPut("{id}/updatetime",Name ="UpdateOrderTime")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateOrderTime([FromBody] UpdateOrderTimeDTO orderTimeDTO,int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 0 || orderTimeDTO.EstimatedDeliveryTime <= 0 || orderTimeDTO.EstimatedDeliveryTime <= 0) return BadRequest( "Invalid Data");

            var order = await _order.GetOrderById(id);

            var authResult = await authorizationService.AuthorizeAsync(
                User,
                order.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (order == null) return NotFound($"No order with id {id}");

            if (await _order.UpdateOrderTime(id,orderTimeDTO)) return Ok();
            else return BadRequest("Failed To Update Order");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}",Name ="DeleteOrder")]
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

        [HttpGet("{code}/getlines", Name ="GetLinesByOrderId")]
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
        [HttpPut("{id}/updatestatus",Name ="UpdateOrderStatus")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateOrderStatus(int id ,[FromBody] UpdateOrderStatusDTO statusDTO, [FromServices] IAuthorizationService authorizationService)
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
        [HttpPost("{id}/line",Name ="AddLineToOrder")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddLineToOrder([FromBody] CreateOrderLineDTO lineDTO,int id)
        {
            if (id <= 0) return BadRequest("invalid data");
            if (lineDTO == null) return BadRequest("invalid data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            var newlineId = await _order.AddOrderLine(id,lineDTO);
            return Created();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{orderid}/{lineid}",Name ="DeleteLine")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteLineFromOrder(int orderid,int lineid)
        {
            if(orderid <= 0 || lineid <= 0) { return BadRequest("Invalid data"); }
            if (!await _order.IsExist(orderid)) return NotFound($"No Order with id {orderid}");

            if (await _order.DeleteOrderLine(lineid)) return Ok("Deleted Successfully");
            return BadRequest("failed to delete");
        }

        [HttpPost("{id}/cratefeedback",Name ="CreateFeedback")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> CreateFeedback(int id, [FromBody] OrderFeedbackDTO feedbackDTO)
        {
            if (id <= 0) return BadRequest("Invalid data");
            if (feedbackDTO == null) return BadRequest("Invalid data");
            if (!await _order.IsExist(id)) return NotFound($"No Order with id {id}");

            if (await _order.CreateOrderFeedback(feedbackDTO)!=-1) return Ok();
            return BadRequest("failed to add");
        }
        [HttpGet("{id}/feedback",Name ="GetOrderFeedback")]
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
        [HttpGet("{id}/orderhistorystatus", Name = "GetOrderHistoryStatus")]
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
        [HttpPut(Name = "PlaceOrder")]
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
                return CreatedAtAction(nameof(GetLineFromId),newId);
            }
            return BadRequest("failed to add line");

        }


        [Authorize(Roles = "Customer")]
        [HttpDelete("{lineid}/{customerId}/delete-cart", Name = "DeleteLineCart")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> DeleteLineCart(int id,int customerId, [FromServices] IAuthorizationService authorizationService)
        {
            if (id < 0) return BadRequest("Invalid Data");
            if (!await _order.IsOrderLineExist(id)) return NotFound($"No order line with id {id}");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                customerId,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            if (await _order.DeleteLineFromCart(id)) return Ok();
            return BadRequest("failed to delete");
        }

        [HttpGet("{id}/getline", Name = "GetLineFromId")]
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
    }
}
