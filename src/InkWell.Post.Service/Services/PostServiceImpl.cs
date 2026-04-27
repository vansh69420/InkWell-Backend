using System.Text.Json;
using InkWell.Post.Service.DTOs.Responses;
using InkWell.Post.Service.Enums;
using InkWell.Post.Service.Models;
using InkWell.Post.Service.Repositories;
using Microsoft.AspNetCore.Http;
using InkWell.Post.Service.DTOs.Requests;
using InkWell.Post.Service.External;
using InkWell.Post.Service.Utilities;

namespace InkWell.Post.Service.Services
{
    public class PostServiceImpl : IPostService
    {
        private const string ViewedPostsSessionKey = "inkwell.viewed.posts";

        private readonly IPostRepository _postRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITaxonomyClient _taxonomyClient;

        public PostServiceImpl(
            IPostRepository postRepository,
            IHttpContextAccessor httpContextAccessor,
            ITaxonomyClient taxonomyClient)
        {
            _postRepository = postRepository;
            _httpContextAccessor = httpContextAccessor;
            _taxonomyClient = taxonomyClient;
        }
        public async Task<IReadOnlyList<PostSummaryResponse>> GetPublishedPostsAsync()
        {
            var posts = await _postRepository.FindPublishedOrderByPublishedAtDescAsync();
            return posts
                .OrderByDescending(p => p.IsFeatured)
                .ThenByDescending(p => p.PublishedAt ?? p.CreatedAt)
                .Select(MapSummary)
                .ToList();
        }

        public async Task<PostDetailResponse?> GetPostBySlugAsync(string slug, Guid? currentUserId = null)
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

            return MapDetail(post, categoryIds, tagIds, currentUserId);
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
                PublishedAt = post.PublishedAt,
                IsFeatured = post.IsFeatured,
            };
        }

        private PostDetailResponse MapDetail(
            BlogPost post,
            IReadOnlyList<Guid> categoryIds,
            IReadOnlyList<Guid> tagIds,
            Guid? currentUserId = null)
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
                TagIds = tagIds.ToList(),
                IsFeatured = post.IsFeatured,
                IsLikedByCurrentUser = currentUserId.HasValue &&
                    _postRepository.GetLikeAsync(post.PostId, currentUserId.Value).Result != null
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

        public async Task<IReadOnlyList<MyPostSummaryResponse>> GetMyPostsAsync(Guid currentUserId)
        {
            var posts = await _postRepository.FindMyPostsAsync(currentUserId);
            return posts.Select(p => new MyPostSummaryResponse
            {
                PostId = p.PostId,
                AuthorId = p.AuthorId,
                Title = p.Title,
                Slug = p.Slug,
                Status = p.Status,
                ReadTimeMin = p.ReadTimeMin,
                ViewCount = p.ViewCount,
                LikesCount = p.LikesCount,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                PublishedAt = p.PublishedAt
            }).ToList();
        }

        public async Task<PostEditorResponse?> GetPostForEditAsync(Guid currentUserId, bool isAdmin, Guid postId)
        {
            var post = await _postRepository.FindByPostIdAsync(postId);
            if (post == null) return null;

            if (!isAdmin && post.AuthorId != currentUserId)
                throw new UnauthorizedAccessException("Not allowed.");

            var categoryIds = await _postRepository.GetCategoryIdsByPostIdAsync(post.PostId);
            var tagIds = await _postRepository.GetTagIdsByPostIdAsync(post.PostId);

            return MapEditor(post, categoryIds, tagIds);
        }

        public async Task<PostEditorResponse> CreatePostAsync(Guid currentUserId, CreatePostRequest request)
        {
            ValidateWriteInput(request.Title, request.Content);

            await ValidateTaxonomyAsync(request.CategoryIds, request.TagIds);

            var baseSlug = SlugGenerator.Generate(request.Title);
            var slug = await ResolveUniqueSlugAsync(baseSlug, ignorePostId: null);

            var readTime = ReadTimeCalculator.ComputeMinutesFromHtml(request.Content);
            var excerpt = string.IsNullOrWhiteSpace(request.Excerpt)
                ? ReadTimeCalculator.BuildExcerptFromHtml(request.Content)
                : request.Excerpt.Trim();

            var now = DateTime.UtcNow;

            var post = new BlogPost
            {
                PostId = Guid.NewGuid(),
                AuthorId = currentUserId,
                Title = request.Title.Trim(),
                Slug = slug,
                Content = request.Content,
                Excerpt = excerpt,
                FeaturedImageUrl = request.FeaturedImageUrl,
                Status = PostStatus.Draft,
                ReadTimeMin = readTime,
                ViewCount = 0,
                LikesCount = 0,
                CreatedAt = now,
                UpdatedAt = now,
                PublishedAt = null
            };

            await _postRepository.AddAsync(post);

            await _postRepository.ReplaceCategoriesAsync(post.PostId, request.CategoryIds ?? new());
            await _postRepository.ReplaceTagsAsync(post.PostId, request.TagIds ?? new());

            await _postRepository.SaveChangesAsync();

            var categoryIds = await _postRepository.GetCategoryIdsByPostIdAsync(post.PostId);
            var tagIds = await _postRepository.GetTagIdsByPostIdAsync(post.PostId);

            return MapEditor(post, categoryIds, tagIds);
        }

        public async Task<PostEditorResponse?> UpdatePostAsync(Guid currentUserId, bool isAdmin, Guid postId, UpdatePostRequest request)
        {
            ValidateWriteInput(request.Title, request.Content);

            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return null;

            if (!isAdmin && post.AuthorId != currentUserId)
                throw new UnauthorizedAccessException("Not allowed.");

            await ValidateTaxonomyAsync(request.CategoryIds, request.TagIds);

            // auto-regenerate slug on title change (your decision)
            var baseSlug = SlugGenerator.Generate(request.Title);
            var slug = await ResolveUniqueSlugAsync(baseSlug, ignorePostId: postId);

            post.Title = request.Title.Trim();
            post.Slug = slug;
            post.Content = request.Content;

            post.ReadTimeMin = ReadTimeCalculator.ComputeMinutesFromHtml(request.Content);

            post.Excerpt = string.IsNullOrWhiteSpace(request.Excerpt)
                ? ReadTimeCalculator.BuildExcerptFromHtml(request.Content)
                : request.Excerpt.Trim();

            post.FeaturedImageUrl = request.FeaturedImageUrl;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.ReplaceCategoriesAsync(post.PostId, request.CategoryIds ?? new());
            await _postRepository.ReplaceTagsAsync(post.PostId, request.TagIds ?? new());

            await _postRepository.SaveChangesAsync();

            var categoryIds = await _postRepository.GetCategoryIdsByPostIdAsync(post.PostId);
            var tagIds = await _postRepository.GetTagIdsByPostIdAsync(post.PostId);

            return MapEditor(post, categoryIds, tagIds);
        }

        public async Task<PostEditorResponse?> PublishPostAsync(Guid currentUserId, bool isAdmin, Guid postId)
        {
            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return null;

            if (!isAdmin && post.AuthorId != currentUserId)
                throw new UnauthorizedAccessException("Not allowed.");

            post.Status = PostStatus.Published;
            post.PublishedAt = DateTime.UtcNow;
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.SaveChangesAsync();

            var categoryIds = await _postRepository.GetCategoryIdsByPostIdAsync(post.PostId);
            var tagIds = await _postRepository.GetTagIdsByPostIdAsync(post.PostId);

            return MapEditor(post, categoryIds, tagIds);
        }

        public async Task<PostEditorResponse?> UnpublishPostAsync(Guid currentUserId, bool isAdmin, Guid postId)
        {
            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return null;

            if (!isAdmin && post.AuthorId != currentUserId)
                throw new UnauthorizedAccessException("Not allowed.");

            post.Status = PostStatus.Unpublished;
            post.PublishedAt = null; // your decision
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.SaveChangesAsync();

            var categoryIds = await _postRepository.GetCategoryIdsByPostIdAsync(post.PostId);
            var tagIds = await _postRepository.GetTagIdsByPostIdAsync(post.PostId);

            return MapEditor(post, categoryIds, tagIds);
        }

        public async Task<bool> DeletePostAsync(Guid currentUserId, bool isAdmin, Guid postId)
        {
            var post = await _postRepository.FindByPostIdAsync(postId);
            if (post == null) return false;

            if (!isAdmin && post.AuthorId != currentUserId)
                throw new UnauthorizedAccessException("Not allowed.");

            await _postRepository.DeleteByPostIdAsync(postId);
            return true;
        }

        private static void ValidateWriteInput(string title, string content)
        {
            if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title is required.");
            if (string.IsNullOrWhiteSpace(content)) throw new ArgumentException("Content is required.");
        }

        private async Task<string> ResolveUniqueSlugAsync(string baseSlug, Guid? ignorePostId)
        {
            if (string.IsNullOrWhiteSpace(baseSlug))
                throw new ArgumentException("Slug could not be generated from title.");

            var slug = baseSlug;
            var i = 2;

            while (await _postRepository.SlugExistsAsync(slug, ignorePostId))
            {
                slug = $"{baseSlug}-{i}";
                i++;
            }

            return slug;
        }

        private async Task ValidateTaxonomyAsync(List<Guid> categoryIds, List<Guid> tagIds)
        {
            categoryIds ??= new();
            tagIds ??= new();

            // validate categories
            var catTasks = categoryIds.Distinct().Select(async id => new { Id = id, Ok = await _taxonomyClient.CategoryExistsAsync(id) });
            var cats = await Task.WhenAll(catTasks);
            var invalidCats = cats.Where(x => !x.Ok).Select(x => x.Id).ToList();

            // validate tags
            var tagTasks = tagIds.Distinct().Select(async id => new { Id = id, Ok = await _taxonomyClient.TagExistsAsync(id) });
            var tags = await Task.WhenAll(tagTasks);
            var invalidTags = tags.Where(x => !x.Ok).Select(x => x.Id).ToList();

            if (invalidCats.Count > 0 || invalidTags.Count > 0)
            {
                throw new ArgumentException($"Invalid taxonomy ids. Categories: [{string.Join(", ", invalidCats)}], Tags: [{string.Join(", ", invalidTags)}]");
            }
        }

        private PostEditorResponse MapEditor(BlogPost post, IReadOnlyList<Guid> categoryIds, IReadOnlyList<Guid> tagIds)
        {
            return new PostEditorResponse
            {
                PostId = post.PostId,
                AuthorId = post.AuthorId,
                Title = post.Title,
                Slug = post.Slug,
                Content = post.Content,
                Excerpt = post.Excerpt ?? string.Empty,
                FeaturedImageUrl = post.FeaturedImageUrl,
                Status = post.Status,
                ReadTimeMin = post.ReadTimeMin,
                ViewCount = post.ViewCount,
                LikesCount = post.LikesCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                PublishedAt = post.PublishedAt,
                CategoryIds = categoryIds.ToList(),
                TagIds = tagIds.ToList(),
                IsFeatured = post.IsFeatured
            };
        }

        public async Task<bool> LikePostAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return false;

            var existing = await _postRepository.GetLikeAsync(postId, userId);
            if (existing != null) return false;

            var like = new PostLike
            {
                PostLikeId = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _postRepository.AddLikeAsync(like);
            post.LikesCount++;
            await _postRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnlikePostAsync(Guid postId, Guid userId)
        {
            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return false;

            var like = await _postRepository.GetLikeAsync(postId, userId);
            if (like == null) return false;

            await _postRepository.RemoveLikeAsync(like);
            post.LikesCount = Math.Max(0, post.LikesCount - 1);
            await _postRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> FeaturePostAsync(Guid postId)
        {
            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return false;

            post.IsFeatured = true;
            post.UpdatedAt = DateTime.UtcNow;
            await _postRepository.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfeaturePostAsync(Guid postId)
        {
            var post = await _postRepository.GetTrackedByPostIdAsync(postId);
            if (post == null) return false;

            post.IsFeatured = false;
            post.UpdatedAt = DateTime.UtcNow;
            await _postRepository.SaveChangesAsync();
            return true;
        }
    }
}