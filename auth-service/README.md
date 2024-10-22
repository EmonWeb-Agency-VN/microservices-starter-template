# Auth Service

[![English](https://img.shields.io/badge/lang-english-blue.svg)](README.md) [![Vietnamese](https://img.shields.io/badge/lang-vietnamese-blue.svg)](README.vi.md)

## Basic information

- IDE: `Visual Studio`
- Language: `C#`
- Framework: `DOTNET`
- Template: `ASP.NET Core Web API (Native AOT)`
- Lib: `OpenAPI`
- Database: `MySQL`

## Basic features

### Sign Up

- Create an account with email, phone number.
- Verify account with OTP code via email.

**Steps:**

1. User enters email/phone number and password to register an account.
2. Backend verifies if email or phone number already exists.
3. Create account in database with unverified status.
4. Backend sends an OTP code via email.
5. User enters OTP code to verify.
6. If OTP is valid, account moves to verified status.
7. Send a successful registration response and the user can log in.

**APIs:**

- `POST /api/auth/signup` : Create a new account for the user via email or phone number.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)",
    "phone_number": "+84123456789",
    "password": "StrongPassword123"
    }
  - Response code:
    - 201
    - 400
- `POST /api/auth/verify` : Verify the user account using OTP code.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)",
    "otp_code": "123456"
    }
  - Response code:
    - 200
    - 400

### Sign In

- Sign in with email, phone number.
- Support saving login session (Remember Me) via cookie or JWT.

**Steps:**

1. User enters email/phone number and password to log in.
2. Backend authenticates login information, checks encrypted password (bcrypt).
3. If information is correct, create JWT token or session.

- If remember session: Save JWT in cookie with longer lifetime.
- If don't remember session: Save JWT in localStorage or sessionStorage.

4. Return JWT token and user information.

**APIs:**

- `POST /api/auth/login` : Log in to the system with email/phone number and password.
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

### Forgot Password

- Password reset email feature.

**Steps:**

1. User requests to reset password by entering email.
2. Backend checks if email exists.
3. Generate a token or password reset link and send it via email.
4. User clicks on the link and enters a new password.
5. Backend validates the token and updates the new password (bcrypt-encoded).

**APIs:**

- `POST /api/auth/forgot-password` : Sends a password reset email to the user.
  - Request body
    {
    "email": "[user@example.com](mailto:user@example.com)"
    }
  - Response code:
    - 200
    - 404
- `POST /api/auth/reset-password`: Resets a new password for the user after a forgot password request.
  - Request body
    {
    "reset_token": "password_reset_token",
    "new_password": "NewStrongPassword123"
    }
  - Response code:
    - 200
    - 400

### Change Password

- Allows users to change their password after logging in.
- Verify the old password before allowing changes.

**Steps:**

1. User logs in and accesses the change password page.
2. Enters the old password and new password.
3. Backend validates the old password.
4. If correct, encrypts the new password and updates it to the database.

**APIs:**

- `POST /api/auth/change-password`: Change the password for a logged in user.
  - Request body
    {
    "current_password": "OldPassword123",
    "new_password": "NewStrongPassword123"
    }
  - Response code:
    - 200
    - 400

### Sign Out

- Log out of the system and delete the token from the browser/device
- Support log out on multiple devices.

**Steps:**

1. User clicks log out.
2. Backend deletes JWT token from session list (if saved).
3. Frontend deletes JWT token from cookie or localStorage.
4. Redirects user to login page.

**APIs:**

- `POST /api/auth/logout`: Logs out of the system, invalidates current token.
  - Response code:
    - 200

### Refresh token

- Provides a mechanism to refresh JWT token to maintain user session without re-login.
- Supports separate refresh token and access token.

**Steps:**

1. User requests to refresh token when access token expires.
2. Backend checks refresh token.
3. If valid, generate new access token and return to user.
4. Save new access token in frontend (cookie or localStorage).

**APIs:**

- `POST /api/auth/refresh-token`: Refresh access token using refresh token.
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

### Brute Force Protection

- Limit the number of submissions from the frontend using hcaptcha
- Limit the number of failed login attempts

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
      consecutive.

- Temporarily lock the account after multiple failures.

**Steps:**

1. Track the number of consecutive failed logins from the same IP or account.
2. If it exceeds 3 times, temporarily lock the account for 15 minutes -> 1 hour -> 24 hours -> permanently lock.

### Session Management

- Display and manage the user's current login sessions.
- Support logging out of all devices or just a specific session.
- Automatically expire sessions after a long period of inactivity.

**Steps:**

1. Display a list of active sessions (based on JWT or session).
2. The user can choose to log out of a specific session or all sessions.
3. The backend destroys the corresponding token and removes the session from the list.

**APIs:**

- `GET /api/auth/sessions`: Get a list of the user's current login sessions.
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
- `POST /api/auth/sessions/revoke`: Logs out of a specific session or all sessions.
  - Request body
    {
    "session_id": "session_1" // To log out of a specific session, if not sent, log out all
    }
  - Response code:
    - 200

### Check permissions (Access Control)

- Support user authorization with different roles (Admin, User).
- Integrate permission checks when accessing APIs or resources.

**Steps:**

1. Backend checks JWT token to determine user role.
2. Based on the role, check access to specific API or resource.
3. If not authorized, return error 403 (Forbidden).

### Activity Log

- Record login, logout, and important changes (password, account information).

**Steps:**

1. Record each login, logout, password change or user role change event.
2. Store in a database or dedicated log system.
3. Provide an interface for administrators to review the activity log.

### Security Policies

- Require complex passwords (requirements for length, special characters).
- Support password encryption with secure algorithms (bcrypt).

**Steps:**

1. When a user registers or changes a password, check if the password is complex enough (requirements for special characters, numbers, length).
2. Encrypt the password before saving it to the database using bcrypt.

## Advanced Features (By Project)

### Sign Up

- Support OAuth (Google, Facebook) for registration.

### Sign In

- Log in with OAuth.
- Multi-Factor Authentication (MFA).

### Forgot Password

- A link or OTP code to authenticate and allow the user to create a new password.

### Change Password

- Allows the user to change the password after logging in.
- Verify the old password before allowing the change.

### Sign Out

- Supports logging out on multiple devices.

### Brute Force Protection

- Sends email alerts if there is suspicious activity.

### Session Management

- Displays and manages the user's current login sessions.
- Supports logging out of all devices or just a specific session.
- Automatically expires sessions after a long period of inactivity.

### Two-Factor Authentication (2FA)

- Authentication via SMS, email, or OTP app (Microsoft Authenticator).
- Optionally required 2FA for accounts with Admin rights.

### API Key Management

- Issue and manage API keys for external applications or services.
- Allow access restrictions based on API keys.

## Database

- View file [DATABASE.md](../DATABASE.md)
