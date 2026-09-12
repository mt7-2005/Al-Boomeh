using Al_BoomehDAL.Classes;
using Al_BoomehServices;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Al_Boomeh.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly CategoriesService _category;

        public CategoriesController(CategoriesService category)
        {
            _category = category;
        }

        [HttpGet("{pagenumber}/{pagesize}", Name = "GetAllCategories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<List<CategoryInfoDTO>>> GetAllCategories(int pagenumber, int pagesize)
        {
            if (pagenumber <= 0 || pagesize <= 0) return BadRequest("Invalid Data");

            var categoriesList = await _category.GetAllCategories(pagenumber, pagesize);
            if (categoriesList == null || categoriesList.Count == 0) return NotFound("No Categories Found");
            return Ok(categoriesList);
        }

        [HttpGet("{id}", Name = "GetCategoryById")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CategoryInfoDTO>> GetCategoryById(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            var category = await _category.GetCategoryById(id);
            if (category == null) return NotFound($"No category with id {id}");

            return Ok(category);
            
        }

        [HttpPost(Name = "AddCategory")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> AddCategory([FromBody] CreateCategoryDTO categoryDTO)
        {
            if (categoryDTO == null || string.IsNullOrWhiteSpace(categoryDTO.CategoryName))
                return BadRequest("Invalid Data");

            int newid = await _category.CreateCategory(categoryDTO);

            return CreatedAtAction(nameof(GetCategoryById), new { id = categoryDTO }, categoryDTO);
        }

        [HttpPut("{id}", Name = "UpdateCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> UpdateCategory( int id, [FromBody] UpdateCategoryDTO categoryDTO)
        {
            if (id <= 0 || categoryDTO == null) return BadRequest("Invalid Data");

            
            if (!await _category.IsExist(id)) return NotFound($"No category with id {id}");

            if (await _category.UpdateCategory(id,categoryDTO)) return Ok("Updated");
            return BadRequest("Failed to update category");
        }

        [HttpDelete("{id}", Name = "DeleteCategory")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCategory(int id)
        {
            if (id <= 0) return BadRequest("Invalid Data");

            if (!await _category.IsExist(id)) return NotFound($"No category with id {id}");


            if (await _category.DeleteCategory(id)) return Ok();
            return BadRequest("Failed to delete category");
        }
    }
}