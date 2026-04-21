using InkWell.Taxonomy.Service.DTOs.Requests;
using InkWell.Taxonomy.Service.DTOs.Responses;
using InkWell.Taxonomy.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace InkWell.Taxonomy.Service.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<CategoryResponse>> GetBySlug(string slug)
    {
        var category = await _service.GetBySlugAsync(slug);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpGet("{categoryId:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid categoryId)
    {
        var category = await _service.GetByIdAsync(categoryId);
        return category == null ? NotFound() : Ok(category);
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { categoryId = created.CategoryId }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{categoryId:guid}")]
    public async Task<ActionResult<CategoryResponse>> Update(Guid categoryId, [FromBody] UpdateCategoryRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(categoryId, request);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpDelete("{categoryId:guid}")]
    public async Task<IActionResult> Delete(Guid categoryId)
    {
        try
        {
            var deleted = await _service.DeleteAsync(categoryId);
            return deleted ? NoContent() : NotFound();
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }
}