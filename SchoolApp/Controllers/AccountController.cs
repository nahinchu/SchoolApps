using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using SchoolApp.DTOs;
using SchoolApp.Filters;
using SchoolApp.Models;
using SchoolApp.Services;
using SchoolApp.Services.HashPassService;
using SchoolApp.Services.MinioService;
using SchoolApp.UnitOfWork;
using System.Security.Claims;

namespace SchoolApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IPasswordService _passwordService;
        private readonly IEmailService _emailService;
        private readonly IMemoryCache _cache;
        private readonly IFileStorageService _storage;

        public AccountController(IUnitOfWork uow, IPasswordService passwordService, IEmailService emailService, IMemoryCache cache, IFileStorageService storage)
        {
            _uow = uow;
            _passwordService = passwordService;
            _emailService = emailService;
            _cache = cache;
            _storage = storage;
        }

        // ─── Helpers ─────────────────────────────────────────────────────────

        private string GenerateOtp() => Random.Shared.Next(100000, 999999).ToString();

        private void SetSession(User user)
        {
            HttpContext.Session.SetString("UserName", user.FullName ?? user.Email);
            HttpContext.Session.SetInt32("UserId", user.UserId);
            if (!string.IsNullOrEmpty(user.AvatarUrl))
                HttpContext.Session.SetString("AvatarUrl", user.AvatarUrl);
            else
                HttpContext.Session.Remove("AvatarUrl");
            string role = user.Role.ToLower() switch
            {
                "admin"   => "Admin",
                "manager" => "Manager",
                _         => "Student"
            };
            HttpContext.Session.SetString("Role", role);
        }

        private IActionResult RedirectAfterLogin(User user)
        {
            string role = user.Role.ToLower();
            if (role == "admin" || role == "manager")
                return RedirectToAction("Dashboard", "Admin");
            return RedirectToAction("Index", "Course");
        }

        // ─── Login ────────────────────────────────────────────────────────────

        public IActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = _uow.Users.GetByEmail(model.Email);

            if (user == null)
            {
                // Chạy bcrypt verify giả để tránh timing attack
                _passwordService.Verify(model.Password,
                    "$2a$12$abcdefghijklmnopqrstuvwxyz0123456789ABCDEFGHIJKLMNOPQRSTUV");
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                ModelState.AddModelError("", "Tài khoản này đăng nhập bằng Google. Vui lòng dùng nút 'Đăng nhập với Google'.");
                return View(model);
            }

            if (!_passwordService.Verify(model.Password, user.Password))
            {
                ModelState.AddModelError("", "Sai email hoặc mật khẩu");
                return View(model);
            }

            // Chưa xác thực email → gửi OTP và redirect sang trang xác thực
            if (!user.IsEmailVerified)
            {
                var otp = GenerateOtp();
                _cache.Set($"otp_register_{user.Email}", otp, TimeSpan.FromMinutes(10));
                _ = _emailService.SendOtpEmailAsync(user.Email, user.FullName ?? user.Email, otp, "register");
                TempData["Info"] = "Email chưa được xác thực. Chúng tôi đã gửi mã OTP mới về email của bạn.";
                return RedirectToAction("VerifyEmail", new { email = user.Email });
            }

            SetSession(user);
            TempData["Success"] = "Đăng nhập thành công!";
            return RedirectAfterLogin(user);
        }

        // ─── Register ─────────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult Register()
        {
            if (HttpContext.Session.GetString("Role") != null)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool emailExists = _uow.Users.GetAll()
                .Any(u => u.Email == model.Email.Trim().ToLower());

            if (emailExists)
            {
                ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                return View(model);
            }

            var user = new User
            {
                Email = model.Email.Trim().ToLower(),
                Password = _passwordService.Hash(model.Password),
                Role = "student",
                RegisteredDate = DateTime.Now,
                IsEmailVerified = false
            };

            _uow.Users.Add(user);
            _uow.SaveChanges();

            var otp = GenerateOtp();
            _cache.Set($"otp_register_{user.Email}", otp, TimeSpan.FromMinutes(10));
            _ = _emailService.SendOtpEmailAsync(user.Email, user.Email, otp, "register");

            TempData["Success"] = "Đăng ký thành công! Vui lòng kiểm tra email để lấy mã OTP xác thực.";
            return RedirectToAction("VerifyEmail", new { email = user.Email });
        }

        // ─── Verify Email OTP ────────────────────────────────────────────────

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("Login");
            return View(new VerifyEmailViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyEmail(VerifyEmailViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cacheKey = $"otp_register_{model.Email}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != model.Otp)
            {
                ModelState.AddModelError("Otp", "Mã OTP không đúng hoặc đã hết hạn.");
                return View(model);
            }

            var user = _uow.Users.GetByEmail(model.Email);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Login");
            }

            user.IsEmailVerified = true;
            _uow.SaveChanges();
            _cache.Remove(cacheKey);

            SetSession(user);
            TempData["Success"] = "Xác thực email thành công! Chào mừng bạn đến với SchoolApp.";
            return RedirectAfterLogin(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResendOtp(string email, string purpose = "register")
        {
            var user = _uow.Users.GetByEmail(email);
            if (user != null)
            {
                var otp = GenerateOtp();
                _cache.Set($"otp_{purpose}_{email}", otp, TimeSpan.FromMinutes(10));
                _ = _emailService.SendOtpEmailAsync(email, user.FullName ?? email, otp, purpose);
            }
            TempData["Success"] = "Đã gửi lại mã OTP. Vui lòng kiểm tra email.";
            if (purpose == "forgot")
                return RedirectToAction("VerifyForgotOtp", new { email });
            return RedirectToAction("VerifyEmail", new { email });
        }

        // ─── Logout ───────────────────────────────────────────────────────────

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ─── Edit Profile ─────────────────────────────────────────────────────

        [HttpGet]
        [AuthorizeUser]
        public IActionResult EditProfile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _uow.Users.GetById(userId!.Value);
            if (user == null) return NotFound();

            var model = new EditProfileViewModel
            {
                FullName = user.FullName,
                Phone = user.Phone,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                AvatarUrl = user.AvatarUrl
            };
            return View(model);
        }

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public IActionResult EditProfile(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _uow.Users.GetById(userId!.Value);
            if (user == null) return NotFound();

            user.FullName = model.FullName?.Trim();
            user.Phone = model.Phone?.Trim();
            user.DateOfBirth = model.DateOfBirth;
            user.Address = model.Address?.Trim();

            _uow.SaveChanges();

            HttpContext.Session.SetString("UserName", user.FullName ?? user.Email);

            TempData["Success"] = "Cập nhật thông tin thành công!";
            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public IActionResult ChangePassword(ChangePasswordViewModel model)
        {
            TempData["ActiveTab"] = "password";

            if (!ModelState.IsValid)
            {
                var errors = ModelState
                    .Where(x => x.Value!.Errors.Count > 0)
                    .ToDictionary(k => k.Key, v => v.Value!.Errors[0].ErrorMessage);
                TempData["PwdErrors"] = System.Text.Json.JsonSerializer.Serialize(errors);
                return RedirectToAction("EditProfile");
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _uow.Users.GetById(userId!.Value);
            if (user == null) return NotFound();

            if (string.IsNullOrEmpty(user.Password))
            {
                TempData["PwdErrors"] = System.Text.Json.JsonSerializer.Serialize(
                    new Dictionary<string, string> { { "", "Tài khoản đăng nhập bằng Google không thể đổi mật khẩu." } });
                return RedirectToAction("EditProfile");
            }

            if (!_passwordService.Verify(model.CurrentPassword, user.Password))
            {
                TempData["PwdErrors"] = System.Text.Json.JsonSerializer.Serialize(
                    new Dictionary<string, string> { { "CurrentPassword", "Mật khẩu hiện tại không đúng" } });
                return RedirectToAction("EditProfile");
            }

            user.Password = _passwordService.Hash(model.NewPassword);
            _uow.SaveChanges();

            TempData["ActiveTab"] = null;
            TempData["Success"] = "Đổi mật khẩu thành công!";
            return RedirectToAction("EditProfile");
        }

        // ─── Forgot Password ──────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ForgotPassword() => View(new ForgotPasswordViewModel());

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            const string safeMsg = "Nếu email tồn tại trong hệ thống, chúng tôi đã gửi mã OTP về email của bạn.";

            if (!ModelState.IsValid)
                return View(model);

            var user = _uow.Users.GetByEmail(model.Email);

            if (user != null && !string.IsNullOrEmpty(user.Password))
            {
                var otp = GenerateOtp();
                _cache.Set($"otp_forgot_{user.Email}", otp, TimeSpan.FromMinutes(10));
                _ = _emailService.SendOtpEmailAsync(user.Email, user.FullName ?? user.Email, otp, "forgot");
            }

            TempData["Success"] = safeMsg;
            return RedirectToAction("VerifyForgotOtp", new { email = model.Email });
        }

        // ─── Verify Forgot OTP ────────────────────────────────────────────────

        [HttpGet]
        public IActionResult VerifyForgotOtp(string email)
        {
            if (string.IsNullOrEmpty(email))
                return RedirectToAction("ForgotPassword");
            return View(new VerifyForgotOtpViewModel { Email = email });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult VerifyForgotOtp(VerifyForgotOtpViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var cacheKey = $"otp_forgot_{model.Email}";
            if (!_cache.TryGetValue(cacheKey, out string? storedOtp) || storedOtp != model.Otp)
            {
                ModelState.AddModelError("Otp", "Mã OTP không đúng hoặc đã hết hạn.");
                return View(model);
            }

            _cache.Remove(cacheKey);
            HttpContext.Session.SetString("ResetEmail", model.Email);
            return RedirectToAction("ResetPassword");
        }

        // ─── Reset Password ───────────────────────────────────────────────────

        [HttpGet]
        public IActionResult ResetPassword()
        {
            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên đặt lại mật khẩu không hợp lệ. Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }
            return View(new ResetPasswordViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = HttpContext.Session.GetString("ResetEmail");
            if (string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Phiên đặt lại mật khẩu không hợp lệ. Vui lòng thử lại.";
                return RedirectToAction("ForgotPassword");
            }

            var user = _uow.Users.GetByEmail(email);
            if (user == null)
            {
                TempData["Error"] = "Tài khoản không tồn tại.";
                return RedirectToAction("Login");
            }

            user.Password = _passwordService.Hash(model.NewPassword);
            _uow.SaveChanges();

            HttpContext.Session.Remove("ResetEmail");
            TempData["Success"] = "Đặt lại mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login");
        }

        // ─── Upload Avatar ────────────────────────────────────────────────────

        [HttpPost]
        [AuthorizeUser]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            var user = _uow.Users.GetById(userId!.Value);
            if (user == null) return Json(new { success = false, message = "Người dùng không tồn tại" });

            if (avatar == null || avatar.Length == 0)
                return Json(new { success = false, message = "Vui lòng chọn ảnh" });

            if (!string.IsNullOrEmpty(user.AvatarUrl))
                await _storage.DeleteAsync(user.AvatarUrl);

            var url = await _storage.UploadAvatarAsync(avatar);
            if (url == null)
                return Json(new { success = false, message = "Ảnh không hợp lệ (PNG/JPG/GIF/WEBP, tối đa 5MB)" });

            user.AvatarUrl = url;
            _uow.SaveChanges();
            HttpContext.Session.SetString("AvatarUrl", url);

            return Json(new { success = true, avatarUrl = url });
        }

        // ─── Google OAuth ─────────────────────────────────────────────────────

        public IActionResult GoogleLogin()
        {
            var redirectUrl = Url.Action("GoogleCallback", "Account");
            var properties = new AuthenticationProperties { RedirectUri = redirectUrl };
            return Challenge(properties, GoogleDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> GoogleCallback()
        {
            var result = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            if (!result.Succeeded)
            {
                TempData["Error"] = "Đăng nhập Google thất bại. Vui lòng thử lại.";
                return RedirectToAction("Login");
            }

            var claims = result.Principal!.Claims.ToList();
            var googleId = claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (string.IsNullOrEmpty(googleId) || string.IsNullOrEmpty(email))
            {
                TempData["Error"] = "Không lấy được thông tin từ Google.";
                return RedirectToAction("Login");
            }

            // Tìm theo GoogleId trước
            var user = _uow.Users.GetByGoogleId(googleId);

            if (user == null)
            {
                var existingByEmail = _uow.Users.GetByEmail(email);

                if (existingByEmail != null)
                {
                    // Email đã tồn tại → link GoogleId và login
                    if (string.IsNullOrEmpty(existingByEmail.GoogleId))
                        existingByEmail.GoogleId = googleId;
                    existingByEmail.IsEmailVerified = true;
                    _uow.SaveChanges();
                    user = existingByEmail;
                }
                else
                {
                    // Lần đầu login Google → tạo account mới
                    user = new User
                    {
                        FullName = name ?? email,
                        Email = email.Trim().ToLower(),
                        GoogleId = googleId,
                        Role = "student",
                        RegisteredDate = DateTime.Now,
                        IsEmailVerified = true
                    };
                    _uow.Users.Add(user);
                    _uow.SaveChanges();
                }
            }

            SetSession(user);
            TempData["Success"] = "Đăng nhập Google thành công!";
            return RedirectAfterLogin(user);
        }
    }
}
