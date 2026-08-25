$ErrorActionPreference = 'Stop'
$apiUrl = if ($env:API_URL) { $env:API_URL } else { 'http://localhost:8080' }
$apiKey = if ($env:WEBHOOK_API_KEY) { $env:WEBHOOK_API_KEY } else { 'dev-webhook-key' }
$payload = @{
    id_transacao = "TRX-$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())"
    id_contrato = 'CTR-987654'
    valor = 250.90
    data_pagamento = [DateTimeOffset]::UtcNow.ToString('O')
    status = 'Sucesso'
} | ConvertTo-Json

Invoke-RestMethod -Uri "$apiUrl/webhooks/pagamento" -Method Post -Headers @{ 'X-Api-Key' = $apiKey } -ContentType 'application/json' -Body $payload
