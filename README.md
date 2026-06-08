# 🦷 Hệ thống Quản lý Nha khoa | Dental Clinic Management System

[Tiếng Việt](#tiếng-viet) | [English](#english)

---

<a name="tiếng-viet"></a>
## 🇻🇳 Tiếng Việt

Đây là ứng dụng web quản lý phòng khám nha khoa được phát triển bằng **ASP.NET Core MVC**.

### 🚀 Các tính năng chính
- **Quản lý Nhân sự (Staff Management):**
  - Xem danh sách nhân viên.
  - Thêm mới nhân viên (với mã ID tự động sinh).
  - Cập nhật thông tin chi tiết nhân viên.
- **Xác thực & Bảo mật (Authentication):**
  - Đăng nhập an toàn và quản lý quyền hạn.
- **Quản lý Lương bác sĩ (Doctor Payroll):**
  - Thiết lập tiền công theo giờ, hệ số học hàm/học vị, hệ số làm việc cuối tuần.
  - Quản lý các ca bệnh phức tạp và hệ số bệnh nhân.
  - Tự động tính phiếu lương hàng tháng và báo cáo tổng hợp.
- **Giao diện Quản trị (Admin Panel):**
  - Bảng điều khiển (Dashboard) trực quan và hiện đại cho các nghiệp vụ quản lý phòng khám.

### 🛠️ Công nghệ sử dụng
- **Backend:** C#, ASP.NET Core 10.0 / 8.0 (MVC architecture)
- **Database:** Entity Framework Core (Code-First) & SQL Server
- **Frontend:** HTML5, CSS3, JS, Bootstrap 5, Razor Views (.cshtml)

### 📦 Hướng dẫn cài đặt
1. **Clone repository:**
   ```bash
   git clone https://github.com/Babiboyy55/dentistry.git
   cd dentistry
   ```
2. **Cấu hình chuỗi kết nối Database:**
   - Mở file `Nhakhoa/appsettings.json`.
   - Cập nhật `ConnectionStrings:DefaultConnection` trỏ tới SQL Server local của bạn.
3. **Cập nhật Database (Migration):**
   - Sử dụng **Package Manager Console (PMC)** trong Visual Studio (chọn Default project là `Nhakhoa`):
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

### 📂 Cấu trúc dự án
- `Controllers/`: Xử lý logic nghiệp vụ và điều hướng (ví dụ: `AuthController`, `PayrollController`).
- `Models/`: Các lớp thực thể (Entities) ánh xạ với bảng trong cơ sở dữ liệu (`User`, `DoctorSalaryConfig`, v.v.).
- `ViewModels/`: Các mô hình dữ liệu dùng để truyền tải giữa Controller và View.
- `Views/`: Các file giao diện người dùng (.cshtml) bao gồm Layout chung và các trang chức năng.
- `Data/`: Chứa `ApplicationDbContext` - cấu hình tương tác với cơ sở dữ liệu.
- `Migrations/`: Lịch sử các lần thay đổi thiết kế cơ sở dữ liệu.

---

<a name="english"></a>
## 🇬🇧 English

This is a dental clinic management web application built with **ASP.NET Core MVC**.

### 🚀 Key Features
- **Staff Management:**
  - View staff directory.
  - Create new staff members with auto-generated IDs.
  - Update detailed staff information.
- **Authentication & Security:**
  - Secure login and role/permission management.
- **Doctor Payroll Management:**
  - Configure base hourly rates, academic degree coefficients, and weekend multipliers.
  - Manage complex sessions and patient coefficients.
  - Automatic monthly salary slip generation and aggregate reports.
- **Admin Dashboard:**
  - Clean and intuitive dashboard for clinic administration tasks.

### 🛠️ Tech Stack
- **Backend:** C#, ASP.NET Core 10.0 / 8.0 (MVC architecture)
- **Database:** Entity Framework Core (Code-First) & SQL Server
- **Frontend:** HTML5, CSS3, JS, Bootstrap 5, Razor Views (.cshtml)

### 📦 Installation
1. **Clone the repository:**
   ```bash
   git clone https://github.com/Babiboyy55/dentistry.git
   cd dentistry
   ```
2. **Configure Database Connection String:**
   - Open `Nhakhoa/appsettings.json`.
   - Update `ConnectionStrings:DefaultConnection` to point to your local SQL Server instance.
3. **Update Database (Migration):**
   - Using **Package Manager Console (PMC)** in Visual Studio (select `Nhakhoa` as the default project):
     ```powershell
     Update-Database
     ```
   - Or using **.NET CLI** in terminal:
     ```bash
     cd Nhakhoa
     dotnet ef database update
     ```
4. **Run the Application:**
   - Open the `Nhakhoa.slnx` solution file in Visual Studio and press `F5` or `Ctrl + F5`.
   - Or run via command line:
     ```bash
     dotnet run
     ```

### 📂 Project Structure
- `Controllers/`: Handles business logic and routing (e.g., `AuthController`, `PayrollController`).
- `Models/`: Database entity classes (`User`, `DoctorSalaryConfig`, etc.).
- `ViewModels/`: Data models for passing data between Views and Controllers.
- `Views/`: User interface templates (.cshtml) including layout and page templates.
- `Data/`: Contains `ApplicationDbContext` for DB interactions.
- `Migrations/`: Database schema version history.

---
*Dự án liên tục được cập nhật và hoàn thiện các tính năng mới phục vụ cho công tác quản lý phòng khám nha khoa hiệu quả nhất.*  
*This project is continuously updated and polished to provide the most efficient clinic management experience.*
