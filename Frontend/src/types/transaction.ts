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
 * TODO: カスタムカテゴリ対応後は削除予定（後方互換のため現在は残す）
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
 * TODO: カスタムカテゴリ対応後は削除予定（後方互換のため現在は残す）
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
 * 税の扱い（内税・外税）
 */
export const TaxInclusionType = {
  Unknown: "Unknown",
  Exclusive: "Exclusive", // 外税
  Inclusive: "Inclusive", // 内税
  NoTax: "NoTax", // 非課税
} as const;

export type TaxInclusionType =
  (typeof TaxInclusionType)[keyof typeof TaxInclusionType];

/**
 * 税区分の日本語ラベル
 */
export const TaxInclusionTypeLabels: Record<TaxInclusionType, string> = {
  Unknown: "不明",
  Exclusive: "外税",
  Inclusive: "内税",
  NoTax: "非課税",
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
  payer: string | null;
  payee: string | null;
  category: TransactionCategory; // 後方互換
  userTransactionCategory?: UserTransactionCategory | null;
  paymentMethod: string | null;
  taxInclusionType?: TaxInclusionType;
  itemCount: number;
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
  category: TransactionCategory; // 後方互換
  userItemCategoryId?: string | null;
  userIncomeItemCategoryId?: string | null;
  userItemCategory?: UserItemCategory | null;
  userIncomeItemCategory?: UserIncomeItemCategory | null;
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
  sourceUrl?: string | null;
  receiptAttachedAt?: string | null;
  amountTotal: number;
  currency: string;
  payer: string | null;
  payee: string;
  paymentMethod: string | null;
  category: TransactionCategory; // 後方互換
  userTransactionCategory?: UserTransactionCategory | null;
  notes?: string;
  taxInclusionType?: TaxInclusionType;
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

/**
 * レシート画像URL取得結果
 */
export interface ReceiptImageUrlResult {
  signedUrl: string;
  expiresAt: string;
}

/**
 * レシート添付リクエスト
 */
export interface AttachReceiptRequest {
  file: File;
}

/**
 * ページング結果
 */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  currentPage: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage?: boolean;
  hasNextPage?: boolean;
}

/**
 * 取引一覧取得リクエスト
 */
export interface GetTransactionsRequest {
  page?: number;
  pageSize?: number;
  startDate?: string;
  endDate?: string;
  category?: TransactionCategory; // 後方互換
  userTransactionCategoryId?: string | null;
  payer?: string;
  payee?: string;
  minAmount?: number;
  maxAmount?: number;
  type?: TransactionType;
}

/**
 * 取引作成用の項目
 */
export interface CreateTransactionItem {
  id?: string;
  name: string;
  quantity: number;
  unitPrice: number | null;
  amount: number;
  category: string; // 後方互換
  userItemCategoryId: string | null; // 支出用
  userIncomeItemCategoryId?: string | null;
}

/**
 * 取引作成用の税情報
 */
export interface CreateTaxDetail {
  taxRate: number | null;
  taxAmount: number | null;
  taxableAmount: number | null;
  taxType?: string;
}

/**
 * 店舗詳細
 */
export interface ShopDetails {
  name: string | null;
  branch: string | null;
  phoneNumber: string | null;
  address: string | null;
  postalCode: string | null;
}

/**
 * 取引作成リクエスト
 */
export interface CreateTransactionRequest {
  type: TransactionType;
  transactionDate: string;
  amountTotal: number;
  currency?: string;
  payer?: string;
  payee?: string;
  paymentMethod?: string;
  category: TransactionCategory; // 後方互換
  userTransactionCategoryId?: string | null; // 新規追加
  notes?: string;
  taxInclusionType?: TaxInclusionType;
  items?: CreateTransactionItem[];
  taxes?: CreateTaxDetail[];
  shopDetails?: ShopDetails;
}

/**
 * 取引作成/更新結果
 */
export interface TransactionResult {
  transactionId: string;
  transactionDate: string;
  sourceUrl?: string | null;
  receiptAttachedAt?: string | null;
  amountTotal: number;
  currency: string;
  payee: string | null;
  category: TransactionCategory;
  taxInclusionType?: TaxInclusionType;
  notes?: string | null;
  processedAt: string;
  validationWarnings: string[];
}

/**
 * 取引更新用の項目
 */
export interface UpdateTransactionItem {
  id?: string;
  name: string;
  quantity: number;
  unitPrice: number | null;
  amount: number;
  category: string; // 後方互換
  userItemCategoryId: string | null; // 支出用
  userIncomeItemCategoryId?: string | null;
}

/**
 * 取引更新用の税情報
 */
export interface UpdateTaxDetail {
  id?: string; // 既存の場合は指定
  taxRate: number | null;
  taxAmount: number | null;
  taxableAmount: number | null;
  taxType?: string;
}

/**
 * 取引更新用の店舗詳細
 */
export interface UpdateShopDetails {
  id?: string; // 既存の場合は指定
  name: string | null;
  branch: string | null;
  phoneNumber: string | null;
  address: string | null;
  postalCode: string | null;
}

/**
 * 取引更新リクエスト
 */
export interface UpdateTransactionRequest {
  transactionDate: string;
  currency?: string;
  payer?: string;
  payee?: string;
  paymentMethod?: string;
  category: TransactionCategory; // 後方互換
  userTransactionCategoryId?: string | null; // 新規追加
  notes?: string;
  taxInclusionType?: TaxInclusionType;
  items?: UpdateTransactionItem[];
  taxes?: UpdateTaxDetail[];
  shopDetails?: UpdateShopDetails;
}

/**
 * 取引エクスポートリクエスト
 * GetTransactionsRequest を継承
 */
export interface ExportTransactionsRequest extends GetTransactionsRequest {
  /**
   * 添付画像を含めるか（true = Zip with images, false = CSV only in Zip）
   */
  includeReceiptImages: boolean;
}

/**
 * エクスポート結果
 */
export interface TransactionExportResult {
  /**
   * ファイル名
   */
  fileName: string;

  /**
   * ZIPファイルバイナリ（Base64エンコード済み）
   */
  zipDataBase64: string;

  /**
   * エクスポートされた取引件数
   */
  totalCount: number;

  /**
   * 添付画像を含めた件数
   */
  imagesIncludedCount: number;

  /**
   * 画像取得に失敗した件数
   */
  imagesFailedCount: number;
}

/**
 * ユーザー取引カテゴリ（取引レベル）
 */
export interface UserTransactionCategory {
  id: string;
  name: string;
  colorCode: string;
  isIncome: boolean;
  isCustom: boolean;
}

/**
 * ユーザー商品カテゴリ（支出用・明細レベル）
 */
export interface UserItemCategory {
  id: string;
  name: string;
  colorCode: string;
  isCustom: boolean;
}

/**
 * ユーザー収入項目カテゴリ（収入用・明細レベル）
 */
export interface UserIncomeItemCategory {
  id: string;
  name: string;
  colorCode: string;
  isCustom: boolean;
}
