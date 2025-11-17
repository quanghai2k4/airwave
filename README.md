# AirWave - Air Quality Monitoring System

Hệ thống giám sát chất lượng không khí với chỉ số AQI.

## Cấu trúc dự án

- **AirWave.Shared**: Thư viện chứa model và helper dùng chung
- **AirWave.Sensor**: Console app giả lập cảm biến, gửi dữ liệu AQI mỗi 20 giây qua MQTT
- **AirWave.Server**: Worker Service nhận dữ liệu từ MQTT và lưu vào SQLite
- **AirWave.API**: Web API cung cấp endpoints để truy xuất dữ liệu AQI
- **AirWave.Client**: Blazor Server app hiển thị dashboard với real-time data

## Hướng dẫn chạy

### Cách 1: Chạy tất cả services (Khuyến nghị)
```bash
./start-all.sh
```

Để dừng tất cả:
```bash
./stop-all.sh
```

### Cách 2: Chạy từng service riêng lẻ

#### 1. Chạy Server (MQTT Subscriber)
```bash
cd AirWave.Server
dotnet run
```

#### 2. Chạy API
```bash
cd AirWave.API
dotnet run
```
- API: http://localhost:5045
- Swagger: http://localhost:5045/swagger
- Health: http://localhost:5045/health

#### 3. Chạy Sensor (MQTT Publisher)
```bash
cd AirWave.Sensor
dotnet run
```

#### 4. Chạy Client Dashboard
```bash
cd AirWave.Client
dotnet run
```
- Client: http://localhost:5281

### Xem logs
```bash
tail -f /tmp/airwave-*.log
```

## Tính năng

### Core Features
- ✅ Sensor publish AQI data mỗi 20 giây qua MQTT
- ✅ Server subscribe MQTT và lưu vào SQLite với UTC timestamp
- ✅ API cung cấp RESTful endpoints:
  - `GET /api/aqi` - Lấy 100 records gần nhất
  - `GET /api/aqi/latest` - Lấy record mới nhất
  - `GET /api/aqi/filter?startDate=&endDate=` - Lọc theo ngày
  - `GET /health` - Health check
  - `GET /health/ready` - Readiness check
- ✅ Client Blazor Server hiển thị:
  - Real-time data (auto-refresh mỗi 20 giây)
  - Bảng dữ liệu lịch sử với pagination
  - Color-coded categories theo WHO standards
  - Filter theo ngày (Vietnam timezone)

### Optimizations (Production-Ready)
- ✅ **Security**: CORS policy restricted to localhost origins only
- ✅ **Performance**: Response caching (5-30s duration)
- ✅ **Reliability**: EF Core Migrations for database schema management
- ✅ **Protection**: Rate limiting (100 requests/minute per client)
- ✅ **Logging**: Serilog structured logging to file and console
- ✅ **Validation**: FluentValidation for input validation
- ✅ **Monitoring**: Health check endpoints for orchestration
- ✅ **Timezone**: Proper UTC storage with Vietnam time display

## MQTT Broker

Sử dụng HiveMQ public broker: broker.hivemq.com:1883
Topic: airwave/aqi
