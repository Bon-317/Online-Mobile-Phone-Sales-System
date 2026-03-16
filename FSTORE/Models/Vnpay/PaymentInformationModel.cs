//namespace FSTORE.Models.Vnpay
//{
//    public class PaymentInformationModel
//    {
//        public string OrderId { get; set; } = string.Empty;
//        public string OrderType { get; set; } = "other";
//        public string OrderDescription { get; set; } = string.Empty;
//        public string Name { get; set; } = string.Empty;
//        public double Amount { get; set; }
//        public string Description { get; set; } = string.Empty;
//        public string ReturnUrl { get; set; } = string.Empty;

//        // ✅ Giữ lại UID (dùng để lưu/thanh toán cho người dùng)
//        public string Uid { get; set; } = string.Empty;
//    }
//}


using FSTORE.Models;

public class PaymentInformationModel
{
    public string OrderId { get; set; } = string.Empty;
    public string OrderType { get; set; } = "other";
    public string OrderDescription { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public double Amount { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public string Uid { get; set; } = string.Empty;

    // ✅ Thêm danh sách sản phẩm
    public List<CartItem> Items { get; set; } = new();
}
