using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Sabemi.Payment.Api.Middleware;
using Sabemi.Payment.Api.Security;
using Sabemi.Payment.Application.Abstractions;
using Sabemi.Payment.Application.Services;
using Sabemi.Payment.Application.Validation;
using Sabemi.Payment.Infrastructure.Persistence;
using Sabemi.Payment.Infrastructure.Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, _, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console());

var connectionString = builder.Configuration.GetConnectionString("Payments")
    ?? "Host=localhost;Port=5432;Database=sabemi_payments;Username=sabemi;Password=change-me-local-only";

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<PaymentDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddHealthChecks().AddNpgSql(connectionString);
builder.Services.AddCors(options => options.AddPolicy("local-frontend", policy => policy
    .WithOrigins("http://localhost:3000", "http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddScoped<IPaymentEventRepository, PaymentEventRepository>();
builder.Services.AddScoped<IContractStatusRepository, ContractStatusRepository>();
builder.Services.AddScoped<PaymentWebhookService>();
builder.Services.AddScoped<PaymentQueryService>();
builder.Services.AddScoped<PaymentWebhookValidator>();

var app = builder.Build();

if (app.Configuration.GetValue("APPLY_MIGRATIONS", false))
{
    await using var scope = app.Services.CreateAsyncScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseSerilogRequestLogging();
app.UseCors("local-frontend");
app.UseSwagger();
app.UseSwaggerUI();
app.UseMiddleware<WebhookApiKeyMiddleware>();
app.MapControllers();
app.MapHealthChecks("/health", new HealthCheckOptions());

app.Run();

public partial class Program;
