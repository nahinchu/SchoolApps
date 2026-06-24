using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Minio;
using QuestPDF.Infrastructure;
using SchoolApp.Data;
using SchoolApp.Services;
using SchoolApp.Services.CertificateService;
using SchoolApp.Services.CloudinaryService;
using SchoolApp.Services.CompletionService;
using SchoolApp.Services.EmailService;
using SchoolApp.Services.ExcelExportService;
using SchoolApp.Services.HashPassService;
using SchoolApp.Services.MinioService;
using SchoolApp.Services.NotificationService;
using SchoolApp.Services.PayosService;
using SchoolApp.UnitOfWork;
using SchoolApp.Hubs;
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
            builder.Services.AddLocalization(options => options.ResourcesPath = "");
            builder.Services.AddControllersWithViews()
                .AddViewLocalization()
                .AddDataAnnotationsLocalization();

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
            builder.Services.AddSignalR();

            builder.Services.AddHttpClient<PayOSService>();
            builder.Services.AddSingleton<IMinioClient>(sp =>
            {
                var config = builder.Configuration.GetSection("MinIO");
                // Local chưa cấu hình MinIO: dùng credential tạm để client khởi tạo
                // được (các trang không upload vẫn chạy). Khi thực sự gọi MinIO mới
                // cần creds thật.
                var endpoint = config["Endpoint"];
                if (string.IsNullOrWhiteSpace(endpoint)) endpoint = "localhost:9000";
                var accessKey = config["AccessKey"];
                var secretKey = config["SecretKey"];
                if (string.IsNullOrWhiteSpace(accessKey)) accessKey = "minioadmin";
                if (string.IsNullOrWhiteSpace(secretKey)) secretKey = "minioadmin";
                return new MinioClient()
                    .WithEndpoint(endpoint)
                    .WithCredentials(accessKey, secretKey)
                    .WithSSL(bool.Parse(config["UseSSL"] ?? "false"))
                    .Build();
            });
            builder.Services.AddScoped<IFileStorageService, MinioFileStorageService>();

            var authBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

            // Chỉ đăng ký Google OAuth khi có ClientId/Secret. Nếu để trống (vd: chạy
            // local chưa cấu hình), bỏ qua thay vì để middleware validate ném lỗi 500
            // ở mọi request.
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            if (!string.IsNullOrWhiteSpace(googleClientId) && !string.IsNullOrWhiteSpace(googleClientSecret))
            {
                authBuilder.AddGoogle(options =>
                {
                    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.ClientId = googleClientId;
                    options.ClientSecret = googleClientSecret;
                    options.CallbackPath = "/signin-google";

                    // Fix "Correlation failed": cookie phải được gửi lại khi Google redirect về
                    // SameAsRequest: không ép HTTPS trong dev (HTTP vẫn hoạt động)
                    // Lax: cho phép gửi cookie trên top-level cross-site redirect (OAuth flow)
                    options.CorrelationCookie.SameSite = SameSiteMode.Lax;
                    options.CorrelationCookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.CorrelationCookie.HttpOnly = true;

                    options.Events.OnRemoteFailure = ctx =>
                    {
                        ctx.Response.Redirect("/Account/Login");
                        ctx.HandleResponse();
                        return Task.CompletedTask;
                    };
                });
            }

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

            var supportedCultures = new[] { "vi-VN", "en-GB" };
            app.UseRequestLocalization(new RequestLocalizationOptions()
                .SetDefaultCulture("vi-VN")
                .AddSupportedCultures(supportedCultures)
                .AddSupportedUICultures(supportedCultures));

            app.UseHttpsRedirection();

            // Phục vụ file Unity WebGL. Build dùng nén Brotli (.br), nên phải:
            //  - đăng ký MIME cho .br/.wasm/.data (nếu không UseStaticFiles trả 404)
            //  - gửi header Content-Encoding: br + Content-Type đúng của file gốc
            //    để trình duyệt tự giải nén.
            var unityProvider = new FileExtensionContentTypeProvider();
            unityProvider.Mappings[".wasm"] = "application/wasm";
            unityProvider.Mappings[".data"] = "application/octet-stream";
            unityProvider.Mappings[".symbols.json"] = "application/octet-stream";
            unityProvider.Mappings[".br"] = "application/octet-stream";
            unityProvider.Mappings[".gz"] = "application/octet-stream";
            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = unityProvider,
                OnPrepareResponse = ctx =>
                {
                    var name = ctx.File.Name;
                    var headers = ctx.Context.Response.Headers;
                    if (name.EndsWith(".br"))
                    {
                        headers["Content-Encoding"] = "br";
                        ctx.Context.Response.ContentType = UnityContentType(name);
                    }
                    else if (name.EndsWith(".gz"))
                    {
                        headers["Content-Encoding"] = "gzip";
                        ctx.Context.Response.ContentType = UnityContentType(name);
                    }
                }
            });
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
            app.MapHub<ChessHub>("/chess-hub");
            app.Run();
        }

        // Trả về Content-Type GỐC của file Unity đã nén (.wasm.br -> wasm...).
        private static string UnityContentType(string fileName)
        {
            if (fileName.Contains(".wasm")) return "application/wasm";
            if (fileName.Contains(".js")) return "application/javascript";
            if (fileName.Contains(".data")) return "application/octet-stream";
            if (fileName.Contains(".symbols.json")) return "application/octet-stream";
            return "application/octet-stream";
        }
    }
}