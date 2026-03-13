#!/bin/bash

echo "=== SQL INJECTION PROTECTION TEST ==="

# Test email field
echo -e "\n${GREEN}Testing SQL injection in email field${NC}"
INJECTIONS=(
  "' OR '1'='1"
  "admin'--"
  "test@test.com; DROP TABLE users;"
  "'; SELECT * FROM users; --"
  "' UNION SELECT * FROM users--"
)

for injection in "${INJECTIONS[@]}"; do
  echo -e "\n${YELLOW}Testing: $injection${NC}"
  
  # Register attempt
  RESPONSE=$(curl -s -X POST http://localhost:5085/api/Auth/register \
    -H "Content-Type: application/json" \
    -d "{
      \"email\": \"$injection\",
      \"password\": \"Katlego@23\",
      \"confirmPassword\": \"Katlego@23\",
      \"firstName\": \"Hacker\",
      \"lastName\": \"Test\",
      \"consentGiven\": true,
      \"registrationType\": \"client\"
    }")
  
  if [[ $RESPONSE == *"Invalid email format"* ]] || [[ $RESPONSE == *"Invalid registration data"* ]]; then
    echo -e "${GREEN}✓ Blocked: $RESPONSE${NC}"
  else
    echo -e "${RED}⚠ Possible vulnerability: $RESPONSE${NC}"
  fi
done

# Test login with injection
echo -e "\n${GREEN}Testing SQL injection in login${NC}"
for injection in "${INJECTIONS[@]}"; do
  echo -e "\n${YELLOW}Testing login with: $injection${NC}"
  
  RESPONSE=$(curl -s -X POST http://localhost:5085/api/Auth/login \
    -H "Content-Type: application/json" \
    -d "{
      \"email\": \"$injection\",
      \"password\": \"anypassword\"
    }")
  
  # Check if it was blocked
  if [[ $RESPONSE == *"Invalid email or password"* ]] || [[ $RESPONSE == *"Invalid login data"* ]]; then
    echo -e "${GREEN}✓ BLOCKED: $RESPONSE${NC}"
  else
    echo -e "${RED}⚠ POSSIBLE VULNERABILITY: $RESPONSE${NC}"
  fi
done
