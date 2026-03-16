using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;
using FSTORE.Services;
using System;
using System.Threading.Tasks;

namespace FSTORE.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly ProfileService _profileService;

        public ProfileController(ProfileService profileService)
        {
            _profileService = profileService;
        }

        // ✅ Trang hiển thị thông tin cá nhân
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirst("uid")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _profileService.GetProfileAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Login", "Auth");
            }

            return View(user);
        }

        // ✅ Trang chỉnh sửa thông tin cá nhân
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var userId = User.FindFirst("uid")?.Value ?? User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var user = await _profileService.GetProfileAsync(userId);
            if (user == null)
            {
                TempData["Error"] = "Không thể tải hồ sơ người dùng.";
                return RedirectToAction("Index");
            }

            return View(user);
        }

        // ✅ Lưu thay đổi hồ sơ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UserProfile model)
        {
            var uid = User.FindFirst("uid")?.Value ?? "";

            if (string.IsNullOrEmpty(uid))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Login", "Auth");
            }

            // Gán lại UID cho chắc
            model.Uid = uid;

            // Gọi service để cập nhật dữ liệu
            var success = await _profileService.UpdateProfileAsync(uid, model);

            if (success)
            {
                TempData["Message"] = "✅ Cập nhật thông tin thành công!";
                // 👉 Sau khi lưu thành công: chuyển về trang Index
                return RedirectToAction("Index");
            }
            else
            {
                TempData["Error"] = "❌ Cập nhật thất bại. Vui lòng thử lại.";
                // 👉 Nếu thất bại: ở lại trang Edit
                return View("Edit", model);
            }
        }
    }
}
