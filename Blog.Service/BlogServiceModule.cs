using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using MicroBase.Service.Loggings;
using MicroBase.Service.Settings;
using Blog.Service.API;
using Blog.Service.CMS;
using MicroBase.Service.API.GuestContact;
using MicroBase.Service.Review;

namespace Blog.Service
{
    public static class BlogServiceModule
    {
        public static void ModuleRegister(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IBlogCategoryService, BlogCategoryService>();
            services.AddTransient<IBlogCategoryService, BlogCategoryService>();
            services.AddTransient<IBlogService, BlogService>();
            services.AddTransient<ITagService, TagService>();
            services.AddTransient<IBlogCategoryMapService, BlogCategoryMapService>();
            services.AddTransient<IExceptionMonitorService, ExceptionMonitorService>();
            services.AddTransient<ISiteSettingService, SiteSettingService>();
            services.AddTransient<IBlogCMSService, BlogCMSService>();

            services.AddTransient<IGuestContactService, GuestContactService>();
            services.AddTransient<IReviewService, ReviewService>();
            services.AddTransient<IBlogCacheService, BlogCacheService>();
        }

        public static Type[] GetModuleTypes()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            return types;
        }
    }
}