using Microsoft.AspNetCore.Mvc;
using FSTORE.Services;
using System.Threading.Tasks;
using System.Security.Claims;

namespace FSTORE.Controllers
{
    public class OrderHistoryController : Controller
    {
        private readonly OrderHistoryService _orderHistoryService;

        public OrderHistoryController(OrderHistoryService orderHistoryService)
        {
            _orderHistoryService = orderHistoryService;
        }

        public async Task<IActionResult> Index()
        {
            var uid = User.FindFirst("uid")?.Value ?? User.Identity?.Name;

            if (string.IsNullOrEmpty(uid))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("Login", "Auth");
            }

            var orders = await _orderHistoryService.GetPaidOrdersByUserAsync(uid);
            return View(orders);
        }

    }

}
