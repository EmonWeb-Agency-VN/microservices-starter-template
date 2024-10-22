# Auth Service

[![Tiếng Anh](https://img.shields.io/badge/lang-english-blue.svg)](README.md) [![Tiếng Việt](https://img.shields.io/badge/lang-vietnamese-blue.svg)](README.vi.md)

## Thông tin cơ bản

- IDE: `Visual Studio`
- Language: `C#`
- Framework: `DOTNET`
- Template: `ASP.NET Core Web API (Native AOT)`
- Lib: `OpenAPI`
- Database: `MySQL`

## Tính năng cơ bản

### Đăng ký (Sign Up)

- Tạo tài khoản bằng email, số điện thoại.
- Xác thực tài khoản bằng mã OTP qua email.

**Các bước:**

1. Người dùng nhập email/số điện thoại và mật khẩu để đăng ký tài khoản.
2. Backend xác minh nếu email hoặc số điện thoại đã tồn tại.
3. Tạo tài khoản trong cơ sở dữ liệu với trạng thái chưa xác minh.
4. Backend gửi một mã OTP qua email.
5. Người dùng nhập mã OTP để xác thực.
6. Nếu OTP hợp lệ, tài khoản chuyển sang trạng thái xác minh.
7. Gửi phản hồi đăng ký thành công và người dùng có thể đăng nhập.

**Các API:**

- `POST /api/auth/signup` : Tạo tài khoản mới cho người dùng qua email hoặc số điện thoại.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)",
    "phone_number": "+84123456789",
    "password": "StrongPassword123"
    }
  - Response code:
    - 201
    - 400
- `POST /api/auth/verify` : Xác thực tài khoản người dùng bằng mã OTP.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)",
    "otp_code": "123456"
    }
  - Response code:
    - 200
    - 400

### Đăng nhập (Sign In)

- Đăng nhập bằng email, số điện thoại.
- Hỗ trợ lưu phiên đăng nhập (Remember Me) qua cookie hoặc JWT.

**Các bước:**

1. Người dùng nhập email/số điện thoại và mật khẩu để đăng nhập.
2. Backend xác thực thông tin đăng nhập, kiểm tra mật khẩu đã mã hóa (bcrypt).
3. Nếu thông tin đúng, tạo JWT token hoặc session.
   - Nếu nhớ phiên: Lưu JWT trong cookie với thời gian sống dài hơn.
   - Nếu không nhớ phiên: Lưu JWT trong localStorage hoặc sessionStorage.
4. Trả về JWT token và thông tin người dùng.

**Các API:**

- `POST /api/auth/login` : Đăng nhập vào hệ thống bằng email/số điện thoại và mật khẩu.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)",
    "password": "StrongPassword123",
    "remember_me": true
    }
  - Response code:
    - 201
    - 400
  - Response body
    {
    "access_token": "jwt_access_token",
    "refresh_token": "jwt_refresh_token",
    "expires_in": 3600
    }

### Quên mật khẩu (Forgot Password)

- Tính năng gửi email đặt lại mật khẩu.

**Các bước:**

1. Người dùng yêu cầu đặt lại mật khẩu bằng cách nhập email.
2. Backend kiểm tra nếu email tồn tại.
3. Tạo mã token hoặc liên kết đặt lại mật khẩu và gửi qua email.
4. Người dùng nhấn vào liên kết và nhập mật khẩu mới.
5. Backend xác thực token và cập nhật mật khẩu mới (mã hóa bcrypt).

**Các API:**

- `POST /api/auth/forgot-password` : Gửi email đặt lại mật khẩu cho người dùng.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)"
    }
  - Response code:
    - 200
    - 404
- `POST /api/auth/reset-password`: Đặt lại mật khẩu mới cho người dùng sau khi yêu cầu quên mật khẩu.
  - Request body
    {
    "reset_token": "password_reset_token",
    "new_password": "NewStrongPassword123"
    }
  - Response code:
    - 200
    - 400

### Đổi mật khẩu (Change Password)

- Cho phép người dùng thay đổi mật khẩu sau khi đăng nhập.
- Xác minh mật khẩu cũ trước khi cho phép thay đổi.

**Các bước:**

1. Người dùng đăng nhập và truy cập trang đổi mật khẩu.
2. Nhập mật khẩu cũ và mật khẩu mới.
3. Backend xác thực mật khẩu cũ.
4. Nếu đúng, mã hóa mật khẩu mới và cập nhật vào cơ sở dữ liệu.

**Các API:**

- `POST /api/auth/change-password`: Đổi mật khẩu cho người dùng đã đăng nhập.
  - Request body
    {
    "current_password": "OldPassword123",
    "new_password": "NewStrongPassword123"
    }
  - Response code:
    - 200
    - 400

### Đăng xuất (Sign Out)

- Đăng xuất khỏi hệ thống và xóa token khỏi trình duyệt/thiết bị.
- Hỗ trợ đăng xuất trên nhiều thiết bị.

**Các bước:**

1. Người dùng nhấn đăng xuất.
2. Backend xóa JWT token khỏi danh sách phiên hoạt động (nếu có lưu).
3. Frontend xóa token JWT khỏi cookie hoặc localStorage.
4. Điều hướng người dùng về trang đăng nhập.

**Các API:**

- `POST /api/auth/logout`: Đăng xuất khỏi hệ thống, vô hiệu hóa token hiện tại.
  - Response code:
    - 200

### Làm mới mã token (Token Refresh)

- Cung cấp cơ chế làm mới mã token JWT để duy trì phiên người dùng mà không cần đăng nhập lại.
- Hỗ trợ refresh token và access token tách biệt.

**Các bước:**

1. Người dùng yêu cầu làm mới token khi access token hết hạn.
2. Backend kiểm tra refresh token.
3. Nếu hợp lệ, tạo access token mới và trả về cho người dùng.
4. Lưu access token mới ở frontend (cookie hoặc localStorage).

**Các API:**

- `POST /api/auth/refresh-token`: Làm mới access token bằng refresh token.
  - Request body
    {
    "refresh_token": "jwt_refresh_token"
    }
  - Response code:
    - 200
    - 403
  - Response body
    {
    "access_token": "new_jwt_access_token",
    "expires_in": 3600
    }

### Bảo vệ chống brute force (Brute Force Protection)

- Hạn chế số lần gửi từ frontend bằng hcaptcha
- Giới hạn số lần đăng nhập thất bại liên tiếp.
- Tạm thời khóa tài khoản sau nhiều lần thất bại.

**Các bước:**

1. Theo dõi số lần đăng nhập thất bại liên tiếp từ cùng một IP hoặc tài khoản.
2. Nếu vượt quá 3 lần, tạm thời khóa tài khoản 15 phút -> 1 giờ -> 24 giờ -> khóa vĩnh viễn.

### Quản lý phiên (Session Management)

- Hiển thị và quản lý các phiên đăng nhập hiện tại của người dùng.
- Hỗ trợ đăng xuất khỏi tất cả các thiết bị hoặc chỉ một phiên cụ thể.
- Tự động hết hạn phiên sau thời gian dài không hoạt động.

**Các bước:**

1. Hiển thị danh sách các phiên hoạt động (dựa trên JWT hoặc session).
2. Người dùng có thể chọn đăng xuất khỏi một phiên cụ thể hoặc tất cả phiên.
3. Backend hủy token tương ứng và xóa phiên khỏi danh sách.

**Các API:**

- `GET /api/auth/sessions`: Lấy danh sách các phiên đăng nhập hiện tại của người dùng.
  - Response code:
    - 200
  - Response body
    [
    {
    "session_id": "session_1",
    "device_info": "Chrome, Windows",
    "ip_address": "123.123.123.123",
    "login_time": "2024-10-18T12:34:56Z"
    },
    {
    "session_id": "session_2",
    "device_info": "Safari, MacOS",
    "ip_address": "124.124.124.124",
    "login_time": "2024-10-19T08:21:33Z"
    }
    ]
- `POST /api/auth/sessions/revoke`: Đăng xuất khỏi phiên cụ thể hoặc tất cả các phiên.
  - Request body
    {
    "session_id": "session_1" // Để đăng xuất khỏi phiên cụ thể, nếu không gửi sẽ đăng xuất tất cả
    }
  - Response code:
    - 200

### Kiểm tra quyền (Access Control)

- Hỗ trợ phân quyền người dùng với các vai trò khác nhau (Admin, User).
- Tích hợp các kiểm tra quyền khi truy cập các API hoặc tài nguyên.

**Các bước:**

1. Backend kiểm tra JWT token để xác định vai trò người dùng.
2. Dựa trên vai trò, kiểm tra quyền truy cập API hoặc tài nguyên cụ thể.
3. Nếu không đủ quyền, trả về lỗi 403 (Forbidden).

### Nhật ký hoạt động (Activity Log)

- Ghi lại lịch sử đăng nhập, đăng xuất, và các thay đổi quan trọng (mật khẩu, thông tin tài khoản).

**Các bước:**

1. Ghi lại mỗi sự kiện đăng nhập, đăng xuất, thay đổi mật khẩu hoặc thay đổi vai trò của người dùng.
2. Lưu trữ trong cơ sở dữ liệu hoặc hệ thống log chuyên dụng.
3. Cung cấp giao diện cho quản trị viên xem xét nhật ký hoạt động.

### Chính sách bảo mật (Security Policies)

- Bắt buộc mật khẩu phức tạp (yêu cầu về độ dài, ký tự đặc biệt).
- Hỗ trợ mã hóa mật khẩu với các thuật toán an toàn (bcrypt).

**Các bước:**

1. Khi người dùng đăng ký hoặc thay đổi mật khẩu, kiểm tra xem mật khẩu có đủ độ phức tạp hay không (yêu cầu về ký tự đặc biệt, chữ số, độ dài).
2. Mã hóa mật khẩu trước khi lưu vào cơ sở dữ liệu sử dụng bcrypt.

## Tính năng nâng cao (Theo dự án)

### Đăng ký (Sign Up)

- Hỗ trợ OAuth (Google, Facebook) để đăng ký.

### Đăng nhập (Sign In)

- Đăng nhập bằng OAuth.
- Xác thực đa yếu tố (Multi-Factor Authentication - MFA).

### Quên mật khẩu (Forgot Password)

- Liên kết hoặc mã OTP để xác thực và cho phép người dùng tạo mật khẩu mới.

### Đổi mật khẩu (Change Password)

- Cho phép người dùng thay đổi mật khẩu sau khi đăng nhập.
- Xác minh mật khẩu cũ trước khi cho phép thay đổi.

### Đăng xuất (Sign Out)

- Hỗ trợ đăng xuất trên nhiều thiết bị.

### Bảo vệ chống brute force (Brute Force Protection)

- Gửi cảnh báo qua email nếu có hoạt động đáng ngờ.

### Quản lý phiên (Session Management)

- Hiển thị và quản lý các phiên đăng nhập hiện tại của người dùng.
- Hỗ trợ đăng xuất khỏi tất cả các thiết bị hoặc chỉ một phiên cụ thể.
- Tự động hết hạn phiên sau thời gian dài không hoạt động.

### Xác thực hai yếu tố (Two-Factor Authentication - 2FA)

- Xác thực qua SMS, email, hoặc ứng dụng OTP (Microsoft Authenticator).
- Tùy chọn bắt buộc 2FA cho các tài khoản có quyền Admin.

### Quản lý khóa API (API Key Management)

- Cấp phát và quản lý khóa API cho các ứng dụng hoặc dịch vụ bên ngoài.
- Cho phép giới hạn quyền truy cập dựa trên khóa API.

## Cơ sở dữ liệu

- Xem file [DATABASE.md](../DATABASE.md)
