namespace QuanLyBanHangDienTu.WebApp.Models.Statistical
{
    public class StatisticalViewModel
    {
        // Thông tin cơ bản về sản phẩm
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Image { get; set; }

        // Số lượng và doanh thu - tổng cộng
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal TotalCost { get; set; }
        public decimal TotalProfit => TotalRevenue - TotalCost;

        // Phân tách theo mã giảm giá
        public int QuantityWithCoupon { get; set; }
        public int QuantityWithoutCoupon { get; set; }

        public decimal RevenueWithCoupon { get; set; }
        public decimal RevenueWithoutCoupon { get; set; }

        public decimal CostWithCoupon { get; set; }
        public decimal CostWithoutCoupon { get; set; }

        public decimal TotalDiscountCoupon { get; set; }

        public decimal ProfitWithCoupon => RevenueWithCoupon - CostWithCoupon - TotalDiscountCoupon;
        public decimal ProfitWithoutCoupon => RevenueWithoutCoupon - CostWithoutCoupon;

        // Sau khi áp giảm giá
        public decimal RevenueAfterDiscount => TotalRevenue - TotalDiscountCoupon;
        public decimal ProfitAfterDiscount => RevenueAfterDiscount - TotalCost;

        // Thời gian bán hàng
        public DateTime FirstSoldDate { get; set; }
        public DateTime LastSoldDate { get; set; }
    }
}
