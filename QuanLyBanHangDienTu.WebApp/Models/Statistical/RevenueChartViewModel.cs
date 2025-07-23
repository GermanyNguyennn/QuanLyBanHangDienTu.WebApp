namespace QuanLyBanHangDienTu.WebApp.Models.Statistical
{
    public class RevenueChartViewModel
    {
        public DateTime Date { get; set; }
        public decimal RevenueBeforeDiscount { get; set; }
        public decimal RevenueAfterDiscount { get; set; }

        public decimal ProfitBeforeDiscount { get; set; }
        public decimal ProfitAfterDiscount { get; set;}
    }
}
