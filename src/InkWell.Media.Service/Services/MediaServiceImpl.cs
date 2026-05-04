using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using InkWell.Media.Service.DTOs.Requests;
using InkWell.Media.Service.DTOs.Responses;
using InkWell.Media.Service.Models;
using InkWell.Media.Service.Repositories;

namespace InkWell.Media.Service.Services;

public class MediaServiceImpl : IMediaService
{
    private readonly IMediaRepository _repo;
    private readonly Cloudinary _cloudinary;

    private static readonly HashSet<string> AllowedMimeTypes = new()
    {
        "image/jpeg", "image/png", "image/gif", "image/webp", "application/pdf"
    };

    private const long MaxSizeBytes = 10 * 1024 * 1024; // 10MB

    public MediaServiceImpl(
        IMediaRepository repo,
        IConfiguration config)
    {
        _repo = repo;

        var cloudName = config["Cloudinary:CloudName"]
            ?? throw new InvalidOperationException("Cloudinary:CloudName missing.");
        var apiKey = config["Cloudinary:ApiKey"]
            ?? throw new InvalidOperationException("Cloudinary:ApiKey missing.");
        var apiSecret = config["Cloudinary:ApiSecret"]
            ?? throw new InvalidOperationException("Cloudinary:ApiSecret missing.");

        var account = new Account(cloudName, apiKey, apiSecret);
        _cloudinary = new Cloudinary(account);
        _cloudinary.Api.Secure = true;
    }

    public async Task<MediaResponse> UploadAsync(IFormFile file, Guid uploaderId)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("File is empty.");

        if (file.Length > MaxSizeBytes)
            throw new ArgumentException("File size exceeds 10MB limit.");

        if (!AllowedMimeTypes.Contains(file.ContentType))
            throw new ArgumentException($"File type {file.ContentType} is not allowed.");

        var filename = $"{Guid.NewGuid()}";
        string url;
        string mimeType = file.ContentType;

        if (file.ContentType == "application/pdf")
        {
            // Upload PDF as raw file
            var rawParams = new RawUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = $"inkwell/docs/{filename}",
                Overwrite = true
            };
            var rawResult = await _cloudinary.UploadAsync(rawParams);
            if (rawResult.Error != null)
                throw new Exception($"Cloudinary upload failed: {rawResult.Error.Message}");
            url = rawResult.SecureUrl.ToString();
        }
        else
        {
            // Upload image
            var imageParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                PublicId = $"inkwell/media/{filename}",
                Overwrite = true
            };
            var imageResult = await _cloudinary.UploadAsync(imageParams);
            if (imageResult.Error != null)
                throw new Exception($"Cloudinary upload failed: {imageResult.Error.Message}");
            url = imageResult.SecureUrl.ToString();
        }

        var media = new MediaFile
        {
            MediaId = Guid.NewGuid(),
            UploaderId = uploaderId,
            Filename = filename,
            OriginalName = file.FileName,
            Url = url,
            MimeType = mimeType,
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