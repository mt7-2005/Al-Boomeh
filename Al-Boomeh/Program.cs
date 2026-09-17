using Al_Boomeh.Api.Middleware;
using Al_Boomeh.Authorization;
using Al_Boomeh.Services;
using Al_BoomehAPI.Middleware;
using Al_BoomehDAL.Classes;
using Al_BoomehDAL.Data;
using Al_BoomehDAL.Interfaces;
using Al_BoomehDAL.Models;
using Al_BoomehServices;
using Al_BoomehServices.Interfaces;
using Al_BoomehServices.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using FluentValidation;
using System.Threading.RateLimiting;
using Al_BoomehServices.Validators;
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
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditScope, AuditScope>();
builder.Services.AddValidatorsFromAssemblyContaining<AppValidators>();
builder.Services.AddScoped<ICurrentUser, CurrentUserService>();
builder.Services.AddScoped<AuditingSaveChangesInterceptor>();
builder.Services.AddScoped<ISmsSender, SmsSender>();

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

builder.Services.AddScoped<ICategoriesService, CategoryService>();
builder.Services.AddScoped<IUsersService, UserService>();
builder.Services.AddScoped<ICustomersService, CustomerService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOtpService, OtpService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IDriversService, DriversService>();
builder.Services.AddScoped<IExtrasService, ProductOptionService>();
builder.Services.AddScoped<IProductsService, ProductService>();
builder.Services.AddScoped<IStoresService, StoreService>();
builder.Services.AddScoped<IVouchersService, VoucherService>();
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
app.MapControllers();

try
{
    app.Run();
}
finally
{
    Log.CloseAndFlush();
}