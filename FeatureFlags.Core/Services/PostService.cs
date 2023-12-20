using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Repositories;

namespace FeatureFlags.Core.Services
{
    public interface IPostService
    {
        Task CreatePostAsync(Post post);
        Task DeletePostAsync(int postId);
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length, string keyword = "", int userId = 0, List<int>? flags = null, CancellationToken token = default);
        Task UpdatePostAsync(Post post);
        Task<int> GetRandomUserIdAsync();
        Task<bool> TitleExistsAsync(string title);
    }

    internal sealed class PostService(IPostRepository postRepository) : IPostService
    {
        private readonly IPostRepository _postRepository = postRepository ?? throw new ArgumentNullException(nameof(postRepository));

        public async Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length, string keyword = "", int userId = 0, List<int>? flags = null, CancellationToken token = default)
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

                return await _postRepository.LoadPostsAsync(start, length, keyword, userId, flags);
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

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            try
            {
                return await _postRepository.GetPostByIdAsync(postId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task CreatePostAsync(Post post)
        {
            try
            {
                await ValidatePostDataAsync(post.Title, post.Content, post.UserId);

                await _postRepository.CreatePostAsync(post);
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

        public async Task UpdatePostAsync(Post post)
        {
            try
            {
                await ValidatePostDataAsync(post.Title, post.Content, post.UserId, post.Id);

                await _postRepository.UpdatePostAsync(post);
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

        private async Task ValidatePostDataAsync(string title, string content, int userId, int postId = 0)
        {
            ValidateTitle(title);
            ValidateTitleLength(title);
            ValidateContent(content);
            await ValidateUniqueTitlePerUserAsync(title, userId, postId);
        }

        private static void ValidateTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new ArgumentException("Title cannot be empty.");
            }
        }

        private static void ValidateTitleLength(string title)
        {
            const int maxLength = 100;

            if (title.Length > maxLength)
            {
                throw new ArgumentException($"Title cannot exceed {maxLength} characters.");
            }
        }

        private static void ValidateContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("Content cannot be empty.");
            }
        }

        private async Task ValidateUniqueTitlePerUserAsync(string title, int userId, int postId = 0)
        {
            var existingPost = await _postRepository.GetPostByTitleAndUserIdAsync(title, userId, postId);

            if (existingPost != null)
            {
                throw new ArgumentException("A post with the same title already exists for this user.");
            }
        }

        public async Task DeletePostAsync(int postId)
        {
            try
            {
                await _postRepository.DeletePostAsync(postId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<int> GetRandomUserIdAsync()
        {
            try
            {
                return await _postRepository.GetRandomUserIdAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> TitleExistsAsync(string title)
        {
            try
            {
                return await _postRepository.TitleExistsAsync(title);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
