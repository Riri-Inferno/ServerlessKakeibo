using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.Transaction.Mappers;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;

namespace ServerlessKakeibo.Api.Application.Transaction;

/// <summary>
/// 取引作成インタラクター
/// </summary>
public class TransactionCreateInteractor : ITransactionCreateUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<TransactionCreateInteractor> _logger;
    private readonly IConfiguration _configuration;

    public TransactionCreateInteractor(
        IUnitOfWork unitOfWork,
        TransactionDomainService transactionDomainService,
        ILogger<TransactionCreateInteractor> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 取引を新規作成
    /// </summary>
    public async Task<TransactionResult> ExecuteAsync(
        CreateTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation(
                "取引作成処理を開始します。UserId: {UserId}",
                userId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // 1. ユーザーのTenantIdを取得
                var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

                // 2. リクエスト → エンティティ変換
                var transactionEntity = TransactionCreateMapper.ToEntity(
                    request, userId, tenantId);

                // 3. 子エンティティの追加
                foreach (var itemReq in request.Items)
                {
                    var itemEntity = TransactionCreateMapper.ToItemEntity(
                        itemReq, transactionEntity.Id, userId, tenantId);
                    transactionEntity.Items.Add(itemEntity);
                }

                foreach (var taxReq in request.Taxes)
                {
                    var taxEntity = TransactionCreateMapper.ToTaxEntity(
                        taxReq, transactionEntity.Id, userId, tenantId);
                    transactionEntity.Taxes.Add(taxEntity);
                }

                if (request.ShopDetails != null)
                {
                    transactionEntity.ShopDetail = TransactionCreateMapper.ToShopEntity(
                        request.ShopDetails, transactionEntity.Id, userId, tenantId);
                }

                // 4. ドメインモデルに変換して検証
                var transactionDomain = Application.RegistReceiptDetails.Mappers.TransactionMapper
                    .ToDomainModel(transactionEntity);
                var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

                // 5. 検証結果の処理
                var warnings = new List<string>();
                if (!validationResult.IsValid)
                {
                    var criticalErrors = validationResult.Errors
                        .Where(e => e.Severity == ErrorSeverity.Critical)
                        .ToList();

                    var validationWarnings = validationResult.Errors
                        .Where(e => e.Severity == ErrorSeverity.Warning)
                        .ToList();

                    if (criticalErrors.Any())
                    {
                        var errorMessage = string.Join(", ", criticalErrors.Select(e => e.Message));
                        _logger.LogError("取引データに致命的エラーがあります: {Errors}", errorMessage);
                        throw new InvalidOperationException($"取引データが不正です: {errorMessage}");
                    }

                    if (validationWarnings.Any())
                    {
                        warnings = validationWarnings.Select(w => w.Message).ToList();
                        _logger.LogWarning("取引データに警告があります: {Warnings}",
                            string.Join(", ", warnings));
                    }
                }

                // 6. データベースに保存
                var writeRepo = _unitOfWork.WriteRepository<TransactionEntity>();
                await writeRepo.AddAsync(transactionEntity, cancellationToken);

                _logger.LogInformation(
                    "取引を作成しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}",
                    transactionEntity.Id, userId, transactionEntity.AmountTotal);

                // 7. 結果DTOを返す
                return new TransactionResult
                {
                    TransactionId = transactionEntity.Id,
                    TransactionDate = transactionEntity.TransactionDate ?? DateTimeOffset.UtcNow,
                    AmountTotal = transactionEntity.AmountTotal ?? 0,
                    Currency = transactionEntity.Currency,
                    Payee = transactionEntity.Payee,
                    Category = transactionEntity.Category,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    ValidationWarnings = warnings
                };

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引作成中にエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
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
            _logger.LogError("ユーザーが存在しません。UserId: {UserId}", userId);
            throw new InvalidOperationException($"ユーザーが存在しません。UserId: {userId}");
        }

        if (user.TenantId == Guid.Empty)
        {
            var defaultTenantId = GetDefaultTenantId();
            _logger.LogWarning(
                "ユーザーのTenantIdが未設定です。デフォルトTenantIdを使用します。UserId: {UserId}, TenantId: {TenantId}",
                userId, defaultTenantId);
            return defaultTenantId;
        }

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
