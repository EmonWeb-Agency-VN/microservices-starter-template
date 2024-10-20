# User Service

## Thông tin

- IDE: `Visual Studio`
- Language: `C#`
- Framework: `DOTNET`
- Template: `ASP.NET Core Web API (Native AOT)`
- Lib: `OpenAPI`
- Database: `MySQL`

## Tính năng

### User Management & Settings

**Profile Update**

Người dùng có thể cập nhật thông tin cá nhân như tên, email, số điện thoại, và avatar. Các thay đổi này giúp người dùng duy trì hồ sơ của mình, đồng thời có thể phù hợp với các yêu cầu quản lý hoặc tương tác của hệ thống.

**Cách triển khai**:

- Các trường chính cần cập nhật: tên, email, số điện thoại, ảnh đại diện (avatar).
- Quy trình bảo mật: Khi thay đổi email hoặc số điện thoại, có thể yêu cầu xác thực thông qua OTP hoặc email xác nhận.
- API: `/api/users/update-profile` (PATCH).

**User Preferences**

Người dùng có thể lưu các tùy chọn cá nhân để điều chỉnh trải nghiệm theo ý muốn, ví dụ như ngôn ngữ, chế độ hiển thị sáng/tối (dark/light mode), hoặc thông báo mà họ muốn nhận.

**Cách triển khai**:

- Lưu các tùy chọn vào trường JSON (`preferences`).
- Tùy chọn phổ biến:
  - **Theme**: Dark mode hoặc light mode.
  - **Language**: Ngôn ngữ ưa thích (ví dụ: English, Vietnamese).
  - **Notification preferences**: Tùy chọn về cách nhận thông báo (email, in-app).
- API: `/api/users/preferences` (PATCH/GET).

**3. Account Deactivation/Deletion**

Cho phép người dùng tự yêu cầu vô hiệu hóa hoặc xóa tài khoản của họ. Với vô hiệu hóa, tài khoản sẽ tạm thời bị ngừng sử dụng nhưng dữ liệu vẫn được giữ lại. Xóa tài khoản thì thông tin có thể bị xóa hoàn toàn hoặc sau một thời gian grace period.

**Cách triển khai**:

- **Vô hiệu hóa tài khoản**: Cập nhật trường `is_active` thành `false`. Tài khoản vẫn tồn tại trong hệ thống nhưng người dùng không thể đăng nhập.
- **Xóa tài khoản (soft delete)**: Cập nhật trường `deleted_at` với timestamp hiện tại. Dữ liệu sẽ vẫn còn trong hệ thống nhưng không hiển thị.
- **Xóa tài khoản (hard delete)**: Xóa toàn bộ dữ liệu liên quan đến người dùng khỏi hệ thống.
- API: `/api/users/deactivate` (POST), `/api/users/delete-account` (POST).

### User Analytics & Insights

**Activity Tracking**

Theo dõi và ghi nhận các hành động của người dùng trong hệ thống, như đăng nhập, đăng xuất, thay đổi thông tin cá nhân, hoặc tương tác với các tính năng khác.

**Cách triển khai**:

- Ghi lại các hành động quan trọng của người dùng trong bảng `user_activities`.
- Các hoạt động thường được ghi lại:
  - **Đăng nhập/đăng xuất**.
  - **Cập nhật hồ sơ**.
- **Metadata**: Có thể ghi lại thông tin thêm như địa chỉ IP, loại thiết bị, phiên bản trình duyệt để phục vụ cho mục đích phân tích và bảo mật.
- API: `/api/users/activities` (GET).

**User Insights**

Thống kê các thông tin quan trọng về người dùng, chẳng hạn như tần suất đăng nhập, số lần cập nhật hồ sơ, hoặc các tương tác khác để hiểu mức độ hoạt động của người dùng.

**Cách triển khai**:

- Tạo các báo cáo tổng hợp từ bảng `user_activities` để hiểu rõ hơn về hành vi người dùng.
- Ví dụ:
  - **Số lần đăng nhập**: Đếm số lần ghi nhận hoạt động "login" trong `user_activities`.
  - **Số lần cập nhật hồ sơ**: Đếm số lần người dùng thay đổi thông tin cá nhân.
  - **Tương tác tính năng**: Đo mức độ người dùng sử dụng các tính năng cụ thể.
- Các dữ liệu này có thể được hiển thị qua dashboard quản trị hoặc dưới dạng báo cáo cho team quản lý.

### User Search & Filtering

**User Search**

Cho phép tìm kiếm người dùng dựa trên các tiêu chí như tên, email, hoặc vai trò, giúp admin hoặc các bộ phận liên quan dễ dàng quản lý danh sách người dùng.

**Cách triển khai**:

- Sử dụng các query tìm kiếm trong cơ sở dữ liệu.
- Các tiêu chí phổ biến:
  - **Tên**: Tìm kiếm người dùng theo tên hoặc một phần tên.
  - **Email**: Tìm kiếm người dùng theo địa chỉ email.
  - **Vai trò**: Lọc người dùng dựa trên vai trò của họ trong hệ thống.
- API: `/api/users/search` (GET) với các tham số truy vấn (`name`, `email`, `role`).

**Filtering & Sorting**

Admin có thể lọc và sắp xếp danh sách người dùng theo các tiêu chí khác nhau, giúp dễ dàng quản lý và theo dõi.

- **Cách triển khai**:
- Cung cấp các tuỳ chọn lọc như:
  - **Trạng thái**: Active/inactive.
  - **Vai trò**: Admin, User, etc.
  - **Ngày tạo**: Lọc theo khoảng thời gian tạo tài khoản.
- Các tuỳ chọn sắp xếp phổ biến:
  - **Theo tên**: Sắp xếp bảng người dùng theo tên (A-Z hoặc Z-A).
  - **Theo thời gian đăng ký**: Sắp xếp theo thời gian tạo tài khoản (gần nhất hoặc xa nhất).
- API: `/api/users/filter` (GET) với các tham số (`status`, `role`, `created_at`, etc.).

### Data Export

**User Data Export**

Cho phép người dùng tải về dữ liệu cá nhân của họ theo yêu cầu. Đây là một yêu cầu quan trọng trong các hệ thống cần tuân thủ GDPR hoặc các quy định bảo vệ dữ liệu cá nhân.

**Cách triển khai**:

- Khi người dùng yêu cầu, hệ thống sẽ tổng hợp toàn bộ dữ liệu liên quan của người dùng từ các bảng như `users`, `user_activities`.
- Dữ liệu có thể được xuất dưới dạng file CSV, JSON, hoặc PDF.
- Quy trình xuất dữ liệu:
  1. Người dùng gửi yêu cầu (qua UI hoặc API).
  2. Hệ thống tạo bản sao dữ liệu và gửi lại cho người dùng thông qua email hoặc link tải.
- API: `/api/users/export-data` (POST).
- Phải đảm bảo rằng quá trình xuất dữ liệu tuân thủ các yêu cầu về bảo mật và riêng tư, chẳng hạn mã hóa dữ liệu hoặc yêu cầu xác thực trước khi xuất dữ liệu.
