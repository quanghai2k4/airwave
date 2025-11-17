#!/bin/bash

echo "🚀 Starting AirWave Services..."
echo ""

# Start Server
echo "📡 Starting AirWave.Server (MQTT Subscriber)..."
cd /home/merrill/workspace/airwave/AirWave.Server
dotnet run > /tmp/airwave-server.log 2>&1 &
SERVER_PID=$!
echo "   PID: $SERVER_PID"
sleep 3

# Start API
echo "🌐 Starting AirWave.API (REST API)..."
cd /home/merrill/workspace/airwave/AirWave.API
dotnet run > /tmp/airwave-api.log 2>&1 &
API_PID=$!
echo "   PID: $API_PID"
sleep 3

# Start Sensor
echo "📊 Starting AirWave.Sensor (MQTT Publisher)..."
cd /home/merrill/workspace/airwave/AirWave.Sensor
dotnet run > /tmp/airwave-sensor.log 2>&1 &
SENSOR_PID=$!
echo "   PID: $SENSOR_PID"
sleep 3

# Start Client
echo "💻 Starting AirWave.Client (Blazor Dashboard)..."
cd /home/merrill/workspace/airwave/AirWave.Client
dotnet run > /tmp/airwave-client.log 2>&1 &
CLIENT_PID=$!
echo "   PID: $CLIENT_PID"

echo ""
echo "✅ All services started!"
echo ""
echo "📝 Service URLs:"
echo "   - Blazor Client: http://localhost:5281"
echo "   - REST API:      http://localhost:5045"
echo "   - Health Check:  http://localhost:5045/health"
echo "   - Swagger:       http://localhost:5045/swagger"
echo ""
echo "📋 Logs:"
echo "   - Server: tail -f /tmp/airwave-server.log"
echo "   - API:    tail -f /tmp/airwave-api.log"
echo "   - Sensor: tail -f /tmp/airwave-sensor.log"
echo "   - Client: tail -f /tmp/airwave-client.log"
echo ""
echo "🛑 To stop all services: pkill -f 'dotnet.*AirWave'"
echo ""

# Wait a bit and show status
sleep 5
echo "🔍 Service Status:"
curl -s http://localhost:5045/health > /dev/null 2>&1 && echo "   ✅ API is healthy" || echo "   ❌ API not responding"
curl -s http://localhost:5281 > /dev/null 2>&1 && echo "   ✅ Client is running" || echo "   ❌ Client not responding"

echo ""
echo "Press Ctrl+C to view logs, or run: tail -f /tmp/airwave-*.log"
