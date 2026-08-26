# Falhas de validação no painel

## Objetivo

Registrar payloads de webhook que não podem ser processados por falha de JSON ou validação de campos, para que permaneçam auditáveis e apareçam no painel administrativo.

## Desenho aprovado

Os eventos válidos continuam usando o fluxo atual. Eventos inválidos serão gravados na tabela `PaymentEvents` com `ProcessingStatus.ValidationFailed`, payload bruto, mensagem de erro e os identificadores extraídos quando disponíveis. Campos que não puderem ser extraídos serão nulos. A coluna do payload bruto será texto para aceitar JSON inválido sem perder o conteúdo recebido.

O endpoint continuará exigindo `X-Api-Key`. Falhas de autenticação não serão registradas como eventos, pois não são notificações confiáveis do banco parceiro. Falhas de JSON ou de validação serão persistidas e responderão `400`.

O worker continuará consumindo somente eventos `Pending`; falhas de validação não serão processadas novamente. O dashboard exibirá `ValidationFailed` como alerta visual vermelho, mantendo filtros por contrato, status do pagamento e status de processamento.

## Persistência e idempotência

`TransactionId`, `ContractId`, `Amount`, `PaymentDate` e `PaymentStatus` serão opcionais na entidade e no contrato de resposta. O índice único de `TransactionId` continuará protegendo duplicidade quando o identificador existir; payloads sem identificador poderão ser registrados individualmente.

Será criada uma migration para tornar os campos extraídos opcionais e converter `RawPayload` de `jsonb` para `text`.

## Testes

Serão adicionados testes unitários para persistência de payload inválido e testes de UI para o status de falha de validação. Os testes existentes de eventos válidos, idempotência e processamento devem continuar passando.
