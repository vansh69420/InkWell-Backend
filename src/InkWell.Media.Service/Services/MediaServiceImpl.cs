using InkWell.Media.Service.DTOs.Requests;
using InkWell.Media.Service.DTOs.Responses;
using InkWell.Media.Service.Models;
using InkWell.Media.Service.Repositories;
using Microsoft.AspNetCore.Http;

namespace InkWell.Media.Service.Services;

public class MediaServiceImpl : IMediaService
{
    private readonly IMediaRepository _repo;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _config;

    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"
    };

    private const long MaxSizeBytes = 10 * 1024 * 1024; // 10MB

    public MediaServiceImpl(
        IMediaRepository repo,
        IWebHostEnvironment env,
        IConfiguration config)
    {
        _repo = repo;
        _env = env;
        _config = config;
    }

    public async Task<MediaResponse> UploadAsync(IFormFile file, Guid uploaderId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxSizeBytes)
            throw new ArgumentException("File size exceeds 10MB limit.");

        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new ArgumentException($"File type {file.ContentType} is not allowed.");

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(file.FileName);
        var filename = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, filename);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var baseUrl = _config["Media:BaseUrl"] ?? "http://localhost:5036";
        var url = $"{baseUrl}/uploads/{filename}";

        var media = new MediaFile
        {
            MediaId = Guid.NewGuid(),
            UploaderId = uploaderId,
            Filename = filename,
            OriginalName = file.FileName,
            Url = url,
            MimeType = file.ContentType,
            SizeKb = file.Length / 1024,
            UploadedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(media);
        await _repo.SaveChangesAsync();

        return Map(media);
    }

    public async Task<List<MediaResponse>> GetMyMediaAsync(Guid uploaderId)
    {
        var items = await _repo.GetByUploaderIdAsync(uploaderId);
        return items.Select(Map).ToList();
    }

    public async Task<List<MediaResponse>> GetAllMediaAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.Select(Map).ToList();
    }

    public async Task<MediaResponse?> UpdateAltTextAsync(
        Guid mediaId,
        Guid uploaderId,
        bool isAdmin,
        UpdateAltTextRequest request)
    {
        var media = await _repo.GetByMediaIdAsync(mediaId);
        if (media == null) return null;

        if (!isAdmin && media.UploaderId != uploaderId)
            throw new UnauthorizedAccessException("Not allowed.");

        media.AltText = request.AltText;
        await _repo.SaveChangesAsync();
        return Map(media);
    }

    public async Task<bool> DeleteAsync(Guid mediaId, Guid uploaderId, bool isAdmin)
    {
        var media = await _repo.GetByMediaIdAsync(mediaId);
        if (media == null) return false;

        if (!isAdmin && media.UploaderId != uploaderId)
            throw new UnauthorizedAccessException("Not allowed.");

        media.IsDeleted = true;
        await _repo.SaveChangesAsync();
        return true;
    }

    public async Task<MediaResponse?> LinkToPostAsync(
        Guid mediaId,
        Guid uploaderId,
        bool isAdmin,
        LinkPostRequest request)
    {
        var media = await _repo.GetByMediaIdAsync(mediaId);
        if (media == null) return null;

        if (!isAdmin && media.UploaderId != uploaderId)
            throw new UnauthorizedAccessException("Not allowed.");

        media.LinkedPostId = request.LinkedPostId;
        await _repo.SaveChangesAsync();
        return Map(media);
    }

    private MediaResponse Map(MediaFile m) => new()
    {
        MediaId = m.MediaId,
        UploaderId = m.UploaderId,
        Filename = m.Filename,
        OriginalName = m.OriginalName,
        Url = m.Url,
        MimeType = m.MimeType,
        SizeKb = m.SizeKb,
        AltText = m.AltText,
        LinkedPostId = m.LinkedPostId,
        UploadedAt = m.UploadedAt
    };
}