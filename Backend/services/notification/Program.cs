using Microsoft.EntityFrameworkCore;
using notification.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

String connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

builder.Services.AddDbContext<AppDbContext>(options => options.UseMySql(
    connectionString,
    ServerVersion.AutoDetect(connectionString)
));

var app = builder.Build();

app.UseAuthorization();

app.MapControllers();

app.Run();
