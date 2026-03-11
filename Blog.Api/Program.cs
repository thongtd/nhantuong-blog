using Blog.Entity;
using Blog.Service;
using Blog.Service.API;
using MicroBase.BaseMvc;
using MicroBase.BaseMvc.Filters;
using MicroBase.BaseMvc.Middlewares;
using MicroBase.Entity;
using MicroBase.FileManager;
using MicroBase.NoDependencyService;
using MicroBase.RedisProvider;
using MicroBase.Service;
using MicroBase.Share;
using MicroBase.Share.Models.Base;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.OpenApi.Models;
using Minio;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager Configuration = builder.Configuration;

// Add services to the container.
var hostUrls = Configuration.GetSection("Application:Hosts").Get<string[]>();
builder.WebHost.UseUrls(hostUrls);

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<SwaggerHeaderFilter>();
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "nhantuong.vn API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please insert JWT with Bearer into field",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement {
    {
        new OpenApiSecurityScheme
        {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "Bearer"
        }
        },
        new string[] { }
    }});
});

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        builder => builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
});

#region Register Services

builder.Services.AddHttpClient();
builder.Services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();

MySqlModuleRegister.AddMicroDbContext<MicroDbContext>(builder.Services, Configuration);
MySqlModuleRegister.AddMicroDbContext<BlogDbContext>(builder.Services, Configuration);
BaseServiceModule.RegisterGenericServices(builder.Services);

RedisServiceModule.ModuleRegister(builder.Services, Configuration);
NoDependencyServiceModule.ModuleRegister(builder.Services, Configuration);
FileUploadServiceModule.ModuleRegister(builder.Services, Configuration);
BaseSqlServiceModule.ModuleRegister(builder.Services, Configuration);
BlogServiceModule.ModuleRegister(builder.Services, Configuration);

#endregion

var endpointMinio = Configuration.GetValue<string>("FileManage:MinioConfig:Endpoint");
var accessKeyMinio = Configuration.GetValue<string>("FileManage:MinioConfig:AccessKey");
var secretKeyMinio = Configuration.GetValue<string>("FileManage:MinioConfig:SecretKey");
var useSSLMinio = Configuration.GetValue<bool>("FileManage:MinioConfig:UseSSL");

builder.Services.AddMinio(configureClient => configureClient
    .WithEndpoint(endpointMinio)
    .WithCredentials(accessKeyMinio, secretKeyMinio)
    .WithSSL(useSSLMinio)
    .Build());

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var modelState = actionContext.ModelState.Values;
        return new BadRequestObjectResult(new BaseResponse<object>
        {
            Success = false,
            Code = StatusCodes.Status400BadRequest,
            Message = CommonMessage.MODEL_STATE_INVALID,
            Errors = ModelStateService.GetModelStateErros(actionContext.ModelState)
        });
    };
});

var app = builder.Build();
app.UseCors("AllowAllOrigins");

// Đúng thứ tự: Routing => Authentication => Authorization => Endpoints

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ErrorHandlerMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DefaultModelsExpandDepth(-1);
    });
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var blogCacheService = services.GetRequiredService<IBlogCacheService>();
    await blogCacheService.BuildGroupsToCacheAsync();
    await blogCacheService.BuildBlogsToCacheAsync();
    await blogCacheService.BuildTagsToCacheAsync();
}

app.Run();