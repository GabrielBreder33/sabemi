#!/usr/bin/env sh
set -eu

API_URL="${API_URL:-http://localhost:8080}"
API_KEY="${WEBHOOK_API_KEY:-dev-webhook-key}"
TRANSACTION_ID="TRX-$(date +%s)"

curl --fail-with-body -X POST "$API_URL/webhooks/pagamento" \
  -H "X-Api-Key: $API_KEY" \
  -H 'Content-Type: application/json' \
  -d "{\"id_transacao\":\"$TRANSACTION_ID\",\"id_contrato\":\"CTR-987654\",\"valor\":250.90,\"data_pagamento\":\"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",\"status\":\"Sucesso\"}"
