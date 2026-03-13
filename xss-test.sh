#!/bin/bash

echo "=== XSS PROTECTION TEST ==="

XSS_PAYLOADS=(
  "<script>alert('XSS')</script>"
  "<img src=x onerror=alert('XSS')>"
  "javascript:alert('XSS')"
  "\"><script>alert('XSS')</script>"
)

for payload in "${XSS_PAYLOADS[@]}"; do
  echo -e "\n${YELLOW}Testing XSS payload: $payload${NC}"
  
  # Test in registration fields
  RESPONSE=$(curl -s -X POST http://localhost:5085/api/Auth/register \
    -H "Content-Type: application/json" \
    -d "{
      \"email\": \"xss@test.com\",
      \"password\": \"Angel@23\",
      \"confirmPassword\": \"Angel@23\",
      \"firstName\": \"$payload\",
      \"lastName\": \"Test\",
      \"consentGiven\": true,
      \"registrationType\": \"client\"
    }")
  
  if [[ $RESPONSE == *"Invalid registration data"* ]]; then
    echo -e "${GREEN}✓ Blocked by validation${NC}"
  else
    echo -e "${YELLOW}⚠ Check response for XSS: $RESPONSE${NC}"
  fi
done
