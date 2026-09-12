using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Al_Boomeh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly CustomersService _customersService;
        private readonly UsersService _userService;
        public CustomersController(CustomersService customer,UsersService usersService)
        {
            _userService = usersService;
            _customersService = customer;
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("", Name = "GetAllCustomers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<List<ResponseCustomerDTO>>> GetAllCustomers([FromQuery] int pagenumber,[FromQuery] int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var customersList = await _customersService.GetAllCustomers(pagenumber, pagesize);
            if (customersList == null || customersList.Count == 0) return NotFound("No Customers Found");
            return Ok(customersList);
        }

        //owner
        [HttpGet("{customerId}/get-customers-orders", Name = "GetCustomerOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<List<OrderInfoDTO>>?> GetCustomerOrders(int customerId,[FromQuery] int pageNumber,[FromQuery] int pageSize, [FromServices] IAuthorizationService authorizationService)
        {
            if (pageNumber <= 0 || pageSize <= 0|| customerId<=0) return BadRequest("Invalid Data");
            if (!await _customersService.IsExist(customerId)) return NotFound($"No customer with id {customerId}");



            var authResult = await authorizationService.AuthorizeAsync(
                User,
                customerId,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();


            var orderList = await _customersService.GetCustomerOrders(customerId, pageNumber, pageSize);

            return Ok(orderList);
        }
        //owner
        [HttpGet("{id}", Name = "GetCustomerById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseCustomerDTO>> GetCustomerById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0) return BadRequest("Invalid Data");




            var authResult = await authorizationService.AuthorizeAsync(
                User,
                id,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var customer = await _customersService.GetCustomerById(id);

            if (customer == null) return NotFound($"No customer with id {id}");


            return Ok(customer);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("count-by-status", Name = "GetCustomerCountByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult<Dictionary<enCustomerStatus, int>>> GetCustomerCountByStatus()
        {
            var countDic = await _customersService.GetCustomerCountByStatus();
            if (countDic == null) return NotFound("No data found");
            return Ok(countDic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost(Name = "AddCustomer")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddCustomer([FromBody] CreateCustomerDTO customerDTO)
        {
            if (customerDTO == null) return BadRequest("Invalid Data");

            int  newid = await _customersService.CreateCustomer(customerDTO);
            if(newid==-1)
             return BadRequest("Failed to create customer, phone may already exist");

            return CreatedAtAction(nameof(GetCustomerById), new { id = newid }, new { id = newid });

        }

        [Authorize(Roles = "Customer")]
        [HttpPut("{id}", Name = "UpdateCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCustomer( int id, [FromBody] UpdateCustomerDTO customerDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0 || customerDTO == null) return BadRequest("Invalid Data");

           
            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");

            var authResult = await authorizationService.AuthorizeAsync(
              User,
              id,
              "CustomerOwnerOrAdmin");

            if (await _customersService.UpdateCustomer(id,customerDTO)) return Ok("Updated");
            return BadRequest("Failed to update customer");
        }
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}/block", Name = "BlockCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> BlockCustomer(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            if (await _customersService.BlockCustomer(id)) return Ok("Blocked Successfully");
            return BadRequest("Customer already blocked or failed to block");
        }
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteCustomer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCustomer(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            if (await _customersService.DeleteCustomer(id)) return Ok();
            return BadRequest("Failed to delete customer");
        }

        //owner
        [HttpGet("{id}/addresses", Name = "GetCustomerAddresses")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<AddressDTO>>> GetCustomerAddresses(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");




            var authResult = await authorizationService.AuthorizeAsync(
                User,
                id,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var addresses = await _customersService.GetCustomerAddress(id);
            if (addresses == null || addresses.Count == 0) return NotFound("No addresses found");

            return Ok(addresses);
        }

       
        [HttpGet("{id}/cards", Name = "GetCustomerCards")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CardDTO>>> GetCustomerCards(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            var cards = await _customersService.GetCustomerCards(id);
            if (cards == null || cards.Count == 0) return NotFound("No cards found");
            return Ok(cards);
        }

        [HttpPost("{id}/cards", Name = "AddCard")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddCard( int id, [FromBody] CardDTO cardDTO)
        {
            if (id <= 0 || cardDTO == null) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");

            int newcardid =await _customersService.AddCardToCustomer(id,cardDTO);

            return CreatedAtAction(nameof(AddCard), newcardid);
        }

        [HttpDelete("{id}/cards/{cardid}", Name = "DeleteCard")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCard(int id, int cardid)
        {
            if (id <= 0 || cardid <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            if (await _customersService.DeleteCardFromCustomer(id,cardid)) return Ok();
            return BadRequest("Failed to delete card");
        }

       
        [HttpGet("{id}/vouchers", Name = "GetCustomerVoucher")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<VoucherDTO>>> GetCustomerVoucher(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            var vouchers = await _customersService.GetCustomerVouchers(id);
            if (vouchers == null || vouchers.Count == 0) return NotFound("No vouchers found");
            return Ok(vouchers);
        }

        
        [HttpGet("{id}/issues", Name = "GetCustomerIssue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<IssueDTO>>> GetCustomerIssue(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");

            var issues = await _customersService.GetIssues(id);
            if (issues == null || issues.Count == 0) return NotFound("No issues found");
            return Ok(issues);
        }

        [HttpPost("{id}/issues/{issueid}", Name = "AddIssue")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddIssue(int id, int issueid)
        {
            if (id <= 0 || issueid <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");

            int newissue = await _customersService.AddIssueToCustomer(id,issueid);
            return CreatedAtAction(nameof(AddIssue), newissue);
        }

        [HttpDelete("{id}/issues/{issuecustomerid}", Name = "DeleteIssue")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteIssue(int id, int issuecustomerid)
        {
            if (id <= 0 || issuecustomerid <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            if (await _customersService.DeleteIssueFromCustomer(issuecustomerid)) return Ok();
            return BadRequest("Failed to delete issue");
        }

        
        [HttpGet("{id}/fav-stores", Name = "GetFavStores")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<StoreInfoDTO>>> GetFavStores(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");

            var stores = await _customersService.GetFavStores(id);
            if (stores == null || stores.Count == 0) return NotFound("No favorite stores found");
            return Ok(stores);
        }

        [HttpPost("{id}/fav-stores", Name = "AddFavStore")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddFavStore( int id, [FromBody] FavStoresDTO storeDTO)
        {
            if (id <= 0 || storeDTO == null) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");

            int newfav =await _customersService.AddFavStore(storeDTO);

            return CreatedAtAction(nameof(AddFavStore), newfav);
        }

        [HttpDelete("{id}/fav-stores/{favid}", Name = "DeleteStoreFromFav")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteStoreFromFav(int id, int favid)
        {
            if (id <= 0 || favid <= 0) return BadRequest("Invalid Data");

            if (!await _customersService.IsExist(id)) return NotFound($"No customer with id {id}");


            if (await _customersService.DeleteStoreFromFav(favid)) return Ok();
            return BadRequest("Failed to delete favorite store");
        }
    }
}