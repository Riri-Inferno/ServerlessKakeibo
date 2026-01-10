using ServerlessKakeibo.Api.Application.ReceiptParsing.Dto;
using ServerlessKakeibo.Api.Infrastructure.Data.Entities;
using ServerlessKakeibo.Api.Domain.ValueObjects;

namespace ServerlessKakeibo.Api.Application.registReceiptDetails.Mappers;

/// <summary>
/// 取引マッパー
/// </summary>
public static class TransactionMapper
{
    /// <summary>
    /// ReceiptParseResult → TransactionEntity 変換
    /// </summary>
    public static TransactionEntity ToEntity(
        ReceiptParseResult parseResult,
        Guid userId,
        Guid tenantId,
        TransactionCategory? category = null)
    {
        var transaction = new TransactionEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TenantId = tenantId,
            TransactionDate = parseResult.Normalized.TransactionDate?.ToUniversalTime(),
            AmountTotal = parseResult.Normalized.AmountTotal,
            Currency = parseResult.Normalized.Currency,
            Payer = parseResult.Normalized.Payer,
            Payee = parseResult.Normalized.Payee,
            PaymentMethod = parseResult.Normalized.PaymentMethod?.ToString(),
            ReceiptType = parseResult.ReceiptType.ToString(),
            Confidence = parseResult.Confidence,
            ParseStatus = parseResult.ParseStatus.ToString(),
            WarningsJson = parseResult.Warnings.Any()
                ? System.Text.Json.JsonSerializer.Serialize(parseResult.Warnings)
                : null,
            MissingFieldsJson = parseResult.MissingFields.Any()
                ? System.Text.Json.JsonSerializer.Serialize(parseResult.MissingFields)
                : null,
            RawDataJson = parseResult.Raw.HasValue
                ? parseResult.Raw.Value.GetRawText()
                : null,
            Category = category ?? TransactionCategory.Uncategorized,
            CreatedBy = userId,
            UpdatedBy = userId
        };

        // 取引項目の変換
        transaction.Items = parseResult.Normalized.Items
            .Select(item => new TransactionItemEntity
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Name = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                Amount = item.Amount,
                Category = item.Category ?? ItemCategory.Uncategorized,
                TenantId = tenantId,
                CreatedBy = userId
            })
            .ToList();

        // 税情報の変換
        transaction.Taxes = parseResult.Normalized.Taxes
            .Select(tax => new TaxDetailEntity
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                TaxRate = tax.TaxRate,
                TaxAmount = tax.TaxAmount,
                TaxableAmount = tax.TaxableAmount,
                TaxType = "消費税",
                IsFixedAmount = false,
                ApplicableCategory = null,
                TenantId = tenantId,
                CreatedBy = userId
            })
            .ToList();

        // 店舗情報の変換
        if (parseResult.Normalized.ShopDetails != null)
        {
            var shop = parseResult.Normalized.ShopDetails;
            transaction.ShopDetail = new ShopDetailEntity
            {
                Id = Guid.NewGuid(),
                TransactionId = transaction.Id,
                Name = shop.Name,
                Branch = shop.Branch,
                Address = shop.Address,
                PhoneNumber = shop.PhoneNumber,
                PostalCode = shop.PostalCode,
                InvoiceRegistrationNumber = shop.InvoiceRegistrationNumber,
                RegisteredBusinessName = shop.RegisteredBusinessName,
                TenantId = tenantId,
                CreatedBy = userId
            };
        }

        return transaction;
    }

    /// <summary>
    /// TransactionEntity → Transaction (ドメインモデル) 変換
    /// </summary>
    public static Domain.Transaction.Models.Transaction ToDomainModel(TransactionEntity entity)
    {
        return new Domain.Transaction.Models.Transaction
        {
            Id = entity.Id,
            TransactionDate = entity.TransactionDate,
            AmountTotal = entity.AmountTotal,
            Currency = entity.Currency,
            Payer = entity.Payer,
            Payee = entity.Payee,
            PaymentMethod = entity.PaymentMethod != null
                ? Domain.ValueObjects.PaymentMethod.FromString(entity.PaymentMethod)
                : null,
            Items = entity.Items.Select(i => new Domain.Transaction.Models.TransactionItem
            {
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Amount = i.Amount,
                Category = i.Category
            }).ToList(),
            Taxes = entity.Taxes.Select(t => new Domain.Receipt.Models.TaxDetail
            {
                TaxType = t.TaxType ?? "消費税",
                TaxRate = t.TaxRate,
                TaxAmount = t.TaxAmount,
                TaxableAmount = t.TaxableAmount,
                IsFixedAmount = t.IsFixedAmount,
                ApplicableCategory = t.ApplicableCategory
            }).ToList(),
            ShopDetails = entity.ShopDetail != null
                ? new Domain.Receipt.Models.ShopDetails
                {
                    Name = entity.ShopDetail.Name,
                    Branch = entity.ShopDetail.Branch,
                    PhoneNumber = entity.ShopDetail.PhoneNumber,
                    Address = entity.ShopDetail.Address,
                    PostalCode = entity.ShopDetail.PostalCode,
                    InvoiceRegistrationNumber = entity.ShopDetail.InvoiceRegistrationNumber,
                    RegisteredBusinessName = entity.ShopDetail.RegisteredBusinessName
                }
                : null
        };
    }
}
