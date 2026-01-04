using Microsoft.OpenApi.Models;
using ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;
using ServerlessKakeibo.Api.Application.ReceiptParsing;
using ServerlessKakeibo.Api.Service;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Common.Settings;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// HTTPクライアントファクトリーの登録
builder.Services.AddHttpClient();

// 設定の読み込み (appsettings.json + User Secrets)
builder.Services.Configure<GcpAuthSettings>(
    builder.Configuration.GetSection("GcpAuth"));
builder.Services.Configure<VertexAiSettings>(
    builder.Configuration.GetSection("VertexAi"));

// サービスの登録
builder.Services.AddScoped<IGcpAuthService, GcpAuthService>();
builder.Services.AddScoped<IVertexAiService, VertexAiService>();

// ユースケースの登録
builder.Services.AddScoped<IReceiptParsingUseCase, ReceiptParsingInteractor>();

// APIコントローラー
builder.Services.AddControllers();

// Swagger/OpenAPI設定
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ServerlessKakeibo API",
        Version = "v1"
    });

    // Annotationsサポート
    options.EnableAnnotations();
});

// CORS設定（開発環境用）
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
    });
}

var app = builder.Build();

#region Middleware設定

// 開発環境設定
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    // Swagger UI
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "ServerlessKakeibo API v1");
        options.RoutePrefix = string.Empty; // ルートでSwagger UIを表示
    });

    app.UseCors("AllowAll");
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

// コントローラーのマッピング
app.MapControllers();

#endregion

// Health Checkエンドポイント
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
    .WithName("HealthCheck")
    .WithTags("System")
    .ExcludeFromDescription();

app.Run();

// 統合テスト用のパーシャルクラス定義
public partial class Program { }
