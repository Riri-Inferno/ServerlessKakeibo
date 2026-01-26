import apiClient from "../api/axios";
import type {
  MonthlySummaryResult,
  MonthlyComparisonResult,
  CategoryBreakdownResult,
  MonthlyTrendResult,
  HighlightsResult,
} from "../types/statistics";

interface ApiResponse<T> {
  status: "Success" | "Fail";
  message: string | null;
  data: T;
}

export const statisticsRepository = {
  /**
   * 月次サマリーを取得
   *
   * @param year 対象年
   * @param month 対象月
   * @returns 月次サマリー
   */
  async getMonthlySummary(
    year: number,
    month: number,
  ): Promise<MonthlySummaryResult> {
    const response = await apiClient.get<ApiResponse<MonthlySummaryResult>>(
      "/TransactionSummary/monthly",
      {
        params: { Year: year, Month: month },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "月次サマリーの取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 前月比込み月次サマリーを取得
   *
   * @param year 対象年
   * @param month 対象月
   * @returns 前月比込みサマリー
   */
  async getMonthlyComparison(
    year: number,
    month: number,
  ): Promise<MonthlyComparisonResult> {
    const response = await apiClient.get<ApiResponse<MonthlyComparisonResult>>(
      "/api/Statistics/monthly-comparison",
      {
        params: { Year: year, Month: month },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "前月比サマリーの取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * カテゴリ別支出内訳を取得
   *
   * @param year 対象年
   * @param month 対象月
   * @returns 全カテゴリの支出内訳
   */
  async getCategoryBreakdown(
    year: number,
    month: number,
  ): Promise<CategoryBreakdownResult> {
    const response = await apiClient.get<ApiResponse<CategoryBreakdownResult>>(
      "/api/Statistics/category-breakdown",
      {
        params: { Year: year, Month: month },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "カテゴリ別内訳の取得に失敗しました",
      );
    }

    return response.data.data;
  },

  /**
   * 月次推移を取得
   *
   * @param months 取得する月数（デフォルト: 6）
   * @returns 直近N ヶ月の推移データ
   */
  async getMonthlyTrend(months: number = 6): Promise<MonthlyTrendResult> {
    const response = await apiClient.get<ApiResponse<MonthlyTrendResult>>(
      "/api/Statistics/trend",
      {
        params: { Months: months },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(response.data.message || "月次推移の取得に失敗しました");
    }

    return response.data.data;
  },

  /**
   * 月次ハイライトを取得
   *
   * @param year 対象年
   * @param month 対象月
   * @returns ハイライト情報
   */
  async getHighlights(year: number, month: number): Promise<HighlightsResult> {
    const response = await apiClient.get<ApiResponse<HighlightsResult>>(
      "/api/Statistics/highlights",
      {
        params: { Year: year, Month: month },
      },
    );

    if (response.data.status !== "Success") {
      throw new Error(
        response.data.message || "ハイライトの取得に失敗しました",
      );
    }

    return response.data.data;
  },
};
