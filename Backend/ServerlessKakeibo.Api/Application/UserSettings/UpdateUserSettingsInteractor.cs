using ServerlessKakeibo.Api.Application.UserSettings.Dto;
using ServerlessKakeibo.Api.Application.UserSettings.Mappers;
using ServerlessKakeibo.Api.Application.UserSettings.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;

namespace ServerlessKakeibo.Api.Application.UserSettings;

/// <summary>
/// ユーザー設定更新インタラクター
/// </summary>
public class UpdateUserSettingsInteractor : IUpdateUserSettingsUseCase
{
    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<Infrastructure.Data.Entities.UserEntity> _userRepository;
    private readonly IUserSettingsRepository _settingsRepository;
    private readonly ILogger<UpdateUserSettingsInteractor> _logger;

    public UpdateUserSettingsInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<Infrastructure.Data.Entities.UserEntity> userRepository,
        IUserSettingsRepository settingsRepository,
        ILogger<UpdateUserSettingsInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// ユーザー設定を更新
    /// </summary>
    public async Task<UpdateUserSettingsResult> ExecuteAsync(
        Guid userId,
        UpdateUserSettingsRequest request,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("ユーザーIDが無効です", nameof(userId));

        if (request == null)
            throw new ArgumentNullException(nameof(request));

        try
        {
            _logger.LogInformation("ユーザー設定更新を開始します。UserId: {UserId}", userId);

            return await _transactionHelper.ExecuteInTransactionAsync(async () =>
            {
                // ユーザー情報を取得
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user == null)
                {
                    _logger.LogError("ユーザーが見つかりません。UserId: {UserId}", userId);
                    return new UpdateUserSettingsResult
                    {
                        Success = false,
                        ErrorMessage = "ユーザー情報が見つかりません"
                    };
                }

                // 設定を取得または作成
                var settings = await _settingsRepository.GetOrCreateAsync(
                    userId,
                    user.TenantId,
                    cancellationToken);

                // バリデーション
                if (request.ClosingDay.HasValue &&
                    (request.ClosingDay.Value < 1 || request.ClosingDay.Value > 31))
                {
                    _logger.LogWarning(
                        "締め日が不正です。UserId: {UserId}, ClosingDay: {ClosingDay}",
                        userId, request.ClosingDay);

                    return new UpdateUserSettingsResult
                    {
                        Success = false,
                        ErrorMessage = "締め日は1〜31の範囲で指定してください"
                    };
                }

                // 設定を更新
                if (request.DisplayNameOverride != null)
                {
                    // 空文字列の場合はnullとして扱う(Google情報に戻す)
                    settings.DisplayNameOverride = string.IsNullOrWhiteSpace(request.DisplayNameOverride)
                        ? null
                        : request.DisplayNameOverride;
                }

                // nullの場合月末締めになる
                settings.ClosingDay = request.ClosingDay;

                if (!string.IsNullOrWhiteSpace(request.TimeZone))
                {
                    settings.TimeZone = request.TimeZone;
                }

                if (!string.IsNullOrWhiteSpace(request.CurrencyCode))
                {
                    settings.CurrencyCode = request.CurrencyCode;
                }

                await _settingsRepository.UpdateAsync(settings, cancellationToken);

                _logger.LogInformation(
                    "ユーザー設定を更新しました。UserId: {UserId}, ClosingDay: {ClosingDay}",
                    userId, settings.ClosingDay);

                // 更新後の設定を取得(User情報を含む)
                var updatedSettings = await _settingsRepository.GetByUserIdAsync(
                    userId,
                    cancellationToken);

                return new UpdateUserSettingsResult
                {
                    Success = true,
                    Settings = UserSettingsMapper.ToDto(user, updatedSettings)
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ユーザー設定更新中にエラーが発生しました。UserId: {UserId}", userId);
            throw;
        }
    }
}
