using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.TransactionUpdate.Mappers;
using ServerlessKakeibo.Api.Application.TransactionUpdate.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionUpdate;

/// <summary>
/// 取引更新インタラクター
/// </summary>
public class TransactionUpdateInteractor : ITransactionUpdateUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericWriteRepository<TransactionItemEntity> _itemWriteRepository;
    private readonly IGenericWriteRepository<TaxDetailEntity> _taxWriteRepository;
    private readonly IGenericWriteRepository<ShopDetailEntity> _shopWriteRepository;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<TransactionUpdateInteractor> _logger;
    private readonly IConfiguration _configuration;

    public TransactionUpdateInteractor(
        ITransactionHelper transactionHelper,
        ITransactionRepository transactionRepository,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<TransactionItemEntity> itemWriteRepository,
        IGenericWriteRepository<TaxDetailEntity> taxWriteRepository,
        IGenericWriteRepository<ShopDetailEntity> shopWriteRepository,
        TransactionDomainService transactionDomainService,
        ILogger<TransactionUpdateInteractor> logger,
        IConfiguration configuration)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _itemWriteRepository = itemWriteRepository ?? throw new ArgumentNullException(nameof(itemWriteRepository));
        _taxWriteRepository = taxWriteRepository ?? throw new ArgumentNullException(nameof(taxWriteRepository));
        _shopWriteRepository = shopWriteRepository ?? throw new ArgumentNullException(nameof(shopWriteRepository));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 取引を更新
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
                "取引更新処理を開始します。UserId: {UserId}, TransactionId: {TransactionId}",
                userId, transactionId);

            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // 1. ユーザーのTenantIdを取得
                var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

                // 2. 既存データ取得（削除前の状態）
                var existingEntity = await _transactionRepository.GetForUpdateAsync(
                    transactionId, userId, cancellationToken);

                if (existingEntity == null)
                {
                    throw new InvalidOperationException(
                        $"指定された取引が見つかりません。TransactionId: {transactionId}");
                }

                _logger.LogDebug(
                    "取得時 - Transaction xmin: {Xmin}, Items: {ItemCount}件",
                    existingEntity.RowVersion,
                    existingEntity.Items.Count);

                // 3. 削除対象の子エンティティを特定して事前削除（物理削除）
                await DeleteRemovedChildEntitiesAsync(
                    existingEntity, request, userId, cancellationToken);

                // 4. 削除後に再取得
                existingEntity = await _transactionRepository.GetForUpdateAsync(
                    transactionId, userId, cancellationToken);

                if (existingEntity == null)
                {
                    throw new InvalidOperationException(
                        $"取引の再取得に失敗しました。TransactionId: {transactionId}");
                }

                // 5. エンティティを更新
                TransactionUpdateMapper.UpdateEntity(request, existingEntity, userId);

                // 6. 子エンティティの処理（Full Replace方式）
                TransactionUpdateMapper.ProcessChildEntities(
                    existingEntity, request, userId, tenantId);

                _logger.LogDebug(
                    "ProcessChildEntities後 - Items数: {Count}, AmountTotal: {Amount}",
                    existingEntity.Items.Count,
                    existingEntity.AmountTotal);

                // 7. ドメインモデルに変換して検証
                var transactionDomain = Application.RegistReceiptDetails.Mappers.TransactionMapper
                    .ToDomainModel(existingEntity);
                var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

                // 8. 検証結果の処理
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

                // 9. ログ出力（SaveChangesはTransactionHelperが実行）
                _logger.LogDebug(
                    "SaveChanges直前 - Items数: {Count}, AmountTotal: {Amount}",
                    existingEntity.Items.Count,
                    existingEntity.AmountTotal);

                _logger.LogInformation(
                    "取引を更新しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}",
                    existingEntity.Id, userId, existingEntity.AmountTotal);

                // 10. 結果DTOを返す
                return new TransactionResult
                {
                    TransactionId = existingEntity.Id,
                    TransactionDate = existingEntity.TransactionDate ?? DateTimeOffset.UtcNow,
                    AmountTotal = existingEntity.AmountTotal ?? 0,
                    Currency = existingEntity.Currency,
                    Payee = existingEntity.Payee,
                    Category = existingEntity.Category,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    ValidationWarnings = warnings
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
    /// 削除対象の子エンティティを事前削除（物理削除）
    /// </summary>
    private async Task DeleteRemovedChildEntitiesAsync(
        TransactionEntity existingEntity,
        UpdateTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken)
    {
        // Items の物理削除
        var requestItemIds = request.Items
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        var itemsToDelete = existingEntity.Items
            .Where(i => !requestItemIds.Contains(i.Id))
            .ToList();

        foreach (var item in itemsToDelete)
        {
            await _itemWriteRepository.DeleteAsync(item.Id, cancellationToken);
            _logger.LogDebug("TransactionItem を物理削除しました。ItemId: {ItemId}", item.Id);
        }

        // Taxes の物理削除
        var requestTaxIds = request.Taxes
            .Where(t => t.Id.HasValue)
            .Select(t => t.Id!.Value)
            .ToHashSet();

        var taxesToDelete = existingEntity.Taxes
            .Where(t => !requestTaxIds.Contains(t.Id))
            .ToList();

        foreach (var tax in taxesToDelete)
        {
            await _taxWriteRepository.DeleteAsync(tax.Id, cancellationToken);
            _logger.LogDebug("TaxDetail を物理削除しました。TaxId: {TaxId}", tax.Id);
        }

        // ShopDetail の物理削除
        if (request.ShopDetails == null && existingEntity.ShopDetail != null)
        {
            await _shopWriteRepository.DeleteAsync(existingEntity.ShopDetail.Id, cancellationToken);
            _logger.LogDebug("ShopDetail を物理削除しました。ShopDetailId: {ShopDetailId}",
                existingEntity.ShopDetail.Id);
        }
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
