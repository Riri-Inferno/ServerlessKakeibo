using ServerlessKakeibo.Api.Application.TransactionExport.Dto;
using ServerlessKakeibo.Api.Domain.ValueObjects;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Application.TransactionExport.Mappers;

/// <summary>
/// 取引エクスポートマッパー
/// </summary>
public static class TransactionExportMapper
{
    private const string DateTimeFormat = "yyyy/MM/dd HH:mm:ss";

    /// <summary>
    /// TransactionEntity → TransactionExportDto 変換
    /// </summary>
    public static TransactionExportDto ToExportDto(TransactionEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new TransactionExportDto
        {
            Id = entity.Id.ToString(),
            取引種別 = entity.Type.ToJapanese(),
            取引日時 = FormatDateTime(entity.TransactionDate),
            金額 = entity.AmountTotal,
            通貨 = entity.Currency,
            支払者 = entity.Payer ?? string.Empty,
            受取者 = entity.Payee ?? string.Empty,
            支払方法 = entity.PaymentMethod != null
                ? PaymentMethod.FromString(entity.PaymentMethod).Value
                : string.Empty,
            カテゴリ = entity.Category.ToJapanese(),
            税区分 = entity.TaxInclusionType?.ToJapanese()
                ?? TaxInclusionType.Unknown.ToJapanese(),
            メモ = entity.Notes ?? string.Empty,
            明細件数 = entity.Items?.Count ?? 0,
            添付画像 = ExtractImageFileName(entity.SourceUrl, entity.Id),
            画像添付日時 = FormatDateTime(entity.ReceiptAttachedAt),
            作成日時 = FormatDateTime(entity.CreatedAt),
            更新日時 = FormatDateTime(entity.UpdatedAt)
        };
    }

    /// <summary>
    /// DateTimeOffset? を日本時間の文字列に変換
    /// </summary>
    private static string FormatDateTime(DateTimeOffset? dateTime)
    {
        return dateTime.HasValue
            ? dateTime.Value.ToOffset(TimeSpan.FromHours(9)).ToString(DateTimeFormat)
            : string.Empty;
    }

    /// <summary>
    /// DateTimeOffset を日本時間の文字列に変換（非nullable版）
    /// </summary>
    private static string FormatDateTime(DateTimeOffset dateTime)
    {
        return dateTime.ToOffset(TimeSpan.FromHours(9)).ToString(DateTimeFormat);
    }

    /// <summary>
    /// GCSパスから画像ファイル名を抽出
    /// 例: receipts/.../99a80948-1baa-4009-8415-c0256cf6e31a.jpg
    ///  → 99a80948-1baa-4009-8415-c0256cf6e31a.jpg
    /// </summary>
    private static string ExtractImageFileName(string? sourceUrl, Guid transactionId)
    {
        if (string.IsNullOrWhiteSpace(sourceUrl))
            return string.Empty;

        try
        {
            // パスから拡張子を取得
            var extension = Path.GetExtension(sourceUrl);

            // TransactionId + 拡張子
            return $"{transactionId}{extension}";
        }
        catch
        {
            return string.Empty;
        }
    }
}
