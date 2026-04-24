using InkWell.Media.Service.DbContexts;
using InkWell.Media.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Media.Service.Repositories;

public class MediaRepository : IMediaRepository
{
    private readonly MediaDbContext _db;

    public MediaRepository(MediaDbContext db)
    {
        _db = db;
    }

    public async Task<List<MediaFile>> GetByUploaderIdAsync(Guid uploaderId)
    {
        return await _db.Media
            .Where(m => m.UploaderId == uploaderId && !m.IsDeleted)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task<MediaFile?> GetByMediaIdAsync(Guid mediaId)
    {
        return await _db.Media
            .FirstOrDefaultAsync(m => m.MediaId == mediaId && !m.IsDeleted);
    }

    public async Task<List<MediaFile>> GetAllAsync()
    {
        return await _db.Media
            .Where(m => !m.IsDeleted)
            .OrderByDescending(m => m.UploadedAt)
            .ToListAsync();
    }

    public async Task AddAsync(MediaFile media)
    {
        await _db.Media.AddAsync(media);
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}