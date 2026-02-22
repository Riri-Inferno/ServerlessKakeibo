import type {
  TransactionDetail,
  TransactionSummary,
} from "../types/transaction";

/**
 * TransactionDetail から TransactionSummary に変換
 */
export function toTransactionSummary(
  detail: TransactionDetail
): TransactionSummary {
  return {
    id: detail.id,
    type: detail.type,
    transactionDate: detail.transactionDate,
    amountTotal: detail.amountTotal,
    currency: detail.currency,
    payer: detail.payer,
    payee: detail.payee,
    userTransactionCategory: detail.userTransactionCategory,
    paymentMethod: detail.paymentMethod,
    taxInclusionType: detail.taxInclusionType,
    itemCount: detail.items.length,
  };
}

/**
 * 複数の Detail を Summary に一括変換
 */
export function toTransactionSummaries(
  details: TransactionDetail[]
): TransactionSummary[] {
  return details.map(toTransactionSummary);
}
