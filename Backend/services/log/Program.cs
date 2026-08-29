using DotPulsar;
using DotPulsar.Abstractions;
using log.Database;
using log.Pulsar;
using log.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
string pulsarConnectionString = builder.Configuration.GetConnectionString("Pulsar")!;

builder.Services.AddDbContext<AppDbContext>(option => option.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddSingleton<IPulsarClient>(sp =>
{
    return PulsarClient.Builder().ServiceUrl(new Uri(pulsarConnectionString)).Build();
});

builder.Services.AddHostedService<LogConsumer>();

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

builder.Services.AddScoped<ILogRepository, LogRepository>();

builder.Services.AddScoped<ILogService, LogService>();

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
