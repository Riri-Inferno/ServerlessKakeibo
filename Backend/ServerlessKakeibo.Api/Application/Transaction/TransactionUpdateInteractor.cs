using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Application.Transaction.Mappers;

namespace ServerlessKakeibo.Api.Application.TransactionUpdate;

/// <summary>
/// 取引更新インタラクター
/// </summary>
public class TransactionUpdateInteractor : ITransactionUpdateUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericWriteRepository<TransactionEntity> _transactionWriteRepository;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<TransactionUpdateInteractor> _logger;
    private readonly IConfiguration _configuration;

    public TransactionUpdateInteractor(
        ITransactionHelper transactionHelper,
        ITransactionRepository transactionRepository,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<TransactionEntity> transactionWriteRepository,
        TransactionDomainService transactionDomainService,
        ILogger<TransactionUpdateInteractor> logger,
        IConfiguration configuration)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _transactionWriteRepository = transactionWriteRepository ?? throw new ArgumentNullException(nameof(transactionWriteRepository));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 取引を更新（DELETE → INSERT 方式）
    /// </summary>
    public async Task<TransactionResult> ExecuteAsync(
        Guid transactionId,
        UpdateTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (transactionId == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation(
                "取引更新処理を開始します（DELETE→INSERT方式）。UserId: {UserId}, TransactionId: {TransactionId}",
                userId, transactionId);

            return await _transactionHelper.ExecuteInTransactionWithIntermediateSaveAsync(
            async (saveChanges) =>
            {
                // 1. ユーザーのTenantIdを取得
                var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

                // 2. 既存の取引が存在するか確認
                var existingEntity = await _transactionRepository.GetDetailByIdAsync(
                    transactionId, userId, cancellationToken);

                if (existingEntity == null)
                {
                    throw new KeyNotFoundException(
                        $"指定された取引が見つかりません。TransactionId: {transactionId}");
                }

                var existingType = existingEntity.Type;

                _logger.LogDebug("既存取引を確認しました。TransactionId: {TransactionId}", transactionId);

                // 3. 既存の取引を削除（CASCADE で子も削除される）
                await _transactionWriteRepository.DeleteAsync(transactionId, cancellationToken);
                await saveChanges();

                _logger.LogDebug("既存取引を削除しました。TransactionId: {TransactionId}", transactionId);

                // 4.新しい取引エンティティを作成
                var newEntity = CreateTransactionEntity(
                    transactionId, request, userId, tenantId, existingType);

                // 既存の添付情報を引き継ぐ
                newEntity.SourceUrl = existingEntity.SourceUrl;
                newEntity.ReceiptAttachedAt = existingEntity.ReceiptAttachedAt;

                // 収入の場合は既存の AmountTotal を維持
                if (existingType == TransactionType.Income)
                {
                    newEntity.AmountTotal = existingEntity.AmountTotal;
                }

                // 5. ドメイン検証
                var transactionDomain = TransactionCreateMapper.ToDomainModel(newEntity);
                var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

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

                // 6. 新しい取引を追加
                await _transactionWriteRepository.AddAsync(newEntity, cancellationToken);

                _logger.LogInformation(
                    "取引を再作成しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}",
                    newEntity.Id, userId, newEntity.AmountTotal);

                // 7. 結果DTOを返す
                return new TransactionResult
                {
                    TransactionId = newEntity.Id,
                    TransactionDate = newEntity.TransactionDate ?? DateTimeOffset.UtcNow,
                    AmountTotal = newEntity.AmountTotal ?? 0,
                    Currency = newEntity.Currency,
                    Payee = newEntity.Payee,
                    Category = newEntity.Category,
                    Notes = newEntity.Notes,
                    TaxInclusionType = newEntity.TaxInclusionType,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    ValidationWarnings = warnings,
                    SourceUrl = newEntity.SourceUrl,
                    ReceiptAttachedAt = newEntity.ReceiptAttachedAt
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引更新中にエラーが発生しました。UserId: {UserId}, TransactionId: {TransactionId}",
                userId, transactionId);
            throw;
        }
    }

    /// <summary>
    /// リクエストから新しい TransactionEntity を作成
    /// </summary>
    private TransactionEntity CreateTransactionEntity(
        Guid transactionId,
        UpdateTransactionRequest request,
        Guid userId,
        Guid tenantId,
        TransactionType existingType)
    {
        var now = DateTimeOffset.UtcNow;

        var entity = new TransactionEntity
        {
            Id = transactionId,
            UserId = userId,
            TenantId = tenantId,
            Type = existingType,  // 既存の Type を引き継ぐ
            TransactionDate = request.TransactionDate,
            Currency = request.Currency,
            Payer = request.Payer,
            Payee = request.Payee,
            PaymentMethod = request.PaymentMethod,
            Category = request.Category,
            Notes = request.Notes,
            TaxInclusionType = request.TaxInclusionType,
            CreatedBy = userId,
            UpdatedBy = userId,
            CreatedAt = now,
            UpdatedAt = now,
            Items = new List<TransactionItemEntity>(),
            Taxes = new List<TaxDetailEntity>()
        };

        // Items 追加（収入の場合は空でもOK）
        if (request.Items != null)
        {
            foreach (var itemReq in request.Items)
            {
                entity.Items.Add(TransactionCreateMapper.ToItemEntity(
                    new CreateTransactionItemRequest
                    {
                        Name = itemReq.Name,
                        Quantity = itemReq.Quantity,
                        UnitPrice = itemReq.UnitPrice,
                        Amount = itemReq.Amount,
                        Category = itemReq.Category
                    },
                    transactionId,
                    userId,
                    tenantId
                ));
            }
        }

        // Taxes 追加
        if (request.Taxes != null)
        {
            foreach (var taxReq in request.Taxes)
            {
                entity.Taxes.Add(TransactionCreateMapper.ToTaxEntity(
                    new CreateTaxDetailRequest
                    {
                        TaxRate = taxReq.TaxRate,
                        TaxAmount = taxReq.TaxAmount,
                        TaxableAmount = taxReq.TaxableAmount,
                        TaxType = taxReq.TaxType
                    },
                    transactionId,
                    userId,
                    tenantId
                ));
            }
        }

        // ShopDetail
        if (request.ShopDetails != null)
        {
            entity.ShopDetail = TransactionCreateMapper.ToShopEntity(
                new CreateShopDetailRequest
                {
                    Name = request.ShopDetails.Name,
                    Branch = request.ShopDetails.Branch,
                    PostalCode = request.ShopDetails.PostalCode,
                    Address = request.ShopDetails.Address,
                    PhoneNumber = request.ShopDetails.PhoneNumber
                },
                transactionId,
                userId,
                tenantId
            );
        }

        // 金額計算：支出の場合は Items + Taxes（税の扱いに応じて）、収入の場合は既存値を維持
        if (existingType == TransactionType.Expense)
        {
            var itemsTotal = entity.Items.Sum(i => i.Amount ?? 0);
            var taxTotal = entity.Taxes.Sum(t => t.TaxAmount ?? 0);

            // 税の扱いに応じて AmountTotal を設定
            if (request.TaxInclusionType == TaxInclusionType.Inclusive ||
                request.TaxInclusionType == TaxInclusionType.NoTax)
            {
                // 内税または非課税：itemsTotal がそのまま合計
                entity.AmountTotal = itemsTotal;
            }
            else // Exclusive または Unknown
            {
                // 外税：itemsTotal + 税額
                entity.AmountTotal = itemsTotal + taxTotal;
            }
        }
        // 収入の場合は AmountTotal は変更しない（既存の取引の値を維持する必要があるため、呼び出し元で設定）

        return entity;
    }

    /// <summary>
    /// ユーザーのTenantIdを取得
    /// </summary>
    private async Task<Guid> GetUserTenantIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);

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
