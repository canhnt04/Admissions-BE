# CRM Admissions Backend

Đây là dự án Backend cho hệ thống CRM Tuyển sinh (Admissions), được xây dựng theo kiến trúc **Microservices** và **Clean Architecture**.

## 🏗 Cấu trúc dự án (Architecture)

Dự án được tổ chức theo cấu trúc thư mục tiêu chuẩn cho hệ thống Microservices:

`	ext
src/
├── gateway/
│   └── ApiGateway/          # Điểm vào duy nhất (entry point) định tuyến request đến các services
├── services/
│   ├── Auth/                # Dịch vụ xác thực và phân quyền (Authentication & Authorization)
│   ├── Driving/             # Dịch vụ quản lý tuyển sinh đào tạo lái xe
│   ├── Formal/              # Dịch vụ quản lý tuyển sinh hệ chính quy (Đại học, Cao đẳng...)
│   ├── LeadAssignment/      # Dịch vụ quản lý phân bổ khách hàng (Assignment Queue & SLA)
│   └── ShortTerm/           # Dịch vụ quản lý tuyển sinh hệ ngắn hạn
└── shared/                  # Các thành phần, core logic, và hạ tầng dùng chung cho toàn bộ dự án
    ├── Shared.Authentication/ # Xử lý JWT, Token Generation, cấu hình Authentication chung & ICurrentUserService
    ├── Shared.Common/         # Chứa Interface, Exception handling, Result wrapper dùng chung
    ├── Shared.Contracts/      # Định nghĩa các Events (Message Contracts) dùng cho RabbitMQ
    ├── Shared.Logging/        # Middleware & Utility cho Logging hệ thống
    └── Shared.Messaging/      # Cấu hình MassTransit (Event-Driven) & Outbox Pattern chung
`

Mỗi dịch vụ (service) trong thư mục src/services/ đều được triển khai độc lập theo mô hình **Clean Architecture**, bao gồm các lớp:
- ***.API**: Lớp giao tiếp với bên ngoài (Controllers, cấu hình Dependency Injection, Middleware).
- ***.Application**: Chứa logic ứng dụng, Use Cases, CQRS (Commands/Queries bằng MediatR), DTOs, và Interfaces.
- ***.Domain**: Chứa các Entities cốt lõi, Value Objects, Domain Events, và Domain Exceptions.
- ***.Infrastructure**: Tương tác với cơ sở dữ liệu (Entity Framework Core), gửi sự kiện thông qua Message Broker, triển khai các Outbox Entities.

## 🚀 Công nghệ sử dụng

- **Framework**: .NET 8 (C#)
- **Database**: Microsoft SQL Server (Mỗi Microservice có Database/DbContext riêng biệt)
- **Message Broker**: RabbitMQ tích hợp qua MassTransit (Có áp dụng Outbox Pattern để bảo đảm an toàn dữ liệu)
- **Gateway**: YARP / Ocelot
- **Containerization**: Docker & Docker Compose
- **Architecture**: Microservices, Clean Architecture, CQRS (MediatR), Domain-Driven Design (DDD) principles.
- **Authentication**: JWT Bearer Token (Thiết kế tập trung ICurrentUserService).

---

## 🛠 Hướng dẫn chạy dự án (How to run)

### Yêu cầu hệ thống (Prerequisites)
1. Cài đặt **Docker Desktop** (bắt buộc để chạy nhanh qua docker-compose).
2. Cài đặt **.NET 8 SDK**.
3. IDE: Visual Studio 2022, JetBrains Rider, hoặc VS Code.

### Cách 1: Chạy toàn bộ hệ thống bằng Docker Compose (Khuyên dùng)

Đây là cách nhanh nhất để khởi chạy toàn bộ database, message broker và các services.

1. Mở Terminal / PowerShell / Command Prompt tại thư mục gốc của dự án (nơi chứa file docker-compose.yml).
2. Chạy lệnh sau:
   ```bash
   docker-compose up -d --build
   ```
3. Sau khi Docker pull image và build thành công, các dịch vụ sẽ hoạt động ở các port sau:
   - **API Gateway**: http://localhost:5000 (Gửi toàn bộ request API qua đây)
   - **SQL Server**: localhost:1433 (SA Password: Your_Strong_Passw0rd!)
   - **RabbitMQ Management UI**: http://localhost:15672 (guest/guest)
4. Để dừng hệ thống:
   ```bash
   docker-compose down
   ```

### Cách 2: Chạy qua Visual Studio / Rider (Dành cho Development)

Nếu bạn muốn debug trực tiếp các services bằng IDE:

1. Khởi chạy các dịch vụ hạ tầng (Database & Message Broker) bằng lệnh:
   ```bash
   docker-compose up -d crm-sqlserver crm-rabbitmq
   ```
2. Mở file solution CrmAdmissions.slnx bằng Visual Studio 2022 hoặc JetBrains Rider.
3. Thiết lập **Multiple Startup Projects**:
   - Chuột phải vào Solution -> chọn Configure Startup Projects...
   - Chọn Multiple startup projects và set **Start** cho các project API và Gateway, ví dụ: Auth.API, Formal.API, ShortTerm.API, Driving.API, LeadAssignment.API và ApiGateway.
4. Nhấn F5 hoặc nút Run để khởi chạy các dịch vụ. Toàn bộ request sẽ đi qua cổng của ApiGateway.
