# Sabemi Payment Webhook Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Construir a aplicação full stack executável por Docker Compose para receber, autenticar, persistir, processar e consultar webhooks de pagamentos.

**Architecture:** O backend será uma solução .NET 8 em camadas (`Api`, `Application`, `Domain`, `Infrastructure`) com PostgreSQL/EF Core. O evento persistido em `Pending` funcionará como fila durável; um `BackgroundService` reivindicará lotes usando `FOR UPDATE SKIP LOCKED`, processará a regra de dois segundos e atualizará o contrato de forma transacional. O frontend React/TypeScript será separado, servido por Nginx e consumirá a API por `/api`.

**Tech Stack:** .NET 8, ASP.NET Core Web API, Entity Framework Core 8, PostgreSQL, FluentValidation, Serilog, Swagger/OpenAPI, xUnit, React 18, TypeScript, Vite, Vitest, Docker Compose e Nginx.

## Global Constraints

- A aplicação deverá iniciar com `docker compose up --build`.
- O webhook deverá usar `X-Api-Key`, configurado por variável de ambiente e nunca hardcoded.
- `TransactionId` deverá ter constraint/índice `UNIQUE` no banco.
- O endpoint do webhook deverá responder sem aguardar a regra de negócio de aproximadamente dois segundos.
- O processamento deverá usar `BackgroundService` com polling durável; não usar `Task.Run` dentro de controller.
- Frontend e backend deverão permanecer em diretórios e builds independentes.
- Valores monetários deverão usar `decimal(18,2)` no PostgreSQL e datas deverão ser ISO 8601 na API.
- O ambiente local não dependerá de API externa; Swagger, `curl` e scripts locais deverão ser suficientes para testar.
- Cada comportamento novo deverá seguir ciclo TDD: teste falhando, implementação mínima, teste passando e refatoração somente depois do verde.

---

## Mapa de arquivos

### Raiz

- `docker-compose.yml`: PostgreSQL, API e frontend, redes, volumes, healthchecks e variáveis.
- `.env.example`: apenas valores de exemplo para banco, API key e portas.
- `.gitignore`: artefatos .NET, Node, Docker e segredos.
- `README.md`: arquitetura, execução, migrations, testes, Swagger, payload e troubleshooting.
- `scripts/send-webhook.ps1`: envio de payload de teste no PowerShell.
- `scripts/send-webhook.sh`: envio equivalente em shell/curl.

### Backend

- `backend/Sabemi.Payment.sln`: solução .NET 8.
- `backend/src/Sabemi.Payment.Domain/Entities/PaymentEvent.cs`: entidade de auditoria/fila.
- `backend/src/Sabemi.Payment.Domain/Entities/ContractStatus.cs`: estado materializado do contrato.
- `backend/src/Sabemi.Payment.Domain/Enums/ProcessingStatus.cs`: `Pending`, `Processing`, `Processed`, `Failed`.
- `backend/src/Sabemi.Payment.Domain/Enums/PaymentStatus.cs`: `Sucesso`, `Erro`.
- `backend/src/Sabemi.Payment.Domain/Rules/ContractStatusRules.cs`: regra de ordenação temporal.
- `backend/src/Sabemi.Payment.Application/Contracts/PaymentWebhookRequest.cs`: contrato do payload de entrada.
- `backend/src/Sabemi.Payment.Application/Contracts/PaymentResponse.cs`: DTOs de entrada, lista, detalhe e contrato.
- `backend/src/Sabemi.Payment.Application/Validation/PaymentWebhookValidator.cs`: validação FluentValidation.
- `backend/src/Sabemi.Payment.Application/Abstractions/IPaymentEventRepository.cs`: contrato de persistência do evento.
- `backend/src/Sabemi.Payment.Application/Abstractions/IContractStatusRepository.cs`: contrato do estado do contrato.
- `backend/src/Sabemi.Payment.Application/Services/PaymentWebhookService.cs`: ingestão idempotente.
- `backend/src/Sabemi.Payment.Application/Services/PaymentQueryService.cs`: filtros, paginação e detalhes.
- `backend/src/Sabemi.Payment.Application/Services/PaymentProcessor.cs`: regra de processamento.
- `backend/src/Sabemi.Payment.Infrastructure/Persistence/PaymentDbContext.cs`: DbContext e transações.
- `backend/src/Sabemi.Payment.Infrastructure/Persistence/Configurations/*.cs`: mapeamentos das duas entidades.
- `backend/src/Sabemi.Payment.Infrastructure/Persistence/Repositories/*.cs`: repositórios PostgreSQL.
- `backend/src/Sabemi.Payment.Infrastructure/Migrations/*`: migrations do EF Core.
- `backend/src/Sabemi.Payment.Api/Controllers/WebhooksController.cs`: endpoint autenticado de ingestão.
- `backend/src/Sabemi.Payment.Api/Controllers/PaymentsController.cs`: consulta administrativa.
- `backend/src/Sabemi.Payment.Api/Controllers/ContractsController.cs`: consulta de contrato.
- `backend/src/Sabemi.Payment.Api/Background/PaymentProcessingWorker.cs`: worker de polling.
- `backend/src/Sabemi.Payment.Api/Middleware/ExceptionHandlingMiddleware.cs`: ProblemDetails global.
- `backend/src/Sabemi.Payment.Api/Security/WebhookApiKeyMiddleware.cs`: autenticação do webhook.
- `backend/src/Sabemi.Payment.Api/Program.cs`: composição, DI, Serilog, Swagger, CORS e migrations locais.
- `backend/tests/Sabemi.Payment.UnitTests/*`: domínio, validação e casos de aplicação.
- `backend/tests/Sabemi.Payment.IntegrationTests/*`: PostgreSQL/Testcontainers e concorrência.

### Frontend

- `frontend/package.json`, `frontend/tsconfig.json`, `frontend/vite.config.ts`: setup React/Vite.
- `frontend/src/types/payment.ts`: tipos da API.
- `frontend/src/services/paymentsApi.ts`: chamadas HTTP.
- `frontend/src/hooks/usePayments.ts`: filtros, paginação e polling.
- `frontend/src/components/StatusBadge.tsx`: representação visual dos estados.
- `frontend/src/components/PaymentFilters.tsx`: formulário de filtros.
- `frontend/src/components/PaymentTable.tsx`: tabela e detalhes de erro.
- `frontend/src/pages/DashboardPage.tsx`: composição do dashboard.
- `frontend/src/App.tsx`, `frontend/src/main.tsx`, `frontend/src/styles.css`: shell da aplicação e visual responsivo.
- `frontend/src/**/*.test.tsx`: testes Vitest/Testing Library.
- `frontend/Dockerfile`, `frontend/nginx.conf`: build e proxy `/api` para a API.

---

## Task 1: Scaffold do monorepo e baseline executável

**Files:**
- Create: `backend/Sabemi.Payment.sln` e quatro projetos de produção.
- Create: `backend/tests/Sabemi.Payment.UnitTests`, `backend/tests/Sabemi.Payment.IntegrationTests`.
- Create: `frontend/package.json`, `frontend/tsconfig.json`, `frontend/vite.config.ts`, `frontend/src/main.tsx`.
- Create: `.gitignore`, `.env.example`, `docker-compose.yml` inicial.
- Test: `backend/tests/.../ArchitectureTests.cs`, `frontend/src/App.test.tsx`.

**Interfaces:**
- Produces namespaces `Sabemi.Payment.Domain`, `Sabemi.Payment.Application`, `Sabemi.Payment.Infrastructure` e `Sabemi.Payment.Api`.
- Produces frontend script commands `npm run dev`, `npm run build` e `npm test -- --run`.

- [ ] **Step 1: Escrever o teste de baseline do frontend**

```tsx
it('renders the payment dashboard shell', () => {
  render(<App />);
  expect(screen.getByRole('heading', { name: /pagamentos/i })).toBeInTheDocument();
});
```

- [ ] **Step 2: Rodar o teste para verificar a falha**

Run: `cd frontend; npm test -- --run src/App.test.tsx`
Expected: FAIL porque `App` e o shell ainda não existem.

- [ ] **Step 3: Criar o scaffold mínimo**

Executar `dotnet new sln`, `dotnet new webapi`, `dotnet new classlib` e `dotnet new xunit`; adicionar referências na direção `Api -> Application -> Domain` e `Infrastructure -> Application/Domain`. Criar o Vite React/TypeScript e configurar Vitest com jsdom. O `App` deve renderizar somente um heading `Pagamentos`.

- [ ] **Step 4: Rodar testes e builds do baseline**

Run: `dotnet test backend/Sabemi.Payment.sln` e `cd frontend; npm test -- --run; npm run build`
Expected: todos os testes passam e os dois builds terminam com exit code 0.

- [ ] **Step 5: Criar o commit do scaffold**

```powershell
git add .
git commit -m "chore: scaffold full stack payment webhook"
```

## Task 2: Domínio e validação do webhook

**Files:**
- Create: `backend/src/Sabemi.Payment.Domain/Entities/PaymentEvent.cs`.
- Create: `backend/src/Sabemi.Payment.Domain/Entities/ContractStatus.cs`.
- Create: `backend/src/Sabemi.Payment.Domain/Enums/ProcessingStatus.cs` e `PaymentStatus.cs`.
- Create: `backend/src/Sabemi.Payment.Domain/Rules/ContractStatusRules.cs`.
- Create: `backend/src/Sabemi.Payment.Application/Contracts/PaymentWebhookRequest.cs`.
- Create: `backend/src/Sabemi.Payment.Application/Validation/PaymentWebhookValidator.cs`.
- Test: `backend/tests/Sabemi.Payment.UnitTests/Domain/ContractStatusRulesTests.cs`.
- Test: `backend/tests/Sabemi.Payment.UnitTests/Validation/PaymentWebhookValidatorTests.cs`.

**Interfaces:**
- `PaymentWebhookRequest(string TransactionId, string ContractId, decimal Amount, DateTimeOffset PaymentDate, string Status)`.
- `ContractStatusRules.ShouldApply(PaymentEvent incoming, ContractStatus? current): bool`.
- `PaymentWebhookValidator : AbstractValidator<PaymentWebhookRequest>`.
- `ProcessingStatus` values `Pending`, `Processing`, `Processed`, `Failed`.

- [ ] **Step 1: Escrever testes de regra e validação**

```csharp
[Fact]
public void Rejects_non_positive_amount() { /* request amount 0 => validation failure */ }

[Fact]
public void Applies_event_when_payment_date_is_newer() { /* newer event => true */ }

[Fact]
public void Does_not_apply_older_event() { /* older event => false */ }

[Fact]
public void Applies_last_processed_event_when_dates_are_equal() { /* equal dates => true */ }
```

- [ ] **Step 2: Rodar testes em RED**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests --filter FullyQualifiedName~ContractStatusRulesTests|FullyQualifiedName~PaymentWebhookValidatorTests`
Expected: FAIL por tipos, regras e validator inexistentes.

- [ ] **Step 3: Implementar domínio e validação mínima**

Criar entidades com construtores/fábricas que rejeitem identificadores vazios, `Amount <= 0` e status fora de `Sucesso`/`Erro`. O validator deve exigir `TransactionId`, `ContractId`, `PaymentDate`, `Amount > 0` e status permitido. `ShouldApply` deve retornar true sem estado atual, comparar `PaymentDate` e retornar true em empate para o último worker que gravar.

- [ ] **Step 4: Rodar testes em GREEN**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests --filter FullyQualifiedName~ContractStatusRulesTests|FullyQualifiedName~PaymentWebhookValidatorTests`
Expected: todos os testes selecionados passam.

- [ ] **Step 5: Commitar o domínio**

```powershell
git add backend/src/Sabemi.Payment.Domain backend/src/Sabemi.Payment.Application/Contracts backend/src/Sabemi.Payment.Application/Validation backend/tests/Sabemi.Payment.UnitTests
git commit -m "feat: add payment domain and webhook validation"
```

## Task 3: Persistência PostgreSQL, migrations e idempotência

**Files:**
- Create: `backend/src/Sabemi.Payment.Application/Abstractions/IPaymentEventRepository.cs`.
- Create: `backend/src/Sabemi.Payment.Application/Abstractions/IContractStatusRepository.cs`.
- Create: `backend/src/Sabemi.Payment.Infrastructure/Persistence/PaymentDbContext.cs`.
- Create: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Configurations/PaymentEventConfiguration.cs`.
- Create: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Configurations/ContractStatusConfiguration.cs`.
- Create: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Repositories/PaymentEventRepository.cs`.
- Create: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Repositories/ContractStatusRepository.cs`.
- Create: `backend/src/Sabemi.Payment.Infrastructure/Migrations/*`.
- Test: `backend/tests/Sabemi.Payment.UnitTests/Persistence/InMemoryPaymentEventRepositoryTests.cs`.

**Interfaces:**
- `IPaymentEventRepository.AddPendingAsync(PaymentEvent event, CancellationToken ct): Task<PaymentEvent>`.
- `IPaymentEventRepository.GetByTransactionIdAsync(string transactionId, CancellationToken ct): Task<PaymentEvent?>`.
- `IPaymentEventRepository.ClaimPendingAsync(int batchSize, CancellationToken ct): Task<IReadOnlyList<PaymentEvent>>`.
- `IPaymentEventRepository.GetPageAsync(PaymentQuery query, CancellationToken ct): Task<PagedResult<PaymentResponse>>`.
- `IContractStatusRepository.GetAsync(string contractId, CancellationToken ct): Task<ContractStatus?>`.
- `IContractStatusRepository.UpsertAsync(ContractStatus status, CancellationToken ct): Task`.

- [ ] **Step 1: Escrever teste da constraint de idempotência**

```csharp
[Fact]
public async Task Concurrent_insert_of_same_transaction_returns_one_persisted_event()
{
    var first = repository.AddPendingAsync(CreateEvent("TRX-1"), CancellationToken.None);
    var second = repository.AddPendingAsync(CreateEvent("TRX-1"), CancellationToken.None);
    await Task.WhenAll(first, second);
    Assert.Single(await repository.ListAsync(CancellationToken.None));
}
```

O teste de unidade deverá usar a implementação real da regra de conflito; o teste PostgreSQL de concorrência da Task 8 validará a constraint real.

- [ ] **Step 2: Rodar o teste em RED**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests --filter FullyQualifiedName~Concurrent_insert`
Expected: FAIL porque o contexto, repositório e modelo ainda não existem.

- [ ] **Step 3: Implementar o DbContext e mapeamentos**

Configurar `PaymentEvents.TransactionId` como índice único, `Amount` como `decimal(18,2)`, enums armazenados como texto, comprimento máximo nos identificadores e `RawPayload` como `jsonb`. Configurar `ContractStatuses.ContractId` como chave primária. Implementar `AddPendingAsync` para capturar conflito de índice único e retornar o registro existente.

- [ ] **Step 4: Implementar claim transacional**

Em `ClaimPendingAsync`, abrir transação `ReadCommitted`, executar SQL PostgreSQL equivalente a:

```sql
SELECT * FROM "PaymentEvents"
WHERE "ProcessingStatus" = 'Pending'
ORDER BY "ReceivedAt"
FOR UPDATE SKIP LOCKED
LIMIT @batchSize;
```

Marcar as linhas selecionadas como `Processing`, incrementar `AttemptCount`, salvar e confirmar a transação antes de retornar o lote.

- [ ] **Step 5: Criar e aplicar migration**

Run: `dotnet ef migrations add InitialCreate --project backend/src/Sabemi.Payment.Infrastructure --startup-project backend/src/Sabemi.Payment.Api --output-dir Persistence/Migrations`
Run: `dotnet ef database update --project backend/src/Sabemi.Payment.Infrastructure --startup-project backend/src/Sabemi.Payment.Api`
Expected: tabelas, índice único e tipos esperados aparecem no PostgreSQL.

- [ ] **Step 6: Rodar o teste em GREEN e commit**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests`
Expected: testes de persistência passam.

```powershell
git add backend
git commit -m "feat: add postgres persistence and idempotency constraint"
```

## Task 4: Ingestão HTTP autenticada e consultas administrativas

**Files:**
- Create: `backend/src/Sabemi.Payment.Application/Services/PaymentWebhookService.cs`.
- Create: `backend/src/Sabemi.Payment.Application/Services/PaymentQueryService.cs`.
- Create: `backend/src/Sabemi.Payment.Application/Contracts/PaymentResponse.cs`.
- Create: `backend/src/Sabemi.Payment.Api/Security/WebhookApiKeyMiddleware.cs`.
- Create: `backend/src/Sabemi.Payment.Api/Middleware/ExceptionHandlingMiddleware.cs`.
- Create: `backend/src/Sabemi.Payment.Api/Controllers/WebhooksController.cs`.
- Create: `backend/src/Sabemi.Payment.Api/Controllers/PaymentsController.cs`.
- Create: `backend/src/Sabemi.Payment.Api/Controllers/ContractsController.cs`.
- Modify: `backend/src/Sabemi.Payment.Api/Program.cs`.
- Test: `backend/tests/Sabemi.Payment.UnitTests/Application/PaymentWebhookServiceTests.cs`.

**Interfaces:**
- `PaymentWebhookService.ReceiveAsync(PaymentWebhookRequest request, string rawPayload, CancellationToken ct): Task<ReceivePaymentResult>`.
- `PaymentQueryService.GetPageAsync(PaymentQuery query, CancellationToken ct): Task<PagedResult<PaymentResponse>>`.
- `POST /webhooks/pagamento`: `202` with `{ transactionId, processingStatus, duplicate }`.
- `GET /api/pagamentos`: paged result with `items`, `page`, `pageSize`, `totalItems`, `totalPages`.
- `GET /api/pagamentos/{transactionId}`: detail or `404`.
- `GET /api/contratos/{contractId}`: contract state or `404`.

- [ ] **Step 1: Escrever testes de aplicação para API key, payload válido e duplicidade**

```csharp
[Fact]
public async Task Valid_payload_is_saved_as_pending() { /* result Accepted and repository contains Pending */ }

[Fact]
public async Task Duplicate_transaction_is_accepted_without_second_insert() { /* duplicate true; insert count remains one */ }

[Fact]
public async Task Invalid_payload_is_rejected_before_persistence() { /* validation failure; repository insert count zero */ }
```

- [ ] **Step 2: Rodar testes em RED**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests --filter FullyQualifiedName~PaymentWebhookServiceTests`
Expected: FAIL porque service/resultados ainda não existem.

- [ ] **Step 3: Implementar service, middleware e controllers**

O middleware deverá aplicar a chave somente para o path `/webhooks/pagamento`, comparar o valor configurado e retornar `401` sem chamar o controller quando inválido. O controller deverá capturar o body bruto, desserializar o DTO, executar FluentValidation e chamar o service; a persistência deverá ocorrer antes da resposta `202`. O middleware global deverá mapear validação para `400`, não encontrado para `404` e exceções não tratadas para `500` com `traceId`.

- [ ] **Step 4: Configurar Swagger, Serilog, CORS e health check**

Adicionar `AddOpenApi/SwaggerGen`, `UseSwagger`, `UseSwaggerUI`, Serilog com request logging, CORS permitindo `http://localhost:5173` e `http://localhost:8080`, e `AddHealthChecks().AddNpgSql(...)` em `/health`.

- [ ] **Step 5: Rodar testes em GREEN**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests`
Expected: todos os testes passam.

- [ ] **Step 6: Commitar a camada HTTP**

```powershell
git add backend/src/Sabemi.Payment.Application backend/src/Sabemi.Payment.Api backend/tests/Sabemi.Payment.UnitTests
git commit -m "feat: expose authenticated webhook and payment queries"
```

## Task 5: Worker durável e regra de processamento

**Files:**
- Create: `backend/src/Sabemi.Payment.Application/Services/PaymentProcessor.cs`.
- Create: `backend/src/Sabemi.Payment.Api/Background/PaymentProcessingWorker.cs`.
- Modify: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Repositories/PaymentEventRepository.cs`.
- Modify: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Repositories/ContractStatusRepository.cs`.
- Test: `backend/tests/Sabemi.Payment.UnitTests/Application/PaymentProcessorTests.cs`.

**Interfaces:**
- `PaymentProcessor.ProcessAsync(PaymentEvent paymentEvent, CancellationToken ct): Task`.
- `PaymentProcessingWorker.ExecuteAsync(CancellationToken stoppingToken): Task`.

- [ ] **Step 1: Escrever testes para sucesso, falha e evento antigo**

```csharp
[Fact]
public async Task Processing_success_marks_event_processed_and_updates_contract() { /* status + contract */ }

[Fact]
public async Task Processing_failure_marks_event_failed_with_error_message() { /* failed status + message */ }

[Fact]
public async Task Older_event_does_not_regress_contract_status() { /* current remains newer */ }
```

- [ ] **Step 2: Rodar testes em RED**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests --filter FullyQualifiedName~PaymentProcessorTests`
Expected: FAIL por processor e worker ausentes.

- [ ] **Step 3: Implementar o processor transacional**

Executar `Task.Delay(TimeSpan.FromSeconds(2), ct)`, aplicar `ContractStatusRules.ShouldApply`, criar/atualizar `ContractStatus` quando necessário e marcar o evento `Processed` com `ProcessedAt`. Toda a alteração do evento e do contrato deverá ocorrer na mesma transação. Exceções deverão marcar `Failed` e persistir a mensagem limitada a 2000 caracteres.

- [ ] **Step 4: Implementar o BackgroundService**

O loop deverá buscar até 10 eventos, processar cada evento, capturar exceções por item, registrar logs estruturados e aguardar dois segundos quando não houver itens. O token de shutdown deverá interromper polling e delay. Eventos reivindicados como `Processing` não deverão ser processados por outra instância.

- [ ] **Step 5: Rodar testes em GREEN**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests`
Expected: todos os testes passam.

- [ ] **Step 6: Commitar o processamento assíncrono**

```powershell
git add backend
git commit -m "feat: process payment events with durable background worker"
```

## Task 6: Frontend administrativo

**Files:**
- Create: `frontend/src/types/payment.ts`.
- Create: `frontend/src/services/paymentsApi.ts`.
- Create: `frontend/src/hooks/usePayments.ts`.
- Create: `frontend/src/components/StatusBadge.tsx`.
- Create: `frontend/src/components/PaymentFilters.tsx`.
- Create: `frontend/src/components/PaymentTable.tsx`.
- Create: `frontend/src/pages/DashboardPage.tsx`.
- Modify: `frontend/src/App.tsx`, `frontend/src/main.tsx`, `frontend/src/styles.css`.
- Test: `frontend/src/components/*.test.tsx`, `frontend/src/pages/DashboardPage.test.tsx`.

**Interfaces:**
- `PaymentItem { transactionId, contractId, amount, paymentDate, paymentStatus, processingStatus, errorMessage }`.
- `PaymentFilters { contractId, status, processingStatus, page, pageSize }`.
- `getPayments(filters: PaymentFilters): Promise<PagedPayments>`.
- `usePayments(filters): { data, isLoading, error, refresh }`.

- [ ] **Step 1: Escrever testes de badges, filtros e erro**

```tsx
it('shows failed processing with its error message', () => { /* red badge and message */ });

it('requests payments with selected contract and status filters', async () => { /* query params */ });

it('shows empty state when no payments match filters', () => { /* empty copy */ });
```

- [ ] **Step 2: Rodar testes em RED**

Run: `cd frontend; npm test -- --run src/components src/pages`
Expected: FAIL por componentes, hook e service ausentes.

- [ ] **Step 3: Implementar tipos, serviço e hook**

O service usará `fetch` com `VITE_API_BASE_URL ?? '/api'`, validará `response.ok` e converterá erros para mensagem. O hook fará fetch inicial, polling de 5 segundos, cleanup do timer e atualização quando os filtros mudarem.

- [ ] **Step 4: Implementar dashboard responsivo**

Renderizar cartões de resumo, filtros controlados, tabela paginada, badges com cores sem depender apenas de emoji/cor e `title`/texto para acessibilidade. Valores devem usar `Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' })`; datas, `Intl.DateTimeFormat('pt-BR')`. Mensagens de erro devem ficar visíveis.

- [ ] **Step 5: Rodar testes e build em GREEN**

Run: `cd frontend; npm test -- --run; npm run build`
Expected: testes passam e `dist/` é gerado.

- [ ] **Step 6: Commitar o frontend**

```powershell
git add frontend
git commit -m "feat: add payment administration dashboard"
```

## Task 7: Docker, scripts e documentação operacional

**Files:**
- Create: `backend/src/Sabemi.Payment.Api/Dockerfile`.
- Create: `frontend/Dockerfile`.
- Create: `frontend/nginx.conf`.
- Modify: `docker-compose.yml`, `.env.example`, `README.md`.
- Create: `scripts/send-webhook.ps1`, `scripts/send-webhook.sh`, `scripts/smoke-test.ps1`.

**Interfaces:**
- API publicada em `http://localhost:8080`.
- Frontend publicado em `http://localhost:3000`.
- Swagger em `http://localhost:8080/swagger`.
- PostgreSQL em `localhost:5432`.

- [ ] **Step 1: Escrever o smoke test operacional**

Criar `scripts/smoke-test.ps1` que aguarde `/health`, envie o payload com `X-Api-Key`, confirme `202`, aguarde até 10 segundos consultando `GET /api/pagamentos`, e falhe se o evento não chegar a `Processed`.

- [ ] **Step 2: Rodar o smoke test em RED**

Run: `docker compose up --build -d; ./scripts/smoke-test.ps1`
Expected: falha porque os Dockerfiles, compose e endpoints ainda não estão conectados.

- [ ] **Step 3: Implementar imagens e compose**

Usar build multi-stage .NET 8 (`restore`, `build`, `publish`, runtime aspnet) e Node/Nginx no frontend. O compose deverá declarar `postgres` com volume, healthcheck `pg_isready`, `api` dependente de banco saudável e `frontend` dependente de API. A API deverá aplicar migrations no startup quando `APPLY_MIGRATIONS=true`.

- [ ] **Step 4: Implementar scripts de webhook**

Os scripts deverão enviar este payload padrão:

```json
{
  "id_transacao": "TRX-123456",
  "id_contrato": "CTR-987654",
  "valor": 250.90,
  "data_pagamento": "2026-08-25T10:30:00Z",
  "status": "Sucesso"
}
```

Usar `X-Api-Key: dev-webhook-key` derivada do `.env.example`, nunca uma credencial real.

- [ ] **Step 5: Documentar o fluxo completo**

O README deverá conter comandos de inicialização, logs, migrations, testes, Swagger, exemplos `curl`, comportamento de duplicidade, consulta de falha, portas, variáveis e encerramento dos containers.

- [ ] **Step 6: Rodar smoke test em GREEN**

Run: `docker compose down -v; docker compose up --build -d; ./scripts/smoke-test.ps1`
Expected: health `200`, webhook `202`, uma linha persistida e estado `Processed` após aproximadamente dois segundos.

- [ ] **Step 7: Commitar operação e documentação**

```powershell
git add docker-compose.yml .env.example README.md scripts backend/src/Sabemi.Payment.Api/Dockerfile frontend/Dockerfile frontend/nginx.conf
git commit -m "chore: containerize and document local payment webhook"
```

## Task 8: Testes de integração, concorrência e verificação final

**Files:**
- Create: `backend/tests/Sabemi.Payment.IntegrationTests/PaymentWebhookConcurrencyTests.cs`.
- Create: `backend/tests/Sabemi.Payment.IntegrationTests/PaymentProcessingFlowTests.cs`.
- Modify: `backend/tests/Sabemi.Payment.IntegrationTests/CustomWebApplicationFactory.cs`.
- Modify: `README.md` se os comandos finais mudarem.

**Interfaces:**
- Os testes de integração usarão `Testcontainers.PostgreSql` e `WebApplicationFactory<Program>`.
- O teste de concorrência enviará duas requisições simultâneas com o mesmo `id_transacao` e contará uma única linha no banco.

- [ ] **Step 1: Escrever o teste de concorrência**

```csharp
[Fact]
public async Task Simultaneous_webhooks_create_one_event()
{
    var requests = Enumerable.Range(0, 2).Select(_ => SendWebhookAsync("TRX-CONCURRENT"));
    var responses = await Task.WhenAll(requests);
    Assert.All(responses, response => Assert.Contains((int)response.StatusCode, new[] { 200, 202 }));
    Assert.Equal(1, await CountEventsAsync("TRX-CONCURRENT"));
}
```

- [ ] **Step 2: Rodar integração em RED**

Run: `dotnet test backend/tests/Sabemi.Payment.IntegrationTests`
Expected: falha até que o factory, banco de teste e pipeline HTTP estejam configurados.

- [ ] **Step 3: Implementar factory e testes de fluxo**

Configurar o container PostgreSQL, substituir connection string por uma database isolada, aplicar migrations, registrar API key de teste e desabilitar o delay real somente nos testes unitários; o teste de integração deverá aceitar a janela de até 10 segundos para o worker. Cobrir `401`, payload inválido, `202`, duplicidade, concorrência, `Processed`, `Failed` e não regressão temporal.

- [ ] **Step 4: Rodar suíte completa**

Run: `dotnet test backend/Sabemi.Payment.sln; cd frontend; npm test -- --run; npm run build`
Expected: exit code 0 em todas as suítes.

- [ ] **Step 5: Executar verificação Docker do zero**

Run: `cd ..; docker compose down -v; docker compose up --build -d; ./scripts/smoke-test.ps1; docker compose ps`
Expected: três serviços saudáveis, smoke test verde e nenhuma alteração manual necessária.

- [ ] **Step 6: Verificar requisitos contra a especificação**

Conferir explicitamente autenticação, constraint única, concorrência, status de erro, processamento assíncrono, filtros, paginação, README, Swagger, `.env.example`, migrations e separação de deploy. Registrar qualquer limitação real no README; não declarar conclusão sem saída fresca dos comandos.

- [ ] **Step 7: Commitar a suíte final**

```powershell
git add backend/tests README.md
git commit -m "test: verify webhook concurrency and processing flow"
```

---

## Self-review do plano

- Cenário e stack: Task 1 cria o monorepo e Task 7 documenta execução.
- Endpoint, autenticação e validação: Tasks 2 e 4.
- Idempotência no banco e concorrência: Tasks 3 e 8.
- Persistência de eventos e contrato: Tasks 2, 3 e 5.
- Processamento assíncrono de aproximadamente dois segundos: Task 5.
- Consultas, filtros e paginação: Task 4.
- Dashboard React, estados e erros: Task 6.
- Arquitetura separada e Docker: Tasks 1 e 7.
- Logging, exceções, Swagger e health: Task 4.
- Testes unitários, integração, frontend e smoke: Tasks 2, 3, 4, 5, 6 e 8.
- Não há marcadores pendentes nem etapas sem comando/critério de saída.
- Os nomes públicos usados entre tarefas são consistentes: `PaymentWebhookRequest`, `PaymentProcessor`, `PaymentQueryService`, `IPaymentEventRepository`, `IContractStatusRepository` e `ProcessingStatus`.
