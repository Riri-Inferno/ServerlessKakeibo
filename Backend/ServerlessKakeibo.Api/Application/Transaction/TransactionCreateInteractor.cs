using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.Transaction.Mappers;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.Transaction;

/// <summary>
/// 取引作成インタラクター
/// </summary>
public class TransactionCreateInteractor : ITransactionCreateUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericWriteRepository<TransactionEntity> _transactionWriteRepository;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<TransactionCreateInteractor> _logger;
    private readonly IConfiguration _configuration;

    public TransactionCreateInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<TransactionEntity> transactionWriteRepository,
        TransactionDomainService transactionDomainService,
        ILogger<TransactionCreateInteractor> logger,
        IConfiguration configuration)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _transactionWriteRepository = transactionWriteRepository ?? throw new ArgumentNullException(nameof(transactionWriteRepository));
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
                "取引作成処理を開します。UserId: {UserId}",
                userId);

            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // 1. ユーザーのTenantIdを取得
                var tenantId = await GetUserTenantIdAsync(userId, cancellationToken);

                // 2. リクエスト → エンティティ変換
                var transactionEntity = TransactionCreateMapper.ToEntity(
                    request, userId, tenantId);

                // 3. 子エンティティの追加
                // 支出の場合は Items から AmountTotal を再計算、収入の場合はリクエスト値を使用
                if (request.Type == TransactionType.Expense)
                {
                    foreach (var itemReq in request.Items)
                    {
                        var itemEntity = TransactionCreateMapper.ToItemEntity(
                            itemReq, transactionEntity.Id, userId, tenantId);
                        transactionEntity.Items.Add(itemEntity);
                    }

                    // 支出の場合は Items 合計 + 税額で AmountTotal を上書き
                    var itemsTotal = transactionEntity.Items.Sum(i => i.Amount ?? 0);
                    var taxTotal = 0m;

                    foreach (var taxReq in request.Taxes)
                    {
                        var taxEntity = TransactionCreateMapper.ToTaxEntity(
                            taxReq, transactionEntity.Id, userId, tenantId);
                        transactionEntity.Taxes.Add(taxEntity);
                        taxTotal += taxEntity.TaxAmount ?? 0;
                    }

                    transactionEntity.AmountTotal = itemsTotal + taxTotal;
                }
                else // Income の場合
                {
                    // 収入の場合は Items は任意（あれば追加）
                    if (request.Items != null)
                    {
                        foreach (var itemReq in request.Items)
                        {
                            var itemEntity = TransactionCreateMapper.ToItemEntity(
                                itemReq, transactionEntity.Id, userId, tenantId);
                            transactionEntity.Items.Add(itemEntity);
                        }
                    }

                    // 税金も任意（源泉徴収など）
                    if (request.Taxes != null)
                    {
                        foreach (var taxReq in request.Taxes)
                        {
                            var taxEntity = TransactionCreateMapper.ToTaxEntity(
                                taxReq, transactionEntity.Id, userId, tenantId);
                            transactionEntity.Taxes.Add(taxEntity);
                        }
                    }

                    // AmountTotal はリクエスト値をそのまま使用
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
                await _transactionWriteRepository.AddAsync(transactionEntity, cancellationToken);

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
            });
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
        var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            _logger.LogError("ユーザーが存在しません。取引作成を拒否します。UserId: {UserId}", userId);
            throw new KeyNotFoundException($"ユーザーが存在しません。UserId: {userId}");
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
