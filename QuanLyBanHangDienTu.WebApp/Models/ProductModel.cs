using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using QuanLyBanHangDienTu.WebApp.Repository.Validation;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class ProductModel
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string Version { get; set; }
        public decimal Price { get; set; }
        public decimal ImportPrice { get; set; }
        public int Quantity { get; set; }
        public int Sold { get; set; }
        public string Slug { get; set; }
        public int BrandId { get; set; }
        public int CategoryId { get; set; }
        public int CompanyId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [ForeignKey("CategoryId")]
        public CategoryModel? Category { get; set; }
        [ForeignKey("BrandId")]
        public BrandModel? Brand { get; set; }

        [NotMapped]
        [FileExtension]
        public IFormFile? ImageUpload { get; set; }

        public ProductDetailPhoneModel? ProductDetailPhones { get; set; }
        public ProductDetailLaptopModel? ProductDetailLaptops { get; set; }
        public ICollection<OrderDetailModel> OrderDetails { get; set; } = new List<OrderDetailModel>();

    }
}
