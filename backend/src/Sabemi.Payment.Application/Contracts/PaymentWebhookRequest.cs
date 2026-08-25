using System.Text.Json.Serialization;

namespace Sabemi.Payment.Application.Contracts;

public sealed record PaymentWebhookRequest(
    [property: JsonPropertyName("id_transacao")] string TransactionId,
    [property: JsonPropertyName("id_contrato")] string ContractId,
    [property: JsonPropertyName("valor")] decimal Amount,
    [property: JsonPropertyName("data_pagamento")] DateTimeOffset PaymentDate,
    [property: JsonPropertyName("status")] string Status);
