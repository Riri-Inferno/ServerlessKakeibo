using ServerlessKakeibo.Api.Application.TransactionUpsert.Dto;
using ServerlessKakeibo.Api.Application.TransactionUpsert.Mappers;
using ServerlessKakeibo.Api.Application.TransactionUpsert.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionUpsert;

/// <summary>
/// 取引Upsertインタラクター
/// </summary>
public class TransactionUpsertInteractor : ITransactionUpsertUseCase
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITransactionRepository _transactionRepository;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<TransactionUpsertInteractor> _logger;
    private readonly IConfiguration _configuration;

    public TransactionUpsertInteractor(
        IUnitOfWork unitOfWork,
        ITransactionRepository transactionRepository,
        TransactionDomainService transactionDomainService,
        ILogger<TransactionUpsertInteractor> logger,
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 取引を登録/更新
    /// </summary>
    public async Task<UpsertTransactionResult> ExecuteAsync(
        UpsertTransactionRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        var operation = request.Id.HasValue ? UpsertOperation.Updated : UpsertOperation.Created;

        try
        {
            _logger.LogInformation(
                "取引{Operation}処理を開始します。UserId: {UserId}, TransactionId: {TransactionId}",
                operation, userId, request.Id);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            // ★ デバッグ: DbContext のハッシュコードを確認
            var context = (_transactionRepository as TransactionRepository)?.GetType()
                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_transactionRepository) as ApplicationDbContext;

            _logger.LogWarning("DbContext ハッシュ: {Hash}", context?.GetHashCode() ?? -1);

            // ★ UnitOfWork の DbContext も確認
            var uowContext = _unitOfWork.GetType()
                .GetField("_context", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(_unitOfWork) as ApplicationDbContext;

            _logger.LogWarning("DbContext ハッシュ: UnitOfWork={UowHash}", uowContext?.GetHashCode() ?? -1);

            var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

            // 2. 既存データ取得(更新時)
            TransactionEntity? existingEntity = null;
            if (request.Id.HasValue)
            {
                existingEntity = await _transactionRepository.GetForUpdateAsync(
            request.Id.Value, userId, cancellationToken);

                if (existingEntity == null)
                {
                    throw new InvalidOperationException(
                $"指定された取引が見つかりません。TransactionId: {request.Id.Value}");
                }

                // ★ 取得直後の xmin を確認
                _logger.LogWarning(
                    "取得時 - Transaction xmin: {Xmin}, Items: {Items}",
                    existingEntity.RowVersion,
                    string.Join(", ", existingEntity.Items.Select(i => $"{i.Name}(xmin:{i.RowVersion})")));
            }

            // 3. リクエスト → エンティティ変換
            var transactionEntity = TransactionUpsertMapper.ToEntity(
        request, userId, tenantId, existingEntity);

            // 4. 子エンティティの処理
            ProcessChildEntities(
        transactionEntity, request, userId, tenantId);

            // ★ ProcessChildEntities 直後
            _logger.LogWarning(
                "ProcessChildEntities直後 - Items数: {Count}, 内訳: {Items}",
                transactionEntity.Items.Count,
                string.Join(", ", transactionEntity.Items.Select(i => $"{i.Name}(Id:{i.Id}, New:{i.Id == Guid.Empty})")));

            // 5. ドメインモデルに変換して検証
            var transactionDomain = Application.RegistReceiptDetails.Mappers.TransactionMapper
        .ToDomainModel(transactionEntity);
            var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

            // ★ 検証後
            _logger.LogWarning(
                "検証後 - Items数: {Count}, 内訳: {Items}",
                transactionEntity.Items.Count,
                string.Join(", ", transactionEntity.Items.Select(i => $"{i.Name}")));

            // 6. 検証結果の処理
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

            // 7. データベースに保存
            if (operation == UpsertOperation.Created)
            {
                var writeRepo = _unitOfWork.WriteRepository<TransactionEntity>();
                await writeRepo.AddAsync(transactionEntity, cancellationToken);
            }
            /* 更新時は何もしない（EFが自動的に追跡して SaveChanges で更新される）*/

            // ★ SaveChanges 直前に移動
            _logger.LogWarning(
                "SaveChanges直前 - Items数: {Count}, 内訳: {Items}",
                transactionEntity.Items.Count,
                string.Join(", ", transactionEntity.Items.Select(i => $"{i.Name}(Id:{i.Id})")));

            _logger.LogInformation(
                "取引を{Operation}しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}",
                operation, transactionEntity.Id, userId, transactionEntity.AmountTotal);


            // 8. 結果DTOを返す
            return new UpsertTransactionResult
            {
                TransactionId = transactionEntity.Id,
                Operation = operation,
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
            _logger.LogError(ex, "取引{Operation}中にエラーが発生しました。UserId: {UserId}",
                operation, userId);
            throw;
        }
    }

    /// <summary>
    /// 子エンティティ(Items, Taxes, ShopDetail)を処理
    /// </summary>
    private void ProcessChildEntities(
    TransactionEntity transactionEntity,
    UpsertTransactionRequest request,
    Guid userId,
    Guid tenantId)
    {
        var now = DateTimeOffset.UtcNow;

        // ========== Items 処理 ==========
        var requestItemIds = request.Items
            .Where(i => i.Id.HasValue)
            .Select(i => i.Id!.Value)
            .ToHashSet();

        // 削除された Items を特定
        var itemsToRemove = transactionEntity.Items
            .Where(i => !requestItemIds.Contains(i.Id))
            .ToList();

        foreach (var item in itemsToRemove)
        {
            transactionEntity.Items.Remove(item);
        }

        // 更新 or 追加
        foreach (var itemReq in request.Items)
        {
            if (itemReq.Id.HasValue)
            {
                // 既存項目の更新
                var existingItem = transactionEntity.Items
                    .FirstOrDefault(i => i.Id == itemReq.Id.Value);

                if (existingItem != null)
                {
                    // Mapper を使って既存エンティティを更新
                    TransactionUpsertMapper.ToItemEntity(
                        itemReq, transactionEntity.Id, userId, tenantId, existingItem);
                }
            }
            else
            {
                // 新規項目の追加
                // var newItem = TransactionUpsertMapper.ToItemEntity(
                //     itemReq, transactionEntity.Id, userId, tenantId, null);
                // transactionEntity.Items.Add(newItem);
                // 新規項目の追加
                var newItem = new TransactionItemEntity  // ★ Mapper を使わず直接作成
                {
                    Id = Guid.NewGuid(),
                    TransactionId = transactionEntity.Id,
                    TenantId = tenantId,
                    CreatedBy = userId,
                    Name = itemReq.Name,
                    Quantity = itemReq.Quantity,
                    UnitPrice = itemReq.UnitPrice,
                    Amount = itemReq.Amount,
                    Category = itemReq.Category,
                    UpdatedBy = userId
                };

                transactionEntity.Items.Add(newItem);
            }
        }

        // ========== Taxes 処理 ==========
        var requestTaxIds = request.Taxes
            .Where(t => t.Id.HasValue)
            .Select(t => t.Id!.Value)
            .ToHashSet();

        var taxesToRemove = transactionEntity.Taxes
            .Where(t => !requestTaxIds.Contains(t.Id))
            .ToList();

        foreach (var tax in taxesToRemove)
        {
            transactionEntity.Taxes.Remove(tax);
        }

        foreach (var taxReq in request.Taxes)
        {
            if (taxReq.Id.HasValue)
            {
                var existingTax = transactionEntity.Taxes
                    .FirstOrDefault(t => t.Id == taxReq.Id.Value);

                if (existingTax != null)
                {
                    // Mapper を使って既存エンティティを更新
                    TransactionUpsertMapper.ToTaxEntity(
                        taxReq, transactionEntity.Id, userId, tenantId, existingTax);
                }
            }
            else
            {
                var newTax = TransactionUpsertMapper.ToTaxEntity(
                    taxReq, transactionEntity.Id, userId, tenantId, null);
                transactionEntity.Taxes.Add(newTax);
            }
        }

        // ========== ShopDetail 処理 ==========
        if (request.ShopDetails != null)
        {
            // 既存エンティティを渡す
            transactionEntity.ShopDetail = TransactionUpsertMapper.ToShopEntity(
                request.ShopDetails, transactionEntity.Id, userId, tenantId,
                transactionEntity.ShopDetail);
        }
        else
        {
            transactionEntity.ShopDetail = null;
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
