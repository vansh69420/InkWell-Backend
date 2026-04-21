namespace InkWell.Post.Service.DTOs.Responses
{
    public class AuthorPostsResponse
    {
        public Guid AuthorId { get; set; }
        public List<PostSummaryResponse> Posts { get; set; } = new();
    }
}