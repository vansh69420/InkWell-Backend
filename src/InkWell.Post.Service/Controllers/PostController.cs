using InkWell.Post.Service.DTOs.Responses;
using InkWell.Post.Service.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("published")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetPublishedPosts()
        {
            var posts = await _postService.GetPublishedPostsAsync();
            return Ok(posts);
        }

        [HttpGet("slug/{slug}")]
        public async Task<ActionResult<PostDetailResponse>> GetPostBySlug(string slug)
        {
            var post = await _postService.GetPostBySlugAsync(slug);

            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

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

        [HttpGet("category/{categoryId:guid}")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetByCategory(Guid categoryId)
        {
            var posts = await _postService.GetPostsByCategoryAsync(categoryId);
            return Ok(posts);
        }

        [HttpGet("tag/{tagId:guid}")]
        public async Task<ActionResult<IReadOnlyList<PostSummaryResponse>>> GetByTag(Guid tagId)
        {
            var posts = await _postService.GetPostsByTagAsync(tagId);
            return Ok(posts);
        }

        [HttpGet("author/{authorId:guid}")]
        public async Task<ActionResult<AuthorPostsResponse>> GetByAuthor(Guid authorId)
        {
            var response = await _postService.GetPostsByAuthorAsync(authorId);
            return Ok(response);
        }

        [HttpGet("count")]
        public async Task<ActionResult<PostCountResponse>> GetCount()
        {
            var count = await _postService.GetPostCountAsync();
            return Ok(count);
        }

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
    }
}