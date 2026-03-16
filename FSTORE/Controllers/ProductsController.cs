using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;
using FSTORE.Models;

namespace FSTORE.Controllers
{
    public class ProductController : Controller
    {
        private readonly FirestoreDb _db;

        public ProductController()
        {
            string path = Path.Combine(AppContext.BaseDirectory, "firebaseConfig.json");
            Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
            _db = FirestoreDb.Create("fstore-5f656");
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var products = new List<Product>();
                var query = _db.Collection("Products").WhereEqualTo("visible", true);
                var snapshot = await query.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();

                    var product = new Product
                    {
                        ProductId = doc.Id,
                        name = data.GetValueOrDefault("title")?.ToString() ?? "Không tên",
                        price = data.GetValueOrDefault("price") is double d ? d : Convert.ToDouble(data.GetValueOrDefault("price") ?? 0),
                        category = data.GetValueOrDefault("category")?.ToString() ?? "Chưa phân loại",
                        imageUrl = data.GetValueOrDefault("imageUrl")?.ToString() ?? Url.Content("~/images/default.png"),
                        description = data.GetValueOrDefault("description")?.ToString() ?? "",
                        stock = data.GetValueOrDefault("stock") is int s ? s : Convert.ToInt32(data.GetValueOrDefault("stock") ?? 0),
                        visible = data.GetValueOrDefault("visible") is bool v && v,

                        // Thông số kỹ thuật tách riêng
                        chipset = data.GetValueOrDefault("chipset")?.ToString() ?? "",
                        ram = data.GetValueOrDefault("ram")?.ToString() ?? "",
                        storage = data.GetValueOrDefault("storage")?.ToString() ?? "",
                        rearCamera = data.GetValueOrDefault("rearCamera")?.ToString() ?? "",
                        frontCamera = data.GetValueOrDefault("frontCamera")?.ToString() ?? "",
                        battery = data.GetValueOrDefault("battery")?.ToString() ?? "",
                        os = data.GetValueOrDefault("os")?.ToString() ?? "",
                        screenSize = data.GetValueOrDefault("screenSize")?.ToString() ?? "",
                        resolution = data.GetValueOrDefault("resolution")?.ToString() ?? ""
                    };

                    products.Add(product);
                }

                return View(products);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải sản phẩm: " + ex.Message;
                return View(new List<Product>());
            }
        }

        [HttpGet]
        public async Task<IActionResult> Suggest(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return PartialView("_SearchSuggestions", new List<Product>());

            var query = _db.Collection("Products").WhereEqualTo("visible", true);
            var snapshot = await query.GetSnapshotAsync();

            var results = new List<Product>();
            foreach (var doc in snapshot.Documents)
            {
                var data = doc.ToDictionary();
                var name = data.GetValueOrDefault("title")?.ToString() ?? "";
                var description = data.GetValueOrDefault("description")?.ToString() ?? "";

                if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                    description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    var product = doc.ConvertTo<Product>();
                    product.ProductId = doc.Id;
                    results.Add(product);
                }
            }

            return PartialView("_SearchSuggestions", results.Take(10).ToList());
        }

        [HttpGet]
        public async Task<IActionResult> Search(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return RedirectToAction("Index");

            try
            {
                var products = new List<Product>();
                var query = _db.Collection("Products").WhereEqualTo("visible", true);
                var snapshot = await query.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();
                    var name = data.GetValueOrDefault("title")?.ToString() ?? "";
                    var description = data.GetValueOrDefault("description")?.ToString() ?? "";

                    if (name.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
                        description.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        var product = new Product
                        {
                            ProductId = doc.Id,
                            name = name,
                            price = data.GetValueOrDefault("price") is double d ? d : Convert.ToDouble(data.GetValueOrDefault("price") ?? 0),
                            category = data.GetValueOrDefault("category")?.ToString() ?? "Chưa phân loại",
                            imageUrl = data.GetValueOrDefault("imageUrl")?.ToString() ?? Url.Content("~/images/default.png"),
                            description = description,
                            stock = data.GetValueOrDefault("stock") is int s ? s : Convert.ToInt32(data.GetValueOrDefault("stock") ?? 0),
                            visible = data.GetValueOrDefault("visible") is bool v && v,

                            chipset = data.GetValueOrDefault("chipset")?.ToString() ?? "",
                            ram = data.GetValueOrDefault("ram")?.ToString() ?? "",
                            storage = data.GetValueOrDefault("storage")?.ToString() ?? "",
                            rearCamera = data.GetValueOrDefault("rearCamera")?.ToString() ?? "",
                            frontCamera = data.GetValueOrDefault("frontCamera")?.ToString() ?? "",
                            battery = data.GetValueOrDefault("battery")?.ToString() ?? "",
                            os = data.GetValueOrDefault("os")?.ToString() ?? "",
                            screenSize = data.GetValueOrDefault("screenSize")?.ToString() ?? "",
                            resolution = data.GetValueOrDefault("resolution")?.ToString() ?? ""
                        };

                        products.Add(product);
                    }
                }

                ViewBag.SearchKeyword = keyword;
                return View("Index", products);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tìm kiếm sản phẩm: " + ex.Message;
                return View("Index", new List<Product>());
            }
        }

        public async Task<IActionResult> Category(string category)
        {
            if (string.IsNullOrEmpty(category))
                return RedirectToAction("Index");

            try
            {
                var products = new List<Product>();
                var query = _db.Collection("Products")
                               .WhereEqualTo("visible", true)
                               .WhereEqualTo("category", category);
                var snapshot = await query.GetSnapshotAsync();

                foreach (var doc in snapshot.Documents)
                {
                    var data = doc.ToDictionary();

                    var product = new Product
                    {
                        ProductId = doc.Id,
                        name = data.GetValueOrDefault("title")?.ToString() ?? "Không tên",
                        price = data.GetValueOrDefault("price") is double d ? d : Convert.ToDouble(data.GetValueOrDefault("price") ?? 0),
                        category = data.GetValueOrDefault("category")?.ToString() ?? "Chưa phân loại",
                        imageUrl = data.GetValueOrDefault("imageUrl")?.ToString() ?? Url.Content("~/images/default.png"),
                        description = data.GetValueOrDefault("description")?.ToString() ?? "",
                        stock = data.GetValueOrDefault("stock") is int s ? s : Convert.ToInt32(data.GetValueOrDefault("stock") ?? 0),
                        visible = data.GetValueOrDefault("visible") is bool v && v,

                        // Thông số kỹ thuật tách riêng
                        chipset = data.GetValueOrDefault("chipset")?.ToString() ?? "",
                        ram = data.GetValueOrDefault("ram")?.ToString() ?? "",
                        storage = data.GetValueOrDefault("storage")?.ToString() ?? "",
                        rearCamera = data.GetValueOrDefault("rearCamera")?.ToString() ?? "",
                        frontCamera = data.GetValueOrDefault("frontCamera")?.ToString() ?? "",
                        battery = data.GetValueOrDefault("battery")?.ToString() ?? "",
                        os = data.GetValueOrDefault("os")?.ToString() ?? "",
                        screenSize = data.GetValueOrDefault("screenSize")?.ToString() ?? "",
                        resolution = data.GetValueOrDefault("resolution")?.ToString() ?? ""
                    };

                    products.Add(product);
                }

                ViewBag.SelectedCategory = category;
                return View("Index", products);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi lọc sản phẩm: " + ex.Message;
                return View("Index", new List<Product>());
            }
        }

        public async Task<IActionResult> Detail(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest("Thiếu mã sản phẩm");

            try
            {
                var docRef = _db.Collection("Products").Document(id);
                var docSnap = await docRef.GetSnapshotAsync();

                if (!docSnap.Exists)
                    return NotFound("Sản phẩm không tồn tại");

                var data = docSnap.ToDictionary();

                var product = new Product
                {
                    ProductId = docSnap.Id,
                    name = data.GetValueOrDefault("title")?.ToString() ?? "Không tên",
                    price = data.GetValueOrDefault("price") is double d ? d : Convert.ToDouble(data.GetValueOrDefault("price") ?? 0),
                    category = data.GetValueOrDefault("category")?.ToString() ?? "Chưa phân loại",
                    imageUrl = data.GetValueOrDefault("imageUrl")?.ToString() ?? Url.Content("~/images/default.png"),
                    imageUrls = data.ContainsKey("imageUrls") && data["imageUrls"] is IEnumerable<object> list
                    ? list.Select(x => x.ToString()).ToList() : new List<string>(),

                    description = data.GetValueOrDefault("description")?.ToString() ?? "",
                    stock = data.GetValueOrDefault("stock") is int s ? s : Convert.ToInt32(data.GetValueOrDefault("stock") ?? 0),
                    visible = data.GetValueOrDefault("visible") is bool v && v,

                    // Thông số kỹ thuật tách riêng
                    chipset = data.GetValueOrDefault("chipset")?.ToString() ?? "",
                    ram = data.GetValueOrDefault("ram")?.ToString() ?? "",
                    storage = data.GetValueOrDefault("storage")?.ToString() ?? "",
                    rearCamera = data.GetValueOrDefault("rearCamera")?.ToString() ?? "",
                    frontCamera = data.GetValueOrDefault("frontCamera")?.ToString() ?? "",
                    battery = data.GetValueOrDefault("battery")?.ToString() ?? "",
                    os = data.GetValueOrDefault("os")?.ToString() ?? "",
                    screenSize = data.GetValueOrDefault("screenSize")?.ToString() ?? "",
                    resolution = data.GetValueOrDefault("resolution")?.ToString() ?? ""
                };

                // ============ LẤY SẢN PHẨM LIÊN QUAN ============
                var relatedProducts = new List<Product>();

                // Query sản phẩm cùng danh mục
                var relatedQuery = _db.Collection("Products")
                    .WhereEqualTo("visible", true)
                    .WhereEqualTo("category", product.category);

                var relatedSnapshot = await relatedQuery.GetSnapshotAsync();

                foreach (var doc in relatedSnapshot.Documents)
                {
                    // Bỏ qua sản phẩm hiện tại
                    if (doc.Id == id)
                        continue;

                    var relatedData = doc.ToDictionary();

                    var relatedProduct = new Product
                    {
                        ProductId = doc.Id,
                        name = relatedData.GetValueOrDefault("title")?.ToString() ?? "Không tên",
                        price = relatedData.GetValueOrDefault("price") is double dp ? dp : Convert.ToDouble(relatedData.GetValueOrDefault("price") ?? 0),
                        category = relatedData.GetValueOrDefault("category")?.ToString() ?? "Chưa phân loại",
                        imageUrl = relatedData.GetValueOrDefault("imageUrl")?.ToString() ?? Url.Content("~/images/default.png"),
                        description = relatedData.GetValueOrDefault("description")?.ToString() ?? "",
                        stock = relatedData.GetValueOrDefault("stock") is int st ? st : Convert.ToInt32(relatedData.GetValueOrDefault("stock") ?? 0)
                    };

                    relatedProducts.Add(relatedProduct);

                    // Giới hạn 8 sản phẩm
                    if (relatedProducts.Count >= 8)
                        break;
                }

                ViewBag.RelatedProducts = relatedProducts;
                // ============ KẾT THÚC LẤY SẢN PHẨM LIÊN QUAN ============

                return View(product);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi khi tải chi tiết sản phẩm: " + ex.Message;
                return View();
            }
        }
    }
}