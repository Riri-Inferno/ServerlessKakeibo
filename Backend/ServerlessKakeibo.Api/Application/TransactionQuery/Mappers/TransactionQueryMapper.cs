using ServerlessKakeibo.Api.Application.TransactionQuery.Dto;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Common.Helpers;

namespace ServerlessKakeibo.Api.Application.TransactionQuery.Mappers;

/// <summary>
/// 取引クエリマッパー
/// </summary>
public static class TransactionQueryMapper
{
    /// <summary>
    /// TransactionEntity → TransactionDetailResult 変換
    /// </summary>
    public static TransactionDetailResult ToDetailResult(TransactionEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new TransactionDetailResult
        {
            Id = entity.Id,
            Type = entity.Type,
            TransactionDate = entity.TransactionDate,
            AmountTotal = entity.AmountTotal,
            Currency = entity.Currency,
            Payer = entity.Payer,
            Payee = entity.Payee,
            PaymentMethod = entity.PaymentMethod,
            Category = entity.Category,
            Notes = entity.Notes,
            TaxInclusionType = entity.TaxInclusionType,
            ReceiptType = entity.ReceiptType,
            Confidence = entity.Confidence,
            ParseStatus = entity.ParseStatus,
            Warnings = JsonHelper.DeserializeStringArray(entity.WarningsJson),
            MissingFields = JsonHelper.DeserializeStringArray(entity.MissingFieldsJson),
            Items = entity.Items
                .Select(ToItemDto)
                .ToList(),
            Taxes = entity.Taxes
                .Select(ToTaxDto)
                .ToList(),
            ShopDetails = entity.ShopDetail != null
                ? ToShopDto(entity.ShopDetail)
                : null,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    /// <summary>
    /// TransactionItemEntity → TransactionItemDto 変換
    /// </summary>
    public static TransactionItemDto ToItemDto(TransactionItemEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new TransactionItemDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Quantity = entity.Quantity,
            UnitPrice = entity.UnitPrice,
            Amount = entity.Amount,
            Category = entity.Category
        };
    }

    /// <summary>
    /// TaxDetailEntity → TaxDetailDto 変換
    /// </summary>
    public static TaxDetailDto ToTaxDto(TaxDetailEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new TaxDetailDto
        {
            Id = entity.Id,
            TaxRate = entity.TaxRate,
            TaxAmount = entity.TaxAmount,
            TaxableAmount = entity.TaxableAmount,
            TaxType = entity.TaxType,
            IsFixedAmount = entity.IsFixedAmount,
            ApplicableCategory = entity.ApplicableCategory
        };
    }

    /// <summary>
    /// ShopDetailEntity → ShopDetailDto 変換
    /// </summary>
    public static ShopDetailDto ToShopDto(ShopDetailEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new ShopDetailDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Branch = entity.Branch,
            PostalCode = entity.PostalCode,
            Address = entity.Address,
            PhoneNumber = entity.PhoneNumber,
            InvoiceRegistrationNumber = entity.InvoiceRegistrationNumber,
            RegisteredBusinessName = entity.RegisteredBusinessName
        };
    }

    /// <summary>
    /// TransactionEntity → TransactionSummaryResult 変換(一覧用)
    /// </summary>
    public static TransactionSummaryResult ToSummaryResult(TransactionEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        return new TransactionSummaryResult
        {
            Id = entity.Id,
            Type = entity.Type,
            TransactionDate = entity.TransactionDate,
            AmountTotal = entity.AmountTotal,
            Currency = entity.Currency,
            Payee = entity.Payee,
            Category = entity.Category,
            PaymentMethod = entity.PaymentMethod,
            TaxInclusionType = entity.TaxInclusionType,
            ItemCount = entity.Items?.Count ?? 0
        };
    }
}
