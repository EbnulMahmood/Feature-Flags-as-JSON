using Bogus;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Helpers;
using FeatureFlags.Core.Services;
using FeatureFlags.Core.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FeatureFlags.Web.Controllers
{
    public class PostsController(IPostService postService, IUserService userService) : Controller
    {
        private readonly IPostService _postService = postService ?? throw new ArgumentNullException(nameof(postService));
        private readonly IUserService _userService = userService ?? throw new ArgumentNullException(nameof(userService));

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> PostsDatatable(int draw, int start, int length, string keyword = "", int userId = 0, List<int>? flags = null, CancellationToken token = default)
        {
            var data = new List<List<string>>();
            int recordsTotal = 0;
            int recordsFiltered = 0;
            string message = string.Empty;
            bool isSuccess = false;

            try
            {
                length = length <= 0 ? Constants.datatablePageSize : length;

                IEnumerable<PostDto> postList = await _postService.LoadPostsAsync(start, length, keyword, userId, flags, token) ?? [];
                recordsTotal = postList.FirstOrDefault()?.DataCount ?? 0;
                recordsFiltered = recordsTotal;

                int sl = 1 + start;
                foreach (var item in postList)
                {
                    var postActions = GetPostActions(item.Id, item.Title);

                    List<string> row = [
                        sl++.ToString(),
                        item.Title,
                        item.Content,
                        item.Views.ToString(),
                        $"<a href='{Url.Action(nameof(Edit), "Users", new { id = item.UserId, controllerName = "Posts" })}' class='user-link'>{item.UserName}</a>",
                        item.CreatedAt.ToString("MMM dd, yyyy hh:mm:ss tt"),
                        item.ModifiedAt?.ToString("MMM dd, yyyy hh:mm:ss tt") ?? "-",
                        postActions
                    ];

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

        private string GetPostActions(int postId, string title)
        {
            return $@"
<div class='btn-group action-links' role='group'>
    <a href='{Url.Action(nameof(Edit), "Posts", new { id = postId })}' class='btn btn-outline-warning action-link'>Edit</a>
    <button type='button' href='#' data-title='{title}' data-id='{postId}' class='btn btn-outline-danger action-link delete-action'>Remove</button>
</div>";
        }

        [HttpGet]
        public async Task<JsonResult> ListUserDropdown(string term, int page)
        {
            try
            {
                int resultCount = 100;
                var userDropdownList = await _userService.ListUserDropdownAsync(term, page, resultCount);
                return new JsonResult(userDropdownList);
            }
            catch (Exception)
            {

                throw;
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            var postViewModel = new PostCreateViewModel
            {
                Title = string.Empty,
                Content = string.Empty
            };

            return View(postViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PostCreateViewModel postViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidDataException("Invalid data found");
                }

                if (postViewModel.UserId == 0) throw new InvalidDataException("Invalid User found");

                var post = new Post
                {
                    Title = postViewModel.Title.Trim(),
                    Content = postViewModel.Content.Trim(),
                    UserId = postViewModel.UserId
                };

                await _postService.CreatePostAsync(post);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to create the post.");
            }

            return View(postViewModel);
        }

        #region Seed Data Code
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create(PostCreateViewModel postViewModel)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return View(postViewModel);
        //    }

        //    try
        //    {
        //        List<string> generatedTitles = [];
        //        List<Post> postList = [];

        //        var faker = new Faker<Post>()
        //            .CustomInstantiator(f => new Post
        //            {
        //                Title = GetUniqueTitle(f),
        //                Content = GetRandomContent(f),
        //                UserId = GetRandomUserId(),
        //            });

        //        for (int i = 0; i < 50000; i++)
        //        {
        //            var post = faker.Generate();

        //            // Store the generated title to ensure uniqueness
        //            generatedTitles.Add(post.Title);

        //            // Add the generated post to the list
        //            postList.Add(post);
        //        }

        //        string GetUniqueTitle(Faker faker)
        //        {
        //            string title;
        //            do
        //            {
        //                title = faker.Lorem.Sentence();
        //                if (title.Length > 100)
        //                {
        //                    title = title[..100];
        //                }
        //            }
        //            while (_postService.TitleExistsAsync(title).Result);
        //            return title.Trim();
        //        }

        //        string GetRandomContent(Faker faker)
        //        {
        //            return faker.Lorem.Paragraph().Trim();
        //        }

        //        int GetRandomUserId()
        //        {
        //            return _postService.GetRandomUserIdAsync().Result;
        //        }

        //        foreach (var post in postList)
        //        {
        //            await _postService.CreatePostAsync(post);
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
        //    catch (Exception)
        //    {
        //        ModelState.AddModelError("", "Failed to create the post.");
        //    }

        //    return View(postViewModel);
        //}
        #endregion

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var editViewModel = new PostEditViewModel { Title = string.Empty, Content = string.Empty };

            try
            {
                var post = await _postService.GetPostByIdAsync(id) ?? throw new InvalidDataException("Invalid post");

                editViewModel.Id = post.Id;
                editViewModel.Title = post.Title;
                editViewModel.Content = post.Content;
                editViewModel.UserId = post.UserId;
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }

            return View(editViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, PostEditViewModel editViewModel)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidDataException("Invalid data found");
                }

                if (id != editViewModel.Id)
                {
                    throw new InvalidDataException("Post ID in the request body doesn't match the route parameter.");
                }

                var post = new Post
                {
                    Id = editViewModel.Id,
                    Title = editViewModel.Title.Trim(),
                    Content = editViewModel.Content.Trim(),
                    UserId = editViewModel.UserId,
                };

                await _postService.UpdatePostAsync(post);

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Failed to update the post.");
            }

            return View(editViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id = 0)
        {
            bool isSuccess = false;
            string message;

            try
            {
                if (!ModelState.IsValid)
                {
                    throw new InvalidDataException("Error deleting post");
                }

                if (id == 0) throw new InvalidDataException("Invalid post found");

                var postToDelete = await _postService.GetPostByIdAsync(id) ?? throw new InvalidDataException("Invalid post found");

                await _postService.DeletePostAsync(id);

                isSuccess = true;
                message = "Post successfully deleted";
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
