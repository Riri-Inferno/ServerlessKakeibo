import { ref, computed } from "vue";
import { statisticsRepository } from "../repositories/statisticsRepository";
import { useAuthStore } from "../stores/authStore";
import type {
  MonthlySummaryResult,
  MonthlyComparisonResult,
  CategoryBreakdownResult,
  MonthlyTrendResult,
  HighlightsResult,
} from "../types/statistics";

export function useStatistics() {
  const authStore = useAuthStore();
  const monthlySummary = ref<MonthlySummaryResult | null>(null);
  const monthlyComparison = ref<MonthlyComparisonResult | null>(null);
  const categoryBreakdown = ref<CategoryBreakdownResult | null>(null);
  const monthlyTrend = ref<MonthlyTrendResult | null>(null);
  const highlights = ref<HighlightsResult | null>(null);

  const isLoading = ref(false);
  const errorMessage = ref("");

  const oldestYear = ref<number | null>(null);

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

      // 最古の年を記録（動的な年リストを作るため）
      const oldestMonth = monthlyTrend.value?.months?.[0];
      if (oldestMonth) {
        oldestYear.value = oldestMonth.year;
      }
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

  /**
   * 表示中の月が現在の月かどうかを判定
   * 「今月」ボタンの無効化制御に使用
   */
  const isCurrentMonth = computed(() => {
    const now = new Date();
    const current = new Date(currentYear.value, currentMonth.value - 1);
    return (
      now.getFullYear() === current.getFullYear() &&
      now.getMonth() === current.getMonth()
    );
  });

  /**
   * 表示中の月が未来の月かどうかを判定
   * 「次月」ボタンの無効化制御に使用
   */
  const isFutureMonth = computed(() => {
    const now = new Date();
    const current = new Date(currentYear.value, currentMonth.value - 1);

    // 年が未来
    if (current.getFullYear() > now.getFullYear()) return true;

    // 同じ年で月が未来
    if (
      current.getFullYear() === now.getFullYear() &&
      current.getMonth() > now.getMonth()
    )
      return true;

    return false;
  });

  /**
   * 年オプション（最古の年 ～ 今年）
   */
  const yearOptions = computed(() => {
    const now = new Date();
    const currentYear = now.getFullYear();
    const years: Array<{ value: number; label: string }> = [];

    // 最古の年が不明な場合は過去5年
    const startYear = oldestYear.value ?? currentYear - 5;

    // 最古の年から今年まで
    for (let year = currentYear; year >= startYear; year--) {
      years.push({
        value: year,
        label: `${year}年`,
      });
    }

    return years;
  });

  /**
   * 月オプション（1-12月）
   * 未来の月は無効化
   */
  const monthOptions = computed(() => {
    const now = new Date();
    const months: Array<{ value: number; label: string }> = [];

    for (let i = 1; i <= 12; i++) {
      const isFuture =
        currentYear.value > now.getFullYear() ||
        (currentYear.value === now.getFullYear() && i > now.getMonth() + 1);

      // 未来の月は含めない
      if (!isFuture) {
        months.push({
          value: i,
          label: `${i}月`,
        });
      }
    }

    return months;
  });

  /**
   * 締め日に応じた期間ラベルを取得
   */
  const currentPeriodLabel = computed(() => {
    const closingDay = authStore.settings?.closingDay;
    const year = currentYear.value;
    const month = currentMonth.value;

    // 締め日が設定されていない（月末締め）
    if (closingDay === null || closingDay === undefined) {
      const lastDay = new Date(year, month, 0).getDate();
      return `${year}年${month}月1日 ~ ${year}年${month}月${lastDay}日`;
    }

    // 締め日が設定されている場合
    // 例: 締め日25日、表示月2026年1月 → 2025年12月26日 ~ 2026年1月25日
    let startYear = year;
    let startMonth = month - 1;
    const startDay = closingDay + 1;

    // 前月の計算
    if (startMonth === 0) {
      startMonth = 12;
      startYear = year - 1;
    }

    const endYear = year;
    const endMonth = month;
    const endDay = closingDay;

    return `${startYear}年${startMonth}月${startDay}日 ~ ${endYear}年${endMonth}月${endDay}日`;
  });

  /**
   * 年が変更されたときの処理
   */
  const handleYearChange = async (year: number) => {
    await goToMonth(year, currentMonth.value);
  };

  /**
   * 月が変更されたときの処理
   */
  const handleMonthChange = async (month: number) => {
    await goToMonth(currentYear.value, month);
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
    currentPeriodLabel,

    // Computed
    isCurrentMonth,
    isFutureMonth,
    yearOptions,
    monthOptions,

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
    handleYearChange,
    handleMonthChange,
  };
}
