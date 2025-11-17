Hệ thống giám sát chất lượng không khí (Air Quality)
Xây dựng hệ thống giả lập giám sát chất lượng không khí với chỉ số AQI.
- Ứng dụng Client: giao diện hiển thị chỉ số AQI theo màu sắc (theo chuẩn WHO).
- Ứng dụng Sensor (Console App): giả lập cảm biến AQI.
- Ứng dụng Server (Console App): nhận và lưu dữ liệu AQI.
- Ứng dụng Web API: cung cấp dữ liệu AQI cho Client.
Yêu cầu:
Cảm biến gửi dữ liệu định kỳ 20 giây/lần.
Server lưu AQI + timestamp.
Client gọi API để hiển thị dữ liệu dạng bảng và cảnh báo màu (0–50: Xanh, 51–100: Vàng, 101–150: Cam, >150: Đỏ).
Client có thể xem dữ liệu theo thời gian thực từ cảm biến
Cho phép lọc dữ liệu theo ngày/giờ Việt Nam

Cấu trúc Hệ thống
AirWave.API: Một ứng dụng ASP.NET Core Web API, cung cấp các RESTful endpoint để truy xuất dữ liệu AQI từ database. Tích hợp sẵn Swagger UI để tài liệu hóa và kiểm thử API.

AirWave.Server: Một ứng dụng Worker Service (.NET), chạy nền với vai trò là một MQTT Subscriber. Nó lắng nghe dữ liệu AQI từ broker và lưu trực tiếp vào cơ sở dữ liệu SQLite.

AirWave.Sensor: Một ứng dụng Console giả lập cảm biến, định kỳ 20 giây/lần gửi dữ liệu AQI ngẫu nhiên lên MQTT broker.

AirWave.Client: Một ứng dụng Blazor Server, đóng vai trò là Web Dashboard hiển thị dữ liệu cho người dùng. Giao diện có thể hiển thị dữ liệu thời gian thực, cảnh báo màu sắc và cho phép lọc dữ liệu lịch sử bằng cách gọi đến AirWave.API.

AirWave.Shared: Một thư viện class chứa các model dữ liệu chung (ví dụ: AqiData) được sử dụng bởi tất cả các project khác.

MQTT Broker: Sử dụng HiveMQ public broker (broker.hivemq.com)

hãy sử dụng phiên bản mới nhất của các nuget packages
