using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FSTORE.Controllers
{
    public class SeedController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public SeedController()
        {
            _firestoreDb = FirestoreDb.Create("fstore-5f656");
        }

        public async Task<IActionResult> Init()
        {
            // Tạo sản phẩm mẫu
            var productRef = _firestoreDb.Collection("Products").Document();
            await productRef.SetAsync(new Dictionary<string, object>
            {
                { "title", "iPhone 16 128GB Chính hãng VN/A" },
                { "price", 5000000 },
                { "stock", 100 },
                { "imageUrl", "https://example.com/images/iphone16.jpg" },
                { "description", "Thiết kế mới lạ, hiệu năng vượt trội, khả năng sang trọng" },
                { "category", "Apple" },
                { "visible", true },
                { "chipset", "Apple A18" },
                { "ram", "6GB" },
                { "storage", "128GB" },
                { "rearCamera", "100MP" },
                { "frontCamera", "12MP" },
                { "battery", "4000mAh" },
                { "os", "iOS 18" },
                { "screenSize", "6.7 inch" },
                { "resolution", "2796 x 1290" }
            });



            // Tạo người dùng mẫu
            var userRef = _firestoreDb.Collection("Users").Document("user001");
            await userRef.SetAsync(new Dictionary<string, object>
            {
                { "email", "minhthang@example.com" },
                { "name", "Minh Thang" },
                { "role", "user" }
            });

            // Tạo yêu thích mẫu
            var favoriteRef = _firestoreDb.Collection("Favorites").Document();
            await favoriteRef.SetAsync(new Dictionary<string, object>
            {
                { "UserId", "minhthang@example.com" },
                { "ProductId", productRef.Id },
                { "CreatedAt", Timestamp.GetCurrentTimestamp() }
            });

            return Content("Dữ liệu mẫu đã được tạo thành công!");
        }
    }
}
