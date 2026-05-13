using System.Security.Cryptography;
using ServerlessKakeibo.Api.Application.ApiKey.Dto;
using ServerlessKakeibo.Api.Application.ApiKey.Usecases;
using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Infrastructure.Data.Interfaces;
using ServerlessKakeibo.Api.Infrastructure.Repository.Interfaces;
using ServerlessKakeibo.Api.Service.Authentication.ApiKey;
using ServerlessKakeibo.Api.Service.Interface;

namespace ServerlessKakeibo.Api.Application.ApiKey;

/// <summary>
/// APIキー発行インタラクター
/// </summary>
public class CreateApiKeyInteractor : ICreateApiKeyUseCase
{
    /// <summary>
    /// 現フェーズで発行を許可するスコープ
    /// </summary>
    private static readonly HashSet<string> AllowedScopes = new(StringComparer.Ordinal)
    {
        "read",
    };

    private readonly ITransactionHelper _transactionHelper;
    private readonly IGenericReadRepository<UserEntity> _userReadRepository;
    private readonly IGenericWriteRepository<ApiKeyEntity> _apiKeyWriteRepository;
    private readonly IPasswordHashService _passwordHashService;
    private readonly ILogger<CreateApiKeyInteractor> _logger;

    public CreateApiKeyInteractor(
        ITransactionHelper transactionHelper,
        IGenericReadRepository<UserEntity> userReadRepository,
        IGenericWriteRepository<ApiKeyEntity> apiKeyWriteRepository,
        IPasswordHashService passwordHashService,
        ILogger<CreateApiKeyInteractor> logger)
    {
        _transactionHelper = transactionHelper ?? throw new ArgumentNullException(nameof(transactionHelper));
        _userReadRepository = userReadRepository ?? throw new ArgumentNullException(nameof(userReadRepository));
        _apiKeyWriteRepository = apiKeyWriteRepository ?? throw new ArgumentNullException(nameof(apiKeyWriteRepository));
        _passwordHashService = passwordHashService ?? throw new ArgumentNullException(nameof(passwordHashService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreateApiKeyResult> ExecuteAsync(
        Guid userId,
        CreateApiKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null) throw new ArgumentNullException(nameof(request));
        if (string.IsNullOrWhiteSpace(request.Name))
            throw new ArgumentException("Name は必須です", nameof(request));
        if (request.Name.Length > 100)
            throw new ArgumentException("Name は 100 文字以内で指定してください", nameof(request));

        // スコープの検証
        var scopes = (request.Scopes ?? new List<string>())
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Select(s => s.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (scopes.Count == 0)
            throw new ArgumentException("Scopes に少なくとも1つのスコープを指定してください", nameof(request));

        var invalid = scopes.Where(s => !AllowedScopes.Contains(s)).ToList();
        if (invalid.Count > 0)
            throw new ArgumentException(
                $"発行できないスコープが含まれています: {string.Join(", ", invalid)}",
                nameof(request));

        if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTimeOffset.UtcNow)
            throw new ArgumentException("ExpiresAt は未来の日時を指定してください", nameof(request));

        return await _transactionHelper.ExecuteInTransactionAsync(async () =>
        {
            var user = await _userReadRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null)
                throw new InvalidOperationException("ユーザー情報が見つかりません");

            // キー本体（32バイト乱数）を base64url で生成し、接頭辞 "slk_" を付与
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            var body = Base64Url(randomBytes);
            var plainKey = ApiKeyAuthenticationDefaults.KeyPrefix + body;

            var prefix = plainKey.Substring(0, ApiKeyAuthenticationDefaults.LookupPrefixLength);
            var hash = _passwordHashService.HashPassword(plainKey);

            var entity = new ApiKeyEntity
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Name = request.Name.Trim(),
                KeyPrefix = prefix,
                KeyHash = hash,
                Scopes = string.Join(' ', scopes),
                ExpiresAt = request.ExpiresAt,
                TenantId = user.TenantId,
                CreatedBy = user.Id,
                UpdatedBy = user.Id,
            };
            await _apiKeyWriteRepository.AddAsync(entity, cancellationToken);

            _logger.LogInformation(
                "APIキーを発行しました。UserId: {UserId}, ApiKeyId: {ApiKeyId}, Prefix: {Prefix}",
                user.Id, entity.Id, prefix);

            return new CreateApiKeyResult(
                entity.Id,
                entity.Name,
                plainKey,
                entity.KeyPrefix,
                scopes,
                entity.ExpiresAt,
                entity.CreatedAt
            );
        });
    }

    /// <summary>
    /// バイト列を base64url (パディングなし、+/ → -/_) でエンコード
    /// </summary>
    private static string Base64Url(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
