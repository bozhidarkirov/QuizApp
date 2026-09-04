using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using QuizApp.Core.Interfaces;
using QuizApp.Core.Models;
using QuizApp.Infrastructure.Data;
using QuizApp.Infrastructure.Services;
using QuizApp.Web.Services;
using QuizApp.Web.Hubs;

namespace QuizApp.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddRazorPages();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // ---- Database ----
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString));

            // ---- Identity (роли: Admin, Teacher, Student) ----
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedAccount = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // ---- SignalR (за live режим) ----
            builder.Services.AddSignalR();

            builder.Services.AddScoped<ICategoryService, CategoryService>();
            builder.Services.AddScoped<IQuizService, QuizService>();
            builder.Services.AddScoped<IQuestionService, QuestionService>();
            builder.Services.AddSingleton<Microsoft.AspNetCore.Identity.UI.Services.IEmailSender, NoOpEmailSender>();
            builder.Services.AddScoped<IQuizSessionService, QuizSessionService>();
            builder.Services.AddScoped<ISelfPacedService, SelfPacedService>();

            var app = builder.Build();

            // ---- Seed роли и Admin потребител ----
            using (var scope = app.Services.CreateScope())
            {
                await QuizApp.Infrastructure.Data.SeedData.SeedRolesAndAdminAsync(scope.ServiceProvider);
                await QuizApp.Infrastructure.Data.SeedData.SeedCategoriesAsync(scope.ServiceProvider);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStatusCodePagesWithReExecute("/Home/StatusCode/{0}");
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();
            app.MapRazorPages();
            app.MapHub<QuizHub>("/quizhub");

            app.Run();
        }
    }
}