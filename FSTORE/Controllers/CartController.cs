using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;
using FSTORE.Services;

namespace FSTORE.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.Identity.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");
            var cart = await _cartService.GetCartAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(string productId, string name, double price, string imageUrl, int quantity = 1)
        {
            var userId = User.Identity.Name;
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Auth");

            var item = new CartItem
            {
                ProductId = productId,
                Name = name,
                Price = price,
                ImageUrl = imageUrl,
                Quantity = quantity,
                Selected = true
            };
            await _cartService.AddToCartAsync(userId, item);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(string productId, int quantity)
        {
            var userId = User.Identity.Name;
            await _cartService.UpdateQuantityAsync(userId, productId, quantity);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Remove(string productId)
        {
            var userId = User.Identity.Name;
            await _cartService.RemoveItemAsync(userId, productId);
            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Clear()
        {
            var userId = User.Identity.Name;
            await _cartService.ClearCartAsync(userId);
            return RedirectToAction("Index");
        }

        // ==================== VOUCHER MỚI (THÊM 2 ACTION NÀY) ====================
        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string voucherCode)
        {
            if (string.IsNullOrWhiteSpace(voucherCode))
                return Json(new { success = false, message = "Vui lòng nhập mã voucher!" });

            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId))
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            voucherCode = voucherCode.Trim().ToUpper();
            var result = await _cartService.ApplyVoucherAsync(userId, voucherCode);

            if (result.Success)
            {
                HttpContext.Session.SetString("AppliedVoucherCode", voucherCode);
                HttpContext.Session.SetInt32("VoucherDiscount", (int)result.DiscountAmount);

                return Json(new
                {
                    success = true,
                    message = $"Áp dụng mã {voucherCode} thành công! Giảm {result.DiscountAmount:N0}₫",
                    discountAmount = result.DiscountAmount
                });
            }
            else
            {
                HttpContext.Session.Remove("AppliedVoucherCode");
                HttpContext.Session.Remove("VoucherDiscount");
                return Json(new { success = false, message = result.Message });
            }
        }

        [HttpPost]
        public IActionResult RemoveVoucher()
        {
            HttpContext.Session.Remove("AppliedVoucherCode");
            HttpContext.Session.Remove("VoucherDiscount");
            return Json(new { success = true });
        }
   
    }
}