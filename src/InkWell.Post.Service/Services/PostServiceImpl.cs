using System.Text.Json;
using InkWell.Post.Service.DTOs.Responses;
using InkWell.Post.Service.Enums;
using InkWell.Post.Service.Models;
using InkWell.Post.Service.Repositories;
using Microsoft.AspNetCore.Http;

namespace InkWell.Post.Service.Services
{
    public class PostServiceImpl : IPostService
    {
        private const string ViewedPostsSessionKey = "inkwell.viewed.posts";

        private readonly IPostRepository _postRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostServiceImpl(
            IPostRepository postRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _postRepository = postRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IReadOnlyList<PostSummaryResponse>> GetPublishedPostsAsync()
        {
            var posts = await _postRepository.FindPublishedOrderByPublishedAtDescAsync();
            return posts.Select(MapSummary).ToList();
        }

        public async Task<PostDetailResponse?> GetPostBySlugAsync(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
            {
                return null;
            }

            var post = await _postRepository.FindBySlugAsync(slug);

            if (post == null || post.Status != PostStatus.Published)
            {
                return null;
            }

            var categoryIds = await _postRepository.GetCategoryIdsByPostIdAsync(post.PostId);
            var tagIds = await _postRepository.GetTagIdsByPostIdAsync(post.PostId);

            return MapDetail(post, categoryIds, tagIds);
        }

        public async Task<IReadOnlyList<PostSummaryResponse>> SearchPostsAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return Array.Empty<PostSummaryResponse>();
            }

            var posts = await _postRepository.SearchByTitleAsync(keyword);
            return posts.Select(MapSummary).ToList();
        }

        public async Task<IReadOnlyList<PostSummaryResponse>> GetPostsByCategoryAsync(Guid categoryId)
        {
            var posts = await _postRepository.FindByCategoryIdAsync(categoryId);
            return posts.Select(MapSummary).ToList();
        }

        public async Task<IReadOnlyList<PostSummaryResponse>> GetPostsByTagAsync(Guid tagId)
        {
            var posts = await _postRepository.FindByTagIdAsync(tagId);
            return posts.Select(MapSummary).ToList();
        }

        public async Task<AuthorPostsResponse> GetPostsByAuthorAsync(Guid authorId)
        {
            var posts = await _postRepository.FindPublishedByAuthorIdOrderByCreatedAtDescAsync(authorId);

            return new AuthorPostsResponse
            {
                AuthorId = authorId,
                Posts = posts.Select(MapSummary).ToList()
            };
        }

        public async Task<PostCountResponse> GetPostCountAsync()
        {
            var count = await _postRepository.CountPublishedAsync();
            return new PostCountResponse
            {
                Count = count
            };
        }

        public async Task<bool> IncrementViewCountAsync(Guid postId)
        {
            var post = await _postRepository.FindByPostIdAsync(postId);

            if (post == null || post.Status != PostStatus.Published)
            {
                return false;
            }

            if (HasViewedInThisSession(postId))
            {
                return true;
            }

            await _postRepository.IncrementViewCountAsync(postId);
            MarkViewedInThisSession(postId);

            return true;
        }

        private PostSummaryResponse MapSummary(BlogPost post)
        {
            return new PostSummaryResponse
            {
                PostId = post.PostId,
                AuthorId = post.AuthorId,
                Title = post.Title,
                Slug = post.Slug,
                Excerpt = post.Excerpt ?? string.Empty,
                FeaturedImageUrl = post.FeaturedImageUrl,
                ReadTimeMin = post.ReadTimeMin,
                ViewCount = post.ViewCount,
                LikesCount = post.LikesCount,
                CreatedAt = post.CreatedAt,
                PublishedAt = post.PublishedAt
            };
        }

        private PostDetailResponse MapDetail(
            BlogPost post,
            IReadOnlyList<Guid> categoryIds,
            IReadOnlyList<Guid> tagIds)
        {
            return new PostDetailResponse
            {
                PostId = post.PostId,
                AuthorId = post.AuthorId,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                Excerpt = post.Excerpt ?? string.Empty,
                FeaturedImageUrl = post.FeaturedImageUrl,
                ReadTimeMin = post.ReadTimeMin,
                ViewCount = post.ViewCount,
                LikesCount = post.LikesCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                CategoryIds = categoryIds.ToList(),
                TagIds = tagIds.ToList()
            };
        }

        private bool HasViewedInThisSession(Guid postId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            if (session == null)
            {
                return false;
            }

            try
            {
                var raw = session.GetString(ViewedPostsSessionKey);

                if (string.IsNullOrWhiteSpace(raw))
                {
                    return false;
                }

                var viewedIds = JsonSerializer.Deserialize<HashSet<Guid>>(raw) ?? new HashSet<Guid>();
                return viewedIds.Contains(postId);
            }
            catch
            {
                return false;
            }
        }

        private void MarkViewedInThisSession(Guid postId)
        {
            var session = _httpContextAccessor.HttpContext?.Session;

            if (session == null)
            {
                return;
            }

            HashSet<Guid> viewedIds;

            try
            {
                var raw = session.GetString(ViewedPostsSessionKey);

                if (string.IsNullOrWhiteSpace(raw))
                {
                    viewedIds = new HashSet<Guid>();
                }
                else
                {
                    viewedIds = JsonSerializer.Deserialize<HashSet<Guid>>(raw) ?? new HashSet<Guid>();
                }
            }
            catch
            {
                viewedIds = new HashSet<Guid>();
            }

            viewedIds.Add(postId);
            session.SetString(ViewedPostsSessionKey, JsonSerializer.Serialize(viewedIds));
        }
    }
}