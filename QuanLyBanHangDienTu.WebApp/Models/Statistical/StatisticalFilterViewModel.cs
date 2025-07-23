namespace QuanLyBanHangDienTu.WebApp.Models.Statistical
{
    public class StatisticalFilterViewModel
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public List<StatisticalViewModel>? Statistics { get; set; }
    }
}
