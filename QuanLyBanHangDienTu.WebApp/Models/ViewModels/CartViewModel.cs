namespace QuanLyBanHangDienTu.WebApp.Models.ViewModels
{
    public class CartViewModel
    {
        public List<CartModel>? Cart { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public UserInformationViewModel? userInformation { get; set; }
        public string CouponCode { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
        public decimal TotalAfterDiscount => TotalAmount - DiscountAmount;
    }
}
