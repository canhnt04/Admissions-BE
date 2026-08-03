# CrmAdmissions - Hệ Thống CRM Tuyển Sinh

Dự án CRM Tuyển sinh được xây dựng dựa trên kiến trúc **Microservices** & **Clean Architecture**, phục vụ cho 3 mảng nghiệp vụ chính: **Đào tạo Chính Quy**, **Đào tạo Ngắn Hạn**, và **Đào tạo Lái Xe**.

## 🏗 Kiến trúc Hệ thống (Architecture)
Hệ thống được chia nhỏ thành các domain services độc lập để dễ dàng maintain và scale:

- **Customer Service** (`Customer.API`): Quản lý dữ liệu gốc của khách hàng (Thông tin liên hệ, nguồn, ...). Phân tách rõ ràng dữ liệu domain chung và domain cụ thể của từng nhánh.
- **Formal Service** (`Formal.API`): Xử lý nghiệp vụ riêng cho nhánh Đào tạo Chính quy.
- **ShortTerm Service** (`ShortTerm.API`): Xử lý nghiệp vụ riêng cho nhánh Đào tạo Ngắn hạn.
- **Driving Service** (`Driving.API`): Xử lý nghiệp vụ riêng cho nhánh Đào tạo Lái xe.
- **Lead Assignment Service** (`LeadAssignment.API`): Xử lý logic chia khách (lead) tự động cho nhân viên tư vấn dựa trên SLA, Queue, và thuật toán Round-Robin. Theo dõi và thu hồi lead nếu nhân viên không liên hệ đúng hạn.
- **Auth Service** (`Auth.API`): Identity Provider, quản lý xác thực (Authentication), phân quyền (Role-based JWT) và quản lý User.

## 🚀 Các Công nghệ Sử dụng
- **.NET 8** (ASP.NET Core Web API)
- **Entity Framework Core 8** (SQL Server)
- **MassTransit & RabbitMQ**: Giao tiếp bất đồng bộ (Event-Driven Architecture) bằng Publish/Subscribe. Áp dụng mẫu **Transactional Outbox Pattern** để đảm bảo tính toàn vẹn dữ liệu khi gửi event.
- **gRPC**: Giao tiếp đồng bộ tốc độ cao giữa các services (ví dụ: Fetch thông tin User/Role từ Auth Service).
- **Docker & Docker Compose**: Đóng gói các infrastructure dependencies (SQL Server, RabbitMQ, Seq,...).

## ⚙️ Tính năng cốt lõi đã hoàn thiện
1. **Quản lý Lead (Khách hàng)**: 
   - Seed dữ liệu và tạo lead mới từ `Customer.API`.
   - Publish sự kiện `CustomerCreatedEvent` đến các service nghiệp vụ và `LeadAssignment`.
2. **Giao Lead Tự động (Auto Assignment)**:
   - Nhân viên tư vấn có thể `Check-in` / `Check-out` để nhận lead.
   - Lead mới được tự động gán cho nhân viên (Round-Robin).
   - Có hệ thống Worker (SlaMonitorWorker) chạy ngầm kiểm tra hạn chót xử lý lead. Nếu quá hạn (SLA violation), tự động thu hồi và gán cho người khác.
   - Đòi hỏi bằng chứng liên hệ (`ContactEvidence`).
3. **Đồng bộ User**:
   - Sao chép (Replica) dữ liệu User/Role từ `Auth.API` sang các service khác (như `LeadAssignment.API`) thông qua **gRPC** để phục vụ truy vấn nội bộ mà không cần call chéo liên tục.

## 🏃 Hướng dẫn chạy môi trường Local

1. Start Infrastructure (SQL Server, RabbitMQ) bằng Docker:
   ```bash
   docker-compose up -d
   ```

2. Cập nhật Database (EF Migrations) - Chạy trên từng Service:
   ```bash
   dotnet ef database update --project src/services/Auth/Auth.Infrastructure --startup-project src/services/Auth/Auth.API
   dotnet ef database update --project src/services/Customer/Customer.Infrastructure --startup-project src/services/Customer/Customer.API
   dotnet ef database update --project src/services/LeadAssignment/LeadAssignment.Infrastructure --startup-project src/services/LeadAssignment/LeadAssignment.API
   ```

3. Chạy các API Services:
   Cần chạy ít nhất các service sau để test luồng:
   - `Auth.API` (Port 5001 - gRPC Port 50011)
   - `Customer.API` (Port 5006)
   - `LeadAssignment.API` (Port 5005)

## 🗺 Road Map Tiếp theo
- Tích hợp gửi thông báo (Email / Zalo) khi gán lead thành công hoặc khi quá hạn SLA.
- Xử lý nghiệp vụ cấu hình tài khoản quản lý nhận lead nếu bị violation 3 lần.
- Xây dựng module Báo cáo (Dashboard) và Data Pipeline cho Insight Khách hàng bằng AI.
