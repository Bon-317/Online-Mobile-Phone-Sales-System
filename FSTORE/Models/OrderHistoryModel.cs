using Google.Cloud.Firestore;
using System;
using System.Collections.Generic;

namespace FSTORE.Models
{
    [FirestoreData]
    public class OrderHistoryItem
    {
        [FirestoreProperty("imageUrl")]
        public string ImageUrl { get; set; }

        [FirestoreProperty("name")]
        public string Name { get; set; }

        [FirestoreProperty("price")]
        public double Price { get; set; }

        [FirestoreProperty("quantity")]
        public int Quantity { get; set; }
    }

    [FirestoreData]
    public class OrderHistoryModel
    {
        [FirestoreProperty("orderId")]
        public string OrderId { get; set; }

        [FirestoreProperty("deliveryAddress")]
        public string DeliveryAddress { get; set; }

        [FirestoreProperty("items")]
        public List<OrderHistoryItem> Items { get; set; }

        [FirestoreProperty("createdAt")]
        public long CreatedAt { get; set; } // timestamp từ Firestore

        [FirestoreProperty("paymentMethod")]
        public string PaymentMethod { get; set; }

        [FirestoreProperty("status")]
        public string Status { get; set; }

        [FirestoreProperty("shipping")]
        public double Shipping { get; set; }
    }
}