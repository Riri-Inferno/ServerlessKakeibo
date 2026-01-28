/**
 * 設定API用の型定義
 */

/**
 * ユーザー設定
 */
export interface UserSettings {
  /** 表示名(DisplayNameOverride考慮済み) */
  displayName: string;
  /** メールアドレス */
  email: string | null;
  /** プロフィール画像URL */
  pictureUrl: string | null;
  /** 締め日(1-31, nullは月末締め) */
  closingDay: number | null;
  /** タイムゾーン(IANA形式) */
  timeZone: string;
  /** 通貨コード(ISO 4217) */
  currencyCode: string;
  /** カスタマイズした表示名(nullならGoogle由来) */
  displayNameOverride: string | null;
}

/**
 * ユーザー設定更新リクエスト
 */
export interface UpdateUserSettingsRequest {
  /** 表示名の上書き(null=変更なし, ""=Google情報に戻す) */
  displayNameOverride?: string | null;
  /** 締め日(1-31, nullは月末締め) */
  closingDay?: number | null;
  /** タイムゾーン */
  timeZone?: string;
  /** 通貨コード */
  currencyCode?: string;
}

/**
 * 全取引データ削除結果
 */
export interface DeleteAllTransactionsResult {
  /** 削除された取引数 */
  deletedTransactionCount: number;
  /** 削除された取引明細数 */
  deletedTransactionItemCount: number;
  /** 削除された店舗詳細数 */
  deletedShopDetailCount: number;
  /** 削除された税詳細数 */
  deletedTaxDetailCount: number;
  /** 削除されたGCS画像数 */
  deletedImageCount: number;
  /** 削除に失敗したGCS画像数 */
  failedImageCount: number;
  /** 削除に失敗した画像のパスリスト */
  failedImagePaths: string[];
  /** 削除処理完了日時 */
  completedAt: string;
}
