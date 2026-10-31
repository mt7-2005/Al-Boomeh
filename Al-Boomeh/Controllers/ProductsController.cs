using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Al_BoomehServices.Interfaces;


namespace Al_Boomeh.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _product;
        private readonly IUserService _usersService;
        public ProductsController(IProductService product, IUserService usersService)
        {
            _usersService = usersService;
            _product = product;
        }

        

        [AllowAnonymous]
        [HttpGet("{id}", Name = "GetProductById")]
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
        [HttpGet("by-store", Name = "GetProductsByStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ProductInfoDTO>>> GetProductsByStore([FromQuery] int storeId,[FromQuery] int pageNumber,[FromQuery] int pageSize)
        {
            if (pageNumber <= 0 || pageSize <= 0 || pageSize > 50) return BadRequest("Invalid Data");

            var productsList = await _product.GetProductsByStore(storeId, pageNumber, pageSize);
            if (productsList == null || productsList.Count == 0) return NotFound("No products found");
            return Ok(productsList);
        }

        [AllowAnonymous]
        [HttpGet("by-category", Name = "GetProductsByCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ProductInfoDTO>>> GetProductsByCategory( [FromQuery] int categoryId,[FromQuery] int pageNumber,[FromQuery] int pageSize)
        {
            if (categoryId <= 0 || pageNumber <= 0 || pageSize <= 0) return BadRequest("Invalid Data");

            var productsList = await _product.GetProductsByCategory(categoryId, pageNumber, pageSize);
            if (productsList == null || productsList.Count == 0) return NotFound("No products found");
            return Ok(productsList);
        }

        //owner
        [HttpGet("out-of-stock", Name = "GetOutOfStockProducts")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<ProductInfoDTO>>> GetOutOfStockProducts([FromQuery] int storeid, [FromQuery] int pageNumber,[FromQuery] int pageSize
            , [FromServices] IAuthorizationService authorizationService)
        {
            if (pageNumber <= 0 || pageSize <= 0) return BadRequest("Invalid Data");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeid,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();
            var productsList = await _product.GetOutOfStockProducts(pageNumber, pageSize, storeid);


            if (productsList == null || productsList.Count == 0) return NotFound("No out-of-stock products found");
            return Ok(productsList);
        }

        
        //onwer
        [HttpGet("top", Name = "TopProductPairsMonthForStore")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Dictionary<int, float>>> TopProductPairsMonthForStore([FromQuery] int storeId, [FromServices] IAuthorizationService authorizationService)
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

        [HttpPost(Name = "AddProduct")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddProduct([FromBody] CreateProductDTO productDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (productDTO == null||productDTO.ProductPrice<=0 || productDTO.StockQuantity < 0) return BadRequest("Invalid Data");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                productDTO.StoreId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();


            int newId = await _product.CreateProduct(productDTO);
            if (newId == -1) return BadRequest("Failed to create product, name may already exist in this store");

            return CreatedAtAction(nameof(GetProductById), new { id = newId }, new { id = newId });
        }

        [HttpPut("{id}", Name = "UpdateProduct")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateProduct(int id,int storeId, [FromBody] UpdateProductDTO productDTO, [FromServices] IAuthorizationService authorizationService)
        {
            if (id <= 0 || productDTO == null||productDTO.ProductPrice<=0||productDTO.StockQuantity<0) return BadRequest("Invalid Data");

            
            if (!await _product.IsExist(id)) return NotFound($"No product with id {id}");


            var authResult = await authorizationService.AuthorizeAsync(
                User,
                storeId,
                "StoreOwnerOrAdmin");


            if (!authResult.Succeeded)
                return Forbid();


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