using FSTORE.Models.Vnpay;
using FSTORE.Services;
using FSTORE.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;
using System.Security.Claims;

namespace FSTORE.Controllers
{
    public class PaymentController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly ProfileService _profileService;
        private readonly OrderService _orderService;
        private readonly CartService _cartService;

        public PaymentController(
            IVnPayService vnPayService,
            ProfileService profileService,
            OrderService orderService,
            CartService cartService)
        {
            _vnPayService = vnPayService;
            _profileService = profileService;
            _orderService = orderService;
            _cartService = cartService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymentUrlVnpay(PaymentInformationModel model)
        {
            var uid = User.FindFirst("uid")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(uid))
                return RedirectToAction("Login", "Auth");

            // ===== LẤY GIẢM GIÁ TỪ VOUCHER =====
            var discount = HttpContext.Session.GetInt32("VoucherDiscount") ?? 0;
            var finalAmount = (decimal)model.Amount - discount;
            if (finalAmount < 0) finalAmount = 0;

            var profile = await _profileService.GetProfileAsync(uid);

            model.Uid = uid;
            model.OrderId = DateTime.UtcNow.Ticks.ToString();
            model.OrderDescription = $"Thanh toán đơn hàng {model.OrderId} tại FStore";

            var order = new OrderModel
            {
                OrderId = model.OrderId,
                Uid = uid,
                UserName = model.Name,
                Email = profile?.Email ?? "",
                Phone = profile?.Phone ?? "",
                Address = profile?.Address ?? "",
                Items = model.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = (decimal)i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList(),
                TotalAmount = finalAmount,                                 // ← TIỀN SAU GIẢM
                VoucherCode = HttpContext.Session.GetString("AppliedVoucherCode"),
                DiscountAmount = discount,
                CreatedAt = DateTime.UtcNow,
                PaymentStatus = "Pending"
            };

            await _orderService.SaveOrderAsync(order);
            await _cartService.ClearCartAsync(uid);

            model.Amount = (double)finalAmount; // ← ĐẨY TIỀN SAU GIẢM CHO VNPAY

            var url = _vnPayService.CreatePaymentUrl(model, HttpContext);
            if (string.IsNullOrEmpty(url))
                return Content("Không thể tạo URL thanh toán VNPay — kiểm tra cấu hình VNPAY.");

            return Redirect(url);
        }

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            if (response == null)
            {
                ViewBag.Message = "Không nhận được phản hồi từ VNPay.";
                return View("Error");
            }

            if (response.Success)
            {
                await _orderService.UpdatePaymentStatusAsync(
                    response.OrderId,
                    "Success",
                    response.TransactionId,
                    true
                );

                var order = await _orderService.GetOrderByIdAsync(response.OrderId);

                if (!string.IsNullOrEmpty(order?.Uid))
                {
                    await _cartService.ClearCartAsync(order.Uid);
                }

                return View("Success", order);
            }
            else
            {
                ViewBag.Message = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}";
                return View("Error", response);
            }
        }
    }
}
