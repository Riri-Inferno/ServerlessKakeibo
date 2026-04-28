import type {
  CreateTaxDetail,
  ShopDetails,
  TransactionItemType,
} from "./transaction";

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
 * OCR解析結果の取引項目
 */
export interface NormalizedTransactionItem {
  itemType: TransactionItemType;
  name: string | null;
  quantity: number;
  unitPrice: number | null;
  amount: number;
  categoryCode?: string | null;
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
