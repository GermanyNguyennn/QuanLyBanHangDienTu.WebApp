using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuanLyBanHangDienTu.WebApp.Models
{
    public class ProductDetailPhoneModel
    {
        [Key]
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
        public string? ScreenSize { get; set; } // Kích thước màn hình
        public string? DisplayTechnology { get; set; } // Công nghệ hiển thị
        public string? RearCamera { get; set; } // Camera sau
        public string? FrontCamera { get; set; } // Camera sau
        public string? ChipSet { get; set; } // Chipset
        public bool NFC { get; set; } // NFC
        public string? RAMCapacity { get; set; } // Phiên Bản RAM
        public string? InternalStorage { get; set; } // Bộ nhớ trong
        public string? SimCard { get; set; } // Thẻ sim
        public string? OperatingSystem { get; set; } // Hệ điều hành
        public string? DisplayResolution { get; set; } // Độ phân giải màn hình
        public string? DisplayFeatures { get; set; } // Tính năng màn hình
        public string? CPUType { get; set; } // Loại CPU
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
