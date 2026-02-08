/**
 * 商品カテゴリDTO（支出用）
 */
export interface ItemCategoryDto {
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
 * 商品カテゴリ操作結果（単一）
 */
export interface ItemCategoryResult {
  category: ItemCategoryDto;
  message: string;
}

/**
 * 商品カテゴリ一覧結果
 */
export interface ItemCategoryListResult {
  categories: ItemCategoryDto[];
  totalCount: number;
}

/**
 * 商品カテゴリ作成リクエスト
 */
export interface CreateItemCategoryRequest {
  name: string;
  colorCode: string;
}

/**
 * 商品カテゴリ更新リクエスト
 */
export interface UpdateItemCategoryRequest {
  name: string;
  colorCode: string;
  displayOrder: number;
}
