# Quiz API

Tất cả API cần header `Authorization: Bearer <access-token>`.
Các thao tác thêm, sửa, xóa và thêm câu hỏi yêu cầu role `ADMIN` hoặc `TEACHER`.

| Method | Endpoint | Chức năng |
| --- | --- | --- |
| GET | `/api/quizzes` | Danh sách quiz |
| GET | `/api/quizzes/{id}` | Chi tiết quiz và danh sách ID/thứ tự câu hỏi |
| POST | `/api/quizzes` | Tạo quiz |
| PUT | `/api/quizzes/{id}` | Sửa quiz |
| DELETE | `/api/quizzes/{id}` | Xóa quiz và các liên kết câu hỏi |
| POST | `/api/quizzes/{id}/questions` | Lấy câu hỏi qua Refit theo ID rồi thêm vào quiz |
| POST | `/api/quizzes/{id}/questions/random?count=10` | Lấy câu hỏi ngẫu nhiên qua Refit rồi thêm vào quiz |

Qua API Gateway, thay `/api/quizzes` bằng `/gateway/quizzes`.

Body tạo/sửa quiz:

```json
{
  "title": "Quiz C#",
  "description": "Kiến thức cơ bản",
  "duration": 30,
  "isActive": true
}
```

`title` bắt buộc, tối đa 255 ký tự; `duration` phải lớn hơn 0.
`CreatedBy` lấy từ user ID trong JWT; sửa quiz giữ nguyên người tạo và thời điểm tạo.

Body thêm câu hỏi theo ID:

```json
{ "questionId": 42 }
```

Controller lấy header Authorization từ request, truyền qua service đến tham số
`[Header("Authorization")]` của `IQuestionClient`. Service gọi QuestionService
trước khi ghi liên kết `quiz_question`, và trả dữ liệu câu hỏi đã thêm.
Nội dung câu hỏi vẫn thuộc QuestionService; QuizService lưu `question_id` và
`question_order`, nối tiếp thứ tự hiện có.

Thêm theo ID trả 409 nếu đã có câu hỏi. Ngẫu nhiên nhận `count` từ 1 đến 100,
mặc định 10, bỏ qua câu hỏi đã có và câu trùng trong kết quả. Số câu thêm có thể
ít hơn `count`; nếu tất cả đã có thì trả danh sách rỗng.
Quiz/câu hỏi không tồn tại trả 404. Lỗi xác thực/phân quyền từ QuestionService
trả 401/403; lỗi kết nối/phản hồi khác trả 502 và timeout trả 504.

Sử dụng schema `quiz` và `quiz_question` đã cấu hình trong `AppDbContext`.
Database cần có hai bảng, khóa ngoại cascade và unique `(quiz_id, question_id)`
như model; ứng dụng không tự tạo hoặc sửa schema.

Chạy kiểm thử:

```powershell
dotnet run --project tests/QuizService.SmokeTests/QuizService.SmokeTests.csproj
```

Kiểm thử dùng Refit thật với HTTP transport giả và repository trong bộ nhớ để
kiểm tra CRUD, JWT forwarding, route/query, thứ tự, chống trùng và lỗi downstream.
Chưa kiểm thử tích hợp với PostgreSQL, QuestionService hoặc Gateway đang chạy.
