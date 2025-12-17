using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class OrderModel
    {
        [Key]
        public int Id { get; set; }
        public string? OrderCode { get; set; }

        public string? UserId { get; set; }
        [ForeignKey("UserId")]
        public UserModel? User { get; set; }

        public string? UserName { get; set; }

        public int? CouponId { get; set; }
        [ForeignKey("CouponId")]
        public CouponModel? Coupon { get; set; }

        public string? CouponCode { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int Status { get; set; }
        public string? PaymentMethod { get; set; }
        public ICollection<OrderDetailModel> OrderDetail { get; set; } = new List<OrderDetailModel>();

        // Thông tin người nhận hàng được lưu tại thời điểm đặt hàng
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string District { get; set; }
        public string Ward { get; set; }
    }
}
