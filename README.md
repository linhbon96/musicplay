# MusicPlay

MusicPlay là dự án streaming tập trung vào việc hỗ trợ các nhạc sĩ mới chia sẻ sáng tạo của mình. Hệ thống gồm backend .NET 9 sử dụng MongoDB và frontend React + Ant Design với giao diện lấy cảm hứng từ tone tím than.

## Cấu trúc dự án

```
backend/
  MusicPlay.Api/           # ASP.NET Core Minimal API phục vụ dữ liệu và streaming
frontend/
  src/                     # Ứng dụng React sử dụng Vite + Ant Design
```

## Backend

- **Công nghệ:** ASP.NET Core (net9.0), MongoDB Driver.
- **Tính năng chính:**
  - Đăng ký và đăng nhập bằng username hoặc email với email xác nhận thông qua SendGrid.
  - JWT Authentication với seed tài khoản admin (`admin@musicplay.local`) và user (`user@musicplay.local`). Mật khẩu được lưu dưới dạng hash MD5 của `123456` (`e10adc3949ba59abbe56e057f20f883e`).
  - Upload tệp âm thanh, lưu metadata vào MongoDB và phân quyền duyệt nội dung cho admin.
  - Streaming hỗ trợ `Range` (HTTP 206) để tua mượt và ghi nhận lượt nghe phục vụ gợi ý.
  - Thuật toán đề xuất cá nhân hóa dựa trên ML.NET (content-based) trả về `/api/tracks/personalized`.
  - Dashboard admin với biểu đồ upload 7 ngày, bảng duyệt track và API `/api/admin/overview`.

### Khởi tạo cơ sở dữ liệu MongoDB

Script `backend/MusicPlay.Api/Database/init-mongo.js` giúp tạo database, collection, bucket GridFS và index cần thiết.

```bash
mongosh < backend/MusicPlay.Api/Database/init-mongo.js
```

### Chạy backend

```bash
cd backend/MusicPlay.Api
# Cài đặt .NET 9 SDK (preview) nếu chưa có
# dotnet restore
# dotnet run --urls http://localhost:5174
```

> Lưu ý: Project sử dụng MongoDB, hãy đảm bảo dịch vụ đang chạy tại `mongodb://localhost:27017`.

#### Cấu hình SendGrid

- Tạo API Key SendGrid và cung cấp qua biến môi trường `Email__SendGrid__ApiKey` hoặc cập nhật `appsettings.json`.
- Có thể điều chỉnh địa chỉ gửi (`Email__SendGrid__SenderEmail`) và tên hiển thị (`Email__SendGrid__SenderName`).
- `Email__FrontendBaseUrl` dùng để xây dựng link xác nhận email trong thông báo.

Nếu không cấu hình API key, hệ thống sẽ log cảnh báo và bỏ qua bước gửi email nhưng vẫn cho phép đăng ký.

## Frontend

- **Công nghệ:** React 18, Vite, Ant Design 5, React Query.
- **Điểm nhấn UI:**
  - Hero banner với waveform animation, tone tím than kết hợp điểm nhấn xanh cyan/hồng.
  - Component PlaylistShowcase với danh sách gợi ý và khu vực gợi ý cá nhân hóa.
  - Upload form hỗ trợ kéo thả.
  - SEO meta tags qua `react-helmet-async`.
  - Trang quản trị hiển thị biểu đồ, số liệu tổng quan và bảng duyệt track với hành động phê duyệt.

### Chạy frontend

```bash
cd frontend
npm install
npm run dev
```

Ứng dụng sẽ truy cập API qua proxy `/api` trỏ tới `http://localhost:5174`.

### Chạy toàn bộ stack bằng Docker Compose

```bash
docker compose up --build
```

- Frontend có sẵn tại `http://localhost:5173`.
- Backend có sẵn tại `http://localhost:5174` (Swagger ở `/swagger`).
- MongoDB chạy ở `mongodb://localhost:27017`; script khởi tạo sẽ được chạy tự động nhờ bind `init-mongo.js`.

Trong lần chạy đầu, Docker sẽ cài đặt dependencies cần thiết (NuGet, npm). Các volume đặt tên sẵn giúp cache những phần này giữa các lần khởi động.

## Tài khoản thử nghiệm

| Username | Email                 | Vai trò       | MD5("123456")              |
|----------|-----------------------|---------------|-----------------------------|
| admin    | admin@musicplay.local | Admin, User   | e10adc3949ba59abbe56e057f20f883e |
| user     | user@musicplay.local  | User          | e10adc3949ba59abbe56e057f20f883e |

## SEO & Streaming

- Endpoint `/api/seo/sitemap` trả về danh sách URL để tích hợp sitemap tự động.
- Range streaming `206` được xử lý tại `/api/tracks/{id}/stream` với header `Content-Range`, `Accept-Ranges`.
