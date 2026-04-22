using InkWell.Post.Service.Enums;
using InkWell.Post.Service.Models;

namespace InkWell.Post.Service.Repositories
{
    public interface IPostRepository
    {
        Task<BlogPost?> FindBySlugAsync(string slug);
        Task<BlogPost?> FindByPostIdAsync(Guid postId);

        Task<List<BlogPost>> FindByAuthorIdAsync(Guid authorId);
        Task<List<BlogPost>> FindByAuthorIdOrderByCreatedAtDescAsync(Guid authorId);
        Task<List<BlogPost>> FindPublishedByAuthorIdOrderByCreatedAtDescAsync(Guid authorId);

        Task<List<BlogPost>> FindByStatusAsync(PostStatus status);
        Task<List<BlogPost>> FindPublishedOrderByPublishedAtDescAsync();

        Task<List<BlogPost>> SearchByTitleAsync(string keyword);

        Task<List<BlogPost>> FindByCategoryIdAsync(Guid categoryId);
        Task<List<BlogPost>> FindByTagIdAsync(Guid tagId);

        Task<int> CountByAuthorIdAsync(Guid authorId);
        Task<int> CountPublishedAsync();

        Task<List<Guid>> GetCategoryIdsByPostIdAsync(Guid postId);
        Task<List<Guid>> GetTagIdsByPostIdAsync(Guid postId);

        Task AddAsync(BlogPost post);
        Task SaveChangesAsync();
        Task DeleteByPostIdAsync(Guid postId);
        Task IncrementViewCountAsync(Guid postId);
        Task<BlogPost?> GetTrackedByPostIdAsync(Guid postId);
        Task<bool> SlugExistsAsync(string slug, Guid? ignorePostId = null);

        Task<List<BlogPost>> FindMyPostsAsync(Guid authorId);

        Task ReplaceCategoriesAsync(Guid postId, IReadOnlyCollection<Guid> categoryIds);
        Task ReplaceTagsAsync(Guid postId, IReadOnlyCollection<Guid> tagIds);
    }
}