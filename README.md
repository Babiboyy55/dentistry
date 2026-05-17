# 🦷 Hệ thống Quản lý Nha khoa (Dental Clinic Management System)

Đây là dự án ứng dụng web quản lý phòng khám nha khoa được phát triển bằng **ASP.NET Core MVC**.

## 🚀 Các tính năng chính (Features)

- **Quản lý Nhân sự (Staff Management):** 
  - Xem danh sách nhân viên.
  - Thêm mới nhân viên (với mã ID được cấp tự động).
  - Cập nhật thông tin nhân viên.
- **Xác thực & Bảo mật (Authentication):** 
  - Đăng nhập an toàn.
- **Giao diện Quản trị (Admin Panel):** 
  - Dashboard thân thiện, dễ sử dụng cho các nghiệp vụ quản lý phòng khám.

## 🛠️ Công nghệ sử dụng (Tech Stack)

- **Backend:** C#, ASP.NET Core 8.0 (hoặc version tương ứng)
- **Mô hình kiến trúc:** MVC (Model-View-Controller)
- **Database:** Entity Framework Core (Code-First) & SQL Server
- **Frontend:** HTML5, CSS3, JS, Bootstrap, Razor Views (.cshtml)

## 📦 Hướng dẫn cài đặt (Installation)

1. **Clone repository:**
   ```bash
   git clone https://github.com/Babiboyy55/dentistry.git
   cd dentistry
   ```

2. **Cấu hình chuỗi kết nối Database:**
   - Mở file `Nhakhoa/appsettings.json`.
   - Cập nhật thông tin ở mục `ConnectionStrings:DefaultConnection` để trỏ tới local database (SQL Server) của bạn.

3. **Cập nhật Database (Migration):**
   - Mở **Package Manager Console (PMC)** trong Visual Studio và chọn Default project là `Nhakhoa`, chạy lệnh:
     ```powershell
     Update-Database
     ```
   - Hoặc sử dụng **.NET CLI** trong terminal:
     ```bash
     cd Nhakhoa
     dotnet ef database update
     ```

4. **Chạy ứng dụng:**
   - Mở file `Nhakhoa.slnx` bằng Visual Studio và nhấn `F5` hoặc `Ctrl + F5`.
   - Hoặc chạy bằng dòng lệnh:
     ```bash
     dotnet run
     ```

## 📂 Cấu trúc dự án (Project Structure)

- `Controllers/`: Chứa các bộ điều khiển xử lý logic nghiệp vụ (ví dụ: `AuthController`, `StaffController`).
- `Models/`: Các lớp thực thể (Entities) ánh xạ với bảng trong cơ sở dữ liệu (`User`, v.v.).
- `ViewModels/`: Các mô hình dữ liệu dùng để truyền tải giữa Controller và View.
- `Views/`: Các file giao diện người dùng (.cshtml) bao gồm Layout chung và các trang chức năng.
- `Data/`: Chứa `ApplicationDbContext` - cấu hình tương tác với cơ sở dữ liệu.
- `Migrations/`: Lịch sử các lần thay đổi thiết kế cơ sở dữ liệu.

---
*Dự án liên tục được cập nhật và hoàn thiện các tính năng mới phục vụ cho công tác quản lý phòng khám nha khoa hiệu quả nhất.*
