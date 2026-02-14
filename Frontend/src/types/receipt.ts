import type { CreateTaxDetail, ShopDetails } from "./transaction";

/**
 * 領収書タイプ
 */
export const ReceiptType = {
  Unknown: "Unknown",
  Receipt: "Receipt",
  Invoice: "Invoice",
  CreditCardSlip: "CreditCardSlip",
} as const;

export type ReceiptType = (typeof ReceiptType)[keyof typeof ReceiptType];

/**
 * 解析ステータス
 */
export const ParseStatus = {
  Complete: "Complete",
  Partial: "Partial",
  LowConfidence: "LowConfidence",
  Failed: "Failed",
} as const;

export type ParseStatus = (typeof ParseStatus)[keyof typeof ParseStatus];

/**
 * 金額整合性の検証結果
 */
export interface AmountValidationResult {
  itemsTotal: number | null;
  taxTotal: number | null;
  matchesAsExclusiveTax: boolean | null;
  matchesAsInclusiveTax: boolean | null;
}

/**
 * 支払方法
 */
export const PaymentMethod = {
  Unknown: "Unknown",
  Cash: "Cash",
  CreditCard: "CreditCard",
  DebitCard: "DebitCard",
  ElectronicMoney: "ElectronicMoney",
  QRCodePayment: "QRCodePayment",
  BankTransfer: "BankTransfer",
  Other: "Other",
} as const;

export type PaymentMethod = (typeof PaymentMethod)[keyof typeof PaymentMethod];

/**
 * 支払方法の日本語ラベル
 */
export const PaymentMethodLabels: Record<string, string> = {
  Unknown: "不明",
  Cash: "現金",
  CreditCard: "クレジットカード",
  DebitCard: "デビットカード",
  ElectronicMoney: "電子マネー",
  QRCodePayment: "QRコード決済",
  BankTransfer: "銀行振込",
  Other: "その他",
};

/**
 * OCR解析結果の正規化データ
 */
export interface NormalizedTransaction {
  transactionDate: string | null;
  amountTotal: number | null;
  currency: string;
  payer: string | null;
  payee: string | null;
  paymentMethod: PaymentMethod | null;
  taxes: CreateTaxDetail[];
  items: NormalizedTransactionItem[];
  shopDetails: ShopDetails | null;
  category: string | null;
  categoryCode?: string | null;
  amountValidation?: AmountValidationResult;
}

/**
 * OCR解析結果の取引項目（NormalizedItem → NormalizedTransactionItem に改名）
 */
export interface NormalizedTransactionItem {
  name: string | null;
  quantity: number;
  unitPrice: number | null;
  amount: number;
  category: string | null; // 既存（後方互換、Enum文字列）
  categoryCode?: string | null; // 新規追加（Code文字列）
}

/**
 * OCR解析結果
 */
export interface ReceiptParseResult {
  receiptType: ReceiptType;
  confidence: number;
  normalized: NormalizedTransaction;
  raw: any;
  parseStatus: ParseStatus;
  warnings: string[];
  missingFields: string[];
}

/**
 * 商品カテゴリの日本語ラベル
 * TODO: カスタムカテゴリ対応後は削除予定
 */
export const ItemCategoryLabels: Record<string, string> = {
  Uncategorized: "未分類",
  Food: "食品",
  Beverage: "飲料",
  Snack: "お菓子・スナック",
  FrozenFood: "冷凍食品",
  DairyProduct: "乳製品",
  Seasoning: "調味料",
  Toiletries: "トイレタリー",
  KitchenSupplies: "キッチン用品",
  CleaningSupplies: "掃除用品",
  LaundrySupplies: "洗濯用品",
  Stationery: "文房具",
  Miscellaneous: "雑貨",
  Medicine: "医薬品",
  Supplement: "サプリメント",
  Cosmetics: "化粧品",
  Clothing: "衣類",
  Shoes: "靴",
  Accessories: "アクセサリー",
  Electronics: "電子機器",
  Battery: "電池",
  PetSupplies: "ペット用品",
  BabyProducts: "ベビー用品",
  Packaging: "レジ袋・包装材",
  Tobacco: "タバコ",
  Books: "書籍・雑誌",
  Other: "その他",
};
