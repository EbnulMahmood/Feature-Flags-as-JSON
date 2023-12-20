using Dapper;
using FeatureFlags.Core.Dtos;
using FeatureFlags.Core.Entities;
using FeatureFlags.Core.Enums;
using System.Data;
using System.Linq;

namespace FeatureFlags.Core.Repositories
{
    public interface IPostRepository
    {
        Task CreatePostAsync(Post post);
        Task DeletePostAsync(int postId);
        Task<PostDto?> GetPostByIdAsync(int postId);
        Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length, string keyword = "", int userId = 0, List<int>? flags = null);
        Task UpdatePostAsync(Post post);
        Task<Post?> GetPostByTitleAndUserIdAsync(string title, int userId, int postId = 0);
        Task<int> GetRandomUserIdAsync();
        Task<bool> TitleExistsAsync(string title);
    }

    internal sealed class PostRepository(IDbConnection dbConnection) : IPostRepository
    {
        private readonly IDbConnection _dbConnection = dbConnection;

        public async Task<IEnumerable<PostDto>> LoadPostsAsync(int start, int length, string keyword = "", int userId = 0, List<int>? flags = null)
        {
            var parameters = new DynamicParameters();
            string conditionQuery = string.Empty;

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                conditionQuery += $" AND (p.Title LIKE @{nameof(keyword)} OR p.Content LIKE @{nameof(keyword)}) {Environment.NewLine}";
                parameters.Add($"@{nameof(keyword)}", $"%{keyword.Trim()}%", dbType: DbType.String);
            }

            if (userId != 0)
            {
                conditionQuery += $" AND u.Id = @{nameof(userId)} {Environment.NewLine}";
                parameters.Add($"@{nameof(userId)}", userId, dbType: DbType.Int32);
            }

            if (flags != null)
            {
                if (flags.Contains((int)UserFlags.None))
                {
                    conditionQuery += $" AND u.Flags = '[]'::JSONB {Environment.NewLine}";
                }
                else
                {
                    conditionQuery += $" AND u.Flags @> ALL (ARRAY(SELECT jsonb_build_array(val)::JSONB FROM unnest(@{nameof(flags)}) AS val))";
                    parameters.Add($"@{nameof(flags)}", flags, DbType.Object);
                }
            }

            string query = $@"
SELECT 
    p.Id,
    p.Title,
    p.Content,
    p.Views,
    u.UserName,
    p.UserId,
    p.CreatedAt,
    p.ModifiedAt,
    COUNT(*) OVER() AS DataCount
FROM Posts AS p
JOIN Users AS u ON u.Id = p.UserId
WHERE 1=1
{conditionQuery}
ORDER BY p.CreatedAt DESC
OFFSET @{nameof(start)} ROWS FETCH NEXT @{nameof(length)} ROWS ONLY";

            parameters.Add($"@{nameof(start)}", start, dbType: DbType.Int32);
            parameters.Add($"@{nameof(length)}", length, dbType: DbType.Int32);

            return await _dbConnection.QueryAsync<PostDto>(query, parameters);
        }

        public async Task<PostDto?> GetPostByIdAsync(int postId)
        {
            return await _dbConnection.QueryFirstOrDefaultAsync<PostDto>("SELECT * FROM Posts WHERE Id = @PostId", new { PostId = postId });
        }

        public async Task CreatePostAsync(Post post)
        {
            const string insertSql = @"
INSERT INTO Posts (Title, Content, Views, UserId, CreatedAt, ModifiedAt)
VALUES (@Title, @Content, FLOOR(RANDOM() * 100001), @UserId, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(insertSql, post, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task UpdatePostAsync(Post post)
        {
            const string sql = @"UPDATE Posts SET Title = @Title, Content = @Content, 
                        ModifiedAt = CURRENT_TIMESTAMP 
                        WHERE Id = @Id";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(sql, post, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task DeletePostAsync(int postId)
        {
            const string sql = @"DELETE FROM Posts WHERE Id = @PostId";

            if (_dbConnection.State != ConnectionState.Open)
            {
                _dbConnection.Open();
            }

            using var transaction = _dbConnection.BeginTransaction();

            try
            {
                await _dbConnection.ExecuteAsync(sql, new { PostId = postId }, transaction);
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<Post?> GetPostByTitleAndUserIdAsync(string title, int userId, int postId = 0)
        {
            string conditionQuery = string.Empty;
            var parameters = new DynamicParameters();

            if (postId != 0)
            {
                conditionQuery += $" AND Id != @{nameof(postId)} {Environment.NewLine}";
                parameters.Add($"@{nameof(postId)}", postId, dbType: DbType.Int32);
            }

            parameters.Add($"@{nameof(title)}", title, dbType: DbType.String);
            parameters.Add($"@{nameof(userId)}", userId, dbType: DbType.Int32);

            string sql = $@"
SELECT 
    *
FROM Posts
WHERE Title = @{nameof(title)} 
AND UserId = @{nameof(userId)} 
{conditionQuery}";

            return await _dbConnection.QueryFirstOrDefaultAsync<Post>(sql, parameters);
        }

        public async Task<int> GetRandomUserIdAsync()
        {
            string query = "SELECT Id FROM Users ORDER BY RANDOM() LIMIT 1";

            return await _dbConnection.QueryFirstOrDefaultAsync<int>(query);
        }

        public async Task<bool> TitleExistsAsync(string title)
        {
            string query = "SELECT COUNT(*) FROM Posts WHERE Title = @Title;";
            var parameters = new { Title = title };

            var count = await _dbConnection.ExecuteScalarAsync<int>(query, parameters);

            return count > 0;
        }
    }
}
