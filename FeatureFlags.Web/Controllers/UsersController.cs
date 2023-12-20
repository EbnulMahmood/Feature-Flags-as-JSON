using Bogus;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Enums;
using FeatureFlags.Core.Helper;
using FeatureFlags.Core.Helpers;
using FeatureFlags.Core.Services;
using FeatureFlags.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace FeatureFlags.Web.Controllers
{
    public class UsersController(IUserService userService) : Controller
    {
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> UsersDatatable(int draw, int start, int length, string? flag = null, long? viewsMin = null, long? viewsMax = null, CancellationToken token = default)
        {
            List<List<string>> data = [];
            int recordsTotal = 0;
            int recordsFiltered = 0;
            string message = string.Empty;
            bool isSuccess = false;

            try
            {
                length = length <= 0 ? Constants.datatablePageSize : length;

                IEnumerable<UserDto> userList = await _userService.LoadUsersAsync(start, length, flag, viewsMin, viewsMax, token) ?? [];
                recordsTotal = userList.FirstOrDefault()?.DataCount ?? 0;
                recordsFiltered = recordsTotal;

                int sl = 1 + start;
                foreach (var item in userList)
                {
                    var flags = GetFlags(item.Flags);
                    var userActions = GetUserActions(item.Id, item.Username);

                    var row = new List<string>
                    {
                        sl++.ToString(),
                        item.Username,
                        item.Email,
                        item.CreatedAt.ToString("MMM dd, yyyy hh:mm:ss tt"),
                        item.ModifiedAt?.ToString("MMM dd, yyyy hh:mm:ss tt") ?? "-",
                        flags,
                        userActions
                    };
                    data.Add(row);
                }

                isSuccess = true;
            }
            catch (OperationCanceledException ex)
            {
                message = ex.Message;
            }
            catch (InvalidDataException ex)
            {
                message = ex.Message;
            }
            catch (Exception)
            {
                message = "Internal Server Error";
            }

            return Json(new { draw, recordsTotal, recordsFiltered, data, isSuccess, message });
        }

        private static string GetFlags(List<int> flags)
        {
            var individualFlags = UserFlagsHelper.GetIndividualFlags(flags);
            var flagContainer = new StringBuilder("<div class='flag-container'>");

            foreach (var flag in individualFlags)
            {
                flagContainer.Append($"<span class='flag {flag.ToLower().Replace(" ", "-")}'>{flag}</span>");
            }

            flagContainer.Append("</div>");
            return flagContainer.ToString();
        }

        private string GetUserActions(int userId, string username)
        {
            return $@"
<div class='btn-group action-links' role='group'>
    <a href='{Url.Action(nameof(Edit), "Users", new { id = userId })}' class='btn btn-outline-warning action-link'>Edit</a>
    <button type='button' href='#' data-name='{username}' data-id='{userId}' class='btn btn-outline-danger action-link delete-action'>Remove</button>
</div>";
        }

        [HttpGet]
        public IActionResult Create()
        {
            var userViewModel = new UserCreateViewModel
            {
                Username = string.Empty,
                Email = string.Empty
            };

            return View(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserCreateViewModel userViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidDataException("Invalid data found");
                }

                var user = new User
                {
                    Username = userViewModel.Username.Trim(),
                    Email = userViewModel.Email.Trim(),
                    Flags = userViewModel.Flags
                };

                await _userService.CreateUserAsync(user);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to create the user.");
            }

            return View(userViewModel);
        }

        #region Seed Data Code
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(UserCreateViewModel userViewModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(userViewModel);
        //    }

        //    try
        //    {
        //        List<string> generatedUsernames = [];
        //        List<string> generatedEmails = [];
        //        List<User> userList = [];

        //        var faker = new Faker<User>()
        //            .CustomInstantiator(f => new User
        //            {
        //                Username = GetUniqueUsername(f),
        //                Email = GetUniqueEmail(f),
        //                Flags = GenerateRandomCombinedFlags()
        //            });

        //        for (int i = 0; i < 100000; i++)
        //        {
        //            var user = faker.Generate();

        //            // Store the generated username and email to ensure uniqueness
        //            generatedUsernames.Add(user.Username);
        //            generatedEmails.Add(user.Email);

        //            // Add the generated user to the list of users
        //            userList.Add(user);
        //        }

        //        //string GetUniqueUsername(Faker f)
        //        //{
        //        //    string username;
        //        //    do
        //        //    {
        //        //        username = f.Internet.UserName();
        //        //    }
        //        //    while (generatedUsernames.Contains(username));
        //        //    return username;
        //        //}

        //        //string GetUniqueEmail(Faker f)
        //        //{
        //        //    string email;
        //        //    do
        //        //    {
        //        //        email = f.Internet.Email();
        //        //    }
        //        //    while (generatedEmails.Contains(email));
        //        //    return email;
        //        //}

        //        string GetUniqueUsername(Faker faker)
        //        {
        //            string username;
        //            do
        //            {
        //                username = faker.Internet.UserName();
        //            }
        //            while (_userService.UsernameExistsAsync(username).Result);
        //            return username;
        //        }

        //        string GetUniqueEmail(Faker faker)
        //        {
        //            string email;
        //            do
        //            {
        //                email = faker.Internet.Email();
        //            }
        //            while (_userService.EmailExistsAsync(email).Result);
        //            return email;
        //        }

        //        foreach (var user in userList)
        //        {
        //            await _userService.CreateUserAsync(user);
        //        }

        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch (ArgumentNullException ex)
        //    {
        //        ModelState.AddModelError("", $"Error: {ex.Message}");
        //    }
        //    catch (ArgumentException ex)
        //    {
        //        ModelState.AddModelError("", $"Error: {ex.Message}");
        //    }
        //    catch (Exception ex)
        //    {
        //        ModelState.AddModelError("", $"Error: {ex.Message}");
        //    }

        //    return View(userViewModel);
        //}

        //private static List<int> GenerateRandomCombinedFlags()
        //{
        //    var random = new Random();
        //    var allFlags = Enum.GetValues(typeof(UserFlags))
        //                       .Cast<UserFlags>()
        //                       .Where(flag => flag != UserFlags.None)
        //                       .ToList();

        //    int numberOfFlags = random.Next(1, allFlags.Count + 1);

        //    var selectedFlags = allFlags.OrderBy(_ => random.Next()).Take(numberOfFlags).ToList();

        //    return selectedFlags.Select(flag => (int)flag).ToList();
        //}
        #endregion

        [HttpGet]
        public async Task<IActionResult> Edit(int id, string? controllerName = "")
        {
            var editViewModel = new UserEditViewModel { Username = string.Empty, Email = string.Empty };

            try
            {
                var user = await _userService.GetUserByIdAsync(id) ?? throw new InvalidDataException("Invalid user");

                editViewModel.Id = user.Id;
                editViewModel.Username = user.Username;
                editViewModel.Email = user.Email;
                editViewModel.Flags = user.Flags;
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Invalid user.");
            }

            ViewData["controller"] = controllerName ?? "Users";

            return View(editViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UserEditViewModel userViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidDataException("Invalid data found");
                }

                if (id != userViewModel.Id)
                {
                    throw new InvalidDataException("User ID in the request body doesn't match the route parameter.");
                }

                var user = new User
                {
                    Id = userViewModel.Id,
                    Username = userViewModel.Username.Trim(),
                    Email = userViewModel.Email.Trim(),
                    Flags = userViewModel.Flags
                };

                await _userService.UpdateUserAsync(user);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentNullException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to create the user.");
            }

            return View(userViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> DeleteConfirmed(int id = 0)
        {
            bool isSuccess = false;
            string message;

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidDataException("Error deleting user");
                }

                if (id == 0) throw new InvalidDataException("Invalid user found");

                var userToDelete = await _userService.GetUserByIdAsync(id) ?? throw new InvalidDataException("Invalid user found");

                await _userService.DeleteUserAsync(id);

                isSuccess = true;
                message = "User sucessfully deleted";
            }
            catch (InvalidDataException ex)
            {
                message = ex.Message;
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(new { isSuccess, message });
        }
    }
}
