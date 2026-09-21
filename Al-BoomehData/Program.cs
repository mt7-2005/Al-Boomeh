using Al_BoomehDAL.Data;
using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using Al_BoomehDAL.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var connectionString = configuration.GetConnectionString("DefaultConnection");

var services = new ServiceCollection();

services.AddScoped<IAuditScope, AuditScope>();
services.AddSingleton<AuditingSaveChangesInterceptor>();
// قبل AddDbContext
services.AddScoped<ICurrentUser, SystemCurrentUser>();

services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.CommandTimeout(180);
    });
    options.AddInterceptors(sp.GetRequiredService<AuditingSaveChangesInterceptor>());
});

var provider = services.BuildServiceProvider();

if (args.Contains("--reseed"))
{
    using var scope = provider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    var seeder = new DbSeeder(context);
    await seeder.RunAsync();

    Console.WriteLine("Reseed finished.");
    return;
}

Console.WriteLine("No --reseed flag provided. Nothing to do.");
Console.WriteLine("Usage: dotnet run -- --reseed");