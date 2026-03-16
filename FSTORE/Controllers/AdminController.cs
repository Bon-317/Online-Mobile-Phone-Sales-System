using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using FSTORE.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace FSTORE.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly FirestoreDb _db;
        private const string ADMIN_EMAIL = "th26.orange@gmail.com";

        public AdminController()
        {
            string path = Path.Combine(Directory.GetCurrentDirectory(), "firebaseConfig.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _db = FirestoreDb.Create("fstore-5f656");
        }

        private bool IsAdmin()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            return User.Identity.IsAuthenticated && email == ADMIN_EMAIL;
        }

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");

            string uid = User.FindFirst("uid")?.Value;
            if (string.IsNullOrEmpty(uid)) return RedirectToAction("Login", "Auth");

            var doc = await _db.Collection("Users").Document(uid).GetSnapshotAsync();
            if (!doc.Exists) return RedirectToAction("Login", "Auth");

            var admin = doc.ConvertTo<User>();
            return View(admin);
        }

        public IActionResult Dashboard()
        {
            if (!IsAdmin()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> SalesByCategory()
        {
            if (!IsAdmin()) return Unauthorized();

            // Lấy tất cả đơn hàng thành công
            var ordersSnap = await _db.Collection("Orders").GetSnapshotAsync();

            var categoryToSales = new Dictionary<string, double>();
            var productIdToCategory = new Dictionary<string, string>();

            foreach (var doc in ordersSnap.Documents)
            {
                if (!doc.Exists) continue;
                // Chỉ tính các đơn đã thanh toán thành công nếu có trường PaymentStatus
                var paymentStatus = doc.ContainsField("PaymentStatus") ? doc.GetValue<string>("PaymentStatus") : null;
                if (!string.IsNullOrEmpty(paymentStatus) && !string.Equals(paymentStatus, "Success", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Lấy danh sách items
                if (!doc.ContainsField("Items")) continue;
                var items = doc.GetValue<List<Dictionary<string, object>>>("Items");
                if (items == null) continue;

                foreach (var item in items)
                {
                    if (item == null) continue;
                    // Mỗi item cần có Name/Price/Quantity và Category; nếu không có category thì bỏ qua
                    var price = item.ContainsKey("Price") ? Convert.ToDouble(item["Price"]) : 0d;
                    var quantity = item.ContainsKey("Quantity") ? Convert.ToInt32(item["Quantity"]) : 0;
                    // Lấy category từ item hoặc tra theo ProductId
                    string category = null;
                    if (item.ContainsKey("category")) category = item["category"]?.ToString();
                    if (string.IsNullOrWhiteSpace(category))
                    {
                        var productId = item.ContainsKey("ProductId") ? item["ProductId"]?.ToString() : null;
                        if (!string.IsNullOrWhiteSpace(productId))
                        {
                            if (productIdToCategory.TryGetValue(productId, out var cachedCat))
                            {
                                category = cachedCat;
                            }
                            else
                            {
                                var productSnap = await _db.Collection("Products").Document(productId).GetSnapshotAsync();
                                if (productSnap.Exists && productSnap.ContainsField("category"))
                                {
                                    category = productSnap.GetValue<string>("category");
                                    if (!string.IsNullOrWhiteSpace(category))
                                    {
                                        productIdToCategory[productId] = category;
                                    }
                                }
                            }
                        }
                    }
                    if (string.IsNullOrWhiteSpace(category)) continue;

                    var amount = price * quantity;
                    if (amount <= 0) continue;
                    if (!categoryToSales.ContainsKey(category)) categoryToSales[category] = 0d;
                    categoryToSales[category] += amount;
                }
            }

            // Trường hợp không có doanh số, trả về mảng rỗng thay vì lỗi
            var result = categoryToSales
                .OrderByDescending(kv => kv.Value)
                .Select(kv => new { category = kv.Key, sales = Math.Round(kv.Value, 2) })
                .ToList();

            return Json(result);
        }

        public async Task<IActionResult> Users()
        {
            if (!IsAdmin()) return RedirectToAction("Index", "Home");

            var snapshot = await _db.Collection("Users").GetSnapshotAsync();
            var users = snapshot.Documents
                .Select(doc => doc.ConvertTo<User>())
                .Where(u => u.email != ADMIN_EMAIL)
                .ToList();

            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRole(string uid, string newRole)
        {
            if (!IsAdmin()) return Unauthorized();
            if (string.IsNullOrWhiteSpace(uid) || string.IsNullOrWhiteSpace(newRole))
                return BadRequest("Thiếu thông tin.");

            var docRef = _db.Collection("Users").Document(uid);
            var userSnap = await docRef.GetSnapshotAsync();
            if (!userSnap.Exists) return NotFound("Không tìm thấy người dùng.");

            var user = userSnap.ConvertTo<User>();
            if (user.email == ADMIN_EMAIL)
                return BadRequest("Không thể thay đổi vai trò của admin.");

            await docRef.UpdateAsync("role", newRole);
            TempData["Success"] = $"✅ Đã cập nhật vai trò thành: {newRole}";
            return RedirectToAction("Users");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string uid)
        {
            if (!IsAdmin()) return Unauthorized();
            if (string.IsNullOrWhiteSpace(uid)) return BadRequest("Thiếu UID.");

            var docRef = _db.Collection("Users").Document(uid);
            var userSnap = await docRef.GetSnapshotAsync();
            if (!userSnap.Exists) return NotFound("Không tìm thấy người dùng.");

            var user = userSnap.ConvertTo<User>();
            if (user.email == ADMIN_EMAIL)
                return BadRequest("Không thể xóa tài khoản admin.");

            await docRef.DeleteAsync();
            TempData["Success"] = "✅ Đã xóa người dùng.";
            return RedirectToAction("Users");
        }

        public async Task<IActionResult> Categories()
        {
            if (!IsAdmin()) return Unauthorized();

            var snapshot = await _db.Collection("categories").GetSnapshotAsync();
            var categories = snapshot.Documents.Select(doc => new Category
            {
                Id = doc.Id,
                Name = doc.GetValue<string>("name")
            }).ToList();

            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(string name)
        {
            if (!IsAdmin()) return Unauthorized();
            if (string.IsNullOrWhiteSpace(name)) return RedirectToAction("Categories");

            await _db.Collection("categories").AddAsync(new { name });
            TempData["Success"] = "✅ Đã thêm danh mục.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(string id, string name)
        {
            if (!IsAdmin()) return Unauthorized();
            if (string.IsNullOrWhiteSpace(id) || string.IsNullOrWhiteSpace(name))
                return RedirectToAction("Categories");

            await _db.Collection("categories").Document(id).UpdateAsync("name", name);
            TempData["Success"] = "✅ Đã cập nhật danh mục.";
            return RedirectToAction("Categories");
        }
        public async Task<IActionResult> ProductsByCategory(string category)
        {
            if (!IsAdmin()) return Unauthorized();
            if (string.IsNullOrWhiteSpace(category)) return RedirectToAction("Categories");

            var query = _db.Collection("Products").WhereEqualTo("category", category);
            var snapshot = await query.GetSnapshotAsync();

            var products = snapshot.Documents.Select(doc =>
            {
                var p = doc.ConvertTo<Product>();
                p.ProductId = doc.Id;
                return p;
            }).ToList();

            ViewBag.Category = category;
            return View(products);
        }

        public async Task<IActionResult> EditProduct(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            var doc = await _db.Collection("Products").Document(id).GetSnapshotAsync();
            if (!doc.Exists) return NotFound();

            var product = doc.ConvertTo<Product>();
            product.ProductId = doc.Id;
            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> SaveProduct(Product model, string ImageLinks)
        {
            if (!IsAdmin()) return Unauthorized();

            var imageList = ImageLinks?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(link => link.Trim())
                                       .ToList() ?? new List<string>();

            var updateData = new Dictionary<string, object>
            {
                ["title"] = model.name,
                ["price"] = model.price,
                ["stock"] = model.stock,
                ["imageUrl"] = imageList.FirstOrDefault() ?? model.imageUrl,
                ["imageUrls"] = imageList,
                ["description"] = model.description,
                ["category"] = model.category,
                ["visible"] = true,

                // specs
                ["chipset"] = model.chipset,
                ["ram"] = model.ram,
                ["storage"] = model.storage,
                ["rearCamera"] = model.rearCamera,
                ["frontCamera"] = model.frontCamera,
                ["battery"] = model.battery,
                ["os"] = model.os,
                ["screenSize"] = model.screenSize,
                ["resolution"] = model.resolution
            };

            await _db.Collection("Products").Document(model.ProductId).UpdateAsync(updateData);
            TempData["Success"] = "✅ Đã cập nhật sản phẩm.";
            return RedirectToAction("ProductsByCategory", new { category = model.category });
        }


        public IActionResult AddProduct(string category)
        {
            if (!IsAdmin()) return Unauthorized();
            if (string.IsNullOrWhiteSpace(category)) return RedirectToAction("Categories");

            var model = new Product
            {
                category = category
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(Product model, string ImageLinks)
        {
            if (!IsAdmin()) return Unauthorized();

            var imageList = ImageLinks?.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                       .Select(link => link.Trim())
                                       .ToList() ?? new List<string>();

            var data = new Dictionary<string, object>
            {
                ["title"] = model.name,
                ["price"] = model.price,
                ["stock"] = model.stock,
                ["imageUrl"] = imageList.FirstOrDefault() ?? "/images/default.png",
                ["imageUrls"] = imageList,
                ["description"] = model.description,
                ["category"] = model.category,
                ["visible"] = true,

                // specs
                ["chipset"] = model.chipset,
                ["ram"] = model.ram,
                ["storage"] = model.storage,
                ["rearCamera"] = model.rearCamera,
                ["frontCamera"] = model.frontCamera,
                ["battery"] = model.battery,
                ["os"] = model.os,
                ["screenSize"] = model.screenSize,
                ["resolution"] = model.resolution
            };

            await _db.Collection("Products").AddAsync(data);
            TempData["Success"] = "✅ Đã thêm sản phẩm mới.";
            return RedirectToAction("ProductsByCategory", new { category = model.category });
        }


        [HttpPost]
        public async Task<IActionResult> ShowProduct(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            await _db.Collection("Products").Document(id).UpdateAsync("visible", true);
            TempData["Success"] = "✅ Đã hiện sản phẩm.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> HideProduct(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            await _db.Collection("Products").Document(id).UpdateAsync("visible", false);
            TempData["Success"] = "✅ Đã ẩn sản phẩm.";
            return RedirectToAction("Categories");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteProduct(string id)
        {
            if (!IsAdmin()) return Unauthorized();
            await _db.Collection("Products").Document(id).DeleteAsync();
            TempData["Success"] = "✅ Đã xóa sản phẩm.";
            return RedirectToAction("Categories");
        }

        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            if (!IsAdmin()) return Unauthorized();

            var snapshot = await _db.Collection("Orders").GetSnapshotAsync();
            var orders = snapshot.Documents
                .Where(d => d.Exists)
                .Select(MapOrderDocument)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
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
