using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class ProductDetailLaptopModel
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public ProductModel? Product { get; set; }
        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        public CategoryModel? Category { get; set; }
        public int BrandId { get; set; }
        [ForeignKey("BrandId")]
        public BrandModel? Brand { get; set; }
        public string? GraphicsCardType { get; set; } // Card đồ hoạ
        public string? RAMCapacity { get; set; } // RAM
        public string? RAMType { get; set; } // Loại RAM
        public string? NumberOfRAMSlots { get; set; } // Số khe RAM
        public string? HardDrive {  get; set; } // Ổ cứng
        public string? ScreenSize { get; set; } // Kích thước màn hình
        public string? ScreenTechnology { get; set; } // Công nghệ màn hình
        public string? Battery { get; set; } // Pin
        public string? OperatingSystem { get; set; } // Hệ điều hành
        public string? ScreenResolution { get; set; } // Độ phân giải màn hình
        public string? CPUType { get; set; } // Loại CPU
        public string? Interface {  get; set; } // Cổng giao tiếp
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
