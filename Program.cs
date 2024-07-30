using Microsoft.AspNetCore.Authentication.JwtBearer;
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
                    builder.Configuration["XmlFilePaths:Users"],
                    builder.Configuration["XmlFilePaths:UserRoles"]
                ));

            // Add Repositories
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<IUserRepository, XmlUserRepository>();

            // Register the additional services
            builder.Services.AddScoped<IDbTransactionService, DbTransactionService>();

            // Configure JWT authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = builder.Configuration["Jwt:Issuer"],
                        ValidAudience = builder.Configuration["Jwt:Issuer"],
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
                    };
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
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}