/**
 * 取引カテゴリDTO
 */
export interface TransactionCategoryDto {
  id: string;
  name: string;
  code: string;
  colorCode: string;
  displayOrder: number;
  isIncome: boolean;
  isCustom: boolean;
  isHidden: boolean;
  masterCategoryId: string | null;
}

/**
 * 取引カテゴリ操作結果（単一）
 */
export interface TransactionCategoryResult {
  category: TransactionCategoryDto;
  message: string;
}

/**
 * 取引カテゴリ一覧結果
 */
export interface TransactionCategoryListResult {
  categories: TransactionCategoryDto[];
  totalCount: number;
}

/**
 * 取引カテゴリ作成リクエスト
 */
export interface CreateTransactionCategoryRequest {
  name: string;
  colorCode: string;
  isIncome: boolean;
}

/**
 * 取引カテゴリ更新リクエスト
 */
export interface UpdateTransactionCategoryRequest {
  name: string;
  colorCode: string;
  displayOrder: number;
}
