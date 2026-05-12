using Microsoft.EntityFrameworkCore;
using SchoolApp.Data;
using SchoolApp.Services;
using SchoolApp.UnitOfWork;

namespace SchoolApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStringDB")));
            builder.Services.AddScoped<IUnitOfWork, SchoolApp.UnitOfWork.UnitOfWork>();
            builder.Services.AddSession();

            builder.Services.AddSingleton<IPasswordService, BCryptPasswordService>();

            builder.Services.AddHttpClient<PayOSService>();

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.Use(async (context, next) => {
                context.Request.EnableBuffering();
                await next();
            });
            app.UseSession();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
