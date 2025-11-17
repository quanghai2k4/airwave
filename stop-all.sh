#!/bin/bash

echo "🛑 Stopping all AirWave services..."
pkill -f 'dotnet.*AirWave'
sleep 2
echo "✅ All services stopped!"
echo ""
echo "📋 Logs are still available at:"
echo "   - Server: /tmp/airwave-server.log"
echo "   - API:    /tmp/airwave-api.log"
echo "   - Sensor: /tmp/airwave-sensor.log"
echo "   - Client: /tmp/airwave-client.log"
