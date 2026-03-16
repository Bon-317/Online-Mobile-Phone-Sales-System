using Microsoft.AspNetCore.Mvc;
using FSTORE.Models;

namespace FSTORE.Controllers
{
    [Route("Session")]
    public class SessionController : Controller
    {
        [HttpPost("SetOtp")]
        public IActionResult SetOtp([FromBody] OtpRequest request)
        {
            if (string.IsNullOrEmpty(request.Otp))
                return BadRequest("Thiếu mã OTP");

            HttpContext.Session.SetString("otp", request.Otp);
            return Ok(new { message = "OTP đã được lưu vào session." });
        }
    }
}
