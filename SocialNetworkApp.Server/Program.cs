using Microsoft.Extensions.Options;
using SocialNetworkApp.Server.Settings;

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
