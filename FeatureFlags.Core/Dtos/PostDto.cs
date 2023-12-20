namespace FeatureFlags.Core.Dtos
{
    public sealed record PostDto
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Content { get; set; }
        public long Views { get; set; }
        public required string UserName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ModifiedAt { get; set; }
        public int UserId { get; set; }
        public int DataCount { get; set; }
    }
}
