using FSTORE.Models;
using FSTORE.Services;
using FSTORE.Services.Vnpay;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace FSTORE.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly IVnPayService _vnPayService;
        private readonly CartService _cartService;
        private readonly OrderService _orderService;
        private readonly ProfileService _profileService;

        public CheckoutController(
            IVnPayService vnPayService,
            CartService cartService,
            OrderService orderService,
            ProfileService profileService)
        {
            _vnPayService = vnPayService;
            _cartService = cartService;
            _orderService = orderService;
            _profileService = profileService;
        }

        //[HttpGet]
        //public async Task<IActionResult> PaymentCallbackVnpay()
        //{
        //    var response = _vnPayService.PaymentExecute(Request.Query);
        //    ViewBag.PaymentResult = response;

        //    OrderModel? order = null;

        //    if (response.Success)
        //    {
        //        string uid = null;

        //        // ✅ Kiểm tra có ký tự | trong OrderDescription không
        //        if (!string.IsNullOrEmpty(response.OrderDescription) && response.OrderDescription.Contains('|'))
        //        {
        //            var parts = response.OrderDescription.Split('|');
        //            uid = parts[0];
        //        }

        //        if (string.IsNullOrEmpty(uid))
        //        {
        //            return View("Error", new ErrorViewModel
        //            {
        //                RequestId = "Thiếu UID trong OrderDescription callback từ VNPay."
        //            });
        //        }

        //        var profile = await _profileService.GetProfileAsync(uid);
        //        if (profile == null)
        //        {
        //            return View("Error", new ErrorViewModel
        //            {
        //                RequestId = $"Không tìm thấy thông tin người dùng UID={uid}"
        //            });
        //        }

        //        // ✅ Lấy giỏ hàng và lưu đơn hàng như cũ
        //        var cart = await _cartService.GetCartAsync(uid);
        //        var orderItems = cart.Select(c => new OrderItem
        //        {
        //            ProductId = c.ProductId,
        //            Name = c.Name,
        //            ImageUrl = c.ImageUrl,
        //            Price = c.Price,
        //            Quantity = c.Quantity
        //        }).ToList();

        //        double totalAmount = orderItems.Sum(i => i.Price * i.Quantity);

        //        order = new OrderModel
        //        {
        //            OrderId = !string.IsNullOrEmpty(response.OrderId)
        //                ? response.OrderId
        //                : $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}",
        //            Uid = profile.Uid,
        //            UserName = profile.Name,
        //            Email = profile.Email,
        //            Phone = profile.Phone,
        //            Address = profile.Address,
        //            Items = orderItems,
        //            TotalAmount = totalAmount,
        //            CreatedAt = DateTime.UtcNow,
        //            PaymentStatus = "Success"
        //        };

        //        await _orderService.SaveOrderAsync(order);
        //        await _cartService.ClearCartAsync(uid);
        //    }

        //    return View("Index", order);
        //}

        [HttpGet]
        public async Task<IActionResult> PaymentCallbackVnpay()
        {
            var response = _vnPayService.PaymentExecute(Request.Query);
            ViewBag.PaymentResult = response;

            if (!response.Success)
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = $"Thanh toán thất bại. Mã lỗi: {response.VnPayResponseCode}"
                });
            }

            await _orderService.UpdatePaymentStatusAsync(response.OrderId, "Success", response.TransactionId, true);
            var order = await _orderService.GetOrderByIdAsync(response.OrderId);

            return View("Index", order);
        }



        [HttpGet]
        public IActionResult Index() => View();

        [HttpPost]
        public async Task<IActionResult> CreateCodOrder(PaymentInformationModel model)
        {
            var uid = User.FindFirst("uid")?.Value ?? User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(uid))
            {
                return RedirectToAction("Login", "Auth");
            }

            var profile = await _profileService.GetProfileAsync(uid);
            if (profile == null)
            {
                return View("Error", new ErrorViewModel
                {
                    RequestId = "Không tìm thấy thông tin người dùng để tạo đơn COD."
                });
            }

            var discount = HttpContext.Session.GetInt32("VoucherDiscount") ?? 0;
            var finalAmount = (decimal)model.Amount - discount;
            if (finalAmount < 0) finalAmount = 0;

            var order = new OrderModel
            {
                OrderId = DateTime.UtcNow.Ticks.ToString(),
                Uid = uid,
                UserName = profile.Name ?? model.Name,
                Email = profile.Email ?? string.Empty,
                Phone = profile.Phone ?? string.Empty,
                Address = profile.Address ?? string.Empty,
                Items = model.Items?.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    Name = i.Name,
                    Price = (decimal)i.Price,
                    Quantity = i.Quantity,
                    ImageUrl = i.ImageUrl
                }).ToList() ?? new(),
                TotalAmount = finalAmount,
                VoucherCode = HttpContext.Session.GetString("AppliedVoucherCode"),
                DiscountAmount = discount,
                CreatedAt = DateTime.UtcNow,
                PaymentStatus = "COD"
            };

            await _orderService.SaveOrderAsync(order);
            await _cartService.ClearCartAsync(uid);

            return View("Index", order);
        }
    }
}