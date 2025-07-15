using BirthdayApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using BirthdayApp.Services;

namespace BirthdayApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddAuthentication("MyAuth")
                .AddCookie("MyAuth", options =>
                {
                    options.LoginPath = "/UserList/Login";
                    options.LogoutPath = "/UserList/Logout";
                });
            builder.Services.AddAuthorization();
            builder.Services.AddDbContext<BirthdayContext>(options=>options.UseSqlServer(builder.Configuration.GetConnectionString("Conn1")));
            builder.Services.AddControllersWithViews();
            
            // Register email service
            builder.Services.AddScoped<IEmailService, EmailService>();
            
            var app = builder.Build();
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllerRoute(name:"myRoute",pattern:"{controller=UserList}/{action=Landing}/{id?}");

            app.Run();
        }
    }
}
