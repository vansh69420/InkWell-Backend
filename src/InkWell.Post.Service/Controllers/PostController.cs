using InkWell.Post.Service.DTOs.Responses;
using InkWell.Post.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using InkWell.Post.Service.DTOs.Requests;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace InkWell.Post.Service.Controllers
{
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
        }

        [AllowAnonymous]
        [HttpGet("published")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetPublishedPosts()
        {
            var posts = await _postService.GetPublishedPostsAsync();
            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<PostDetailResponse>> GetPostBySlug(string slug)
        {
            Guid? currentUserId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (Guid.TryParse(userIdClaim, out var parsedId))
                currentUserId = parsedId;

            var post = await _postService.GetPostBySlugAsync(slug, currentUserId);

            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> Search([FromQuery] string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return BadRequest("Keyword is required.");
            }

            var posts = await _postService.SearchPostsAsync(keyword);
            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet("category/{categoryId:guid}")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetByCategory(Guid categoryId)
        {
            var posts = await _postService.GetPostsByCategoryAsync(categoryId);
            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet("tag/{tagId:guid}")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetByTag(Guid tagId)
        {
            var posts = await _postService.GetPostsByTagAsync(tagId);
            return Ok(posts);
        }

        [AllowAnonymous]
        [HttpGet("author/{authorId:guid}")]
        public async Task<ActionResult<AuthorPostsResponse>> GetByAuthor(Guid authorId)
        {
            var response = await _postService.GetPostsByAuthorAsync(authorId);
            return Ok(response);
        }

        [AllowAnonymous]
        [HttpGet("count")]
        public async Task<ActionResult<PostCountResponse>> GetCount()
        {
            var count = await _postService.GetPostCountAsync();
            return Ok(count);
        }

        [AllowAnonymous]
        [HttpPost("{postId:guid}/view")]
        public async Task<IActionResult> RecordView(Guid postId)
        {
            var success = await _postService.IncrementViewCountAsync(postId);

            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpGet("me")]
        public async Task<ActionResult<IReadOnlyList<MyPostSummaryResponse>>> GetMyPosts()
        {
            var userId = GetCurrentUserId();
            var posts = await _postService.GetMyPostsAsync(userId);
            return Ok(posts);
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpGet("{postId:guid}")]
        public async Task<ActionResult<PostEditorResponse>> GetForEdit(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var post = await _postService.GetPostForEditAsync(userId, IsAdmin(), postId);
                return post == null ? NotFound() : Ok(post);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpPost]
        public async Task<ActionResult<PostEditorResponse>> Create([FromBody] CreatePostRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var created = await _postService.CreatePostAsync(userId, request);
                return Ok(created);
            }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (HttpRequestException ex) { return StatusCode(503, ex.Message); }
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpPut("{postId:guid}")]
        public async Task<ActionResult<PostEditorResponse>> Update(Guid postId, [FromBody] UpdatePostRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                var updated = await _postService.UpdatePostAsync(userId, IsAdmin(), postId, request);
                return updated == null ? NotFound() : Ok(updated);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
            catch (ArgumentException ex) { return BadRequest(ex.Message); }
            catch (HttpRequestException ex) { return StatusCode(503, ex.Message); }
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpPut("{postId:guid}/publish")]
        public async Task<ActionResult<PostEditorResponse>> Publish(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var res = await _postService.PublishPostAsync(userId, IsAdmin(), postId);
                return res == null ? NotFound() : Ok(res);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpPut("{postId:guid}/unpublish")]
        public async Task<ActionResult<PostEditorResponse>> Unpublish(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var res = await _postService.UnpublishPostAsync(userId, IsAdmin(), postId);
                return res == null ? NotFound() : Ok(res);
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        [Authorize(Roles = "Author,Admin")]
        [HttpDelete("{postId:guid}")]
        public async Task<IActionResult> Delete(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var ok = await _postService.DeletePostAsync(userId, IsAdmin(), postId);
                return ok ? NoContent() : NotFound();
            }
            catch (UnauthorizedAccessException) { return Forbid(); }
        }

        private Guid GetCurrentUserId()
        {
            var raw = User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(raw) || !Guid.TryParse(raw, out var id))
                throw new UnauthorizedAccessException("Invalid user id.");
            return id;
        }

        private bool IsAdmin() => User.IsInRole("Admin");

        [Authorize]
        [HttpPost("{postId:guid}/like")]
        public async Task<IActionResult> LikePost(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _postService.LikePostAsync(postId, userId);
                if (!result) return BadRequest("Already liked or post not found.");
                return Ok();
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }

        [Authorize]
        [HttpPost("{postId:guid}/unlike")]
        public async Task<IActionResult> UnlikePost(Guid postId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _postService.UnlikePostAsync(postId, userId);
                if (!result) return BadRequest("Not liked or post not found.");
                return Ok();
            }
            catch (UnauthorizedAccessException) { return Unauthorized(); }
        }
    }
}