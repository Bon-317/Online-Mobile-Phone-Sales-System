using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using System.Text.Json;
using System.Text;
using System.Net.Http;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using FSTORE.Models;

namespace FSTORE.Controllers
{
    public class AuthController : Controller
    {
        private readonly FirestoreDb _db;
        private readonly ILogger<AuthController> _logger;
        private const string FirebaseApiKey = "AIzaSyD3Utv99Rner9iUOJe0l6GPKb8MN_eQ764";
        private const string AdminEmail = "th26.orange@gmail.com";

        public AuthController(FirestoreDb db, ILogger<AuthController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // ✅ Trang đăng nhập
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                var email = User.FindFirst(ClaimTypes.Email)?.Value;
                return email == AdminEmail
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "Product");
            }
            ViewBag.HideFooter = true;
            return View();
        }

        // ✅ Xử lý đăng nhập
        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ViewBag.Error = "❌ Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            try
            {
                var payload = new { email, password, returnSecureToken = true };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={FirebaseApiKey}",
                    content
                );

                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var firebaseError = GetFirebaseErrorMessage(result);
                    ViewBag.Error = $"❌ Đăng nhập thất bại: {firebaseError}";
                    return View();
                }

                var data = JsonSerializer.Deserialize<JsonElement>(result);
                string uid = data.GetProperty("localId").GetString();

                // 🔍 Lấy user trong Firestore
                var userDoc = await _db.Collection("Users").Document(uid).GetSnapshotAsync();
                if (!userDoc.Exists)
                {
                    ViewBag.Error = "❌ Không tìm thấy thông tin người dùng trong hệ thống.";
                    return View();
                }

                var user = userDoc.ConvertTo<User>();
                var displayName = string.IsNullOrWhiteSpace(user.name) ? "Account" : user.name;

                // 🔐 Tạo claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, uid), // ✅ chứa UID thật
                    new Claim(ClaimTypes.Name, displayName),
                    new Claim(ClaimTypes.Email, user.email ?? ""),
                    new Claim("uid", uid),
                    new Claim("role", user.role ?? "user")
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                // ✅ Lưu vào cookie
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                // ✅ Lưu thêm vào session (để VNPay / Profile dùng)
                HttpContext.Session.SetString("uid", uid);

                // ✅ Điều hướng
                return user.email == AdminEmail
                    ? RedirectToAction("Index", "Admin")
                    : RedirectToAction("Index", "Product");
            }
            catch (Exception ex)
            {
                ViewBag.Error = $"❌ Lỗi hệ thống: {ex.Message}";
                return View();
            }
        }

        // ✅ Đăng xuất
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }

        // ✅ Trang đăng ký
        [HttpGet]
        public IActionResult Signup()
        {
            ViewBag.HideFooter = true;
            return View();
        }

        // ✅ Xử lý đăng ký
        [HttpPost]
        public async Task<IActionResult> Signup(string name, string address, string phone, string email, string password, string otp)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(address) ||
                string.IsNullOrWhiteSpace(phone) || string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewBag.Error = "❌ Vui lòng nhập đầy đủ thông tin.";
                return View();
            }

            try
            {
                var payload = new { email, password, returnSecureToken = true };
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                using var client = new HttpClient();
                var response = await client.PostAsync(
                    $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={FirebaseApiKey}",
                    content
                );

                var result = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                {
                    var firebaseError = GetFirebaseErrorMessage(result);
                    ViewBag.Error = $"❌ Đăng ký thất bại: {firebaseError}";
                    return View();
                }

                var data = JsonSerializer.Deserialize<JsonElement>(result);
                string uid = data.GetProperty("localId").GetString();

                // ✅ Lưu người dùng vào Firestore (collection Users)
                var user = new User
                {
                    uid = uid,
                    email = email,
                    name = name,
                    address = address,
                    phone = phone,
                    role = "user",
                    imageUrl = ""
                };

                await _db.Collection("Users").Document(uid).SetAsync(user);

                ViewBag.Success = "✅ Đăng ký thành công! Bạn có thể đăng nhập ngay.";
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Signup failed for email {Email}", email);
                ViewBag.Error = $"❌ Lỗi hệ thống: {ex.Message}";
                return View();
            }
        }

        private static string GetFirebaseErrorMessage(string responseContent)
        {
            try
            {
                var data = JsonSerializer.Deserialize<JsonElement>(responseContent);
                var code = data.GetProperty("error").GetProperty("message").GetString();

                return code switch
                {
                    "EMAIL_EXISTS" => "Email đã tồn tại.",
                    "OPERATION_NOT_ALLOWED" => "Email/Password chưa được bật trong Firebase Authentication.",
                    "TOO_MANY_ATTEMPTS_TRY_LATER" => "Thao tác quá nhiều lần, vui lòng thử lại sau.",
                    "WEAK_PASSWORD : Password should be at least 6 characters" => "Mật khẩu phải có ít nhất 6 ký tự.",
                    "INVALID_EMAIL" => "Email không hợp lệ.",
                    _ => code ?? "Không xác định"
                };
            }
            catch
            {
                return "Không đọc được chi tiết lỗi từ Firebase.";
            }
        }

        // ✅ Trang quên mật khẩu
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            ViewBag.HideFooter = true;
            return View("~/Views/Auth/ForgotPassword.cshtml");
        }
    }
}
