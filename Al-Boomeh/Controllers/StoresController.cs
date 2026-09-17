using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;


namespace Al_Boomeh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class StoresController : ControllerBase
    {
        private readonly IStoresService _store;
        private readonly IUsersService _userService;
        public StoresController(IStoresService store, IUsersService usersService)
        {
            _userService = usersService;
            _store = store;
        }

        [AllowAnonymous]
        [HttpGet("All", Name = "GetAllStores")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<List<StoreInfoDTO>>> GetAllStores([FromQuery] int pagenumber,[FromQuery] int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var storesList = await _store.GetAllStores(pagenumber, pagesize);
            if (storesList == null || storesList.Count == 0) return NotFound("No Stores Found");
            return Ok(storesList);
        }

        //owner
        [HttpGet("{storeId}/get-store-orders", Name = "GetStoreOrders")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<List<OrderInfoDTO>?>> GetStoreOrders(int storeId,[FromQuery] int pageNumber,[FromQuery] int pageSize,
             [FromServices] IAuthorizationService authorizationService)
        {
            if (pageNumber <= 0 || pageSize <= 0 || storeId <= 0) return BadRequest("Invalid Data");
            if (!await _store.IsExist(storeId)) return NotFound($"No customer with id {storeId}");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeId,
                "StoreOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var orderList = await _store.GetStoreOrders(storeId, pageNumber, pageSize);

            return Ok(orderList);
        }
        //owner
        [HttpGet("{id}", Name = "GetStoreById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<StoreInfoDTO>> GetStoreById(int id, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            
            var authResult = await authorizationService.AuthorizeAsync(
                User,
                id,
                "StoreOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            var store = await _store.GetStoreById(id);
            if (store == null) return NotFound($"No store with id {id}");

            return Ok(store);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{status}/get-by-status", Name = "GetStoresByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<StoreInfoDTO>>> GetStoresByStatus(StoreStatusEnum status,[FromQuery] int pagenumber,[FromQuery] int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var storesList = await _store.GetStoresByStatus(status, pagenumber, pagesize);
            if (storesList == null || storesList.Count == 0) return NotFound("No stores found");
            return Ok(storesList);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("{area}/{pagenumber}/{pagesize}", Name = "GetStoresByArea")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<StoreInfoDTO>>> GetStoresByArea(string area, int pagenumber, int pagesize)
        {
            if (string.IsNullOrWhiteSpace(area) || pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var storesList = await _store.GetStoresByArea(area, pagenumber, pagesize);
            if (storesList == null || storesList.Count == 0) return NotFound("No stores found");
            return Ok(storesList);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet(Name = "GetStoreCountPairsStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<StoreStatusEnum, int>>> GetStoreCountPairsStatus()
        {
            var countDic = await _store.GetStoreCountPairsStatus();
            if (countDic == null) return NotFound("No data found");
            return Ok(countDic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost(Name = "AddStore")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddStore([FromBody] CreateStoreDTO storeDTO)
        {
            if (storeDTO == null) return BadRequest("Invalid Data");

            int newId = await _store.CreateStore(storeDTO);
            if (newId == -1) return BadRequest("Failed to create store, name may already exist");

            return CreatedAtAction(nameof(GetStoreById), new { id = newId }, new { id = newId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}", Name = "UpdateStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateStore(int id, [FromBody] UpdateStoreDTO storeDTO)
        {
            if (id <= 0 || storeDTO == null) return BadRequest("Invalid Data");

         
            if (!await _store.IsExist(id)) return NotFound($"No store with id {id}");

            if (await _store.UpdateStore(id,storeDTO)) return Ok();
            return BadRequest("Failed to update store");
        }
        //owner
        [HttpPut("{id}/status", Name = "UpdateStoreStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateStoreStatus(int id, [FromBody] UpdateStoreStatusDTO statusDTO
            , [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0 || statusDTO == null) return BadRequest("Invalid Data");

            if (!await _store.IsExist(id)) return NotFound($"No store with id {id}");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                id,
                "StoreOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            if (await _store.UpdateStoreStatus(id, statusDTO)) return Ok();
            return BadRequest("Failed to update store status");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteStore(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _store.IsExist(id)) return NotFound($"No store with id {id}");


            if (await _store.DeleteStore(id)) return Ok();
            return BadRequest("Failed to delete store");
        }

        
        [HttpGet("{id}/issues", Name = "GetStoreIssues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<StoreIssueDTO>>> GetStoreIssues(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _store.IsExist(id)) return NotFound($"No store with id {id}");


            var issues = await _store.GetStoreIssues(id);
            if (issues == null || issues.Count == 0) return NotFound("No issues found");
            return Ok(issues);
        }

        [HttpPost("{id}/issues/{issueId}", Name = "AddIssueToStore")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddIssueToStore(int id, StoreIssueDTO issueDTO)
        {
            if (id <= 0 || issueDTO == null) return BadRequest("Invalid Data");

            if (!await _store.IsExist(id)) return NotFound($"No store with id {id}");


            if (await _store.AddIssueToStore(issueDTO) != -1) return Created();
            return BadRequest("Failed to add issue");
        }

        [HttpDelete("{id}/issues/{storeIssueId}", Name = "DeleteIssueFromStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteIssueFromStore(int id, int storeIssueId)
        {
            if (id <= 0 || storeIssueId <= 0) return BadRequest("Invalid Data");

            if (!await _store.IsExist(id)) return NotFound($"No store with id {id}");


            if (await _store.DeleteIssueFromStore(storeIssueId)) return Ok();
            return BadRequest("Failed to delete issue");
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("partner",Name ="CreatePartner")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Guid>> CreatePartner([FromBody] PartnerDto partner)
        {
            if (!await _store.IsExist(partner.StoreId)) return NotFound($"No store with id {partner.StoreId}");

            var partnerId=await _userService.CreatePartner(partner);

            return Ok(partnerId);
        }
    }
}