using InkWell.Post.Service.DbContexts;
using InkWell.Post.Service.Enums;
using InkWell.Post.Service.Models;
using Microsoft.EntityFrameworkCore;

namespace InkWell.Post.Service.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly PostDbContext _context;

        public PostRepository(PostDbContext context)
        {
            _context = context;
        }

        public async Task<BlogPost?> FindBySlugAsync(string slug)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public async Task<BlogPost?> FindByPostIdAsync(Guid postId)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<List<BlogPost>> FindByAuthorIdAsync(Guid authorId)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> FindByAuthorIdOrderByCreatedAtDescAsync(Guid authorId)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .Where(p => p.AuthorId == authorId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> FindPublishedByAuthorIdOrderByCreatedAtDescAsync(Guid authorId)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .Where(p => p.AuthorId == authorId && p.Status == PostStatus.Published)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> FindByStatusAsync(PostStatus status)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .Where(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> FindPublishedOrderByPublishedAtDescAsync()
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .Where(p => p.Status == PostStatus.Published)
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> SearchByTitleAsync(string keyword)
        {
            var term = keyword.Trim().ToLower();

            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .Where(p =>
                    p.Status == PostStatus.Published &&
                    (
                        (p.Title ?? string.Empty).ToLower().Contains(term) ||
                        (p.Content ?? string.Empty).ToLower().Contains(term)
                    ))
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> FindByCategoryIdAsync(Guid categoryId)
        {
            var query =
                from postCategory in _context.Set<PostCategory>().AsNoTracking()
                join post in _context.Set<BlogPost>().AsNoTracking()
                    on postCategory.PostId equals post.PostId
                where postCategory.CategoryId == categoryId &&
                      post.Status == PostStatus.Published
                select post;

            return await query
                .Distinct()
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<BlogPost>> FindByTagIdAsync(Guid tagId)
        {
            var query =
                from postTag in _context.Set<PostTag>().AsNoTracking()
                join post in _context.Set<BlogPost>().AsNoTracking()
                    on postTag.PostId equals post.PostId
                where postTag.TagId == tagId &&
                      post.Status == PostStatus.Published
                select post;

            return await query
                .Distinct()
                .OrderByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountByAuthorIdAsync(Guid authorId)
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .CountAsync(p => p.AuthorId == authorId);
        }

        public async Task<int> CountPublishedAsync()
        {
            return await _context.Set<BlogPost>()
                .AsNoTracking()
                .CountAsync(p => p.Status == PostStatus.Published);
        }

        public async Task<List<Guid>> GetCategoryIdsByPostIdAsync(Guid postId)
        {
            return await _context.Set<PostCategory>()
                .AsNoTracking()
                .Where(pc => pc.PostId == postId)
                .Select(pc => pc.CategoryId)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<Guid>> GetTagIdsByPostIdAsync(Guid postId)
        {
            return await _context.Set<PostTag>()
                .AsNoTracking()
                .Where(pt => pt.PostId == postId)
                .Select(pt => pt.TagId)
                .Distinct()
                .ToListAsync();
        }

        public async Task AddAsync(BlogPost post)
        {
            await _context.Set<BlogPost>().AddAsync(post);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task DeleteByPostIdAsync(Guid postId)
        {
            var postCategories = await _context.Set<PostCategory>()
                .Where(x => x.PostId == postId)
                .ToListAsync();

            var postTags = await _context.Set<PostTag>()
                .Where(x => x.PostId == postId)
                .ToListAsync();

            _context.Set<PostCategory>().RemoveRange(postCategories);
            _context.Set<PostTag>().RemoveRange(postTags);

            var post = await _context.Set<BlogPost>()
                .FirstOrDefaultAsync(p => p.PostId == postId);

            if (post != null)
            {
                _context.Set<BlogPost>().Remove(post);
            }

            await _context.SaveChangesAsync();
        }

        public async Task IncrementViewCountAsync(Guid postId)
        {
            await _context.Set<BlogPost>()
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.ViewCount, p => p.ViewCount + 1)
                    .SetProperty(p => p.UpdatedAt, DateTime.UtcNow));
        }
    }
}