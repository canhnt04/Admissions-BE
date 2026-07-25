# CRM Admissions Backend

Đây là dự án Backend cho hệ thống CRM Tuyển sinh (Admissions), được xây dựng theo kiến trúc **Microservices** và **Clean Architecture**.

## 🏗 Cấu trúc dự án (Architecture)

Dự án bao gồm các dịch vụ (Services) chính sau:

- **`Auth.API`**: Dịch vụ xác thực và phân quyền (Authentication & Authorization).
- **`Formal.API`**: Dịch vụ quản lý tuyển sinh hệ chính quy (Đại học, Cao đẳng...).
- **`ShortTerm.API`**: Dịch vụ quản lý tuyển sinh hệ ngắn hạn.
- **`Driving.API`**: Dịch vụ quản lý tuyển sinh đào tạo lái xe.
- **`Crm.ApiGateway`**: API Gateway đóng vai trò là điểm vào (entry point) duy nhất cho toàn bộ hệ thống (định tuyến đến các services tương ứng).
- **`Crm.Application`, `Crm.Domain`, `Crm.Infrastructure`**: Các project chứa logic nghiệp vụ, model và hạ tầng dùng chung hoặc được tham chiếu bởi các services.

## 🚀 Công nghệ sử dụng

- **Framework**: .NET (C#)
- **Database**: Microsoft SQL Server
- **Message Broker**: RabbitMQ
- **Gateway**: YARP / Ocelot (Cấu hình qua `Crm.ApiGateway`)
- **Containerization**: Docker & Docker Compose

---

## 🛠 Hướng dẫn chạy dự án (How to run)

### Yêu cầu hệ thống (Prerequisites)
1. Cài đặt **Docker Desktop** (bắt buộc để chạy nhanh qua docker-compose).
2. Cài đặt **.NET SDK** (phiên bản phù hợp, khuyên dùng .NET 8).
3. IDE: Visual Studio 2022, JetBrains Rider, hoặc VS Code.

### Cách 1: Chạy toàn bộ hệ thống bằng Docker Compose (Khuyên dùng)

Đây là cách nhanh nhất để khởi chạy toàn bộ database, message broker và các services.

1. Mở Terminal / PowerShell / Command Prompt tại thư mục gốc của dự án (nơi chứa file `docker-compose.yml`).
2. Chạy lệnh sau:
   ```bash
   docker-compose up -d --build
   ```
3. Sau khi Docker pull image và build thành công, các dịch vụ sẽ hoạt động ở các port sau:
   - **API Gateway**: `http://localhost:5000` (Gửi toàn bộ request API qua đây)
   - **SQL Server**: `localhost:1433` (SA Password: `Your_Strong_Passw0rd!`)
   - **RabbitMQ Management UI**: `http://localhost:15672` (guest/guest)
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
2. Mở file solution `CrmAdmissions.slnx` bằng Visual Studio 2022 hoặc JetBrains Rider.
3. Thiết lập **Multiple Startup Projects**:
   - Chuột phải vào Solution -> chọn `Configure Startup Projects...`
   - Chọn `Multiple startup projects` và set **Start** cho các project: `Auth.API`, `Formal.API`, `ShortTerm.API`, `Driving.API`, và `Crm.ApiGateway`.
4. Nhấn `F5` hoặc nút Run để khởi chạy các dịch vụ. Toàn bộ request sẽ đi qua cổng của `Crm.ApiGateway`.
