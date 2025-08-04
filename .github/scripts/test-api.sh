#!/usr/bin/env bash
set -euo pipefail

API_URL="http://localhost:8080"

echo "🔹 Testing API endpoint..."
curl -v -X POST \
  "$API_URL/api/Slots/play?wager=0.5&gameSessionId=22" \
  -H 'accept: text/plain' \
  -H 'Content-Type: application/json' \
  -d ''

echo "🔹 Testing video availability..."
ID=$(curl -s -X POST \
  "$API_URL/api/Slots/play?wager=1&gameSessionId=22" \
  -H 'accept: application/json' \
  -H 'Content-Type: application/json' \
  -d '' | jq -r '.id')

if [ -z "$ID" ] || [ "$ID" == "null" ]; then
  echo "❌ No ID returned from API"
  exit 1
fi

STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_URL/${ID}.mp4")

if [ "$STATUS" -ne 200 ]; then
  echo "❌ Video file not available (HTTP $STATUS)"
  exit 1
fi

echo "✅ API tests passed successfully!"
