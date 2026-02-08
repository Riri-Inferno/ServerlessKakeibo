/**
 * 給与項目カテゴリDTO（収入用）
 */
export interface IncomeItemCategoryDto {
  id: string;
  name: string;
  code: string;
  colorCode: string;
  displayOrder: number;
  isCustom: boolean;
  isHidden: boolean;
  masterCategoryId: string | null;
}

/**
 * 給与項目カテゴリ操作結果（単一）
 */
export interface IncomeItemCategoryResult {
  category: IncomeItemCategoryDto;
  message: string;
}

/**
 * 給与項目カテゴリ一覧結果
 */
export interface IncomeItemCategoryListResult {
  categories: IncomeItemCategoryDto[];
  totalCount: number;
}

/**
 * 給与項目カテゴリ作成リクエスト
 */
export interface CreateIncomeItemCategoryRequest {
  name: string;
  colorCode: string;
}

/**
 * 給与項目カテゴリ更新リクエスト
 */
export interface UpdateIncomeItemCategoryRequest {
  name: string;
  colorCode: string;
  displayOrder: number;
}
