using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using FSTORE.Models;
using System.Security.Claims;

namespace FSTORE.Controllers
{
    public class FavoritesController : Controller
    {
        private readonly FirestoreDb _firestoreDb;

        public FavoritesController()
        {
            _firestoreDb = FirestoreDb.Create("fstore-5f656");
        }

        // Hiển thị danh sách sản phẩm yêu thích
        public async Task<IActionResult> Index()
        {
            string userId = User.Identity.Name;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var favoritesRef = _firestoreDb.Collection("Favorites");
            var query = favoritesRef.WhereEqualTo("UserId", userId);
            var snapshot = await query.GetSnapshotAsync();

            var productIds = snapshot.Documents
                .Select(doc => doc.GetValue<string>("ProductId"))
                .Distinct()
                .ToList();

            var products = new List<Product>();

            foreach (var productId in productIds)
            {
                var productDoc = await _firestoreDb.Collection("Products").Document(productId).GetSnapshotAsync();
                if (productDoc.Exists)
                {
                    var product = productDoc.ConvertTo<Product>();
                    product.ProductId = productId;
                    products.Add(product);
                }
            }

            return View(products);
        }

        // Thêm hoặc xóa sản phẩm khỏi yêu thích (AJAX)
        [HttpPost]
        public async Task<IActionResult> Toggle(string productId)
        {
            string userId = User.Identity.Name;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(productId))
            {
                return BadRequest();
            }

            var favoritesRef = _firestoreDb.Collection("Favorites");
            var query = favoritesRef
                .WhereEqualTo("UserId", userId)
                .WhereEqualTo("ProductId", productId);

            var snapshot = await query.GetSnapshotAsync();

            if (snapshot.Count > 0)
            {
                foreach (var doc in snapshot.Documents)
                {
                    await doc.Reference.DeleteAsync();
                }
                return Json(new { isFavorite = false });
            }
            else
            {
                await favoritesRef.AddAsync(new Dictionary<string, object>
                {
                    { "UserId", userId },
                    { "ProductId", productId },
                    { "CreatedAt", Timestamp.GetCurrentTimestamp() }
                });
                return Json(new { isFavorite = true });
            }
        }
        [HttpGet]
        public async Task<IActionResult> IsFavorite(string productId)
        {
            string userId = User.Identity.Name;
            if (string.IsNullOrEmpty(userId)) return Json(new { isFavorite = false });

            var favoritesRef = _firestoreDb.Collection("Favorites");
            var query = favoritesRef
                .WhereEqualTo("UserId", userId)
                .WhereEqualTo("ProductId", productId);

            var snapshot = await query.GetSnapshotAsync();
            return Json(new { isFavorite = snapshot.Count > 0 });
        }

    }
}
