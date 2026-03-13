#!/bin/bash

echo "=== RATE LIMITING TEST ==="
echo "Testing rate limiting on login endpoint"

# Test rapid login attempts
echo -e "\n${GREEN}Sending 10 rapid login requests${NC}"
for i in {1..10}; do
  RESPONSE=$(curl -s -w "%{http_code}" -o /dev/null -X POST http://localhost:5085/api/Auth/login \
    -H "Content-Type: application/json" \
    -d '{
      "email": "client@test.com",
      "password": "WrongPassword!"
    }')
  echo "Request $i: HTTP $RESPONSE"
  
  # Check rate limit headers
  if [ $i -eq 1 ]; then
    curl -s -I -X POST http://localhost:5085/api/Auth/login \
      -H "Content-Type: application/json" \
      -d '{"email":"client@test.com","password":"WrongPassword!"}' \
      | grep -i x-ratelimit
  fi
  
  sleep 0.5
done

echo -e "\n${GREEN}Testing different endpoints rate limits${NC}"
# Test check-email rate limiting
for i in {1..6}; do
  RESPONSE=$(curl -s -w "%{http_code}" -o /dev/null "http://localhost:5085/api/Auth/check-email?email=test$i@test.com")
  echo "Check-email $i: HTTP $RESPONSE"
done

# Test refresh token rate limiting
for i in {1..6}; do
  RESPONSE=$(curl -s -w "%{http_code}" -o /dev/null -X POST http://localhost:5085/api/Auth/refresh-token \
    -H "Content-Type: application/json" \
    -d '{"refreshToken": "invalid"}')
  echo "Refresh token $i: HTTP $RESPONSE"
done
