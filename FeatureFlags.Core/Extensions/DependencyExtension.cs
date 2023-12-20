using FeatureFlags.Core.Repositories;
using FeatureFlags.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace FeatureFlags.Core.Extensions
{
    public static class DependencyExtension
    {
        public static void AddDependencyExtension(this IServiceCollection services)
        {
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IPostRepository, PostRepository>();

            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IPostService, PostService>();
        }
    }
}
