using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Application.ResistReceiptDetails.Dto;
using ServerlessKakeibo.Api.Application.ResistReceiptDetails.Mappers;
using ServerlessKakeibo.Api.Application.ResistReceiptDetails.Usecases;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;

namespace ServerlessKakeibo.Api.Application.ResistReceiptDetails;

/// <summary>
/// 領収書詳細保存インタラクター
/// </summary>
public class ResistReceiptDetailsInteractor : IResistReceiptDetailsUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<ResistReceiptDetailsInteractor> _logger;
    private readonly IConfiguration _configuration;

    public ResistReceiptDetailsInteractor(
        IUnitOfWork unitOfWork,
        TransactionDomainService transactionDomainService,
        ILogger<ResistReceiptDetailsInteractor> logger,
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
        ReceiptParseResult parseResult,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (parseResult == null)
            throw new ArgumentNullException(nameof(parseResult));

        if (userId == Guid.Empty)
            throw new ArgumentException("UserId cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation("取引保存処理を開始します。UserId: {UserId}", userId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. TenantId取得
                var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

                // 2. DTO → エンティティ変換
                var transactionEntity = TransactionMapper.ToEntity(parseResult, userId, tenantId);

                // 3. ドメインモデルに変換して検証
                var transactionDomain = TransactionMapper.ToDomainModel(transactionEntity);
                var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

                // 4. 検証結果の処理
                if (!validationResult.IsValid)
                {
                    var criticalErrors = validationResult.Errors
                        .Where(e => e.Severity == Domain.ValueObjects.ErrorSeverity.Critical)
                        .ToList();

                    var warnings = validationResult.Errors
                        .Where(e => e.Severity == Domain.ValueObjects.ErrorSeverity.Warning)
                        .ToList();

                    if (criticalErrors.Any())
                    {
                        // 致命的エラーがある場合は保存拒否
                        var errorMessage = string.Join(", ", criticalErrors.Select(e => e.Message));
                        _logger.LogError("取引データに致命的エラーがあります: {Errors}", errorMessage);

                        throw new InvalidOperationException(
                            $"取引データが不正です: {errorMessage}");
                    }

                    if (warnings.Any())
                    {
                        // 警告のみの場合はログ記録して続行
                        _logger.LogWarning("取引データに警告があります: {Warnings}",
                            string.Join(", ", warnings.Select(w => w.Message)));
                    }
                }

                // 5. カテゴリの推定と設定
                var suggestedCategory = _transactionDomainService.SuggestCategory(transactionDomain);

                if (suggestedCategory != null)
                {
                    transactionEntity.CategoryId = suggestedCategory.Id;
                    transactionEntity.CategoryName = suggestedCategory.Name;

                    _logger.LogDebug(
                        "カテゴリを推定しました。CategoryId: {CategoryId}, CategoryName: {CategoryName}",
                        suggestedCategory.Id,
                        suggestedCategory.Name);
                }
                else
                {
                    _logger.LogDebug("カテゴリを推定できませんでした。未分類として保存します。");
                }

                // 6. データベースに保存
                var writeRepo = _unitOfWork.WriteRepository<TransactionEntity>();
                await writeRepo.AddAsync(transactionEntity, cancellationToken);

                _logger.LogInformation(
                    "取引を保存しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}, Category: {Category}",
                    transactionEntity.Id,
                    userId,
                    transactionEntity.AmountTotal,
                    suggestedCategory?.Name ?? "未分類");

                // 7. 結果DTOを返す
                return new SaveTransactionResultDto
                {
                    TransactionId = transactionEntity.Id,
                    TransactionDate = transactionEntity.TransactionDate,
                    AmountTotal = transactionEntity.AmountTotal,
                    Currency = transactionEntity.Currency,
                    Payee = transactionEntity.Payee,
                    SavedAt = DateTimeOffset.UtcNow,
                    SuggestedCategory = suggestedCategory?.Name,
                    CategoryId = suggestedCategory?.Id,
                    ValidationWarnings = validationResult.Errors
                        .Where(e => e.Severity == Domain.ValueObjects.ErrorSeverity.Warning)
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
    /// ユーザーのTenantIdを取得
    /// </summary>
    private async Task<Guid> GetUserTenantIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        // ユーザーエンティティからTenantIdを取得
        var userRepo = _unitOfWork.ReadRepository<UserEntity>();
        var user = await userRepo.GetByIdAsync(userId, cancellationToken);

        // ユーザーが存在しない場合は保存拒否
        if (user == null)
        {
            _logger.LogError("ユーザーが存在しません。保存を拒否します。UserId: {UserId}", userId);
            throw new InvalidOperationException($"ユーザーが存在しません。UserId: {userId}");
        }

        // TenantIdが未設定の場合はデフォルト値を使用(仮データ対応)
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
