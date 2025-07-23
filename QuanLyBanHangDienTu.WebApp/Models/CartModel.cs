using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class CartModel
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }

        [ForeignKey("UserId")]
        public UserModel? User { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string Image { get; set; }
        public decimal Total => Price * Quantity;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public CartModel() { }

        public CartModel(ProductModel product, string userId, string userName)
        {
            ProductId = product.Id;
            ProductName = product.Name;
            Price = product.Price;
            Quantity = 1;
            Image = product.Image;
            UserId = userId;
            UserName = userName;
        }
    }
}
