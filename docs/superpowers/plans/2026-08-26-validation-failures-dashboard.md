# Falhas de validação no painel Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Persistir falhas de JSON e validação dos webhooks e exibi-las claramente no dashboard administrativo.

**Architecture:** Eventos inválidos continuarão na tabela `PaymentEvents`, identificados por `ProcessingStatus.ValidationFailed`, com payload bruto em texto e campos extraídos opcionais. O endpoint persiste o erro e responde `400`; o worker ignora esse status e o frontend exibe o evento com alerta visual.

**Tech Stack:** .NET 8, ASP.NET Core, Entity Framework Core 8, PostgreSQL, xUnit, React, TypeScript, Vitest e Testing Library.

## Global Constraints

- Não registrar falhas de autenticação como eventos.
- Preservar o payload bruto mesmo quando ele não for JSON válido.
- Não adicionar comentários ao código ou aos arquivos gerados.
- Manter a idempotência por `TransactionId` quando o identificador estiver disponível.

---

### Task 1: Persistência de eventos inválidos

**Files:**
- Modify: `backend/src/Sabemi.Payment.Domain/Enums/ProcessingStatus.cs`
- Modify: `backend/src/Sabemi.Payment.Domain/Entities/PaymentEvent.cs`
- Modify: `backend/src/Sabemi.Payment.Application/Abstractions/IPaymentEventRepository.cs`
- Modify: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Configurations/PaymentEventConfiguration.cs`
- Modify: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Repositories/PaymentEventRepository.cs`
- Create: `backend/src/Sabemi.Payment.Infrastructure/Persistence/Migrations/20260826134104_AllowInvalidWebhookEvents.cs`
- Test: `backend/tests/Sabemi.Payment.UnitTests/Application/PaymentWebhookServiceTests.cs`

**Interfaces:**
- Produces `PaymentEvent.CreateValidationFailed(string rawPayload, string errorMessage, string? transactionId, string? contractId)`.
- Produces `IPaymentEventRepository.AddInvalidAsync(PaymentEvent paymentEvent, CancellationToken cancellationToken)`.

- [ ] **Step 1: Write the failing test**

Adicionar um teste que chama o novo fluxo de registro inválido e verifica `ValidationFailed`, mensagem, payload e inserção única.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests/Sabemi.Payment.UnitTests.csproj --no-restore --filter FullyQualifiedName~Invalid_payload_is_persisted`

Expected: FAIL porque o status, factory method e método de repositório ainda não existem.

- [ ] **Step 3: Write minimal implementation**

Adicionar o status, tornar os campos extraídos opcionais, criar a factory de evento inválido, implementar a persistência com a mesma proteção de unicidade do fluxo válido e ajustar o mapeamento EF para colunas opcionais e `RawPayload` como `text`.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests/Sabemi.Payment.UnitTests.csproj --no-restore --filter FullyQualifiedName~Invalid_payload_is_persisted`

Expected: PASS.

- [ ] **Step 5: Create migration and verify model**

Run: `dotnet ef migrations add AllowInvalidWebhookEvents --project backend/src/Sabemi.Payment.Infrastructure --startup-project backend/src/Sabemi.Payment.Api --output-dir Persistence/Migrations`

Expected: migration que altera `RawPayload` para texto e torna os campos extraídos nullable.

### Task 2: Registrar falhas no endpoint

**Files:**
- Modify: `backend/src/Sabemi.Payment.Application/Services/PaymentWebhookService.cs`
- Modify: `backend/src/Sabemi.Payment.Api/Controllers/WebhooksController.cs`
- Test: `backend/tests/Sabemi.Payment.UnitTests/Application/PaymentWebhookServiceTests.cs`

**Interfaces:**
- Produces `PaymentWebhookService.RecordInvalidAsync(string rawPayload, string errorMessage, string? transactionId, string? contractId, CancellationToken cancellationToken)`.

- [ ] **Step 1: Write the failing test**

Adicionar teste que chama `RecordInvalidAsync` com payload inválido e verifica que o repositório recebeu o evento com `ProcessingStatus.ValidationFailed`.

- [ ] **Step 2: Run test to verify it fails**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests/Sabemi.Payment.UnitTests.csproj --no-restore --filter FullyQualifiedName~RecordInvalidAsync`

Expected: FAIL porque o método ainda não existe.

- [ ] **Step 3: Write minimal implementation**

Capturar `JsonException` e `ValidationException` no controller, persistir o payload bruto e responder `400`. Usar os identificadores desserializados quando existirem e deixar os demais nulos.

- [ ] **Step 4: Run test to verify it passes**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests/Sabemi.Payment.UnitTests.csproj --no-restore`

Expected: todos os testes unitários PASS.

### Task 3: Exibição no dashboard

**Files:**
- Modify: `frontend/src/types/payment.ts`
- Modify: `frontend/src/components/StatusBadge.tsx`
- Modify: `frontend/src/components/PaymentTable.tsx`
- Modify: `frontend/src/App.css`
- Test: `frontend/src/components/StatusBadge.test.tsx`
- Test: `frontend/src/pages/DashboardPage.test.tsx`

**Interfaces:**
- Consumes `processingStatus: ValidationFailed` e campos de pagamento nullable.
- Produces badge, detalhe de erro e alerta acessível para falha de validação.

- [ ] **Step 1: Write the failing test**

Adicionar teste que renderiza `StatusBadge` com `ValidationFailed` e verifica o texto `Falha de validação`; adicionar cenário de dashboard com evento inválido e alerta.

- [ ] **Step 2: Run test to verify it fails**

Run: `npm test -- --run src/components/StatusBadge.test.tsx src/pages/DashboardPage.test.tsx`

Expected: FAIL porque o status não possui rótulo e os tipos/UI não contemplam campos nulos.

- [ ] **Step 3: Write minimal implementation**

Adicionar o rótulo do status, suportar valores nulos e renderizar o detalhe da falha como `role="alert"`, com estilo visual de erro.

- [ ] **Step 4: Run test to verify it passes**

Run: `npm test -- --run`

Expected: todos os testes frontend PASS.

### Task 4: Verificação final

**Files:**
- Modify: `README.md` if the API behavior description needs updating.

- [ ] **Step 1: Run backend unit tests**

Run: `dotnet test backend/tests/Sabemi.Payment.UnitTests/Sabemi.Payment.UnitTests.csproj --no-restore`

Expected: zero failures.

- [ ] **Step 2: Run backend build**

Run: `dotnet build backend/Sabemi.Payment.slnx --no-restore`

Expected: build succeeds with zero errors.

- [ ] **Step 3: Run frontend tests and build**

Run: `npm test -- --run` and `npm run build` from `frontend`.

Expected: tests and production build succeed.

- [ ] **Step 4: Review changes**

Run: `git diff --check` and `git status --short`.

Expected: no whitespace errors and only the intended files changed, besides pre-existing user files.
