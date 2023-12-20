using FeatureFlags.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace FeatureFlags.Core.ViewModels
{
    public class UserCreateViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public required string Email { get; set; }

        public List<int> Flags { get; set; } = [];
    }

    public sealed class UserEditViewModel : UserCreateViewModel
    {
        public int Id { get; set; }
    }
}
