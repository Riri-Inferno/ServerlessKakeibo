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
   * 年オプション（最古の年 ～ 今年）
   */
  const yearOptions = computed(() => {
    const currentClosingMonth = getCurrentClosingMonth();
    const currentYear = currentClosingMonth.year;
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
   * 未来の締め日月は含めない
   */
  const monthOptions = computed(() => {
    const months: Array<{ value: number; label: string }> = [];

    for (let i = 1; i <= 12; i++) {
      if (isSelectableMonth(currentYear.value, i)) {
        months.push({
          value: i,
          label: `${i}月`,
        });
      }
    }

    return months;
  });

  /**
   * 指定された年月が選択可能かどうか
   * （現在の締め日月以前なら選択可能）
   */
  const isSelectableMonth = (year: number, month: number): boolean => {
    const current = getCurrentClosingMonth();

    // 年が過去なら選択可能
    if (year < current.year) return true;

    // 年が未来なら選択不可
    if (year > current.year) return false;

    // 同じ年の場合は月で判定
    return month <= current.month;
  };

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
      return `${year}年${month}月1日〜${lastDay}日の収支`;
    }

    // 締め日が設定されている場合

    // 当月の実際の日数と締め日
    const daysInCurrentMonth = new Date(year, month, 0).getDate();
    const endDay = Math.min(closingDay, daysInCurrentMonth);

    // 前月の年月を計算
    let prevYear = year;
    let prevMonth = month - 1;
    if (prevMonth === 0) {
      prevMonth = 12;
      prevYear -= 1;
    }

    // 前月の実際の日数と締め日
    const daysInPrevMonth = new Date(prevYear, prevMonth, 0).getDate();
    const prevClosingDay = Math.min(closingDay, daysInPrevMonth);

    // 開始日 = 前月締め日 + 1
    let startYear = prevYear;
    let startMonth = prevMonth;
    let startDay = prevClosingDay + 1;

    // 日付が月をまたぐ場合の処理
    if (startDay > daysInPrevMonth) {
      startDay = 1;
      startMonth += 1;
      if (startMonth > 12) {
        startMonth = 1;
        startYear += 1;
      }
    }

    // フォーマット
    const formatYMD = (y: number, m: number, d: number) => {
      if (y === year && m === month) {
        return `${m}月${d}日`;
      }
      if (y === year) {
        return `${m}月${d}日`;
      }
      return `${y}年${m}月${d}日`;
    };

    return `${formatYMD(startYear, startMonth, startDay)}〜${formatYMD(year, month, endDay)}の収支`;
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

  /**
   * 現在日が属する締め日月を取得
   */
  const getCurrentClosingMonth = () => {
    const now = new Date();
    const closingDay = authStore.settings?.closingDay;

    // 月末締めの場合は通常のカレンダー月
    if (closingDay === null || closingDay === undefined) {
      return {
        year: now.getFullYear(),
        month: now.getMonth() + 1,
      };
    }

    // N日締めの場合
    const currentDay = now.getDate();
    let year = now.getFullYear();
    let month = now.getMonth() + 1; // 1-12

    // 今日が締め日より後なら、次の締め日月に属する
    // 例: 1/28で締め日25日 → 2月期間（1/26-2/25）に属する
    if (currentDay > closingDay) {
      month += 1;
      if (month > 12) {
        month = 1;
        year += 1;
      }
    }

    return { year, month };
  };

  /**
   * 表示中の月が現在の締め日月かどうかを判定
   */
  const isCurrentMonth = computed(() => {
    const current = getCurrentClosingMonth();
    return (
      currentYear.value === current.year && currentMonth.value === current.month
    );
  });

  /**
   * 表示中の月が未来の締め日月かどうかを判定
   */
  const isFutureMonth = computed(() => {
    const current = getCurrentClosingMonth();

    // 年が未来
    if (currentYear.value > current.year) return true;

    // 同じ年で月が未来
    if (
      currentYear.value === current.year &&
      currentMonth.value > current.month
    )
      return true;

    return false;
  });

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
