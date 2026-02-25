import type { TransactionDetail } from "../../types/transaction";

/**
 * モック取引データを生成
 */
export async function getMockTransactions(): Promise<TransactionDetail[]> {
  const response = await fetch("/mock-data/transactions.json");
  if (!response.ok) {
    throw new Error("Failed to load mock transactions");
  }
  return response.json();
}
