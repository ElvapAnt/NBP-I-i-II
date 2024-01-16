using Microsoft.Extensions.Options;
using Neo4j.Driver;
using SocialNetworkApp.Server.Repos;
using SocialNetworkApp.Server.Settings;
using SocialNetworkApp.Services;

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

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");

app.Run();
