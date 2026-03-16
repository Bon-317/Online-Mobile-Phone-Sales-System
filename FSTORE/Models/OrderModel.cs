using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace FSTORE.Models
{
    [FirestoreData]
    public class OrderModel
    {
        [FirestoreProperty] public string OrderId { get; set; } = string.Empty;
        [FirestoreProperty] public string Uid { get; set; } = string.Empty;
        [FirestoreProperty] public string UserName { get; set; } = string.Empty;
        [FirestoreProperty] public string Email { get; set; } = string.Empty;
        [FirestoreProperty] public string Phone { get; set; } = string.Empty;
        [FirestoreProperty] public string Address { get; set; } = string.Empty;
        [FirestoreProperty] public List<OrderItem> Items { get; set; } = new();

        // ĐÃ SỬA: đổi từ double → decimal (chuẩn tiền tệ)
        [FirestoreProperty] public decimal TotalAmount { get; set; }

        [FirestoreProperty] public DateTime CreatedAt { get; set; }
        [FirestoreProperty] public string PaymentStatus { get; set; } = "Pending";

        // ĐÃ THÊM 2 FIELD MỚI – BẮT BUỘC ĐỂ HẾT LỖI
        [FirestoreProperty] public string? VoucherCode { get; set; }        // Có thể null
        [FirestoreProperty] public decimal DiscountAmount { get; set; }     // Số tiền được giảm

        // XÓA DÒNG NÀY ĐI (của bạn bị sai tên + không dùng)
        // [FirestoreProperty] public string UpdateModel { get; set; }
    }

    [FirestoreData]
    public class OrderItem
    {
        [FirestoreProperty] public string ProductId { get; set; } = string.Empty;
        [FirestoreProperty] public string Name { get; set; } = string.Empty;
        [FirestoreProperty] public decimal Price { get; set; }           // Đổi sang decimal luôn cho chuẩn
        [FirestoreProperty] public int Quantity { get; set; }
        [FirestoreProperty] public string ImageUrl { get; set; } = string.Empty;
    }
}