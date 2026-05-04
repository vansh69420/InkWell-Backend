#!/bin/bash

cd /Users/phoenix/Downloads/inkwell-backend

# ─── STEP 1: Cleanup function ─────────────────────────────────
cleanup() {
  echo ""
  echo "Stopping all InkWell services..."
  pkill -f "dotnet run" 2>/dev/null
  pkill -f "InkWell" 2>/dev/null
  for port in 5000 5001 5011 5077 5078 5079 5080 5081 5082 5083 5084 5085 5175 5176 5177 5178 5179 5180; do
    pid=$(lsof -ti:$port 2>/dev/null)
    if [ -n "$pid" ]; then
      echo "Killing port $port (PID: $pid)"
      kill -9 $pid 2>/dev/null
    fi
  done
  echo "All services stopped. Goodbye."
  exit 0
}

# ─── STEP 2: Trap Ctrl+C ──────────────────────────────────────
trap cleanup SIGINT SIGTERM

# ─── STEP 3: Kill any previous runs ───────────────────────────
echo "Cleaning up previous instances..."
pkill -f "dotnet run" 2>/dev/null
pkill -f "InkWell" 2>/dev/null

for port in 5000 5001 5011 5077 5078 5079 5080 5081 5082 5083 5084 5085 5175 5176 5177 5178 5179 5180; do
  pid=$(lsof -ti:$port 2>/dev/null)
  if [ -n "$pid" ]; then
    echo "Killing port $port (PID: $pid)"
    kill -9 $pid 2>/dev/null
  fi
done

sleep 2
echo "Cleanup done."

# ─── STEP 4: Start all services ───────────────────────────────
echo "Starting Auth Service..."
(cd src/InkWell.Auth.Service && dotnet run --launch-profile http) &

echo "Starting Post Service..."
(cd src/InkWell.Post.Service && dotnet run --launch-profile http) &

echo "Starting Taxonomy Service..."
(cd src/InkWell.Taxonomy.Service && dotnet run --launch-profile http) &

echo "Starting Comment Service..."
(cd src/InkWell.Comment.Service && dotnet run --launch-profile http) &

echo "Starting Media Service..."
(cd src/InkWell.Media.Service && dotnet run --launch-profile http) &

echo "Starting Communication Service..."
(cd src/InkWell.Communication.Service && dotnet run --launch-profile http) &

echo "Starting Gateway..."
(cd src/InkWell.Gateway && dotnet run --launch-profile http) &

echo ""
echo "All backend services started."
echo "Press Ctrl+C to stop all services."
echo "Start frontend manually: cd inkwell-frontend && ng serve"

# ─── STEP 5: Wait and keep script alive ───────────────────────
wait