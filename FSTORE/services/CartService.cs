using Google.Cloud.Firestore;
using FSTORE.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FSTORE.Services
{
    public class CartService
    {
        private readonly FirestoreDb _firestoreDb;

        public CartService(FirestoreDb firestoreDb)
        {
            _firestoreDb = firestoreDb;
        }

        // ==================== GIỎ HÀNG (giữ nguyên như cũ) ====================
        public async Task AddToCartAsync(string userId, CartItem item)
        {
            var cartRef = _firestoreDb.Collection("carts").Document(userId);
            var snapshot = await cartRef.GetSnapshotAsync();

            List<Dictionary<string, object>> items = snapshot.Exists && snapshot.ContainsField("items")
                ? snapshot.GetValue<List<Dictionary<string, object>>>("items")
                : new List<Dictionary<string, object>>();

            var existing = items.FirstOrDefault(x => x["productId"].ToString() == item.ProductId);
            if (existing != null)
            {
                existing["quantity"] = Convert.ToInt32(existing["quantity"]) + item.Quantity;
            }
            else
            {
                items.Add(item.ToDictionary());
            }

            await cartRef.SetAsync(new Dictionary<string, object> { { "items", items } }, SetOptions.MergeAll);
        }

        public async Task<List<CartItem>> GetCartAsync(string userId)
        {
            var doc = await _firestoreDb.Collection("carts").Document(userId).GetSnapshotAsync();
            if (!doc.Exists || !doc.ContainsField("items")) return new List<CartItem>();

            var items = doc.GetValue<List<Dictionary<string, object>>>("items");
            return items.Select(i => new CartItem
            {
                ProductId = i["productId"].ToString(),
                Name = i.ContainsKey("name") ? i["name"].ToString() : "Không tên",
                Price = Convert.ToDouble(i["price"]),
                Quantity = Convert.ToInt32(i["quantity"]),
                ImageUrl = i.ContainsKey("imageUrl") ? i["imageUrl"].ToString() : "",
                Selected = i.ContainsKey("selected") ? Convert.ToBoolean(i["selected"]) : true
            }).ToList();
        }

        public async Task UpdateQuantityAsync(string userId, string productId, int quantity)
        {
            var cart = await GetCartAsync(userId);
            var item = cart.FirstOrDefault(x => x.ProductId == productId);
            if (item != null) item.Quantity = quantity > 0 ? quantity : 1;
            await SaveCartAsync(userId, cart);
        }

        public async Task RemoveItemAsync(string userId, string productId)
        {
            var cart = await GetCartAsync(userId);
            cart.RemoveAll(x => x.ProductId == productId);
            await SaveCartAsync(userId, cart);
        }

        public async Task ClearCartAsync(string userId)
        {
            await _firestoreDb.Collection("carts").Document(userId).DeleteAsync();
        }

        private async Task SaveCartAsync(string userId, List<CartItem> cart)
        {
            var cartRef = _firestoreDb.Collection("carts").Document(userId);
            await cartRef.SetAsync(new Dictionary<string, object>
            {
                { "items", cart.Select(x => x.ToDictionary()).ToList() }
            }, SetOptions.MergeAll);
        }

        // ==================== ÁP DỤNG VOUCHER – HOÀN HẢO VỚI MODEL CỦA BẠN ====================
        public class ApplyVoucherResult
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
            public decimal DiscountAmount { get; set; }
        }

        public async Task<ApplyVoucherResult> ApplyVoucherAsync(string userId, string voucherCode)
        {
            try
            {
                voucherCode = voucherCode.Trim().ToUpper();
                var voucherDoc = await _firestoreDb.Collection("vouchers").Document(voucherCode).GetSnapshotAsync();

                if (!voucherDoc.Exists)
                    return new ApplyVoucherResult { Success = false, Message = "Mã voucher không tồn tại!" };

                var voucher = voucherDoc.ConvertTo<Voucher>();

                if (!voucher.isActive)
                    return new ApplyVoucherResult { Success = false, Message = "Voucher đã bị vô hiệu hóa!" };

                if (voucher.expiryDate < Timestamp.GetCurrentTimestamp())
                    return new ApplyVoucherResult { Success = false, Message = "Voucher đã hết hạn!" };

                var cart = await GetCartAsync(userId);
                var subtotal = cart.Where(x => x.Selected).Sum(x => x.Price * x.Quantity);

                if (subtotal < voucher.minOrderAmount)
                    return new ApplyVoucherResult
                    {
                        Success = false,
                        Message = $"Đơn hàng cần tối thiểu {voucher.minOrderAmount:N0}₫ để áp dụng mã này!"
                    };

                // Tính tiền giảm (hiện tại chỉ hỗ trợ giảm cố định, nếu muốn % thì mở rộng sau)
                decimal discountAmount = (decimal)voucher.discountValue;

                return new ApplyVoucherResult
                {
                    Success = true,
                    Message = $"Áp dụng mã {voucherCode} thành công! Giảm {discountAmount:N0}₫",
                    DiscountAmount = discountAmount
                };
            }
            catch (Exception)
            {
                return new ApplyVoucherResult { Success = false, Message = "Lỗi hệ thống, vui lòng thử lại!" };
            }
        }
    }
}