# 🚀 Hướng dẫn chạy dự án ASP.NET Core MVC

Dưới đây là hướng dẫn từng bước để tải về và chạy dự án ASP.NET Core MVC sau khi clone từ GitHub.

## 🧰 1. Yêu cầu phần mềm

Đảm bảo bạn đã cài đặt các phần mềm sau:

- ✅ [.NET SDK (Core)](https://dotnet.microsoft.com/en-us/download): phiên bản tương thích với dự án (VD: .NET 6 hoặc .NET 7)
- ✅ [Visual Studio 2022 trở lên](https://visualstudio.microsoft.com/) (bản Community là miễn phí)  
  ➤ Khi cài đặt, chọn workload **ASP.NET and web development** và **.NET Core cross-platform development**
- ✅ [SQL Server Express / Developer](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
- ✅ [SQL Server Management Studio (SSMS)](https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)

## 📦 2. Clone dự án từ GitHub

```bash
git clone https://github.com/your-username/your-repo-name.git
cd your-repo-name
```

> Hoặc tải trực tiếp dưới dạng `.zip` từ GitHub, sau đó giải nén.

## ⚙️ 3. Cấu hình kết nối Cơ sở dữ liệu

1. Mở file `appsettings.json`
2. Tìm đến chuỗi `"ConnectionStrings"` và chỉnh sửa lại theo cấu hình SQL của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=YourDbName;Trusted_Connection=True;MultipleActiveResultSets=true"
}
```

> 🔐 Nếu bạn dùng SQL Server có username/password, thay `Trusted_Connection=True` bằng:
```json
"Server=.;Database=YourDbName;User Id=sa;Password=yourpassword;"
```

## 📂 4. Restore các gói NuGet

Mở project bằng **Visual Studio** → nhấn `Ctrl + Shift + B` hoặc chạy lệnh sau trong thư mục dự án:

```bash
dotnet restore
```

## 🗃️ 5. Tạo Database & Migration

Mở **Package Manager Console** trong Visual Studio (hoặc Terminal) và chạy các lệnh sau:

```bash
# Tạo file Migration (nếu chưa có)
Add-Migration InitialCreate

# Áp dụng Migration để tạo CSDL
Update-Database
```

> ✅ Nếu dự án đã có sẵn Migration, bạn chỉ cần chạy:
```bash
Update-Database
```

> 📝 Nếu sử dụng `dotnet ef` CLI, bạn cần cài đặt:
```bash
dotnet tool install --global dotnet-ef
```
Rồi chạy:
```bash
dotnet ef database update
```

## ▶️ 6. Chạy ứng dụng

- Nhấn `F5` để chạy bằng Visual Studio (Debug)
- Hoặc dùng CLI:

```bash
dotnet run
```

Ứng dụng sẽ khởi chạy.

---

## ✅ Một số lỗi thường gặp

| Vấn đề | Giải pháp |
|-------|-----------|
| Không kết nối được DB | Kiểm tra lại `appsettings.json` và SQL Server đã bật chưa |
| Thiếu Migration | Chạy `Add-Migration InitialCreate` |
| Lỗi EF Core chưa cài | Cài EF Core bằng NuGet hoặc lệnh `dotnet add package` |

---
