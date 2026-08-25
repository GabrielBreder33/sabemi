# Sabemi Payment Webhook — Design

## Objetivo

Construir uma aplicação full stack funcional para receber webhooks de pagamentos, validar sua autenticidade, persistir cada evento de forma idempotente, processá-lo de forma assíncrona e disponibilizar um painel administrativo para consulta.

## Decisões de produto e escopo

- A aplicação será executável localmente com `docker compose up --build`.
- O webhook será autenticado por `X-Api-Key`, configurada por variável de ambiente.
- Os endpoints administrativos ficarão acessíveis sem autenticação para facilitar o teste local.
- A API local será a integração de teste; não haverá dependência de um banco ou API de terceiros.
- O Swagger exibirá o contrato HTTP e permitirá enviar payloads diretamente.
- O frontend fará polling automático a cada cinco segundos e também terá atualização manual.

## Arquitetura

O repositório será organizado em `backend/` e `frontend/`, com deploy independente.

No backend será usada uma arquitetura em camadas inspirada em Clean Architecture:

```text
Sabemi.Payment.Api
        ↓
Sabemi.Payment.Application
        ↓
Sabemi.Payment.Domain

Sabemi.Payment.Infrastructure → Application / Domain
```

- **Domain:** entidades, enums e regras independentes de HTTP ou Entity Framework.
- **Application:** casos de uso, DTOs, validação, contratos de repositório e orquestração.
- **Infrastructure:** `DbContext`, migrations, repositórios e implementação de persistência PostgreSQL.
- **Api:** controllers/endpoints, autenticação do webhook, tratamento global de exceções, Swagger e composição da aplicação.

O frontend será uma aplicação React + TypeScript com componentes pequenos, uma camada de serviço HTTP e tipos próprios para os contratos da API.

## Modelo de dados

### PaymentEvents

Representa o webhook bruto e seu ciclo de processamento:

- `Id` — GUID interno.
- `TransactionId` — identificador externo, obrigatório e único.
- `ContractId` — identificador do contrato.
- `Amount` — valor monetário com precisão decimal.
- `PaymentDate` — data/hora informada pelo banco.
- `PaymentStatus` — status de negócio recebido, como `Sucesso` ou `Erro`.
- `RawPayload` — JSON original para auditoria.
- `ReceivedAt` — instante de recebimento.
- `ProcessingStatus` — `Pending`, `Processing`, `Processed` ou `Failed`.
- `ErrorMessage` — detalhe da falha, quando existir.
- `ProcessedAt` — instante de conclusão, quando aplicável.
- `AttemptCount` — quantidade de tentativas do worker.

Haverá índice `UNIQUE` em `TransactionId`. A inserção será a barreira definitiva contra duplicidade, inclusive sob requisições simultâneas e múltiplas instâncias.

### ContractStatuses

Representa o estado materializado atual de cada contrato:

- `ContractId` — chave primária.
- `LastTransactionId`.
- `PaymentStatus`.
- `PaymentValue`.
- `PaymentDate`.
- `UpdatedAt`.

O worker só substituirá o estado quando o evento for mais recente que o estado armazenado, comparando `PaymentDate`. Em caso de empate, o evento que for processado por último prevalece.

## Fluxo do webhook

1. `POST /webhooks/pagamento` recebe JSON e `X-Api-Key`.
2. O pipeline rejeita credencial ausente ou inválida com `401`.
3. A aplicação valida campos obrigatórios, valor positivo, data válida e status permitido.
4. O evento é persistido como `Pending` junto com o payload bruto.
5. Uma violação da constraint única é tratada como duplicidade idempotente; o endpoint não cria um segundo evento.
6. A API responde rapidamente com `202 Accepted` e o identificador da transação/status atual.
7. O worker reivindica eventos pendentes em transação PostgreSQL, usando `FOR UPDATE SKIP LOCKED`, e os marca como `Processing`.
8. O worker simula a regra demorada com aproximadamente dois segundos de espera cancelável.
9. Em sucesso, atualiza `ContractStatuses` e marca o evento como `Processed` na mesma transação.
10. Em falha, marca o evento como `Failed`, incrementa a tentativa e grava `ErrorMessage`/log estruturado.

O worker será executado por `BackgroundService` com polling controlado e cancelamento no shutdown. Não será usado `Task.Run` dentro de controller.

## API administrativa

- `GET /api/pagamentos?page=1&pageSize=20&status=Sucesso&processingStatus=Processed&contratoId=CTR-987654`
  - Retorna lista paginada, total e dados de cada evento.
- `GET /api/pagamentos/{transactionId}`
  - Retorna detalhes do evento, incluindo erro e payload bruto quando disponível.
- `GET /api/contratos/{contractId}`
  - Retorna o estado materializado atual do contrato.
- `GET /health`
  - Verifica disponibilidade da API e conexão com o banco.

As respostas usarão DTOs, datas em ISO 8601 e valores monetários numéricos. Erros de validação seguirão `ProblemDetails`.

## Frontend

O painel terá:

- resumo de total, processados, pendentes/processando e falhos;
- tabela com transação, contrato, valor, data, status do pagamento e status de processamento;
- filtros por contrato, status de negócio e status de processamento;
- paginação;
- polling de cinco segundos e botão de atualizar;
- detalhe de erro visível na própria linha e no painel de detalhes;
- estados de carregamento, vazio e falha de comunicação;
- formatação brasileira para moeda e data.

## Tratamento de erros e observabilidade

- Middleware global retornará `ProblemDetails` sem expor stack trace em produção.
- Serilog produzirá logs estruturados para recebimento, duplicidade, claim, sucesso e falha de processamento.
- A chave do webhook nunca ficará no código-fonte.
- O CORS será configurado para o frontend local.
- Swagger/OpenAPI ficará habilitado para facilitar o teste da API.

## Testes

- Testes unitários para validação do payload, regra de atualização temporal e tratamento de status.
- Testes de aplicação para inserção idempotente, duplicidade e transação inválida.
- Testes de integração com PostgreSQL para constraint única, requisições concorrentes e fluxo completo do worker.
- Testes do frontend para filtros, estados de processamento e exibição de erros.

## Execução e documentação

Serão entregues:

- `docker-compose.yml` com PostgreSQL, API e frontend;
- Dockerfiles multi-stage para backend e frontend;
- `.env.example` sem credenciais reais;
- migrations do EF Core aplicadas automaticamente no startup em ambiente local;
- `README.md` com arquitetura, comandos, migrations, testes, Swagger, payload e exemplos `curl`;
- scripts PowerShell e shell para enviar um webhook de teste.

## Critérios de aceite

- O ambiente sobe com `docker compose up --build`.
- Um webhook válido retorna resposta rápida e aparece primeiro como pendente/processando e depois como processado.
- Credencial inválida retorna `401`.
- O mesmo `TransactionId` enviado várias vezes gera apenas um evento persistido.
- Envio concorrente do mesmo `TransactionId` continua gerando apenas um evento.
- Falhas ficam visíveis com status e mensagem.
- O painel consulta, filtra e pagina os eventos sem alterações manuais no código.
- Backend, frontend e testes compilam/executam pelos comandos documentados.
