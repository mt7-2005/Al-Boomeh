using Al_BoomehDAL.Classes;
using Al_BoomehServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Al_Boomeh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExtrasController : ControllerBase
    {
        private readonly ExtrasService _extra;

        public ExtrasController(ExtrasService extra)
        {
            _extra = extra;
        }

        [HttpGet("{pagenumber}/{pagesize}", Name = "GetAllExtras")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ExtraInfoDTO>>> GetAllExtras(int pagenumber, int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var extrasList = await _extra.GetAllExtras(pagenumber, pagesize);
            if (extrasList == null || extrasList.Count == 0) return NotFound("No Extras Found");
            return Ok(extrasList);
        }

        [HttpGet("{id}", Name = "GetExtraById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ExtraInfoDTO>> GetExtraById(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            var extra = await _extra.GetExtraById(id);
            if (extra == null) return NotFound($"No extra with id {id}");
            return Ok(extra);
        }

        [HttpGet("product/{productId}", Name = "GetExtrasByProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ExtraInfoDTO>>> GetExtrasByProduct(int productId)
        {
            if (productId <= 0) return BadRequest("Invalid Data");

            var extrasList = await _extra.GetExtrasByProduct(productId);
            if (extrasList == null || extrasList.Count == 0) return NotFound("No extras found");
            return Ok(extrasList);
        }

        [HttpGet("{productId}/mandatory", Name = "GetMandatoryExtras")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ExtraInfoDTO>>> GetMandatoryExtras(int productId)
        {
            if (productId <= 0) return BadRequest("Invalid Data");

            var extrasList = await _extra.GetMandatoryExtras(productId);
            if (extrasList == null || extrasList.Count == 0) return NotFound("No mandatory extras found");
            return Ok(extrasList);
        }

        [HttpGet(Name = "GetExtraCountPairsProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<Dictionary<int, int>>> GetExtraCountPairsProduct()
        {
            var countDic = await _extra.GetExtraCountPairsProduct();
            if (countDic == null) return NotFound("No data found");
            return Ok(countDic);
        }

        [HttpPost(Name = "AddExtra")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddExtra([FromBody] CreateExtraDTO extraDTO)
        {
            if (extraDTO == null||extraDTO.Price<0) return BadRequest("Invalid Data");

            int newId = await _extra.CreateExtra(extraDTO);
            if (newId == -1) return BadRequest("Failed to create extra, name may already exist for this product");

            return CreatedAtAction(nameof(GetExtraById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id}", Name = "UpdateExtra")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateExtra([FromQuery] int id, [FromBody] UpdateExtraDTO extraDTO)
        {
            if (id <= 0 || extraDTO == null || extraDTO.Price < 0) return BadRequest("Invalid Data");

            
            if (!await _extra.IsExist(id)) return NotFound($"No extra with id {id}");

            if (await _extra.UpdateExtra(id,extraDTO)) return Ok();
            return BadRequest("Failed to update extra");
        }

        [HttpDelete("{id}", Name = "DeleteExtra")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteExtra(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _extra.IsExist(id)) return NotFound($"No extra with id {id}");


            if (await _extra.DeleteExtra(id)) return Ok();
            return BadRequest("Failed to delete extra");
        }
    }
}