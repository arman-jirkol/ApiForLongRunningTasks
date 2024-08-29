using ApiForLongRunningTasks;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath);
});

ConfigureAdditionalServices();

var app = builder.Build();

app.UseHangfireDashboard();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

void ConfigureAdditionalServices()
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    var services = builder.Services;

    services.AddDbContext<StoreDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Add Hangfire
    services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
    services.AddHangfireServer();

    // Set the license context for openXml
    ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
}