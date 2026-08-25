$ErrorActionPreference = 'Stop'

$apiUrl = if ($env:API_URL) { $env:API_URL } else { 'http://localhost:8080' }
$apiKey = if ($env:WEBHOOK_API_KEY) { $env:WEBHOOK_API_KEY } else { 'dev-webhook-key' }
$transactionId = "TRX-SMOKE-$([DateTimeOffset]::UtcNow.ToUnixTimeSeconds())"

for ($attempt = 1; $attempt -le 20; $attempt++) {
    try {
        $health = Invoke-WebRequest -Uri "$apiUrl/health" -UseBasicParsing -TimeoutSec 3
        if ($health.StatusCode -eq 200) { break }
    } catch {
        if ($attempt -eq 20) { throw 'API health check did not become available.' }
        Start-Sleep -Seconds 1
    }
}

$payload = @{
    id_transacao = $transactionId
    id_contrato = 'CTR-SMOKE'
    valor = 250.90
    data_pagamento = [DateTimeOffset]::UtcNow.ToString('O')
    status = 'Sucesso'
} | ConvertTo-Json

$response = Invoke-WebRequest -Uri "$apiUrl/webhooks/pagamento" -Method Post -Headers @{ 'X-Api-Key' = $apiKey } -ContentType 'application/json' -Body $payload -UseBasicParsing
if ($response.StatusCode -ne 202) { throw "Expected webhook status 202, got $($response.StatusCode)." }

for ($attempt = 1; $attempt -le 10; $attempt++) {
    $page = Invoke-RestMethod -Uri "$apiUrl/api/pagamentos?page=1&pageSize=20&contratoId=CTR-SMOKE"
    $event = $page.items | Where-Object { $_.transactionId -eq $transactionId }
    if ($event.processingStatus -eq 'Processed') {
        Write-Output "Smoke test passed: $transactionId processed."
        exit 0
    }
    Start-Sleep -Seconds 1
}

throw "Payment event $transactionId was not processed within 10 seconds."
