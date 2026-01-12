using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Application.ReceiptParsing;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.User.Services;
using ServerlessKakeibo.Api.Domain.Receipt.Services;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository;
using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Application.TransactionQuery;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Application.Transaction;
using ServerlessKakeibo.Api.Application.TransactionUpdate;

var builder = WebApplication.CreateBuilder(args);

// JSONシリアライズ時にEnumを文字列にする
builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// HTTP クライアントなど
builder.Services.AddHttpClient();

// 設定バインド
builder.Services.Configure<GcpAuthSettings>(
    builder.Configuration.GetSection("GcpAuth"));
builder.Services.Configure<VertexAiSettings>(
    builder.Configuration.GetSection("VertexAi"));
builder.Services.Configure<GoogleAiStudioSettings>(
    builder.Configuration.GetSection("GoogleAiStudio"));

// DI 登録
#region services
builder.Services.AddScoped<IGcpAuthService, GcpAuthService>();
builder.Services.AddScoped<IVertexAiService, VertexAiService>();
builder.Services.AddScoped<IGoogleAiStudioService, GoogleAiStudioService>();
#endregion

#region usecases
builder.Services.AddScoped<IReceiptParsingUseCase, ReceiptParsingInteractor>();
builder.Services.AddScoped<IRegistReceiptDetailsUseCase, RegistReceiptDetailsInteractor>();
builder.Services.AddScoped<ITransactionQueryUseCase, TransactionQueryInteractor>();
builder.Services.AddScoped<ITransactionCreateUseCase, TransactionCreateInteractor>();
builder.Services.AddScoped<ITransactionUpdateUseCase, TransactionUpdateInteractor>();
#endregion

#region DomainServices
builder.Services.AddScoped<TransactionDomainService>();
builder.Services.AddScoped<UserDomainService>();
builder.Services.AddScoped<ReceiptEvaluatorService>();
#endregion

#region Repositories
builder.Services.AddDbContext<ApplicationDbContext>(opts =>
    opts.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped(typeof(IGenericReadRepository<>), typeof(GenericReadRepository<>));
builder.Services.AddScoped(typeof(IGenericWriteRepository<>), typeof(GenericWriteRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>(); // トランザクションヘルパーに置き換えて消す。
builder.Services.AddScoped<ITransactionHelper, TransactionHelper>();
#endregion

// CORS 設定（開発環境のみ全許可）
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", p =>
            p.AllowAnyOrigin()
             .AllowAnyHeader()
             .AllowAnyMethod());
    });
}

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServerlessKakeibo API",
        Version = "v1"
    });
    c.EnableAnnotations();
});

var app = builder.Build();

// ミドルウェア
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerlessKakeibo API v1");
        c.RoutePrefix = string.Empty;
    });
    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();

// HealthCheck
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithTags("System")
   .ExcludeFromDescription();

app.Run();

// 統合テスト用のパーシャルクラス
public partial class Program { }
