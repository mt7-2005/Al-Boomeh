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
    public class ProductsController : ControllerBase
    {
        private readonly ProductsService _product;
        private readonly UsersService _usersService;
        public ProductsController(ProductsService product,UsersService usersService)
        {
            _usersService = usersService;
            _product = product;
        }

        

        [AllowAnonymous]
        [HttpGet("{id}/product", Name = "GetProductById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ProductInfoDTO>> GetProductById(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            var product = await _product.GetProductById(id);
            if (product == null) return NotFound($"No product with id {id}");
            return Ok(product);
        }

        [AllowAnonymous]
        [HttpGet("store/{storeId}", Name = "GetProductsByStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ProductInfoDTO>>> GetProductsByStore(int storeId,[FromQuery] int pagenumber,[FromQuery] int pagesize)
        {
            if (storeId <= 0 || pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var productsList = await _product.GetProductsByStore(storeId, pagenumber, pagesize);
            if (productsList == null || productsList.Count == 0) return NotFound("No products found");
            return Ok(productsList);
        }

        [AllowAnonymous]
        [HttpGet("{categoryId}", Name = "GetProductsByCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ProductInfoDTO>>> GetProductsByCategory(int categoryId,[FromQuery] int pagenumber,[FromQuery] int pagesize)
        {
            if (categoryId <= 0 || pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var productsList = await _product.GetProductsByCategory(categoryId, pagenumber, pagesize);
            if (productsList == null || productsList.Count == 0) return NotFound("No products found");
            return Ok(productsList);
        }

        //owner
        [HttpGet("out-of-stock/{storeid}", Name = "GetOutOfStockProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ProductInfoDTO>>> GetOutOfStockProducts( [FromQuery] int pagenumber,[FromQuery] int pagesize,int storeid
            , [FromServices] IAuthorizationService authorizationService)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeid,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();
            var productsList = await _product.GetOutOfStockProducts(pagenumber, pagesize, storeid);


            if (productsList == null || productsList.Count == 0) return NotFound("No out-of-stock products found");
            return Ok(productsList);
        }

        
        //onwer
        [HttpGet("{storeId}/top-product", Name = "TopProductPairsMonthForStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Dictionary<int, float>>> TopProductPairsMonthForStore(int storeId, [FromServices] IAuthorizationService authorizationService)
        {
            if (storeId <= 0) return BadRequest("Invalid Data");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();

            var topDic = await _product.TopProductPairsMonthForStore(storeId);
            if (topDic == null) return NotFound("No data found");

            return Ok(topDic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost(Name = "AddProduct")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddProduct([FromBody] CreateProductDTO productDTO)
        {
            if (productDTO == null||productDTO.ProductPrice<=0 || productDTO.StockQuantity < 0) return BadRequest("Invalid Data");

            int newId = await _product.CreateProduct(productDTO);
            if (newId == -1) return BadRequest("Failed to create product, name may already exist in this store");

            return CreatedAtAction(nameof(GetProductById), new { id = newId }, new { id = newId });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProduct(int id, [FromBody] UpdateProductDTO productDTO)
        {
            if (id <= 0 || productDTO == null||productDTO.ProductPrice<=0||productDTO.StockQuantity<0) return BadRequest("Invalid Data");

            
            if (!await _product.IsExist(id)) return NotFound($"No product with id {id}");

            if (await _product.UpdateProduct(id,productDTO)) return Ok();
            return BadRequest("Failed to update product");
        }

        //owner
        [HttpPut("{id}/stock", Name = "UpdateProductStock")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProductStock(int id, [FromBody] UpdateProductStockDTO stockDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0 || stockDTO == null) return BadRequest("Invalid Data");
            if (!await _product.IsExist(id)) return NotFound($"No product with id {id}");



            var authResult = await authorizationService.AuthorizeAsync(
                User,
                stockDTO.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();


            if (await _product.UpdateProductStock(id,stockDTO)) return Ok();
            return BadRequest("Failed to update stock");
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}", Name = "DeleteProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteProduct(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _product.IsExist(id)) return NotFound($"No product with id {id}");


            if (await _product.DeleteProduct(id)) return Ok();
            return BadRequest("Failed to delete product");
        }
    }
}