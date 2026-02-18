/**
 * 統計API用の型定義
 */

/**
 * カテゴリサマリー
 */
export interface CategorySummary {
  categoryId: string;
  categoryName: string;
  colorCode: string;
  amount: number;
  count: number;
}

/**
 * 月次サマリー結果
 */
export interface MonthlySummaryResult {
  year: number;
  month: number;
  income: number;
  expense: number;
  balance: number;
  transactionCount: number;
  incomeCount: number;
  expenseCount: number;
  topExpenseCategories: CategorySummary[];
}

/**
 * 前月比込み月次サマリー結果
 */
export interface MonthlyComparisonResult {
  current: MonthlySummaryResult;
  previous: MonthlySummaryResult | null;
  incomeChangePercent: number | null;
  expenseChangePercent: number | null;
  balanceChangePercent: number | null;
}

/**
 * カテゴリ別支出内訳結果
 */
export interface CategoryBreakdownResult {
  year: number;
  month: number;
  totalExpense: number;
  categories: CategorySummary[];
}

/**
 * 月ラベル
 */
export interface MonthLabel {
  year: number;
  month: number;
  label: string;
}

/**
 * 月次推移結果
 */
export interface MonthlyTrendResult {
  months: MonthLabel[];
  incomes: number[];
  expenses: number[];
  balances: number[];
}

/**
 * 取引ハイライト
 */
export interface TransactionHighlight {
  id: string;
  payee: string;
  amount: number;
  transactionDate: string;
  categoryId: string;
  categoryName: string;
  colorCode: string;
}

/**
 * カテゴリ頻度
 */
export interface CategoryFrequency {
  categoryId: string;
  categoryName: string;
  colorCode: string;
  count: number;
  totalAmount: number;
}

/**
 * 月次ハイライト結果
 */
export interface HighlightsResult {
  maxExpenseTransaction: TransactionHighlight | null;
  mostFrequentCategory: CategoryFrequency | null;
  averageExpensePerDay: number;
  daysWithExpense: number;
}

/**
 * 月次サマリー取得リクエスト
 */
export interface GetMonthlySummaryRequest {
  year: number;
  month: number;
}

/**
 * 月次推移取得リクエスト
 */
export interface GetMonthlyTrendRequest {
  months?: number;
}
