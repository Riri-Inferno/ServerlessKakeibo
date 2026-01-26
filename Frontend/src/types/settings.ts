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
