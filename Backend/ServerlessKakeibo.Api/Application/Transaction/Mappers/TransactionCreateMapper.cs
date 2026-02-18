using ServerlessKakeibo.Api.Contracts;
using ServerlessKakeibo.Api.Domain.ValueObjects;
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
        entity.Type = request.Type;
        entity.TransactionDate = request.TransactionDate;
        entity.AmountTotal = request.AmountTotal; // クライアント指定を優先
        entity.Currency = request.Currency;
        entity.Payer = request.Payer;
        entity.Payee = request.Payee;
        entity.PaymentMethod = request.PaymentMethod;
        entity.UserTransactionCategoryId = request.UserTransactionCategoryId;  // 新規
        entity.TaxInclusionType = request.TaxInclusionType;
        entity.Notes = request.Notes;

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
    /// <param name="request">リクエスト</param>
    /// <param name="transactionId">取引ID</param>
    /// <param name="userId">ユーザーID</param>
    /// <param name="tenantId">テナントID</param>
    /// <param name="transactionType">取引種別（収入/支出）</param>
    public static TransactionItemEntity ToItemEntity(
        CreateTransactionItemRequest request,
        Guid transactionId,
        Guid userId,
        Guid tenantId,
        TransactionType transactionType)
    {
        var entity = new TransactionItemEntity
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
        };

        // 収入/支出による振り分け
        if (transactionType == TransactionType.Income)
        {
            entity.UserIncomeItemCategoryId = request.UserIncomeItemCategoryId;
            entity.UserItemCategoryId = null;
        }
        else // Expense
        {
            entity.UserItemCategoryId = request.UserItemCategoryId;
            entity.UserIncomeItemCategoryId = null;
        }

        return entity;
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

    /// <summary>
    /// TransactionEntity → Transaction (ドメインモデル) 変換
    /// </summary>
    public static Domain.Transaction.Models.Transaction ToDomainModel(TransactionEntity entity)
    {
        return new Domain.Transaction.Models.Transaction
        {
            Id = entity.Id,
            Type = entity.Type,
            TransactionDate = entity.TransactionDate,
            AmountTotal = entity.AmountTotal,
            Currency = entity.Currency,
            Payer = entity.Payer,
            Payee = entity.Payee,
            PaymentMethod = entity.PaymentMethod != null
                ? Domain.ValueObjects.PaymentMethod.FromString(entity.PaymentMethod)
                : null,
            TaxInclusionType = entity.TaxInclusionType ?? Domain.ValueObjects.TaxInclusionType.Unknown,
            Items = entity.Items.Select(i => new Domain.Transaction.Models.TransactionItem
            {
                Name = i.Name,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                Amount = i.Amount,
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
