using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;

namespace Al_BoomehAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IUsersService _usersService;
        public AddressesController(IAddressService address, IUsersService usersService)
        {
            _usersService=usersService;
            _addressService = address;
        }

        [HttpGet("{id}",Name = "GetAddress")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetAddress(int id, [FromServices] IAuthorizationService authorizationService)
        {
            var address = await _addressService.GetAddress(id);
           
            var authResult = await authorizationService.AuthorizeAsync(
               User,
               address.customerId,
               "CustomerOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            if (address == null)
                return NotFound("Address not found");

            return Ok(address);
        }

        //owner
        [HttpPost(Name = "AddAddress")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        
        public async Task<ActionResult> AddAddress([FromBody] AddressDTO addressDto, [FromServices] IAuthorizationService authorizationService)
        {
            if (addressDto == null||string.IsNullOrEmpty(addressDto.AddressName)||string.IsNullOrEmpty(addressDto.Phone)||string.IsNullOrEmpty(addressDto.Latitude)||string.IsNullOrEmpty(addressDto.Longitude))
                return BadRequest(new { message = "Address data is required" });


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                addressDto.customerId,
                "CustomerOwnerOrAdmin");

            if (!authResult.Succeeded)
                return Forbid();

            int newId = await _addressService.AddAddress(addressDto);

            if (newId <= 0)
                return BadRequest("Failed to add address");

            return CreatedAtAction(nameof(GetAddress), new { id = newId }, new { id = newId });
        }

        //owner
        [HttpPut(Name = "UpdateAddress")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult> UpdateAddress( [FromBody] AddressDTO addressDto,[FromServices] IAuthorizationService authorizationService)
        {
            if (addressDto == null || string.IsNullOrEmpty(addressDto.AddressName) || string.IsNullOrEmpty(addressDto.Phone) || string.IsNullOrEmpty(addressDto.Latitude) || string.IsNullOrEmpty(addressDto.Longitude))
                return BadRequest(new { message = "Address data is required" });

            if (!await _addressService.IsExist(addressDto.id))
                return NotFound(new { message = "Address not found" });



            var authResult = await authorizationService.AuthorizeAsync(
                User,
                addressDto.customerId,
                "CustomerOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            bool updated = await _addressService.UpdateAddress(addressDto);
            if (!updated)
                return BadRequest(new { message = "Failed to update address" });

            return Ok("Updated Successfully");
        }

        //owner
        [HttpDelete("{id}",Name = "DeleteAddress")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]

        public async Task<ActionResult> DeleteAddress(int id,int customerId, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Input ");
            }
            if (!await _addressService.IsExist(id))
                return NotFound(new { message = "Address not found" });



            var authResult = await authorizationService.AuthorizeAsync(
                User,
                customerId,
                "CustomerOwnerOrAdmin");


            bool deleted = await _addressService.DeleteAddress(id);
            if (!deleted)
                return BadRequest(new { message = "Failed to delete address" });

            return Ok("Deleted Successfully");
        }
    }
}