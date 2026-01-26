using ServerlessKakeibo.Api.Application.UserSettings.Dto;
using ServerlessKakeibo.Api.Application.UserSettings.Mappers;
using ServerlessKakeibo.Api.Application.UserSettings.Usecases;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.UserSettings;

/// <summary>
/// ユーザー設定取得インタラクター
/// </summary>
public class GetUserSettingsInteractor : IGetUserSettingsUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<Infrastructure.Data.Entities.UserEntity> _userRepository;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ILogger<GetUserSettingsInteractor> _logger;

    public GetUserSettingsInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<Infrastructure.Data.Entities.UserEntity> userRepository,
        IUserSettingsRepository settingsRepository,
        ILogger<GetUserSettingsInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// ユーザー設定を取得
    /// </summary>
    public async Task<UserSettingsDto> ExecuteAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("ユーザーIDが無効です", nameof(userId));

        try
        {
            _logger.LogInformation("ユーザー設定取得を開始します。UserId: {UserId}", userId);

            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // ユーザー情報を取得
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogError("ユーザーが見つかりません。UserId: {UserId}", userId);
                    throw new InvalidOperationException("ユーザー情報が見つかりません");
                }

                // 設定を取得(User情報を含む)
                var settings = await _settingsRepository.GetByUserIdAsync(userId, cancellationToken);

                _logger.LogInformation(
                    "ユーザー設定を取得しました。UserId: {UserId}, HasSettings: {HasSettings}",
                    userId, settings != null);

                // DTOに変換
                return UserSettingsMapper.ToDto(user, settings);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ユーザー設定取得中にエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }
}
