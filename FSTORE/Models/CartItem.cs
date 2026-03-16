using Google.Cloud.Firestore;
using System.Collections.Generic;

namespace FSTORE.Models
{
    [FirestoreData]
    public class CartItem
    {
        [FirestoreProperty] public string ProductId { get; set; }
        [FirestoreProperty] public string Name { get; set; }
        [FirestoreProperty] public double Price { get; set; }
        [FirestoreProperty] public int Quantity { get; set; }
        [FirestoreProperty] public string ImageUrl { get; set; }
        [FirestoreProperty] public bool Selected { get; set; }

        public double Total => Price * Quantity;

        // ✅ Giữ lại logic cũ của bạn
        public Dictionary<string, object> ToDictionary() => new()
        {
            { "productId", ProductId },
            { "name", Name },
            { "price", Price },
            { "quantity", Quantity },
            { "imageUrl", ImageUrl },
            { "selected", Selected }
        };
    }
}
