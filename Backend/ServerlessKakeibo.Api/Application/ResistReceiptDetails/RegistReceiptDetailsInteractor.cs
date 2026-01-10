using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails.Dto;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails.Mappers;
using ServerlessKakeibo.Api.Application.RegistReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Receipt.Models;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;

namespace ServerlessKakeibo.Api.Application.RegistReceiptDetails;

/// <summary>
/// 領収書詳細保存インタラクター
/// </summary>
public class RegistReceiptDetailsInteractor : IRegistReceiptDetailsUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<RegistReceiptDetailsInteractor> _logger;
    private readonly IConfiguration _configuration;

    public RegistReceiptDetailsInteractor(
        IUnitOfWork unitOfWork,
        TransactionDomainService transactionDomainService,
        ILogger<RegistReceiptDetailsInteractor> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 領収書解析結果を取引として保存
    /// </summary>
    public async Task<SaveTransactionResultDto> ExecuteSaveAsync(
        SaveReceiptParseResultRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation("取引保存処理を開始します。UserId: {UserId}", userId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. TenantId取得
                var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

                // 2. リクエスト → ReceiptParseResult 変換(内部処理用)
                var parseResult = MapToParseResult(request);

                // 3. DTO → エンティティ変換(カテゴリを渡す)
                var transactionEntity = TransactionMapper.ToEntity(
                    parseResult,
                    userId,
                    tenantId,
                    request.Category);

                // 4. ドメインモデルに変換して検証
                var transactionDomain = TransactionMapper.ToDomainModel(transactionEntity);
                var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

                // 5. 検証結果の処理
                if (!validationResult.IsValid)
                {
                    var criticalErrors = validationResult.Errors
                        .Where(e => e.Severity == ErrorSeverity.Critical)
                        .ToList();

                    var warnings = validationResult.Errors
                        .Where(e => e.Severity == ErrorSeverity.Warning)
                        .ToList();

                    if (criticalErrors.Any())
                    {
                        var errorMessage = string.Join(", ", criticalErrors.Select(e => e.Message));
                        _logger.LogError("取引データに致命的エラーがあります: {Errors}", errorMessage);

                        throw new InvalidOperationException(
                            $"取引データが不正です: {errorMessage}");
                    }

                    if (warnings.Any())
                    {
                        _logger.LogWarning("取引データに警告があります: {Warnings}",
                            string.Join(", ", warnings.Select(w => w.Message)));
                    }
                }

                // 6. データベースに保存
                var writeRepo = _unitOfWork.WriteRepository<TransactionEntity>();
                await writeRepo.AddAsync(transactionEntity, cancellationToken);

                _logger.LogInformation(
                    "取引を保存しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}, Category: {Category}",
                    transactionEntity.Id,
                    userId,
                    transactionEntity.AmountTotal,
                    transactionEntity.Category.ToJapanese());

                // 7. 結果DTOを返す
                return new SaveTransactionResultDto
                {
                    TransactionId = transactionEntity.Id,
                    TransactionDate = transactionEntity.TransactionDate,
                    AmountTotal = transactionEntity.AmountTotal,
                    Currency = transactionEntity.Currency,
                    Payee = transactionEntity.Payee,
                    SavedAt = DateTimeOffset.UtcNow,
                    Category = transactionEntity.Category,
                    ValidationWarnings = validationResult.Errors
                        .Where(e => e.Severity == ErrorSeverity.Warning)
                        .Select(e => e.Message)
                        .ToList()
                };

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引保存中にエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }

    /// <summary>
    /// リクエスト → ReceiptParseResult 変換
    /// </summary>
    private ReceiptParseResult MapToParseResult(SaveReceiptParseResultRequest request)
    {
        return new ReceiptParseResult
        {
            ReceiptType = request.ReceiptType,
            Confidence = request.Confidence,
            ParseStatus = request.ParseStatus,
            Warnings = request.Warnings,
            MissingFields = request.MissingFields,
            Raw = null,
            Normalized = new NormalizedTransaction
            {
                TransactionDate = request.Normalized.TransactionDate,
                AmountTotal = request.Normalized.AmountTotal,
                Currency = request.Normalized.Currency,
                Payer = request.Normalized.Payer,
                Payee = request.Normalized.Payee,
                PaymentMethod = request.Normalized.PaymentMethod,
                Taxes = request.Normalized.Taxes.Select(t => new TaxInfo
                {
                    TaxRate = t.TaxRate,
                    TaxAmount = t.TaxAmount,
                    TaxableAmount = t.TaxableAmount
                }).ToList(),
                Items = request.Normalized.Items.Select(i => new NormalizedItem
                {
                    Name = i.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Amount = i.Amount,
                    Category = i.Category
                }).ToList(),
                ShopDetails = request.Normalized.ShopDetails != null
                    ? new ShopDetails
                    {
                        Name = request.Normalized.ShopDetails.Name,
                        Branch = request.Normalized.ShopDetails.Branch,
                        PhoneNumber = request.Normalized.ShopDetails.PhoneNumber,
                        Address = request.Normalized.ShopDetails.Address,
                        PostalCode = request.Normalized.ShopDetails.PostalCode,
                        InvoiceRegistrationNumber = request.Normalized.ShopDetails.InvoiceRegistrationNumber,
                        RegisteredBusinessName = request.Normalized.ShopDetails.RegisteredBusinessName
                    }
                    : null
            }
        };
    }

    /// <summary>
    /// ユーザーのTenantIdを取得
    /// </summary>
    private async Task<Guid> GetUserTenantIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userRepo = _unitOfWork.ReadRepository<UserEntity>();
        var user = await userRepo.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogError("ユーザーが存在しません。保存を拒否します。UserId: {UserId}", userId);
            throw new InvalidOperationException($"ユーザーが存在しません。UserId: {userId}");
        }

        if (user.TenantId == Guid.Empty)
        {
            var defaultTenantId = GetDefaultTenantId();

            _logger.LogWarning(
                "ユーザーのTenantIdが未設定です。デフォルトTenantIdを使用します。UserId: {UserId}, TenantId: {TenantId}",
                userId,
                defaultTenantId);

            return defaultTenantId;
        }

        _logger.LogDebug("TenantIdを取得しました。UserId: {UserId}, TenantId: {TenantId}", userId, user.TenantId);
        return user.TenantId;
    }

    /// <summary>
    /// デフォルトTenantIdを取得
    /// </summary>
    private Guid GetDefaultTenantId()
    {
        var tenantIdString = _configuration["DefaultTenantId"]
            ?? "deadeade-0001-0000-0000-000000000001";

        if (!Guid.TryParse(tenantIdString, out var tenantId))
        {
            _logger.LogWarning("デフォルトTenantIdの解析に失敗しました。ハードコード値を使用します。");
            return Guid.Parse("deadeade-0001-0000-0000-000000000001");
        }

        return tenantId;
    }
}
