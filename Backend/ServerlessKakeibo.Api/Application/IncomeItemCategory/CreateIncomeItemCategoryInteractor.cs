using ServerlessKakeibo.Api.Application.IncomeItemCategory.Dto;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Mappers;
using ServerlessKakeibo.Api.Application.IncomeItemCategory.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.IncomeItemCategory;

public class CreateIncomeItemCategoryInteractor : ICreateIncomeItemCategoryUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IUserSettingsRepository _userSettingsRepository;
    private readonly IGenericWriteRepository<UserIncomeItemCategoryEntity> _categoryWriteRepository;
    private readonly ILogger<CreateIncomeItemCategoryInteractor> _logger;

    public CreateIncomeItemCategoryInteractor(
        ITransactionHelper transactionHelper,
        IUserSettingsRepository userSettingsRepository,
        IGenericWriteRepository<UserIncomeItemCategoryEntity> categoryWriteRepository,
        ILogger<CreateIncomeItemCategoryInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userSettingsRepository = userSettingsRepository ?? throw new ArgumentNullException(nameof(userSettingsRepository));
        _categoryWriteRepository = categoryWriteRepository ?? throw new ArgumentNullException(nameof(categoryWriteRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<IncomeItemCategoryResult> ExecuteAsync(
        Guid userId,
        CreateIncomeItemCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "カスタム給与項目カテゴリ作成を開始します。UserId: {UserId}, Name: {Name}",
            userId, request.Name);

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            var userSettings = await _userSettingsRepository.GetByUserIdAsync(userId, cancellationToken);
            if (userSettings == null)
            {
                throw new InvalidOperationException("ユーザー設定が見つかりません");
            }

            var now = DateTimeOffset.UtcNow;
            var customCode = $"Custom_{request.Name}_{Guid.NewGuid().ToString("N")[..8]}";

            var entity = new UserIncomeItemCategoryEntity
            {
                Id = Guid.NewGuid(),
                UserSettingsId = userSettings.Id,
                MasterCategoryId = null,
                Name = request.Name,
                Code = customCode,
                ColorCode = request.ColorCode,
                DisplayOrder = 9999,
                IsCustom = true,
                IsHidden = false,
                TenantId = userSettings.TenantId,
                CreatedAt = now,
                UpdatedAt = now,
                CreatedBy = userId,
                UpdatedBy = userId
            };

            await _categoryWriteRepository.AddAsync(entity, cancellationToken);

            _logger.LogInformation("カスタム給与項目カテゴリを作成しました。CategoryId: {CategoryId}", entity.Id);

            return new IncomeItemCategoryResult
            {
                Category = IncomeItemCategoryMapper.ToDto(entity),
                Message = "カテゴリを作成しました"
            };
        });
    }
}
