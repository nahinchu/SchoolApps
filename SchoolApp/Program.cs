using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Minio;
using SchoolApp.Data;
using SchoolApp.Services;
using SchoolApp.Services.CloudinaryService;
using SchoolApp.Services.EmailService;
using SchoolApp.Services.HashPassService;
using SchoolApp.Services.MinioService;
using SchoolApp.Services.PayosService;
using SchoolApp.Services.ExcelExportService;
using SchoolApp.Services.CompletionService;
using SchoolApp.UnitOfWork;
namespace SchoolApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Cho phép upload file lớn tối đa 500MB
            builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 500 * 1024 * 1024);
            builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 500 * 1024 * 1024);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStringDB")));
            builder.Services.AddScoped<IUnitOfWork, SchoolApp.UnitOfWork.UnitOfWork>();
            builder.Services.AddSession();

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ICourseCompletionService, CourseCompletionService>();
            builder.Services.AddSingleton<IExcelExportService, ExcelExportService>();
            builder.Services.AddSingleton<IExcelImportService, ExcelImportService>();
            builder.Services.AddSingleton<IPasswordService, BCryptPasswordService>();
            builder.Services.AddSingleton<ICloudinaryService, CloudinaryService>();
            builder.Services.AddHttpClient<PayOSService>();
            builder.Services.AddSingleton<IMinioClient>(sp =>
            {
                var config = builder.Configuration.GetSection("MinIO");
                return new MinioClient()
                    .WithEndpoint(config["Endpoint"])
                    .WithCredentials(config["AccessKey"], config["SecretKey"])
                    .WithSSL(bool.Parse(config["UseSSL"] ?? "false"))
                    .Build();
            });
            builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();

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