using Google.Cloud.Firestore;
using FSTORE.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSTORE.Services
{
    public class OrderService
    {
        private readonly FirestoreDb _firestore;

        public OrderService(FirestoreDb firestore)
        {
            _firestore = firestore;
        }

        /// <summary>
        /// Lưu đơn hàng mới hoặc cập nhật đơn hàng hiện có.
        /// </summary>
        public async Task SaveOrderAsync(OrderModel order)
        {
            order.OrderId = string.IsNullOrEmpty(order.OrderId) ? Guid.NewGuid().ToString() : order.OrderId;
            order.CreatedAt = DateTime.UtcNow;
            order.PaymentStatus = string.IsNullOrEmpty(order.PaymentStatus) ? "Đang xử lý" : order.PaymentStatus;

            DocumentReference docRef = _firestore.Collection("Orders").Document(order.OrderId);
            await docRef.SetAsync(ToFirestoreData(order));
        }

        /// <summary>
        /// Lấy danh sách đơn hàng theo UserId.
        /// </summary>
        public async Task<List<OrderModel>> GetOrdersByUserIdAsync(string uid)
        {
            Query query = _firestore.Collection("Orders").WhereEqualTo("Uid", uid);
            QuerySnapshot snapshot = await query.GetSnapshotAsync();

            List<OrderModel> orders = new();
            foreach (DocumentSnapshot doc in snapshot.Documents)
            {
                if (doc.Exists)
                    orders.Add(MapOrderDocument(doc));
            }
            return orders;
        }

        /// <summary>
        /// Lấy đơn hàng theo OrderId.
        /// </summary>
        public async Task<OrderModel?> GetOrderByIdAsync(string orderId)
        {
            DocumentReference docRef = _firestore.Collection("Orders").Document(orderId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            return snapshot.Exists ? MapOrderDocument(snapshot) : null;
        }

        /// <summary>
        /// Cập nhật trạng thái thanh toán của đơn hàng.
        /// </summary>
        public async Task UpdatePaymentStatusAsync(string orderId, string status, string transactionId, bool isSuccess)
        {
            DocumentReference docRef = _firestore.Collection("Orders").Document(orderId);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
            {
                throw new Exception($"Order with ID {orderId} not found.");
            }

            Dictionary<string, object> updates = new Dictionary<string, object>
            {
                { "PaymentStatus", status },
                { "TransactionId", transactionId },
                { "UpdatedAt", DateTime.UtcNow },
                { "IsSuccess", isSuccess }
            };

            await docRef.UpdateAsync(updates);
        }

        private static Dictionary<string, object> ToFirestoreData(OrderModel order)
        {
            return new Dictionary<string, object>
            {
                ["OrderId"] = order.OrderId,
                ["Uid"] = order.Uid,
                ["UserName"] = order.UserName,
                ["Email"] = order.Email,
                ["Phone"] = order.Phone,
                ["Address"] = order.Address,
                ["TotalAmount"] = Convert.ToDouble(order.TotalAmount),
                ["CreatedAt"] = order.CreatedAt,
                ["PaymentStatus"] = order.PaymentStatus,
                ["VoucherCode"] = order.VoucherCode ?? string.Empty,
                ["DiscountAmount"] = Convert.ToDouble(order.DiscountAmount),
                ["Items"] = order.Items.Select(i => new Dictionary<string, object>
                {
                    ["ProductId"] = i.ProductId,
                    ["Name"] = i.Name,
                    ["Price"] = Convert.ToDouble(i.Price),
                    ["Quantity"] = i.Quantity,
                    ["ImageUrl"] = i.ImageUrl
                }).ToList()
            };
        }

        private static OrderModel MapOrderDocument(DocumentSnapshot doc)
        {
            var data = doc.ToDictionary();
            var order = new OrderModel
            {
                OrderId = GetString(data, "OrderId", doc.Id),
                Uid = GetString(data, "Uid"),
                UserName = GetString(data, "UserName"),
                Email = GetString(data, "Email"),
                Phone = GetString(data, "Phone"),
                Address = GetString(data, "Address"),
                TotalAmount = GetDecimal(data, "TotalAmount"),
                DiscountAmount = GetDecimal(data, "DiscountAmount"),
                VoucherCode = GetString(data, "VoucherCode"),
                PaymentStatus = GetString(data, "PaymentStatus", "Pending"),
                CreatedAt = GetDateTime(data, "CreatedAt")
            };

            if (data.TryGetValue("Items", out var itemsObj) && itemsObj is IEnumerable<object> rawItems)
            {
                foreach (var itemObj in rawItems)
                {
                    if (itemObj is not Dictionary<string, object> item) continue;

                    order.Items.Add(new OrderItem
                    {
                        ProductId = GetString(item, "ProductId", GetString(item, "productId")),
                        Name = GetString(item, "Name", GetString(item, "name")),
                        Quantity = GetInt(item, "Quantity", GetInt(item, "quantity")),
                        Price = GetDecimal(item, "Price", GetDecimal(item, "price")),
                        ImageUrl = GetString(item, "ImageUrl", GetString(item, "imageUrl"))
                    });
                }
            }

            return order;
        }

        private static string GetString(Dictionary<string, object> data, string key, string defaultValue = "")
        {
            if (!data.TryGetValue(key, out var value) || value == null) return defaultValue;
            return value.ToString() ?? defaultValue;
        }

        private static int GetInt(Dictionary<string, object> data, string key, int defaultValue = 0)
        {
            if (!data.TryGetValue(key, out var value) || value == null) return defaultValue;

            return value switch
            {
                int i => i,
                long l => (int)l,
                double d => Convert.ToInt32(d),
                _ => int.TryParse(value.ToString(), out var parsed) ? parsed : defaultValue
            };
        }

        private static decimal GetDecimal(Dictionary<string, object> data, string key, decimal defaultValue = 0m)
        {
            if (!data.TryGetValue(key, out var value) || value == null) return defaultValue;

            return value switch
            {
                decimal m => m,
                double d => Convert.ToDecimal(d),
                long l => l,
                int i => i,
                _ => decimal.TryParse(value.ToString(), out var parsed) ? parsed : defaultValue
            };
        }

        private static DateTime GetDateTime(Dictionary<string, object> data, string key)
        {
            if (!data.TryGetValue(key, out var value) || value == null) return DateTime.MinValue;

            return value switch
            {
                Timestamp ts => ts.ToDateTime(),
                DateTime dt => dt,
                _ => DateTime.TryParse(value.ToString(), out var parsed) ? parsed : DateTime.MinValue
            };
        }
    }
}
