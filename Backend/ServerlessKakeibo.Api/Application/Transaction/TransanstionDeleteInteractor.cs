using ServerlessKakeibo.Api.Application.Transaction.Dto;
using ServerlessKakeibo.Api.Application.Transaction.Mappers;
using ServerlessKakeibo.Api.Application.Transaction.Usecases;
using ServerlessKakeibo.Api.Domain.Transaction.Services;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.Transaction;

/// <summary>
/// 取引論理削除インタラクター
/// </summary>
public class TransactionDeleteInteractor : ITransactionDeleteUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly TransactionDomainService _transactionDomainService;
    private readonly ILogger<TransactionDeleteInteractor> _logger;
    private readonly IConfiguration _configuration;

    public TransactionDeleteInteractor(
        ITransactionHelper transactionHelper,
        ITransactionRepository transactionRepository,
        IGenericReadRepository<UserEntity> userReadRepository,
        TransactionDomainService transactionDomainService,
        ILogger<TransactionDeleteInteractor> logger,
        IConfiguration configuration)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _transactionRepository = transactionRepository ?? throw new ArgumentNullException(nameof(transactionRepository));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// 取引を論理削除
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="userId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public async Task<TransactionDeleteResult> ExecuteAsync(
        Guid transactionId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (transactionId == Guid.Empty)
            throw new ArgumentException("Transaction ID cannot be empty", nameof(transactionId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        try
        {
            _logger.LogInformation(
                "取引削除処理を開始します。UserId: {UserId}, TransactionId: {TransactionId}",
                userId, transactionId);

            return await _transactionHelper.ExecuteInTransactionAsync(
            async () =>
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

                // 3. ドメイン検証(削除前の情報提供)
                var domainModel = TransactionCreateMapper.ToDomainModel(existingEntity);
                var validationResult = _transactionDomainService.ValidateDelete(domainModel);

                // 警告ログ出力
                var warnings = validationResult.Warnings.Select(w => w.Message).ToList();
                foreach (var warning in warnings)
                {
                    _logger.LogInformation(
                        "削除情報: {Message} (TransactionId: {TransactionId})",
                        warning, transactionId);
                }

                // 4. 既存の取引を削除
                await _transactionRepository.SoftDeleteWithRelatedDataAsync(
                    transactionId, userId, cancellationToken);

                _logger.LogInformation(
                    "取引を削除しました。TransactionId: {TransactionId}, Amount: {Amount}",
                    transactionId, existingEntity.AmountTotal);

                // 5. 結果DTOを返す
                return new TransactionDeleteResult
                {
                    TransactionId = existingEntity.Id,
                    ProcessedAt = DateTimeOffset.UtcNow,
                    ValidationWarnings = warnings
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引削除中にエラーが発生しました。UserId: {UserId}, TransactionId: {TransactionId}",
                            userId, transactionId);
            throw;
        }
    }

    #region private methods

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
    #endregion
}
