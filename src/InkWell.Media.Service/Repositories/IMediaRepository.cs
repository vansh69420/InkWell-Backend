using InkWell.Media.Service.Models;

namespace InkWell.Media.Service.Repositories;

public interface IMediaRepository
{
    Task<List<MediaFile>> GetByUploaderIdAsync(Guid uploaderId);
    Task<MediaFile?> GetByMediaIdAsync(Guid mediaId);
    Task<List<MediaFile>> GetAllAsync();
    Task AddAsync(MediaFile media);
    Task SaveChangesAsync();
}