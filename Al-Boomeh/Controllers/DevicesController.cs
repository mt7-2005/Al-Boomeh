using Al_BoomehServices.Dtos;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
namespace Al_Boomeh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class DevicesController : ControllerBase
    {
        private readonly IDevicesService _devicesService;
        public DevicesController(IDevicesService devicesService)
        {
            _devicesService= devicesService;
        }
       

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Register([FromBody] DevicesDto devicesDto)
        {
            if(string.IsNullOrEmpty(devicesDto.InstallationId))
                return BadRequest(new { message = "Device data is required" });

            await _devicesService.Register(devicesDto);
            return Ok();
        }

        [HttpPost("unregister")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Unregister([FromBody] string installationId)
        {
            if (string.IsNullOrEmpty(installationId))
                return BadRequest(new { message = "Device data is required" });

            await _devicesService.Unregister(installationId);
            return Ok();
        }
    }
}
