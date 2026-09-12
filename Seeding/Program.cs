using Al_BoomehDAL;
using Al_BoomehDAL.Models;
using Al_BoomehDAL.Seeding;
using Al_BoomehDAL.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .Build();
var connectionString = configuration.GetConnectionString("DefaultConnection");
var services = new ServiceCollection();
services.AddSingleton<IAuditSuppressor, AuditSuppressor>();
services.AddSingleton<AuditingSaveChangesInterceptor>();
services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(connectionString);
    options.AddInterceptors(sp.GetRequiredService<AuditingSaveChangesInterceptor>());
});
var provider = services.BuildServiceProvider();
if (args.Contains("--reseed"))
{
    using var scope = provider.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var suppressor = scope.ServiceProvider.GetRequiredService<IAuditSuppressor>();
    var seeder = new DbSeeder(context, suppressor);
    await seeder.RunAsync();
    Console.WriteLine("Reseed finished.");
    return;
}
Console.WriteLine("No --reseed flag provided. Nothing to do.");
Console.WriteLine("Usage: dotnet run -- --reseed");