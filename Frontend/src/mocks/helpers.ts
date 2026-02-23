import type {
  TransactionDetail,
  TransactionSummary,
} from "../types/transaction";
import { getMockTransactions } from "./data/transactions";
import type {
  MonthlySummaryResult,
  MonthlyComparisonResult,
  CategoryBreakdownResult,
  MonthlyTrendResult,
  HighlightsResult,
  TransactionHighlight,
  CategoryFrequency,
  MonthLabel,
} from "../types/statistics";
import { mockDemoUser } from "./data/demoUser";
import type { UserSettings } from "../types/settings";
import { mockTransactionCategories } from "./data/transactionCategories";
import type {
  TransactionCategoryDto,
  TransactionCategoryListResult,
} from "../types/transactionCategory";
import { mockItemCategories } from "./data/itemCategories";
import type {
  ItemCategoryDto,
  ItemCategoryListResult,
} from "../types/itemCategory";
import { mockIncomeItemCategories } from "./data/incomeItemCategories";
import type {
  IncomeItemCategoryDto,
  IncomeItemCategoryListResult,
} from "../types/incomeItemCategory";

/**
 * TransactionDetail から TransactionSummary に変換
 */
export function toTransactionSummary(
  detail: TransactionDetail
): TransactionSummary {
  return {
    id: detail.id,
    type: detail.type,
    transactionDate: detail.transactionDate,
    amountTotal: detail.amountTotal,
    currency: detail.currency,
    payer: detail.payer,
    payee: detail.payee,
    userTransactionCategory: detail.userTransactionCategory,
    paymentMethod: detail.paymentMethod,
    taxInclusionType: detail.taxInclusionType,
    itemCount: detail.items.length,
  };
}

/**
 * 複数の Detail を Summary に一括変換
 */
export function toTransactionSummaries(
  details: TransactionDetail[]
): TransactionSummary[] {
  return details.map(toTransactionSummary);
}

/**
 * 取引日から年月を抽出
 */
function parseYearMonth(dateString: string): { year: number; month: number } {
  const date = new Date(dateString);
  return {
    year: date.getFullYear(),
    month: date.getMonth() + 1,
  };
}

/**
 * 指定年月の取引をフィルタ
 */
function filterByYearMonth(year: number, month: number) {
  const transactions = getMockTransactions();
  return transactions.filter((t) => {
    const { year: y, month: m } = parseYearMonth(t.transactionDate);
    return y === year && m === month;
  });
}

/**
 * 月次サマリーを生成
 */
export function generateMonthlySummary(
  year: number,
  month: number
): MonthlySummaryResult {
  const transactions = filterByYearMonth(year, month);

  let income = 0;
  let expense = 0;
  let incomeCount = 0;
  let expenseCount = 0;

  // カテゴリ別集計用Map
  const categoryMap = new Map<
    string,
    {
      categoryId: string;
      categoryName: string;
      colorCode: string;
      amount: number;
      count: number;
    }
  >();

  transactions.forEach((t) => {
    if (t.type === "Income") {
      income += t.amountTotal;
      incomeCount++;
    } else {
      expense += t.amountTotal;
      expenseCount++;

      // カテゴリ別集計（支出のみ）
      if (t.userTransactionCategory) {
        const catId = t.userTransactionCategory.id;
        const existing = categoryMap.get(catId);
        if (existing) {
          existing.amount += t.amountTotal;
          existing.count++;
        } else {
          categoryMap.set(catId, {
            categoryId: catId,
            categoryName: t.userTransactionCategory.name,
            colorCode: t.userTransactionCategory.colorCode,
            amount: t.amountTotal,
            count: 1,
          });
        }
      }
    }
  });

  // 上位カテゴリを取得（金額降順）
  const topExpenseCategories = Array.from(categoryMap.values())
    .sort((a, b) => b.amount - a.amount)
    .slice(0, 3);

  return {
    year,
    month,
    income,
    expense,
    balance: income - expense,
    transactionCount: transactions.length,
    incomeCount,
    expenseCount,
    topExpenseCategories,
  };
}

/**
 * 前月比込みサマリーを生成
 */
export function generateMonthlyComparison(
  year: number,
  month: number
): MonthlyComparisonResult {
  const current = generateMonthlySummary(year, month);

  // 前月を計算
  let prevYear = year;
  let prevMonth = month - 1;
  if (prevMonth === 0) {
    prevMonth = 12;
    prevYear--;
  }

  const previous = generateMonthlySummary(prevYear, prevMonth);
  const hasPreviousData = previous.transactionCount > 0;

  // 前月比計算
  const incomeChangePercent =
    hasPreviousData && previous.income > 0
      ? ((current.income - previous.income) / previous.income) * 100
      : null;

  const expenseChangePercent =
    hasPreviousData && previous.expense > 0
      ? ((current.expense - previous.expense) / previous.expense) * 100
      : null;

  const balanceChangePercent =
    hasPreviousData && previous.balance !== 0
      ? ((current.balance - previous.balance) / Math.abs(previous.balance)) *
        100
      : null;

  return {
    current,
    previous: hasPreviousData ? previous : null,
    incomeChangePercent,
    expenseChangePercent,
    balanceChangePercent,
  };
}

/**
 * カテゴリ別内訳を生成
 */
export function generateCategoryBreakdown(
  year: number,
  month: number
): CategoryBreakdownResult {
  const transactions = filterByYearMonth(year, month);
  const expenses = transactions.filter((t) => t.type === "Expense");

  const categoryMap = new Map<
    string,
    {
      categoryId: string;
      categoryName: string;
      colorCode: string;
      amount: number;
      count: number;
    }
  >();

  let totalExpense = 0;

  expenses.forEach((t) => {
    totalExpense += t.amountTotal;

    if (t.userTransactionCategory) {
      const catId = t.userTransactionCategory.id;
      const existing = categoryMap.get(catId);
      if (existing) {
        existing.amount += t.amountTotal;
        existing.count++;
      } else {
        categoryMap.set(catId, {
          categoryId: catId,
          categoryName: t.userTransactionCategory.name,
          colorCode: t.userTransactionCategory.colorCode,
          amount: t.amountTotal,
          count: 1,
        });
      }
    }
  });

  // 全カテゴリを金額降順で返す
  const categories = Array.from(categoryMap.values()).sort(
    (a, b) => b.amount - a.amount
  );

  return {
    year,
    month,
    totalExpense,
    categories,
  };
}

/**
 * 月次推移を生成
 */
export function generateMonthlyTrend(months: number = 6): MonthlyTrendResult {
  const currentYear = 2026;
  const currentMonth = 2;

  const result: {
    months: MonthLabel[];
    incomes: number[];
    expenses: number[];
    balances: number[];
  } = {
    months: [],
    incomes: [],
    expenses: [],
    balances: [],
  };

  for (let i = months - 1; i >= 0; i--) {
    let targetYear = currentYear;
    let targetMonth = currentMonth - i;

    while (targetMonth <= 0) {
      targetMonth += 12;
      targetYear--;
    }

    const summary = generateMonthlySummary(targetYear, targetMonth);

    result.months.push({
      year: targetYear,
      month: targetMonth,
      label: `${targetYear}/${targetMonth}`,
    });

    result.incomes.push(summary.income);
    result.expenses.push(summary.expense);
    result.balances.push(summary.balance);
  }

  return result;
}

/**
 * ハイライトを生成
 */
export function generateHighlights(
  year: number,
  month: number
): HighlightsResult {
  const transactions = filterByYearMonth(year, month);
  const expenses = transactions.filter((t) => t.type === "Expense");

  // 最大支出取引
  let maxExpense = expenses.length > 0 ? expenses[0] : null;
  expenses.forEach((t) => {
    if (maxExpense && t.amountTotal > maxExpense.amountTotal) {
      maxExpense = t;
    }
  });

  const maxExpenseTransaction: TransactionHighlight | null = maxExpense
    ? {
        id: maxExpense.id,
        payee: maxExpense.payee || "",
        amount: maxExpense.amountTotal,
        transactionDate: maxExpense.transactionDate,
        categoryId: maxExpense.userTransactionCategory?.id || "",
        categoryName: maxExpense.userTransactionCategory?.name || "未分類",
        colorCode: maxExpense.userTransactionCategory?.colorCode || "#999999",
      }
    : null;

  // 最頻出カテゴリ
  const categoryFreqMap = new Map<
    string,
    {
      categoryId: string;
      categoryName: string;
      colorCode: string;
      count: number;
      totalAmount: number;
    }
  >();

  expenses.forEach((t) => {
    if (t.userTransactionCategory) {
      const catId = t.userTransactionCategory.id;
      const existing = categoryFreqMap.get(catId);
      if (existing) {
        existing.count++;
        existing.totalAmount += t.amountTotal;
      } else {
        categoryFreqMap.set(catId, {
          categoryId: catId,
          categoryName: t.userTransactionCategory.name,
          colorCode: t.userTransactionCategory.colorCode,
          count: 1,
          totalAmount: t.amountTotal,
        });
      }
    }
  });

  let mostFrequentCategory: CategoryFrequency | null = null;
  categoryFreqMap.forEach((cat) => {
    if (!mostFrequentCategory || cat.count > mostFrequentCategory.count) {
      mostFrequentCategory = cat;
    }
  });

  // 日別平均支出
  const totalExpense = expenses.reduce((sum, t) => sum + t.amountTotal, 0);
  const uniqueDays = new Set(
    expenses.map((t) => t.transactionDate.split("T")[0])
  );
  const daysWithExpense = uniqueDays.size;
  const averageExpensePerDay =
    daysWithExpense > 0 ? totalExpense / daysWithExpense : 0;

  return {
    maxExpenseTransaction,
    mostFrequentCategory,
    averageExpensePerDay,
    daysWithExpense,
  };
}

/**
 * デモ用のユーザー設定を生成
 */
export function generateMockSettings(): UserSettings {
  return {
    displayName: mockDemoUser.displayName,
    email: "demo@example.com",
    pictureUrl: mockDemoUser.pictureUrl,
    closingDay: null, // 月末締め
    timeZone: "Asia/Tokyo",
    currencyCode: "JPY",
    displayNameOverride: null, // 上書きなし（デフォルト表示名を使用）
  };
}

/**
 * UserTransactionCategory から TransactionCategoryDto に変換
 */
function toTransactionCategoryDto(
  category: (typeof mockTransactionCategories)[0],
  index: number
): TransactionCategoryDto {
  return {
    id: category.id,
    name: category.name,
    code: category.name,
    colorCode: category.colorCode,
    displayOrder: index + 1,
    isIncome: category.isIncome,
    isCustom: category.isCustom,
    isHidden: category.isHidden,
    masterCategoryId: category.isCustom ? null : category.id,
  };
}

/**
 * 取引カテゴリ一覧を生成
 */
export function generateTransactionCategories(
  includeHidden = false
): TransactionCategoryListResult {
  let categories = mockTransactionCategories.map((cat, index) =>
    toTransactionCategoryDto(cat, index)
  );

  // includeHidden=false の場合、削除済みカテゴリを除外
  if (!includeHidden) {
    categories = categories.filter((cat) => !cat.isHidden);
  }

  return {
    categories,
    totalCount: categories.length,
  };
}

/**
 * モックデータを ItemCategoryDto に変換
 */
function toItemCategoryDto(
  category: (typeof mockItemCategories)[0],
  index: number
): ItemCategoryDto {
  return {
    id: category.id,
    name: category.name,
    code: category.name,
    colorCode: category.colorCode,
    displayOrder: index + 1,
    isCustom: category.isCustom,
    isHidden: category.isHidden,
    masterCategoryId: category.isCustom ? null : category.id,
  };
}

/**
 * 商品カテゴリ一覧を生成
 */
export function generateItemCategories(
  includeHidden = false
): ItemCategoryListResult {
  let categories = mockItemCategories.map((cat, index) =>
    toItemCategoryDto(cat, index)
  );

  // includeHidden=false の場合、削除済みカテゴリを除外
  if (!includeHidden) {
    categories = categories.filter((cat) => !cat.isHidden);
  }

  return {
    categories,
    totalCount: categories.length,
  };
}

/**
 * モックデータを IncomeItemCategoryDto に変換
 */
function toIncomeItemCategoryDto(
  category: (typeof mockIncomeItemCategories)[0],
  index: number
): IncomeItemCategoryDto {
  return {
    id: category.id,
    name: category.name,
    code: category.name,
    colorCode: category.colorCode,
    displayOrder: index + 1,
    isCustom: category.isCustom,
    isHidden: category.isHidden,
    masterCategoryId: category.isCustom ? null : category.id,
  };
}

/**
 * 収入項目カテゴリ一覧を生成
 */
export function generateIncomeItemCategories(
  includeHidden = false
): IncomeItemCategoryListResult {
  let categories = mockIncomeItemCategories.map((cat, index) =>
    toIncomeItemCategoryDto(cat, index)
  );

  // includeHidden=false の場合、削除済みカテゴリを除外
  if (!includeHidden) {
    categories = categories.filter((cat) => !cat.isHidden);
  }

  return {
    categories,
    totalCount: categories.length,
  };
}

/**
 * IDから商品カテゴリを取得
 */
export function findItemCategoryById(id: string) {
  const category = mockItemCategories.find((cat) => cat.id === id);
  if (!category) return null;

  // isHidden を除外して返す（UserItemCategory型に合わせる）
  return {
    id: category.id,
    name: category.name,
    colorCode: category.colorCode,
    isCustom: category.isCustom,
  };
}

/**
 * IDから収入項目カテゴリを取得
 */
export function findIncomeItemCategoryById(id: string) {
  const category = mockIncomeItemCategories.find((cat) => cat.id === id);
  if (!category) return null;

  // isHidden を除外して返す（UserIncomeItemCategory型に合わせる）
  return {
    id: category.id,
    name: category.name,
    colorCode: category.colorCode,
    isCustom: category.isCustom,
  };
}

/**
 * IDから取引カテゴリを取得
 */
export function findTransactionCategoryById(id: string) {
  const category = mockTransactionCategories.find((cat) => cat.id === id);
  if (!category) return null;

  return {
    id: category.id,
    name: category.name,
    colorCode: category.colorCode,
    isIncome: category.isIncome,
    isCustom: category.isCustom,
  };
}
