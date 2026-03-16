using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;
using FSTORE.Extensions;

namespace FSTORE.Views.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var cart = HttpContext.Session.GetObjectFromJson<List<CartItem>>("Cart") ?? new List<CartItem>();
            int totalQuantity = cart.Sum(x => x.Quantity); // ⚡ đếm tổng số lượng, không chỉ số sản phẩm
            return View(totalQuantity);
        }
    }
}
