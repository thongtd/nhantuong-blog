using Blog.Entity;
using Blog.Service.API;
using DailyContentWriter.Models;
using DailyContentWriter.Services;
using MicroBase.Entity;
using MicroBase.FileManager;
using MicroBase.NoDependencyService;
using MicroBase.RedisProvider;
using MicroBase.Service;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using NLog.Web;

var logger = LogManager.Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

logger.Info("Application starting");

var builder = Host.CreateApplicationBuilder(args);
ConfigurationManager Configuration = builder.Configuration;

builder.Logging.ClearProviders();
builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
builder.Logging.AddNLog();

// Nạp config theo environment
builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

builder.Services.Configure<AppSettings>(builder.Configuration);

builder.Services.AddHttpClient();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

MySqlModuleRegister.AddMicroDbContext<MicroDbContext>(builder.Services, Configuration);
MySqlModuleRegister.AddMicroDbContext<BlogDbContext>(builder.Services, Configuration);
BaseServiceModule.RegisterGenericServices(builder.Services);
NoDependencyServiceModule.ModuleRegister(builder.Services, Configuration);
FileUploadServiceModule.ModuleRegister(builder.Services, Configuration);
BaseSqlServiceModule.ModuleRegister(builder.Services, Configuration);
RedisServiceModule.ModuleRegister(builder.Services, Configuration);

builder.Services.AddSingleton<GoogleSheetService>();
builder.Services.AddSingleton<OpenAiService>();
builder.Services.AddSingleton<ContentTransformer>();
builder.Services.AddSingleton<ContentJobService>();
builder.Services.AddSingleton<SitemapService>();

builder.Services.AddHostedService<SchedulerWorker>();
builder.Services.AddHostedService<SitemapSchedulerWorker>();
builder.Services.AddTransient<IBlogCacheService, BlogCacheService>();

logger.Info("Application started");

var host = builder.Build();
host.Run();