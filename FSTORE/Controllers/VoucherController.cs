using FSTORE.Models;
using FSTORE.Services;
using Microsoft.AspNetCore.Mvc;

namespace FSTORE.Controllers
{
    public class VoucherController : Controller
    {
        private readonly VoucherService _voucherService;

        public VoucherController(VoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        public async Task<IActionResult> Index()
        {
            var vouchers = await _voucherService.GetAllVouchersAsync();
            return View(vouchers);
        }

        public IActionResult Create()
        {
            return View(new Voucher());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher model)
        {
            if (ModelState.IsValid)
            {
                await _voucherService.CreateVoucherAsync(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            await _voucherService.DeleteVoucherAsync(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var voucher = await _voucherService.GetVoucherByIdAsync(id);
            if (voucher == null)
                return NotFound();

            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Voucher model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _voucherService.UpdateVoucherAsync(model);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật voucher: " + ex.Message);
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(string id)
        {
            if (string.IsNullOrEmpty(id))
                return NotFound();

            await _voucherService.ToggleVoucherStatusAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
