using Al_Boomeh.Api.Middleware;
using Al_Boomeh.Authorization;
using Al_Boomeh.Services;
using Al_BoomehAPI.Middleware;
using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Data;
using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using Al_BoomehDAL.Seeding;
using Al_BoomehServices;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Jobs;
using Al_BoomehServices.Services;
using Al_BoomehServices.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Net;
using System.Text;
using System.Threading.RateLimiting;
using static Al_Boomeh.Controllers.OrdersController;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "Logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14)
    .CreateLogger();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,


            ValidateAudience = true,


            ValidateLifetime = true,


            ValidateIssuerSigningKey = true,


            ValidIssuer = "Al-BoomehAPI",


            ValidAudience = "Al-BoomehAPIUsers",


            
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });
builder.Services.AddScoped<IAuthorizationHandler, StoreOwnerOrAdminHandler>();
builder.Services.AddScoped<IAuthorizationHandler, CustomerOwnerOrAdminHandler>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("StoreOwnerOrAdmin", policy =>
        policy.Requirements.Add(new StoreOwnerOrAdminRequirement()));
    options.AddPolicy("CustomerOwnerOrAdmin", policy =>
        policy.Requirements.Add(new CustomerOwnerOrAdminRequirement()));
});
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("AuthLimiter", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ip,
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            });
    });
});

builder.Services.AddSwaggerGen(options =>
{
   
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",


        Type = SecuritySchemeType.Http,


       
        Scheme = "Bearer",


        BearerFormat = "JWT",


        In = ParameterLocation.Header,


        Description = "Enter: Bearer {your JWT token}"
    });


   
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },


            
            new string[] {}
        }
    });
});


builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditScope, AuditScope>();
builder.Services.AddValidatorsFromAssemblyContaining<AppValidators>();
builder.Services.AddScoped<AuditingSaveChangesInterceptor>();
builder.Services.AddScoped<ISmsSender, SmsSender>();
builder.Services.AddScoped<ISendOTP,OtpService>();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddKeyedScoped<ICurrentUser, SystemCurrentUser>("system");
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddHostedService<OrdersCountBackService>();


builder.Services.AddDbContext<AppDbContext>((sp, options) =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.AddInterceptors(sp.GetRequiredService<AuditingSaveChangesInterceptor>());

    if (builder.Environment.IsDevelopment())
    {
        options.LogTo(Console.WriteLine, LogLevel.Information)
               .EnableSensitiveDataLogging();
    }
});

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddHangfireServer();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICancelAbandonedOrders, OrderService>();
builder.Services.AddScoped<IDailyReports, StoreService>();
builder.Services.AddScoped<ICreateDailyReport, StoreService>();
builder.Services.AddScoped<INotificationEngine, StoreService>();
builder.Services.AddScoped<ITokenCleanup, RefreshTokenService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IDriverService, DriversService>();
builder.Services.AddScoped<IProductOptionService, ProductOptionService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IStoreService, StoreService>();
builder.Services.AddScoped<IVoucherService, VoucherService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.Services.GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<ICancelAbandonedOrders>("cancel-abandoned-orders", x => x.CancelAbandonedOrders(), Cron.MinuteInterval(30));

app.Services.GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<ITokenCleanup>("token-cleanup", x => x.TokenCleanup(), "0 3 * * *");


 app.Services.GetRequiredService<IRecurringJobManager>()
    .AddOrUpdate<IDailyReports>("daily-report", x => x.DailyReports(), "0 2 * * *");

app.MapControllers();


try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}