# Sabemi Payment Webhook

Aplicação full stack para receber webhooks de pagamentos, garantir idempotência, processar eventos de forma assíncrona e consultar os resultados em um painel administrativo.

## Arquitetura

```text
Banco parceiro
     │ POST /webhooks/pagamento + X-Api-Key
     ▼
ASP.NET Core API ── grava PaymentEvents (Pending) ──► PostgreSQL
     │                                                   │
     └──────── responde 202 rapidamente                  │ worker com SKIP LOCKED
                                                         ▼
                                      ContractStatuses + PaymentEvents (Processed/Failed)
```

O backend está separado em `Api`, `Application`, `Domain` e `Infrastructure`. O frontend React/TypeScript é servido por Nginx e pode ser implantado separadamente.

## Tecnologias

- .NET 8 e ASP.NET Core Web API
- PostgreSQL 16 e Entity Framework Core 8
- FluentValidation, Serilog e Swagger/OpenAPI
- React + TypeScript + Vite
- xUnit, Vitest e Testing Library
- Docker Compose

## Executar com Docker

1. Copie `.env.example` para `.env` e altere os valores se necessário.
2. Suba o ambiente:

```bash
docker compose up --build
```

Endereços:

- Painel: http://localhost:3000
- API: http://localhost:8080
- Swagger: http://localhost:8080/swagger
- PostgreSQL: localhost:5432

As migrations são aplicadas automaticamente no container da API quando `APPLY_MIGRATIONS=true`.

Para parar e apagar também o banco local:

```bash
docker compose down -v
```

## Testar o webhook

PowerShell:

```powershell
./scripts/send-webhook.ps1
```

Shell/curl:

```bash
./scripts/send-webhook.sh
```

Requisição manual:

```bash
curl -X POST http://localhost:8080/webhooks/pagamento \
  -H "X-Api-Key: dev-webhook-key" \
  -H "Content-Type: application/json" \
  -d '{
    "id_transacao": "TRX-123456",
    "id_contrato": "CTR-987654",
    "valor": 250.90,
    "data_pagamento": "2026-08-25T10:30:00Z",
    "status": "Sucesso"
  }'
```

A resposta esperada é `202 Accepted`. O endpoint persiste o evento como `Pending` e não espera os aproximadamente dois segundos da regra de negócio.

A chave vem de `WEBHOOK_API_KEY` no `.env` e é exposta à API como `Webhook__ApiKey`. Nunca use uma chave real no arquivo de exemplo.

## Idempotência e concorrência

`PaymentEvents.TransactionId` possui índice `UNIQUE` no PostgreSQL. A aplicação também trata a violação de unicidade como uma repetição idempotente, retornando o evento existente sem criar uma segunda linha. Portanto, retries, requisições simultâneas e múltiplas instâncias convergem para o mesmo evento.

O worker reivindica eventos em uma transação com `FOR UPDATE SKIP LOCKED`, marca cada evento como `Processing` e somente uma instância pode processar uma linha reivindicada. Sucesso e atualização do contrato são salvos na mesma transação; falhas ficam como `Failed` com `ErrorMessage` e log estruturado.

O contrato só é atualizado se o `data_pagamento` do evento for mais recente que o último estado aplicado. Em empate, o evento processado por último prevalece.

## API administrativa

```http
GET /api/pagamentos?page=1&pageSize=20
GET /api/pagamentos?status=Sucesso&contratoId=CTR-987654
GET /api/pagamentos?processingStatus=Failed
GET /api/pagamentos/{transactionId}
GET /api/contratos/{contractId}
GET /health
```

Os endpoints administrativos não exigem chave para facilitar o teste local. O webhook exige `X-Api-Key`.

## Desenvolvimento sem Docker

Com PostgreSQL disponível e a connection string configurada:

```bash
dotnet ef database update \
  --project backend/src/Sabemi.Payment.Infrastructure \
  --startup-project backend/src/Sabemi.Payment.Api
dotnet run --project backend/src/Sabemi.Payment.Api
```

Frontend:

```bash
cd frontend
npm install
npm run dev
```

## Testes

Backend unitário e build:

```bash
dotnet test backend/tests/Sabemi.Payment.UnitTests
dotnet build backend/Sabemi.Payment.slnx
```

Frontend:

```bash
cd frontend
npm test -- --run
npm run build
```

Smoke test completo:

```powershell
docker compose up --build -d
./scripts/smoke-test.ps1
```

## Estrutura de dados

`PaymentEvents` preserva o payload bruto, timestamps, status de negócio, status de processamento, tentativas e erro. `ContractStatuses` é a projeção do último pagamento aplicado por contrato.

## Decisões técnicas

- PostgreSQL funciona como fila durável para evitar perda de eventos em reinicializações sem adicionar RabbitMQ ao teste.
- `BackgroundService` substitui `Task.Run` dentro do controller.
- A chave é comparada com `FixedTimeEquals` e configurada fora do código.
- ProblemDetails/JSON de erro, Serilog e health check tornam falhas observáveis.
- O painel usa polling de cinco segundos, filtros, paginação, estados vazios e detalhe de erro.
