#!/bin/bash

echo "=== RACE CONDITION TEST ==="
echo "Testing concurrent registrations with same email"

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

# Generate unique email with timestamp
UNIQUE_EMAIL="race-test-$(date +%s)@test.com"
echo -e "${GREEN}Testing with email: $UNIQUE_EMAIL${NC}"

# Test 1: Same email, concurrent registrations
echo -e "\n${GREEN}Test 1: 5 concurrent registrations with same email${NC}"
for i in {1..5}; do
  curl -s -X POST http://localhost:5085/api/Auth/register \
    -H "Content-Type: application/json" \
    -d "{
      \"email\": \"$UNIQUE_EMAIL\",
      \"password\": \"Killerboy@23\",
      \"confirmPassword\": \"Killerboy@23\",
      \"firstName\": \"Race\",
      \"lastName\": \"Test\",
      \"consentGiven\": true,
      \"registrationType\": \"client\"
    }" &
done
wait

echo -e "\n\n${GREEN}Checking database for $UNIQUE_EMAIL${NC}"
docker exec -it smartfintech-postgres-1 psql -U postgres -d SmartFintechFinancial -c "SELECT email, created_at FROM identity.users WHERE email = '$UNIQUE_EMAIL';"

# Test 2: Different emails, concurrent registrations
echo -e "\n${GREEN}Test 2: 5 concurrent registrations with different emails${NC}"
for i in {1..5}; do
  curl -s -X POST http://localhost:5085/api/Auth/register \
    -H "Content-Type: application/json" \
    -d "{
      \"email\": \"race-$i-$(date +%s)@test.com\",
      \"password\": \"Killerboy@23\",
      \"confirmPassword\": \"Killerboy@23\",
      \"firstName\": \"Race$i\",
      \"lastName\": \"Test\",
      \"consentGiven\": true,
      \"registrationType\": \"client\"
    }" &
done
wait

echo -e "\n${GREEN}Test 2 complete - check logs for any errors${NC}"
