using FSTORE.Library;
using FSTORE.Models.Vnpay;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;

namespace FSTORE.Services.Vnpay
{
    public class VnPayService : IVnPayService
    {
        private readonly IConfiguration _configuration;

        public VnPayService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        //public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        //{
        //    var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
        //    var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
        //    var tick = DateTime.Now.Ticks.ToString();
        //    var pay = new VnPayLibrary();

        //    // Lấy URL callback đúng controller Payment
        //    var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

        //    pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
        //    pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
        //    pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
        //    pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
        //    pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
        //    pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
        //    pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
        //    pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);

        //    // ✅ Giữ OrderDescription sạch, không thêm UID ở đây nữa
        //    pay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
        //    pay.AddRequestData("vnp_OrderType", model.OrderType);
        //    pay.AddRequestData("vnp_ReturnUrl", urlCallBack);
        //    pay.AddRequestData("vnp_TxnRef", tick);

        //    return pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
        //}



        public string CreatePaymentUrl(PaymentInformationModel model, HttpContext context)
        {
            var timeZoneById = TimeZoneInfo.FindSystemTimeZoneById(_configuration["TimeZoneId"]);
            var timeNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZoneById);
            var pay = new VnPayLibrary();

            var urlCallBack = _configuration["Vnpay:PaymentBackReturnUrl"];

            pay.AddRequestData("vnp_Version", _configuration["Vnpay:Version"]);
            pay.AddRequestData("vnp_Command", _configuration["Vnpay:Command"]);
            pay.AddRequestData("vnp_TmnCode", _configuration["Vnpay:TmnCode"]);
            pay.AddRequestData("vnp_Amount", ((int)model.Amount * 100).ToString());
            pay.AddRequestData("vnp_CreateDate", timeNow.ToString("yyyyMMddHHmmss"));
            pay.AddRequestData("vnp_CurrCode", _configuration["Vnpay:CurrCode"]);
            pay.AddRequestData("vnp_IpAddr", pay.GetIpAddress(context));
            pay.AddRequestData("vnp_Locale", _configuration["Vnpay:Locale"]);

            // ✅ Dùng OrderId đã sinh bằng Ticks
            pay.AddRequestData("vnp_TxnRef", model.OrderId);
            pay.AddRequestData("vnp_OrderInfo", model.OrderDescription);
            pay.AddRequestData("vnp_OrderType", model.OrderType);
            pay.AddRequestData("vnp_ReturnUrl", urlCallBack);

            return pay.CreateRequestUrl(_configuration["Vnpay:BaseUrl"], _configuration["Vnpay:HashSecret"]);
        }


        public PaymentResponseModel PaymentExecute(IQueryCollection collections)
        {
            if (collections == null || collections.Count == 0)
                return null;

            var pay = new VnPayLibrary();
            var response = pay.GetFullResponseData(collections, _configuration["Vnpay:HashSecret"]);
            return response;
        }
    }
}
