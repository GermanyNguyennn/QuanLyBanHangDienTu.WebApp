namespace QuanLyBanHangDienTu.WebApp.Models.ViewModels
{
    public class EmailOrderViewModel
    {
        public string? UserName { get; set; }
        public string? OrderCode { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public List<EmailOrderItemViewModel>? Items { get; set; }
        public decimal TotalAmount { get; set; }
        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalAfterDiscount => TotalAmount - DiscountAmount;
    }
}
