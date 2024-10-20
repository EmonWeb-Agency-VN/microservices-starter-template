# Noti Service

## Thông tin cơ bản

- IDE: `Visual Studio Code`
- Language: `Go`
- Framework: `Gin`
- Database: `MySQL`
- Tool: `Firebase`

## Tính năng cơ bản

### Thông báo In-app với Firebase

Gửi thông báo trực tiếp trong ứng dụng thông qua **Firebase Cloud Messaging (FCM)**. Khi có sự kiện quan trọng, người dùng sẽ nhận được thông báo ngay lập tức mà không cần phải rời khỏi ứng dụng.

**Các bước:**

1. **Kích hoạt sự kiện**: Sự kiện như người dùng đăng ký hoặc thay đổi trạng thái đơn hàng xảy ra.
2. **Tạo thông báo**: Hệ thống tạo nội dung thông báo ngắn gọn cho in-app notification.
3. **Gửi thông báo qua Firebase**: Thông báo được đẩy đến Firebase Cloud Messaging.
4. **Người dùng nhận thông báo**: Người dùng nhận được thông báo trong ứng dụng.

**Các API:**

- `POST /api/noti/send-in-app`: API này dùng để gửi thông báo in-app cho người dùng thông qua Firebase.
  - **Request body**:
    ```json
    {
      "recipient_id": "string",
      "title": "string",
      "message": "string"
    }
    ```
  - Response:
    - 201
    - 400

### Thông báo Email

Gửi email thông báo cho người dùng khi có sự kiện quan trọng xảy ra. Nội dung email có thể được cá nhân hóa dựa trên template sẵn có và sự kiện kích hoạt.

**Các bước:**

1. **Kích hoạt sự kiện**: Một sự kiện (ví dụ: đơn hàng hoàn tất) xảy ra.
2. **Chọn template email**: Template được chọn dựa trên sự kiện và ngôn ngữ của người nhận.
3. **Tạo nội dung email**: Cá nhân hóa nội dung email (ví dụ: chèn tên người dùng, chi tiết đơn hàng).
4. **Gửi email**: Gửi email thông qua dịch vụ email (ví dụ: Zoho Mail).
5. **Theo dõi trạng thái**: Kiểm tra xem email đã gửi thành công hay thất bại.

**Các API:**

- `POST /api/noti/send-email`: Gửi email thông qua hệ thống email.
  - **Request body**:
    ```json
    {
      "recipient_email": "string",
      "subject": "string",
      "template_id": "string",
      "template_data": {
        "key": "value"
      }
    }
    ```
  - **Response**:
    - 201
    - 400

### Lịch sử & Trạng thái thông báo

Lưu trữ và theo dõi tất cả các thông báo đã gửi, bao gồm in-app và email, cùng với trạng thái (đã gửi, đã đọc, thành công, thất bại).

**Các bước:**

1. **Lưu trữ thông báo**: Mỗi thông báo được ghi lại với thông tin chi tiết về người nhận, thời gian gửi, và trạng thái.
2. **Kiểm tra trạng thái**: Người quản trị hoặc hệ thống có thể truy vấn trạng thái thông báo đã gửi.
3. **Cập nhật trạng thái**: Đối với in-app notification, trạng thái "đã đọc" được cập nhật khi người dùng tương tác với thông báo.

**Các API:**

- `GET /api/noti/history`: Truy vấn lịch sử các thông báo đã gửi.
  - **Reponse:**
    - 200
  - **Response body**:
    ```json
    {
      [
        {
          "notification_id": "string",
          "type": "in-app",
          "recipient_id": "string",
          "title": "string",
          "message": "string",
          "status": "sent",
          "created_at": "timestamp",
          "read_at": "timestamp"
        },
        {
          "notification_id": "string",
          "type": "email",
          "recipient_email": "string",
          "subject": "string",
          "status": "failed",
          "created_at": "timestamp",
          "sent_at": "timestamp"
        }
      ]
    }
    ```
- `PATCH /api/noti/{id}/status`: \*\*\*\*Cập nhật trạng thái thông báo (ví dụ: "đã đọc" cho in-app notifications).
  - **Request body**:
    ```json
    {
      "status": "read"
    }
    ```
  - **Reponse**:
    - 200
    - 400
- `DELETE /api/noti/{notification_id}`: API này dùng để xóa một thông báo cụ thể.
  - **Response:**
    - 204
    - 404

## Cơ sở dữ liệu

### notifications

| Trường            | Kiểu dữ liệu                              |
| ----------------- | ----------------------------------------- |
| `id`              | UUID                                      |
| `type`            | ENUM(`in-app`, `email`)                   |
| `recipient_id`    | UUID                                      |
| `recipient_email` | VARCHAR(255)                              |
| `title`           | VARCHAR(255)                              |
| `message`         | TEXT                                      |
| `template_id`     | UUID                                      |
| `data`            | JSONB                                     |
| `status`          | ENUM(`pending`, `sent`, `failed`, `read`) |
| `created_at`      | TIMESTAMP                                 |
| `sent_at`         | TIMESTAMP                                 |
| `read_at`         | TIMESTAMP                                 |

### notification_templates

| Trường       | Kiểu dữ liệu |
| ------------ | ------------ |
| `id`         | UUID         |
| `name`       | VARCHAR(255) |
| `subject`    | VARCHAR(255) |
| `body`       | TEXT         |
| `language`   | VARCHAR(10)  |
| `created_at` | TIMESTAMP    |
| `updated_at` | TIMESTAMP    |

### notification_logs

| Trường            | Kiểu dữ liệu                     |
| ----------------- | -------------------------------- |
| `id`              | UUID                             |
| `notification_id` | UUID                             |
| `status`          | ENUM(`queued`, `sent`, `failed`) |
| `error_message`   | TEXT                             |
| `attempts`        | INT                              |
| `created_at`      | TIMESTAMP                        |
