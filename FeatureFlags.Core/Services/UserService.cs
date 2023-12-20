using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Enums;
using FeatureFlags.Core.Repositories;

namespace FeatureFlags.Core.Services
{
    public interface IUserService
    {
        Task CreateUserAsync(User user);
        Task DeleteUserAsync(int userId);
        Task<UserDto?> GetUserByIdAsync(int userId);
        Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, string? flag = null, long? viewsMin = null, long? viewsMax = null, CancellationToken token = default);
        Task<object> ListUserDropdownAsync(string name, int page, int resultCount);
        Task UpdateUserAsync(User user);
        Task<bool> UsernameExistsAsync(string username);
        Task<bool> EmailExistsAsync(string email);
    }

    internal sealed class UserService(IUserRepository userRepository) : IUserService
    {
        private readonly IUserRepository _userRepository = userRepository;

        public async Task<IEnumerable<UserDto>> LoadUsersAsync(int start, int length, string? flag = null, long? viewsMin = null, long? viewsMax = null, CancellationToken token = default)
        {
            try
            {
                if (token.IsCancellationRequested == true)
                {
                    throw new OperationCanceledException(token);
                }

                if (length < 0)
                {
                    throw new InvalidDataException("Page Size is less than zero");
                }

                ValidateViews(viewsMin, viewsMax);

                return await _userRepository.LoadUsersAsync(start, length, flag, viewsMin, viewsMax);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (InvalidDataException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> ListUserDropdownAsync(string name, int page, int resultCount)
        {
            try
            {
                (var userDropdownDtoList, bool morePages) = await _userRepository.ListUserDropdownAsync(name, page, resultCount);

                UserDropdown[] results = userDropdownDtoList?
                    .Select(x => new UserDropdown
                    {
                        Id = x.Id,
                        Text = x.Text,
                    })?
                    .ToArray() ?? [];

                var pagination = new { more = morePages };

                return new { results, pagination };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<UserDto?> GetUserByIdAsync(int userId)
        {
            try
            {
                return await _userRepository.GetUserByIdAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreateUserAsync(User user)
        {
            try
            {
                await ValidateUserDataAsync(user.Username, user.Email, user.Flags);

                await _userRepository.CreateUserAsync(user);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                await ValidateUserDataAsync(user.Username, user.Email, user.Flags, user.Id);

                await _userRepository.UpdateUserAsync(user);
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task ValidateUserDataAsync(string username, string email, List<int> flags, int userId = 0)
        {
            ValidateUsername(username);
            ValidateEmail(email);
            ValidateFlags(flags);
            await ValidateExistingUserAsync(username, email, userId);
        }

        private static void ValidateFlags(List<int> flags)
        {
            foreach (var flag in flags)
            {
                if (!Enum.IsDefined(typeof(UserFlags), flag)) throw new InvalidDataException("Invalid flag");
            }
        }

        private static void ValidateUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Username cannot be empty.");
            }
        }

        private static void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be empty.");
            }

            if (!IsValidEmail(email))
            {
                throw new ArgumentException("Invalid email format.");
            }
        }

        private static bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private async Task ValidateExistingUserAsync(string username, string email, int userId = 0)
        {
            var existingUser = await _userRepository.GetUserByUsernameOrEmailAsync(username, email, userId);

            if (existingUser != null)
            {
                throw new ArgumentException("User with the same Username or Email already exists.");
            }
        }

        private static void ValidateViews(long? viewsMin = null, long? viewsMax = null)
        {
            if (viewsMin < 0 || viewsMax < 0)
            {
                throw new InvalidDataException("Views must be non-negative values");
            }

            if (viewsMin.HasValue && viewsMax.HasValue && viewsMin > viewsMax)
            {
                throw new InvalidDataException("Minimum views should be less than maximum views");
            }
        }

        public async Task DeleteUserAsync(int userId)
        {
            try
            {
                await _userRepository.DeleteUserAsync(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<bool> UsernameExistsAsync(string username)
        {
            try
            {
                return _userRepository.UsernameExistsAsync(username);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<bool> EmailExistsAsync(string email)
        {
            try
            {
                return _userRepository.EmailExistsAsync(email);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
