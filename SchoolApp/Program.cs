using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
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
using SchoolApp.Services.CertificateService;
using SchoolApp.Services.NotificationService;
using SchoolApp.UnitOfWork;
using QuestPDF.Infrastructure;
namespace SchoolApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var builder = WebApplication.CreateBuilder(args);

            // Cho phép upload file lớn tối đa 500MB
            builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 500 * 1024 * 1024);
            builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 500 * 1024 * 1024);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStringDB")));
            builder.Services.AddScoped<IUnitOfWork, SchoolApp.UnitOfWork.UnitOfWork>();
            builder.Services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/app/keys"));

            builder.Services.AddSession();
            builder.Services.AddMemoryCache();

            builder.Services.AddScoped<IEmailService, EmailService>();
            builder.Services.AddScoped<ICertificateService, CertificateService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
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

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddGoogle(options =>
            {
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
                options.CallbackPath = "/signin-google";

                // Fix "Correlation failed": cookie phải được gửi lại khi Google redirect về
                // SameAsRequest: không ép HTTPS trong dev (HTTP vẫn hoạt động)
                // Lax: cho phép gửi cookie trên top-level cross-site redirect (OAuth flow)
                options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.CorrelationCookie.HttpOnly = true;
            });

            var app = builder.Build();


            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            var forwardedOptions = new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            };
            forwardedOptions.KnownNetworks.Clear();
            forwardedOptions.KnownProxies.Clear();
            app.UseForwardedHeaders(forwardedOptions);

            app.Use(async (context, next) => {
                context.Request.EnableBuffering();
                await next();
            });

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            // Phải đứng trước UseAuthentication để SameSite policy áp dụng
            // cho correlation cookie trước khi middleware OAuth xử lý
            app.UseCookiePolicy(new CookiePolicyOptions
            {
                MinimumSameSitePolicy = SameSiteMode.Lax,
                Secure = CookieSecurePolicy.SameAsRequest
            });
            // Auto-migrate database on startup
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<SchoolApp.Data.AppDbContext>();
                context.Database.Migrate();
            }

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}