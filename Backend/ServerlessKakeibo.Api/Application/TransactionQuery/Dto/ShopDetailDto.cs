namespace ServerlessKakeibo.Api.Application.TransactionQuery.Dto;

/// <summary>
/// 店舗詳細情報DTO
/// </summary>
public class ShopDetailDto
{
    /// <summary>
    /// 店舗詳細ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 店舗名
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 支店名
    /// </summary>
    public string? Branch { get; set; }

    /// <summary>
    /// 郵便番号
    /// </summary>
    public string? PostalCode { get; set; }

    /// <summary>
    /// 住所
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 電話番号
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// インボイス登録番号
    /// </summary>
    public string? InvoiceRegistrationNumber { get; set; }

    /// <summary>
    /// 事業者名（インボイス登録名）
    /// </summary>
    public string? RegisteredBusinessName { get; set; }
}
