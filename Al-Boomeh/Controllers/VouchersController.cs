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
    public class VouchersController : ControllerBase
    {
        private readonly IVouchersService _voucher;

        public VouchersController(IVouchersService voucher)
        {
            _voucher = voucher;
        }

        [HttpGet("{code}", Name = "GetVoucherByCode")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VoucherDTO>> GetVoucherByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest("Invalid Data");

            var voucher = await _voucher.GetVoucherByCode(code);
            if (voucher == null) return NotFound($"No voucher with code {code}");
            return Ok(voucher);
        }

        [HttpPost(Name = "AddVoucher")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddVoucher([FromBody] VoucherDTO voucherDTO)
        {
            if (voucherDTO == null) return BadRequest("Invalid Data");

            int newId = await _voucher.AddVoucherToCustomer(voucherDTO);
            if (newId == -1) return BadRequest("Failed to create voucher");

            return CreatedAtAction(nameof(GetVoucherByCode), new { code = voucherDTO.Code }, new { id = newId });
        }

        [HttpPut("{code}/use", Name = "UseVoucher")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UseVoucher(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return BadRequest("Invalid Data");

           
            if (!await _voucher.IsExist(code)) return NotFound($"No voucher with code {code}");

            if (await _voucher.UseVoucher(code)) return Ok();
            return BadRequest("Voucher already used or failed to use");
        }
    }
}