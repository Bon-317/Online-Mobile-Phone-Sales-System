using FSTORE.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace FSTORE.Controllers
{
    public class OrderController : Controller
    {
        private readonly OrderService _orderService;
        private readonly string vnp_HashSecret = "YOUR_VNPAY_SECRET"; // Lấy từ cấu hình

        public OrderController(OrderService orderService)
        {
            _orderService = orderService;
        }

        // ✅ Callback từ VNPay
        [HttpGet]
        [Route("payment/vnpay-return")]
        public async Task<IActionResult> VnPayReturn()
        {
            var queryParams = Request.Query;
            var vnp_SecureHash = queryParams["vnp_SecureHash"].ToString();

            // ✅ Lấy tất cả tham số VNPay (trừ vnp_SecureHash)
            var sortedParams = queryParams
                .Where(k => k.Key.StartsWith("vnp_") && k.Key != "vnp_SecureHash")
                .OrderBy(k => k.Key)
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            // ✅ Tạo chuỗi dữ liệu để kiểm tra chữ ký
            var rawData = string.Join("&", sortedParams.Select(kv => $"{kv.Key}={kv.Value}"));
            var computedHash = ComputeHmacSHA512(rawData, vnp_HashSecret);

            if (!string.Equals(computedHash, vnp_SecureHash, StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Chữ ký không hợp lệ.");
            }

            // ✅ Lấy thông tin cần thiết
            string orderId = sortedParams.GetValueOrDefault("vnp_TxnRef");
            string transactionId = sortedParams.GetValueOrDefault("vnp_TransactionNo");
            string responseCode = sortedParams.GetValueOrDefault("vnp_ResponseCode");

            if (string.IsNullOrEmpty(orderId))
            {
                return BadRequest("Thiếu mã đơn hàng.");
            }

            // ✅ Xác định trạng thái thanh toán
            string status = responseCode == "00" ? "Success" : "Failed";

            // ✅ Cập nhật trạng thái đơn hàng trong Firestore
            await _orderService.UpdatePaymentStatusAsync(orderId, status, transactionId, status == "Success");

            // ✅ Hiển thị kết quả cho người dùng
            ViewBag.Message = status == "Success" ? "Thanh toán thành công!" : "Thanh toán thất bại!";
            return View("PaymentResult");
        }

        private string ComputeHmacSHA512(string data, string key)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return BitConverter.ToString(hash).Replace("-", "").ToLower();
        }
    }
}