using Al_BoomehDAL.Classes;
using Al_BoomehServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;


namespace Al_Boomeh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IDriversService _driver;

        public DriversController(IDriversService driver)
        {
            _driver = driver;
        }

        [HttpGet("{pagenumber}/{pagesize}", Name = "GetAllDrivers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DriverInfoDTO>>> GetAllDrivers(int pagenumber, int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var driversList = await _driver.GetAllDrivers(pagenumber, pagesize);
            if (driversList == null || driversList.Count == 0) return NotFound("No Drivers Found");
            return Ok(driversList);
        }

        [HttpGet("{id}/driver", Name = "GetDriverById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriverInfoDTO>> GetDriverById(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            var driver = await _driver.GetDriverById(id);
            if (driver == null) return NotFound($"No driver with id {id}");
            return Ok(driver);
        }

        [HttpGet("{phone}", Name = "GetDriverByPhone")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DriverInfoDTO>> GetDriverByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return BadRequest("Invalid Data");

            var driver = await _driver.GetDriverByPhone(phone);
            if (driver == null) return NotFound($"No driver with phone {phone}");
            return Ok(driver);
        }

        [HttpGet("{vehicleType}/{pagenumber}/{pagesize}", Name = "GetDriversByVehicleType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DriverInfoDTO>>> GetDriversByVehicleType(enVehicleType vehicleType, int pagenumber, int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var driversList = await _driver.GetDriversByVehicleType(vehicleType, pagenumber, pagesize);
            if (driversList == null || driversList.Count == 0) return NotFound("No Drivers Found");
            return Ok(driversList);
        }

        [HttpGet( Name = "GetDriverCountPairsVehicleType")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<enVehicleType, int>>> GetDriverCountPairsVehicleType()
        {
            var countDic = await _driver.GetDriverCountPairsVehicleType();
            if (countDic == null) return NotFound("No data found");
            return Ok(countDic);
        }

        [HttpPost(Name = "AddDriver")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddDriver([FromBody] CreateDriverDTO driverDTO)
        {
            if (driverDTO == null) return BadRequest("Invalid Data");

            int newId = await _driver.CreateDriver(driverDTO);
            if (newId == -1) return BadRequest("Failed to create driver, phone may already exist");

            return CreatedAtAction(nameof(GetDriverById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id}", Name = "UpdateDriver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateDriver([FromQuery] int id, [FromBody] UpdateDriverInfoDTO driverDTO)
        {
            if (id <= 0 || driverDTO == null) return BadRequest("Invalid Data");

           
            if (!await _driver.IsExist(id)) return NotFound($"No driver with id {id}");

            if (await _driver.UpdateDriverInfo(id,driverDTO)) return Ok();
            return BadRequest("Failed to update driver");
        }

        [HttpDelete("{id}", Name = "DeleteDriver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteDriver(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _driver.IsExist(id)) return NotFound($"No driver with id {id}");


            if (await _driver.DeleteDriver(id)) return Ok();
            return BadRequest("Failed to delete driver");
        }

        
        [HttpGet("{id}/issues", Name = "GetDriverIssues")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<DriverIssueDTO>>> GetDriverIssues(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _driver.IsExist(id)) return NotFound($"No driver with id {id}");

            var issues = await _driver.GetDriverIssues(id);
            if (issues == null || issues.Count == 0) return NotFound("No issues found");
            return Ok(issues);
        }

        [HttpPost("{id}/issues", Name = "AddIssueToDriver")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> AddIssueToDriver([FromQuery] int id, [FromBody] DriverIssueDTO issueDTO)
        {
            if (id <= 0 || issueDTO == null) return BadRequest("Invalid Data");

            if (!await _driver.IsExist(id)) return NotFound($"No driver with id {id}");


            int newIssueId = await _driver.AddIssueToDriver(issueDTO);
            if (newIssueId == -1) return BadRequest("Failed to add issue");

            return StatusCode(StatusCodes.Status201Created, new { id = newIssueId });
        }

        [HttpDelete("{id}/issues/{issueid}", Name = "DeleteIssueFromDriver")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteIssueFromDriver(int id, int issueid)
        {
            if (id <= 0 || issueid <= 0) return BadRequest("Invalid Data");

            if (!await _driver.IsExist(id)) return NotFound($"No driver with id {id}");


            if (await _driver.DeleteIssueFromDriver(issueid)) return Ok();
            return BadRequest("Failed to delete issue");
        }
    }
}