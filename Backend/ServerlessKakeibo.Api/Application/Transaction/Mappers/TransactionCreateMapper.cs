using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;

namespace ServerlessKakeibo.Api.Application.Transaction.Mappers;

/// <summary>
/// 取引作成マッパー
/// </summary>
public static class TransactionCreateMapper
{
    /// <summary>
    /// CreateTransactionRequest → TransactionEntity 変換
    /// </summary>
    /// <remarks>
    /// 【金額計算ルール】
    /// 新規作成時はクライアント指定の AmountTotal を優先します。
    /// これは、ユーザーが手入力した金額やレシート解析結果をそのまま保存するためです。
    /// </remarks>
    public static TransactionEntity ToEntity(
        CreateTransactionRequest request,
        Guid userId,
        Guid tenantId)
    {
        var entity = new TransactionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        // 基本情報
        entity.TransactionDate = request.TransactionDate.ToUniversalTime();
        entity.AmountTotal = request.AmountTotal; // クライアント指定を優先
        entity.Currency = request.Currency;
        entity.Payer = request.Payer;
        entity.Payee = request.Payee;
        entity.PaymentMethod = request.PaymentMethod;
        entity.Category = request.Category;
        // entity.Notes = request.Notes;

        // 手動入力なので解析関連フィールドはnull
        entity.ReceiptType = null;
        entity.Confidence = null;
        entity.ParseStatus = null;
        entity.WarningsJson = null;
        entity.MissingFieldsJson = null;
        entity.RawDataJson = null;

        return entity;
    }

    /// <summary>
    /// CreateTransactionItemRequest → TransactionItemEntity 変換
    /// </summary>
    public static TransactionItemEntity ToItemEntity(
        CreateTransactionItemRequest request,
        Guid transactionId,
        Guid userId,
        Guid tenantId)
    {
        return new TransactionItemEntity
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            TenantId = tenantId,
            CreatedBy = userId,
            UpdatedBy = userId,
            Name = request.Name,
            Quantity = request.Quantity,
            UnitPrice = request.UnitPrice,
            Amount = request.Amount,
            Category = request.Category
        };
    }

    /// <summary>
    /// CreateTaxDetailRequest → TaxDetailEntity 変換
    /// </summary>
    public static TaxDetailEntity ToTaxEntity(
        CreateTaxDetailRequest request,
        Guid transactionId,
        Guid userId,
        Guid tenantId)
    {
        return new TaxDetailEntity
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            TenantId = tenantId,
            CreatedBy = userId,
            UpdatedBy = userId,
            TaxRate = request.TaxRate,
            TaxAmount = request.TaxAmount,
            TaxableAmount = request.TaxableAmount,
            TaxType = request.TaxType
        };
    }

    /// <summary>
    /// CreateShopDetailRequest → ShopDetailEntity 変換
    /// </summary>
    public static ShopDetailEntity? ToShopEntity(
        CreateShopDetailRequest? request,
        Guid transactionId,
        Guid userId,
        Guid tenantId)
    {
        if (request == null)
            return null;

        return new ShopDetailEntity
        {
            Id = Guid.NewGuid(),
            TransactionId = transactionId,
            TenantId = tenantId,
            CreatedBy = userId,
            UpdatedBy = userId,
            Name = request.Name,
            Branch = request.Branch,
            PostalCode = request.PostalCode,
            Address = request.Address,
            PhoneNumber = request.PhoneNumber
        };
    }
}
