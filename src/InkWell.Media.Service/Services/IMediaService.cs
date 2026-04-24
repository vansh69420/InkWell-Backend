using InkWell.Media.Service.DTOs.Requests;
using InkWell.Media.Service.DTOs.Responses;
using Microsoft.AspNetCore.Http;

namespace InkWell.Media.Service.Services;

public interface IMediaService
{
    Task<MediaResponse> UploadAsync(IFormFile file, Guid uploaderId);
    Task<List<MediaResponse>> GetMyMediaAsync(Guid uploaderId);
    Task<List<MediaResponse>> GetAllMediaAsync();
    Task<MediaResponse?> UpdateAltTextAsync(Guid mediaId, Guid uploaderId, bool isAdmin, UpdateAltTextRequest request);
    Task<bool> DeleteAsync(Guid mediaId, Guid uploaderId, bool isAdmin);
    Task<MediaResponse?> LinkToPostAsync(Guid mediaId, Guid uploaderId, bool isAdmin, LinkPostRequest request);
}