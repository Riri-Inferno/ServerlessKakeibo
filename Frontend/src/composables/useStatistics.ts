import { ref, computed } from "vue";
import { statisticsRepository } from "../repositories/statisticsRepository";
import type {
  MonthlySummaryResult,
  MonthlyComparisonResult,
  CategoryBreakdownResult,
  MonthlyTrendResult,
  HighlightsResult,
} from "../types/statistics";

export function useStatistics() {
  const monthlySummary = ref<MonthlySummaryResult | null>(null);
  const monthlyComparison = ref<MonthlyComparisonResult | null>(null);
  const categoryBreakdown = ref<CategoryBreakdownResult | null>(null);
  const monthlyTrend = ref<MonthlyTrendResult | null>(null);
  const highlights = ref<HighlightsResult | null>(null);

  const isLoading = ref(false);
  const errorMessage = ref("");

  // 現在表示中の年月
  const currentYear = ref<number>(new Date().getFullYear());
  const currentMonth = ref<number>(new Date().getMonth() + 1);

  // 表示用ラベル
  const currentMonthLabel = computed(
    () => `${currentYear.value}年${currentMonth.value}月`,
  );

  /**
   * 月次サマリーを取得
   */
  const fetchMonthlySummary = async (year: number, month: number) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      monthlySummary.value = await statisticsRepository.getMonthlySummary(
        year,
        month,
      );
    } catch (error) {
      console.error("月次サマリー取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "月次サマリーの取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 前月比込みサマリーを取得
   */
  const fetchMonthlyComparison = async (year: number, month: number) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      monthlyComparison.value = await statisticsRepository.getMonthlyComparison(
        year,
        month,
      );
    } catch (error) {
      console.error("前月比サマリー取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "前月比サマリーの取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * カテゴリ別内訳を取得
   */
  const fetchCategoryBreakdown = async (year: number, month: number) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      categoryBreakdown.value = await statisticsRepository.getCategoryBreakdown(
        year,
        month,
      );
    } catch (error) {
      console.error("カテゴリ別内訳取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "カテゴリ別内訳の取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * 月次推移を取得
   */
  const fetchMonthlyTrend = async (months: number = 6) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      monthlyTrend.value = await statisticsRepository.getMonthlyTrend(months);
    } catch (error) {
      console.error("月次推移取得エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "月次推移の取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * ハイライトを取得
   */
  const fetchHighlights = async (year: number, month: number) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      highlights.value = await statisticsRepository.getHighlights(year, month);
    } catch (error) {
      console.error("ハイライト取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "ハイライトの取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  // 現在月のすべてのデータを取得
  const fetchCurrentMonth = async () => {
    await Promise.all([
      fetchMonthlyComparison(currentYear.value, currentMonth.value),
      fetchCategoryBreakdown(currentYear.value, currentMonth.value),
      fetchMonthlyTrend(6),
      fetchHighlights(currentYear.value, currentMonth.value),
    ]);
  };

  // 前月に移動
  const goToPreviousMonth = async () => {
    if (currentMonth.value === 1) {
      currentMonth.value = 12;
      currentYear.value--;
    } else {
      currentMonth.value--;
    }
    await fetchCurrentMonth();
  };

  // 次月に移動
  const goToNextMonth = async () => {
    if (currentMonth.value === 12) {
      currentMonth.value = 1;
      currentYear.value++;
    } else {
      currentMonth.value++;
    }
    await fetchCurrentMonth();
  };

  // 今月に戻る
  const goToCurrentMonth = async () => {
    const now = new Date();
    currentYear.value = now.getFullYear();
    currentMonth.value = now.getMonth() + 1;
    await fetchCurrentMonth();
  };

  // 特定の年月に移動
  const goToMonth = async (year: number, month: number) => {
    currentYear.value = year;
    currentMonth.value = month;
    await fetchCurrentMonth();
  };

  /**
   * 現在の年月を取得
   */
  const getCurrentYearMonth = () => {
    const now = new Date();
    return {
      year: now.getFullYear(),
      month: now.getMonth() + 1,
    };
  };

  return {
    // State
    monthlySummary,
    monthlyComparison,
    categoryBreakdown,
    monthlyTrend,
    highlights,
    isLoading,
    errorMessage,
    currentYear,
    currentMonth,
    currentMonthLabel,

    // Methods
    fetchMonthlySummary,
    fetchMonthlyComparison,
    fetchCategoryBreakdown,
    fetchMonthlyTrend,
    fetchHighlights,
    fetchCurrentMonth,
    goToPreviousMonth,
    goToNextMonth,
    goToCurrentMonth,
    goToMonth,
    getCurrentYearMonth,
  };
}
