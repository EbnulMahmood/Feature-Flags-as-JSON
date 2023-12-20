namespace FeatureFlags.Core.Dtos
{
    public sealed record UserDto
    {
        public int Id { get; init; }
        public required string Username { get; init; }
        public required string Email { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public DateTimeOffset? ModifiedAt { get; init; }
        public string FlagsJson { get; set; } = string.Empty;

        public List<int> Flags
        {
            get => !string.IsNullOrEmpty(FlagsJson) ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<int>>(FlagsJson) : [];
            set => FlagsJson = Newtonsoft.Json.JsonConvert.SerializeObject(value);
        }
        public int DataCount { get; set; }
    }
}
