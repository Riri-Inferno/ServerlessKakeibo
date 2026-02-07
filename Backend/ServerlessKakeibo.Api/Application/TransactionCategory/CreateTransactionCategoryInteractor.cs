using ServerlessKakeibo.Api.Application.TransactionCategory.Dto;
using ServerlessKakeibo.Api.Application.TransactionCategory.Mappers;
using ServerlessKakeibo.Api.Application.TransactionCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.TransactionCategory;

/// <summary>
/// 取引カテゴリ作成インタラクター
/// </summary>
public class CreateTransactionCategoryInteractor : ICreateTransactionCategoryUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IGenericWriteRepository<UserTransactionCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<CreateTransactionCategoryInteractor> _logger;

    public CreateTransactionCategoryInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IGenericWriteRepository<UserTransactionCategoryEntity> categoryWriteRepository,
        ILogger<CreateTransactionCategoryInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryWriteRepository = categoryWriteRepository ?? throw new ArgumentNullException(nameof(categoryWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TransactionCategoryResult> ExecuteAsync(
        Guid userId,
        CreateTransactionCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "カスタム取引カテゴリ作成を開始します。UserId: {UserId}, Name: {Name}",
            userId, request.Name);

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            // ユーザー設定を取得
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            var now = DateTimeOffset.UtcNow;

            // カスタムコード生成
            var customCode = $"Custom_{request.Name}_{Guid.NewGuid().ToString("N")[..8]}";

            // エンティティ作成
            var entity = new UserTransactionCategoryEntity
            {
                Id = Guid.NewGuid(),
                UserSettingsId = userSettings.Id,
                MasterCategoryId = null,
                Name = request.Name,
                Code = customCode,
                ColorCode = request.ColorCode,
                DisplayOrder = 9999,
                IsIncome = request.IsIncome,
                IsCustom = true,
                IsHidden = false,
                TenantId = userSettings.TenantId,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await _categoryWriteRepository.AddAsync(entity, cancellationToken);

            _logger.LogInformation(
                "カスタム取引カテゴリを作成しました。CategoryId: {CategoryId}",
                entity.Id);

            return new TransactionCategoryResult
            {
                Category = TransactionCategoryMapper.ToDto(entity),
                Message = "カテゴリを作成しました"
            };
        });
    }
}
