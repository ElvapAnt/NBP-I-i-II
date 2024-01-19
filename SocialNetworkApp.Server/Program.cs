using Microsoft.Extensions.Options;
using Neo4j.Driver;
using SocialNetworkApp.Server.Business.Repos;
using SocialNetworkApp.Server.Settings;
using SocialNetworkApp.Server.Business.Services;
using SocialNetworkApp.Server.Error;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<RedisSettings>(
    builder.Configuration.GetSection("RedisSettings"));
builder.Services.AddSingleton<IRedisSettings>(
    sp=>sp.GetRequiredService<IOptions<RedisSettings>>().Value);

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

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
