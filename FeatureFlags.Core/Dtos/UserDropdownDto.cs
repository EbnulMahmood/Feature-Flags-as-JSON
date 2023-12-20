namespace FeatureFlags.Core.Dtos
{
    public record UserDropdown
    {
        public int Id { get; set; }
        public required string Text { get; set; }
    }

    public sealed record UserDropdownDto : UserDropdown
    {
        public int DataCount { get; set; }
    }
}
