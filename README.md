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
  - Đăng ký và đăng nhập bằng username hoặc email.
  - JWT Authentication với seed tài khoản admin (`admin@musicplay.local`) và user (`user@musicplay.local`). Mật khẩu được lưu dưới dạng hash MD5 của `123456` (`e10adc3949ba59abbe56e057f20f883e`).
  - Upload tệp âm thanh và lưu metadata vào MongoDB.
  - Streaming hỗ trợ `Range` (HTTP 206) để tua mượt.
  - API đề xuất track nổi bật, playlist và sitemap phục vụ SEO.

### Chạy backend

```bash
cd backend/MusicPlay.Api
# Cài đặt .NET 9 SDK (preview) nếu chưa có
# dotnet restore
# dotnet run --urls http://localhost:5174
```

> Lưu ý: Project sử dụng MongoDB, hãy đảm bảo dịch vụ đang chạy tại `mongodb://localhost:27017`.

## Frontend

- **Công nghệ:** React 18, Vite, Ant Design 5, React Query.
- **Điểm nhấn UI:**
  - Hero banner với waveform animation, tone tím than kết hợp điểm nhấn xanh cyan/hồng.
  - Component PlaylistShowcase với danh sách gợi ý.
  - Upload form hỗ trợ kéo thả.
  - SEO meta tags qua `react-helmet-async`.

### Chạy frontend

```bash
cd frontend
npm install
npm run dev
```

Ứng dụng sẽ truy cập API qua proxy `/api` trỏ tới `http://localhost:5174`.

## Tài khoản thử nghiệm

| Username | Email                 | Vai trò       | MD5("123456")              |
|----------|-----------------------|---------------|-----------------------------|
| admin    | admin@musicplay.local | Admin, User   | e10adc3949ba59abbe56e057f20f883e |
| user     | user@musicplay.local  | User          | e10adc3949ba59abbe56e057f20f883e |

## SEO & Streaming

- Endpoint `/api/seo/sitemap` trả về danh sách URL để tích hợp sitemap tự động.
- Range streaming `206` được xử lý tại `/api/tracks/{id}/stream` với header `Content-Range`, `Accept-Ranges`.

## Định hướng tiếp theo

- Tích hợp thực tế với dịch vụ email (SendGrid, AWS SES) để gửi email xác nhận.
- Bổ sung hệ thống gợi ý cá nhân hóa sử dụng machine learning.
- Hoàn thiện trang quản trị với biểu đồ và bảng duyệt nội dung.
