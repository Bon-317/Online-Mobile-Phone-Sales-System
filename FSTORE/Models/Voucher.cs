using FSTORE.Validation;
using Google.Cloud.Firestore;
using System;
using System.ComponentModel.DataAnnotations;

namespace FSTORE.Models
{
    [FirestoreData]
    public class Voucher
    {
        private static DateTime GetDefaultExpiry()
        {
            var future = DateTime.Now.AddDays(30);
            return new DateTime(future.Year, future.Month, future.Day, future.Hour, future.Minute, 0, DateTimeKind.Local);
        }

        [FirestoreDocumentId]
        public string code { get; set; } = string.Empty;

        [FirestoreProperty] public string discountType { get; set; } = "percentage";
        [FirestoreProperty] public double discountValue { get; set; } = 0;
        [FirestoreProperty] public double minOrderAmount { get; set; } = 0;
        [FirestoreProperty] public bool isActive { get; set; } = true;

        // QUAN TRỌNG: Phải là Timestamp KHÔNG nullable, không dùng ?
        [FirestoreProperty] public Timestamp expiryDate { get; set; }

        // Dùng để bind form Create/Edit
        [Display(Name = "Ngày hết hạn")]
        [FutureDate(ErrorMessage = "Ngày hết hạn phải là một ngày trong tương lai.")]
        public DateTime ExpiryDateTime { get; set; } = GetDefaultExpiry();

        public string? DocumentId { get; set; }
    }
}