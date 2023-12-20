using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.Core.ViewModels
{
    public class PostCreateViewModel
    {
        public required string Title { get; set; }
        public required string Content { get; set; }
        [Display(Name = "User")]
        public int UserId { get; set; }
    }

    public sealed class PostEditViewModel : PostCreateViewModel
    {
        public int Id { get; set; }
    }
}
