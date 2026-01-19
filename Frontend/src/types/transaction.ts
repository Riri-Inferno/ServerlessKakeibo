/**
 * 取引種別
 */
export const TransactionType = {
  Expense: "Expense",
  Income: "Income",
} as const;

export type TransactionType =
  (typeof TransactionType)[keyof typeof TransactionType];

/**
 * 取引カテゴリ
 */
export const TransactionCategory = {
  Uncategorized: "Uncategorized",
  Food: "Food",
  DiningOut: "DiningOut",
  DailyNecessities: "DailyNecessities",
  Transportation: "Transportation",
  Education: "Education",
  Medical: "Medical",
  Entertainment: "Entertainment",
  Fashion: "Fashion",
  Utilities: "Utilities",
  Communication: "Communication",
  Other: "Other",
  Salary: "Salary",
  OtherIncome: "OtherIncome",
} as const;

export type TransactionCategory =
  (typeof TransactionCategory)[keyof typeof TransactionCategory];

/**
 * カテゴリ名の日本語マッピング
 */
export const CategoryLabels: Record<string, string> = {
  Uncategorized: "未分類",
  Food: "食費",
  DiningOut: "外食",
  DailyNecessities: "日用品",
  Transportation: "交通費",
  Education: "教育・教養",
  Medical: "医療・健康",
  Entertainment: "趣味・娯楽",
  Fashion: "衣服・美容",
  Utilities: "水道・光熱費",
  Communication: "通信費",
  Other: "その他",
  Salary: "給与",
  OtherIncome: "その他収入",
};

/**
 * 取引一覧のサマリー
 */
export interface TransactionSummary {
  id: string;
  type: TransactionType;
  transactionDate: string;
  amountTotal: number;
  currency: string;
  payee: string;
  category: TransactionCategory;
  paymentMethod: string | null;
  itemCount: number;
}

/**
 * ページング結果
 */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

/**
 * 取引一覧取得リクエスト
 */
export interface GetTransactionsRequest {
  page?: number;
  pageSize?: number;
  startDate?: string;
  endDate?: string;
  category?: TransactionCategory;
  payee?: string;
  minAmount?: number;
  maxAmount?: number;
  type?: TransactionType;
}

/**
 * 取引詳細の明細アイテム
 */
export interface TransactionItem {
  id: string;
  name: string;
  quantity: number;
  unitPrice: number;
  amount: number;
  category: TransactionCategory;
}

/**
 * 取引詳細の税情報
 */
export interface TransactionTax {
  id: string;
  taxRate: number | null;
  taxAmount: number;
  taxableAmount: number;
  taxType: string;
  isFixedAmount: boolean;
  applicableCategory: string | null;
}

/**
 * 取引詳細
 */
export interface TransactionDetail {
  id: string;
  type: TransactionType;
  transactionDate: string;
  amountTotal: number;
  currency: string;
  payer: string | null;
  payee: string;
  paymentMethod: string | null;
  category: TransactionCategory;
  receiptType: string | null;
  confidence: number | null;
  parseStatus: string | null;
  warnings: string[];
  missingFields: string[];
  items: TransactionItem[];
  taxes: TransactionTax[];
  shopDetails: any | null;
  createdAt: string;
  updatedAt: string;
}
