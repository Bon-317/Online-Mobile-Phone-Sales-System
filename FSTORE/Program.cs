using FSTORE.Services.Vnpay;
using FSTORE.Services;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add MVC
builder.Services.AddControllersWithViews();
// VOUCHER SERVICE
builder.Services.AddScoped<VoucherService>();

// SESSION (bắt buộc cho voucher)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

// ✅ Add Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.None; // Cho phép gửi cookie khi redirect từ VNPay
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Bắt buộc HTTPS
});

// ✅ Add Authentication (Cookie)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login";
        options.LogoutPath = "/Auth/Logout";
        options.AccessDeniedPath = "/Auth/AccessDenied";
        options.Cookie.Name = "FSTORE.Auth";
        options.Cookie.SameSite = SameSiteMode.None; // Cho phép cross-site
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });

// ✅ Connect VNPay API
builder.Services.AddScoped<IVnPayService, VnPayService>();

// ✅ Order Services
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<OrderHistoryService>();

// ✅ Forum Services
builder.Services.AddSingleton<IUserService, UserService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// ✅ Firestore setup
var projectId = "fstore-5f656";

var credentialPaths = new[]
{
    Path.Combine(builder.Environment.ContentRootPath, "firebaseConfig.json"),
    Path.Combine(AppContext.BaseDirectory, "firebaseConfig.json")
};

var jsonPath = credentialPaths.FirstOrDefault(File.Exists);
if (string.IsNullOrWhiteSpace(jsonPath))
{
    throw new FileNotFoundException(
        $"Không tìm thấy file cấu hình Firebase. Đã tìm ở: {string.Join("; ", credentialPaths)}");
}

var firestore = new FirestoreDbBuilder
{
    ProjectId = projectId,
    CredentialsPath = jsonPath
}.Build();

builder.Services.AddSingleton(firestore);

// ✅ CartService & ProfileService
builder.Services.AddScoped<CartService>();
builder.Services.AddSingleton<ProfileService>();
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ✅ Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); // Session phải trước Authentication
app.UseAuthentication();
app.UseAuthorization();

// ✅ Default route
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();