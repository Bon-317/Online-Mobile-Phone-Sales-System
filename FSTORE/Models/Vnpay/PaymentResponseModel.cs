namespace FSTORE.Models.Vnpay
{
    public class PaymentResponseModel
    {
        // Mô tả đơn hàng (thường là nội dung bạn gửi trong OrderInfo)
        public string OrderDescription { get; set; }

        // Mã giao dịch tại hệ thống VNPay (vnp_TransactionNo)
        public string TransactionId { get; set; }

        // Mã đơn hàng của bạn (vnp_TxnRef)
        public string OrderId { get; set; }

        // Phương thức thanh toán (vnp_CardType / vnp_BankCode)
        public string PaymentMethod { get; set; }

        // Mã tham chiếu giao dịch hoặc ID ghi nhận nội bộ
        public string PaymentId { get; set; }

        // Mã phản hồi của VNPay (00 = thành công)
        public string VnPayResponseCode { get; set; }

        // Token hoặc checksum nếu cần xác minh (không bắt buộc)
        public string Token { get; set; }

        // ✅ Thuộc tính tiện dụng: kiểm tra thanh toán có thành công không
        public bool Success => VnPayResponseCode == "00";

        public decimal Amount { get; set; }
    }
}
