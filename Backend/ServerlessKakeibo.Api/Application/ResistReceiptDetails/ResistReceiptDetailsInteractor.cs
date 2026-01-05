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

    public ResistReceiptDetailsInteractor(
        IUnitOfWork unitOfWork,
        TransactionDomainService transactionDomainService,
        ILogger<ResistReceiptDetailsInteractor> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _transactionDomainService = transactionDomainService ?? throw new ArgumentNullException(nameof(transactionDomainService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
        {
            throw new ArgumentNullException(nameof(parseResult));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("UserId cannot be empty", nameof(userId));
        }

        try
        {
            _logger.LogInformation("取引保存処理を開始します。UserId: {UserId}", userId);

            return await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                // TODO: 将来的にはユーザーのTenantIdを取得
                var tenantId = Guid.Parse("deadeade-0001-0000-0000-000000000001");

                // 1. DTO → エンティティ変換
                var transactionEntity = TransactionMapper.ToEntity(parseResult, userId, tenantId);

                // 2. ドメインモデルに変換して検証
                var transactionDomain = TransactionMapper.ToDomainModel(transactionEntity);
                var validationResult = _transactionDomainService.ValidateTransaction(transactionDomain);

                if (!validationResult.IsValid)
                {
                    _logger.LogWarning("取引データの検証エラー: {Errors}",
                        string.Join(", ", validationResult.Errors));
                    // 検証エラーでも保存は続行（警告として記録）
                }

                // 3. カテゴリの推定
                var suggestedCategory = _transactionDomainService.SuggestCategory(transactionDomain);
                _logger.LogDebug("推定カテゴリ: {Category}", suggestedCategory);

                // 4. データベースに保存
                var writeRepo = _unitOfWork.WriteRepository<TransactionEntity>();
                await writeRepo.AddAsync(transactionEntity, cancellationToken);

                _logger.LogInformation(
                    "取引を保存しました。TransactionId: {TransactionId}, UserId: {UserId}, Amount: {Amount}",
                    transactionEntity.Id,
                    userId,
                    transactionEntity.AmountTotal);

                // 結果DTOを返す
                return new SaveTransactionResultDto
                {
                    TransactionId = transactionEntity.Id,
                    TransactionDate = transactionEntity.TransactionDate,
                    AmountTotal = transactionEntity.AmountTotal,
                    Currency = transactionEntity.Currency,
                    Payee = transactionEntity.Payee,
                    SavedAt = DateTimeOffset.UtcNow,
                    SuggestedCategory = suggestedCategory
                };

            }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引保存中にエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }
}
