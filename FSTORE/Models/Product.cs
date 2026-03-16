using Google.Cloud.Firestore;

namespace FSTORE.Models
{
    [FirestoreData]
    public class Product
    {
        [FirestoreDocumentId]
        public string ProductId { get; set; }

        [FirestoreProperty("title")]
        public string name { get; set; }

        [FirestoreProperty]
        public double price { get; set; }

        [FirestoreProperty]
        public int stock { get; set; }

        [FirestoreProperty]
        public string imageUrl { get; set; }

        [FirestoreProperty]
        public List<string> imageUrls { get; set; } = new();


        [FirestoreProperty]
        public string description { get; set; }

        [FirestoreProperty]
        public string category { get; set; }

        [FirestoreProperty]
        public bool visible { get; set; }

        // 🔧 Thông số kỹ thuật tách riêng
        [FirestoreProperty]
        public string chipset { get; set; } = string.Empty;

        [FirestoreProperty]
        public string ram { get; set; } = string.Empty;

        [FirestoreProperty]
        public string storage { get; set; } = string.Empty;

        [FirestoreProperty]
        public string rearCamera { get; set; } = string.Empty;

        [FirestoreProperty]
        public string frontCamera { get; set; } = string.Empty;

        [FirestoreProperty]
        public string battery { get; set; } = string.Empty;

        [FirestoreProperty]
        public string os { get; set; } = string.Empty;

        [FirestoreProperty]
        public string screenSize { get; set; } = string.Empty;

        [FirestoreProperty]
        public string resolution { get; set; } = string.Empty;
    }
}
