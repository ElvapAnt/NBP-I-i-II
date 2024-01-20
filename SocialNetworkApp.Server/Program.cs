using Microsoft.Extensions.Options;
using Neo4j.Driver;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Settings;
using SocialNetworkApp.Server.Business.Services;
using SocialNetworkApp.Server.Error;
using StackExchange.Redis;
using SocialNetworkApp.Server.Business.Services.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region redisconfig
builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddSingleton<IRedisSettings>(
    sp=>sp.GetRequiredService<IOptions<RedisSettings>>().Value);

builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;
    return ConnectionMultiplexer.Connect(settings.ConnectionString);
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    var redisSettings = builder.Configuration.GetSection("RedisSettings").Get<RedisSettings>();
    options.Configuration = redisSettings!.ConnectionString;
    options.InstanceName = redisSettings.InstanceName;
});

builder.Services.AddSingleton<ICacheService, RedisCacheService>();
#endregion

#region neo4jconfig
builder.Services.Configure<NeoSettings>(builder.Configuration.GetSection("NeoSettings"));
builder.Services.AddSingleton<INeoSettings>(
       sp=>sp.GetRequiredService<IOptions<NeoSettings>>().Value);

builder.Services.AddSingleton<IDriver>(sp =>
{
    var neoSettings = sp.GetRequiredService<INeoSettings>();
    
    var driver= GraphDatabase.Driver(neoSettings.Uri, AuthTokens.Basic(neoSettings.User, neoSettings.Password));

    AppDomain.CurrentDomain.ProcessExit += (sender, args) => driver.Dispose();

    return driver;
});

builder.Services.AddScoped<UserRepo, UserRepo>();
builder.Services.AddScoped<UserService, UserService>();

builder.Services.AddScoped<PostRepo, PostRepo>();
builder.Services.AddScoped<PostService, PostService>();

builder.Services.AddScoped<ChatRepo, ChatRepo>();
builder.Services.AddScoped<ChatService, ChatService>();

builder.Services.AddScoped<NotificationRepo, NotificationRepo>();
builder.Services.AddScoped<NotificationService, NotificationService>();

builder.Services.AddScoped<WebSocketService, WebSocketService>();

#endregion

builder.Services.AddCors(action =>
{
    action.AddPolicy("CORS", policy =>
    {
        policy.AllowAnyMethod().AllowAnyHeader().AllowAnyOrigin();
    });
});


var app = builder.Build();

app.UseMiddleware<ErrorHandler>();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("CORS");
app.UseHttpsRedirection();

app.UseWebSockets();

// Configure WebSocket requests to be handled by WebSocketService
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/chat") // Or whatever path you want to handle WebSocket requests
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            // Resolve WebSocketService for the current request
            var webSocketService = context.RequestServices.GetRequiredService<WebSocketService>();
            await webSocketService.HandleWebSocketAsync(context);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next();
    }
});

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
