using InkWell.Taxonomy.Service.DTOs.Requests;
using InkWell.Taxonomy.Service.DTOs.Responses;
using InkWell.Taxonomy.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace InkWell.Taxonomy.Service.Controllers;

[ApiController]
[Route("api/tags")]
public class TagsController : ControllerBase
{
    private readonly ITagService _service;

    public TagsController(ITagService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TagResponse>>> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("slug/{slug}")]
    public async Task<ActionResult<TagResponse>> GetBySlug(string slug)
    {
        var tag = await _service.GetBySlugAsync(slug);
        return tag == null ? NotFound() : Ok(tag);
    }

    [HttpGet("{tagId:guid}")]
    public async Task<ActionResult<TagResponse>> GetById(Guid tagId)
    {
        var tag = await _service.GetByIdAsync(tagId);
        return tag == null ? NotFound() : Ok(tag);
    }

    [HttpPost]
    public async Task<ActionResult<TagResponse>> Create([FromBody] CreateTagRequest request)
    {
        try
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { tagId = created.TagId }, created);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpPut("{tagId:guid}")]
    public async Task<ActionResult<TagResponse>> Update(Guid tagId, [FromBody] UpdateTagRequest request)
    {
        try
        {
            var updated = await _service.UpdateAsync(tagId, request);
            return updated == null ? NotFound() : Ok(updated);
        }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [HttpDelete("{tagId:guid}")]
    public async Task<IActionResult> Delete(Guid tagId)
    {
        var deleted = await _service.DeleteAsync(tagId);
        return deleted ? NoContent() : NotFound();
    }
}