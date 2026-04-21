using InkWell.Taxonomy.Service.DTOs.Requests;
using InkWell.Taxonomy.Service.DTOs.Responses;
using InkWell.Taxonomy.Service.Models;
using InkWell.Taxonomy.Service.Repositories;
using InkWell.Taxonomy.Service.Utilities;

namespace InkWell.Taxonomy.Service.Services;

public class TagServiceImpl : ITagService
{
    private readonly ITagRepository _repo;

    public TagServiceImpl(ITagRepository repo)
    {
        _repo = repo;
    }

    public async Task<IReadOnlyList<TagResponse>> GetAllAsync()
    {
        var tags = await _repo.GetAllAsync();
        return tags.Select(Map).ToList();
    }

    public async Task<TagResponse?> GetBySlugAsync(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug)) return null;
        var tag = await _repo.GetBySlugAsync(slug);
        return tag == null ? null : Map(tag);
    }

    public async Task<TagResponse?> GetByIdAsync(Guid tagId)
    {
        var tag = await _repo.GetByIdAsync(tagId);
        return tag == null ? null : Map(tag);
    }

    public async Task<TagResponse> CreateAsync(CreateTagRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? SlugGenerator.Generate(request.Name)
            : SlugGenerator.Generate(request.Slug);

        if (string.IsNullOrWhiteSpace(slug))
            throw new ArgumentException("Slug could not be generated.");

        if (await _repo.SlugExistsAsync(slug))
            throw new InvalidOperationException("Slug already exists.");

        var entity = new Tag
        {
            TagId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Slug = slug,
            PostCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(entity);
        await _repo.SaveChangesAsync();

        return Map(entity);
    }

    public async Task<TagResponse?> UpdateAsync(Guid tagId, UpdateTagRequest request)
    {
        var entity = await _repo.GetByIdAsync(tagId);
        if (entity == null) return null;

        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name is required.");

        entity.Name = request.Name.Trim();

        // Slug stays the same unless explicitly provided
        if (!string.IsNullOrWhiteSpace(request.Slug))
        {
            var newSlug = SlugGenerator.Generate(request.Slug);
            if (string.IsNullOrWhiteSpace(newSlug))
                throw new ArgumentException("Slug is invalid.");

            if (await _repo.SlugExistsAsync(newSlug, ignoreTagId: tagId))
                throw new InvalidOperationException("Slug already exists.");

            entity.Slug = newSlug;
        }

        await _repo.SaveChangesAsync();
        return Map(entity);
    }

    public async Task<bool> DeleteAsync(Guid tagId)
    {
        var entity = await _repo.GetByIdAsync(tagId);
        if (entity == null) return false;

        await _repo.DeleteAsync(entity);
        await _repo.SaveChangesAsync();
        return true;
    }

    private static TagResponse Map(Tag t) => new()
    {
        TagId = t.TagId,
        Name = t.Name,
        Slug = t.Slug,
        PostCount = t.PostCount,
        CreatedAt = t.CreatedAt
    };
}