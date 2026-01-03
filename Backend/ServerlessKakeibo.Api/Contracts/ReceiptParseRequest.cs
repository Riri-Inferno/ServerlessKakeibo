using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto.Enum;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// 領収書等の書面解析リクエスト
/// </summary>
public class ReceiptParseRequest
{
    /// <summary>
    /// 解析対象ファイル（画像 / PDF）
    /// </summary>
    public IFormFile File { get; set; } = default!;

    /// <summary>
    /// 任意オプション（将来拡張用）
    /// </summary>
    public ReceiptParseOptions? Options { get; set; } = null;

    /// <summary>
    /// カスタムプロンプト
    /// 将来的にカスタム可能にする
    /// </summary>
    public string? CustomPrompt { get; set; } = null;
}

/// <summary>
/// 解析オプション
/// </summary>
public class ReceiptParseOptions
{
    /// <summary>
    /// 強制的に期待する書面種別（通常は null）
    /// </summary>
    public ReceiptType? ExpectedReceiptType { get; set; }

    /// <summary>
    /// 生データ(raw)をレスポンスに含めるか
    /// </summary>
    public bool IncludeRaw { get; set; } = true;
}
