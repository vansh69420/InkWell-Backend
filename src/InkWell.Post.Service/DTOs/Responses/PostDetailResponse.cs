namespace InkWell.Post.Service.DTOs.Responses
{
    public class PostDetailResponse
    {
        public Guid PostId { get; set; }
        public Guid AuthorId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Excerpt { get; set; } = string.Empty;
        public string? FeaturedImageUrl { get; set; }
        public int ReadTimeMin { get; set; }
        public int ViewCount { get; set; }
        public int LikesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public List<Guid> CategoryIds { get; set; } = new();
        public List<Guid> TagIds { get; set; } = new();
    }
}