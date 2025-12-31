# 🚀 Hệ Thống Quản Lý & Đánh Giá Sáng Kiến (Initiative Management System)

Dự án được xây dựng trên nền tảng **ASP.NET Core MVC** nhằm tự động hóa quy trình nộp, quản lý và chấm điểm các sáng kiến, đề tài nghiên cứu trong tổ chức.

## 🌟 Tính năng chính

- **Quản lý người dùng & Phân quyền:** Tích hợp .NET Core Identity (Admin, Giảng viên, Hội đồng, Phòng KHCN).
- **Quản lý Sáng kiến:** Cho phép giảng viên nộp hồ sơ, đính kèm tài liệu và theo dõi trạng thái.
- **Hội đồng Đánh giá:** Thành lập các hội đồng chấm điểm, phân công thành viên và quản lý các phiên chấm điểm chuyên biệt.
- **Hệ thống Tiêu chí Linh hoạt:** Thiết lập các bộ tiêu chí (Template) tính điểm hoặc duyệt/loại tùy theo từng giai đoạn.
- **Quy trình Phê duyệt:** Luồng xử lý từ bản nháp -> Chấm điểm -> Yêu cầu sửa đổi -> Kết quả cuối cùng.
- **Nhật ký Hệ thống (Audit Logs):** Theo dõi mọi thay đổi dữ liệu để đảm bảo tính minh bạch.

## 🛠 Công nghệ sử dụng

- **Framework:** .NET 8.0 (hoặc 6.0/7.0) ASP.NET Core MVC
- **Database:** SQL Server
- **ORM:** Entity Framework Core (Code First Approach)
- **Security:** ASP.NET Core Identity
- **UI/UX:** Bootstrap 5, jQuery, DataTables
- **Logging:** Database-level Audit Logs

## 📊 Sơ đồ Database (Tóm tắt)

Hệ thống bao gồm các nhóm bảng chính:
1. **Core:** `Users`, `Roles`, `Departments`
2. **Business:** `Initiatives`, `InitiativeFiles`, `InitiativeCategories`
3. **Evaluation:** `Boards`, `BoardMembers`, `EvaluationCriteria`, `EvaluationTemplates`
4. **Processing:** `InitiativeAssignments`, `EvaluationSessions`, `EvaluationDetails`, `FinalResults`
5. **System:** `SystemAuditLogs`

## ⚙️ Hướng dẫn cài đặt

### 1. Yêu cầu hệ thống
- .NET SDK (phiên bản 8.0 trở lên)
- SQL Server LocalDB hoặc SQL Server Management Studio (SSMS)
- Visual Studio 2022 hoặc VS Code

### 2. Cấu hình Database
Mở file `appsettings.json` và cập nhật chuỗi kết nối của bạn:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=InitiativeDB;Trusted_Connection=True;MultipleActiveResultSets=true"
}
