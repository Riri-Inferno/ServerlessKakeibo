using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Application.Authentication;
using ServerlessKakeibo.Api.Application.Authentication.Usecases;
using ServerlessKakeibo.Api.Application.ReceiptParsing;
using ServerlessKakeibo.Api.Application.ReceiptParsing.UseCase;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Application.Transaction;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Application.TransactionQuery;
using ServerlessKakeibo.Api.Application.TransactionQuery.Usecases;
using ServerlessKakeibo.Api.Application.TransactionUpdate;
using ServerlessKakeibo.Api.Common.Settings;
using ServerlessKakeibo.Api.Domain.Receipt.Services;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.User.Services;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Application.TransactionSummary;
using ServerlessKakeibo.Api.Application.TransactionSummary.Usecases;
using ServerlessKakeibo.Api.Application.TransactionExport.Usecases;
using ServerlessKakeibo.Api.Application.TransactionExport;
using ServerlessKakeibo.Api.Application.Statistics.Usecases;
using ServerlessKakeibo.Api.Application.Statistics;

var builder = WebApplication.CreateBuilder(args);

// JSONシリアライズ時にEnumを文字列にする
builder.Services
    .AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// ApiController の自動 ModelState 400 応答を抑制して、コントローラーで独自ハンドリングする
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
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
builder.Services.Configure<GcpStorageSettings>(
    builder.Configuration.GetSection("GcpStorage"));

#region Authentication settings
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration.GetSection("Authentication:Jwt");
        var secretKey = jwtSettings["SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("JWT SecretKey が設定されていません");
        }

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(secretKey)
            ),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();
#endregion

// DI 登録
#region Services
builder.Services.AddScoped<IGcpAuthService, GcpAuthService>();
builder.Services.AddScoped<IVertexAiService, VertexAiService>();
builder.Services.AddScoped<IGoogleAiStudioService, GoogleAiStudioService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IGcpStorageService, GcpStorageService>();
builder.Services.AddScoped<IGcpStorageService, GcpStorageService>();
#endregion

#region UseCases
builder.Services.AddScoped<IReceiptParsingUseCase, ReceiptParsingInteractor>();
builder.Services.AddScoped<IRegistReceiptDetailsUseCase, RegistReceiptDetailsInteractor>();
builder.Services.AddScoped<ITransactionQueryUseCase, TransactionQueryInteractor>();
builder.Services.AddScoped<ITransactionCreateUseCase, TransactionCreateInteractor>();
builder.Services.AddScoped<ITransactionUpdateUseCase, TransactionUpdateInteractor>();
builder.Services.AddScoped<ITransactionDeleteUseCase, TransactionDeleteInteractor>();
builder.Services.AddScoped<IGoogleLoginUseCase, GoogleLoginInteractor>();
builder.Services.AddScoped<IRefreshTokenUseCase, RefreshTokenInteractor>();
builder.Services.AddScoped<IMonthlySummaryUseCase, MonthlySummaryInteractor>();
builder.Services.AddScoped<ITransactionAttachReceiptUseCase, TransactionAttachReceiptInteractor>();
builder.Services.AddScoped<IGetReceiptImageUrlUseCase, GetReceiptImageUrlInteractor>();
builder.Services.AddScoped<ITransactionExportUseCase, TransactionExportInteractor>();
builder.Services.AddScoped<IStatisticsUseCase, StatisticsInteractor>();
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
builder.Services.AddScoped<ITransactionHelper, TransactionHelper>();
builder.Services.AddScoped<IUserExternalLoginRepository, UserExternalLoginRepository>();
builder.Services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
#endregion

#region CORS settings
// CORS 設定（開発環境のみ全許可）
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", p =>
            p.WithOrigins("http://localhost:5173")
             .AllowAnyHeader()
             .AllowAnyMethod()
             .AllowCredentials()
             .WithExposedHeaders("*"));
    });
}
#endregion

#region Swagger settings
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

    // SwaggerでJWT認証を有効化
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

var app = builder.Build();

#region Middlewares
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

// 認証・認可
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
#endregion

#region Health check
// HealthCheck
app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timestamp = DateTime.UtcNow }))
   .WithTags("System")
   .ExcludeFromDescription();
#endregion

app.Run();

// 統合テスト用のパーシャルクラス
public partial class Program { }
