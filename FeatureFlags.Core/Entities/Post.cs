namespace FeatureFlags.Core.Entities
{
    public record class Post
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public long Views { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
    }
}
