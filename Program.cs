using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SalesOrderApp.Data;
using SalesOrderApp.Interfaces;
using SalesOrderApp.Models;
using SalesOrderApp.Repositories;
using SalesOrderApp.Services;
using System.Text;
using System.Text.Json.Serialization;

namespace SalesOrderApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddHttpContextAccessor();

            // Avoid circular reference issues
            builder.Services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            // Add Database contexts
            builder.Services.AddDbContext<SalesOrderAppDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("SalesOrderAppDb")));
            builder.Services.AddSingleton<XmlDbContext>(provider =>
                new XmlDbContext(
                    builder.Configuration["XmlDbFilePaths:OrderHeaders"],
                    builder.Configuration["XmlDbFilePaths:OrderLines"],
                    builder.Configuration["XmlDbFilePaths:OrderStatus"],
                    builder.Configuration["XmlDbFilePaths:OrderType"],
                    builder.Configuration["XmlDbFilePaths:ProductType"],
                    builder.Configuration["XmlDbFilePaths:SalesOrders"],
                    builder.Configuration["XmlDbFilePaths:Users"],
                    builder.Configuration["XmlDbFilePaths:UserRoles"]
                ));

            // Add Repositories
            builder.Services.AddScoped<UserRepository>();
            builder.Services.AddScoped<XmlUserRepository>();
            builder.Services.AddScoped<SalesOrderRepository>();

            // Register the additional services
            builder.Services.AddScoped<IDbTransactionService, DbTransactionService>();
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Configure Cookie authentication
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Auth/Login"; // Path to login page
                    options.LogoutPath = "/Auth/Logout"; // Path to logout page
                    options.AccessDeniedPath = "/Auth/AccessDenied"; // Path to access denied page
                });

            // Add authorization policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(nameof(UserRoleEnum.Admin), policy => policy.RequireRole(nameof(UserRoleEnum.Admin)));
                options.AddPolicy(nameof(UserRoleEnum.Guest), policy => policy.RequireRole(nameof(UserRoleEnum.Guest)));
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Orders}/{action=Index}/{id?}");

            app.Run();
        }
    }
}