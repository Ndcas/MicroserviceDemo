using DotPulsar;
using DotPulsar.Abstractions;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.IdentityModel.Tokens.Jwt;
using user.Database;
using user.Pulsar;
using user.Services;

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

var builder = WebApplication.CreateBuilder(args);

string redisConnectionString = builder.Configuration.GetConnectionString("Redis")!;
string pulsarConnectionString = builder.Configuration.GetConnectionString("Pulsar")!;
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddControllers();

builder.Services.AddSingleton<IConnectionMultiplexer>(sp => ConnectionMultiplexer.Connect(redisConnectionString));

builder.Services.AddSingleton<IPulsarClient>(sp =>
{
    return PulsarClient.Builder().ServiceUrl(new Uri(pulsarConnectionString)).Build();
});

builder.Services.AddSingleton<ILogProducer, LogProducer>();

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(connectionString,ServerVersion.AutoDetect(connectionString)));

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<IAccountRepository, AccountRepository>();

builder.Services.AddScoped<IAccountService, AccountService>();

builder.Services.AddScoped<ICacheService, CacheService>();

builder.Services.AddScoped<ITokenService, TokenService>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
