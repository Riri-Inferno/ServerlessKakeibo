import { ref, computed } from "vue";
import { transactionCategoryRepository } from "../repositories/transactionCategoryRepository";
import type {
  TransactionCategoryDto,
  CreateTransactionCategoryRequest,
  UpdateTransactionCategoryRequest,
  //   TransactionCategoryListResult,
} from "../types/transactionCategory";

export function useTransactionCategories() {
  const categories = ref<TransactionCategoryDto[]>([]);
  const isLoading = ref(false);
  const isSaving = ref(false);
  const errorMessage = ref("");
  const successMessage = ref("");

  /**
   * 表示用カテゴリ（非表示を除外、displayOrderでソート）
   */
  const visibleCategories = computed(() => {
    const result = categories.value
      .filter((c) => !c.isHidden)
      .sort((a, b) => a.displayOrder - b.displayOrder);

    return result;
  });

  /**
   * 非表示カテゴリ（復元用）
   */
  const hiddenCategories = computed(() =>
    categories.value.filter((c) => c.isHidden),
  );

  /**
   * 支出カテゴリのみ
   */
  const expenseCategories = computed(() =>
    visibleCategories.value.filter((c) => !c.isIncome),
  );

  /**
   * 収入カテゴリのみ
   */
  const incomeCategories = computed(() =>
    visibleCategories.value.filter((c) => c.isIncome),
  );

  /**
   * カテゴリ一覧を取得
   */
  const fetchCategories = async (includeHidden = false) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      const result =
        await transactionCategoryRepository.getCategories(includeHidden);
      categories.value = result.categories;
    } catch (error) {
      console.error("取引カテゴリ取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "取引カテゴリの取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * カスタムカテゴリを作成
   */
  const createCategory = async (request: CreateTransactionCategoryRequest) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result =
        await transactionCategoryRepository.createCategory(request);
      successMessage.value = result.message;
      // 再取得
      await fetchCategories(false);
    } catch (error) {
      console.error("取引カテゴリ作成エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "取引カテゴリの作成に失敗しました";
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * カテゴリを更新
   */
  const updateCategory = async (
    id: string,
    request: UpdateTransactionCategoryRequest,
  ) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result = await transactionCategoryRepository.updateCategory(
        id,
        request,
      );
      successMessage.value = result.message;
      // 再取得
      await fetchCategories(false);
    } catch (error) {
      console.error("取引カテゴリ更新エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "取引カテゴリの更新に失敗しました";
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * カテゴリを削除（非表示化）
   */
  const deleteCategory = async (id: string) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result = await transactionCategoryRepository.deleteCategory(id);
      successMessage.value = result.message;
      // 再取得
      await fetchCategories(false);
    } catch (error) {
      console.error("取引カテゴリ削除エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "取引カテゴリの削除に失敗しました";
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * カテゴリを復元
   */
  const restoreCategory = async (id: string) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result = await transactionCategoryRepository.restoreCategory(id);
      successMessage.value = result.message;
      // 再取得
      await fetchCategories(false);
    } catch (error) {
      console.error("取引カテゴリ復元エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "取引カテゴリの復元に失敗しました";
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * マスタ設定に戻す
   */
  const resetToMaster = async () => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result = await transactionCategoryRepository.resetToMaster();
      categories.value = result.categories;
      successMessage.value = "マスタ設定に戻しました";
    } catch (error) {
      console.error("マスタ設定復元エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "マスタ設定への復元に失敗しました";
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * 並び順を一括更新（ドラッグ&ドロップ用）
   */
  const updateDisplayOrders = async (
    reorderedCategories: TransactionCategoryDto[],
  ) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      // 一括更新APIを呼び出し
      const result = await transactionCategoryRepository.updateCategoryOrders(
        reorderedCategories.map((category, index) => ({
          id: category.id,
          displayOrder: index + 1,
        })),
      );

      categories.value = result.categories;

      await fetchCategories(true);

      successMessage.value = "並び順を更新しました";
    } catch (error) {
      console.error("並び順更新エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "並び順の更新に失敗しました";

      // エラー時のみ再取得
      await fetchCategories(true);
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * エラーメッセージをクリア
   */
  const clearError = () => {
    errorMessage.value = "";
  };

  /**
   * 成功メッセージをクリア
   */
  const clearSuccess = () => {
    successMessage.value = "";
  };

  return {
    // State
    categories,
    visibleCategories,
    hiddenCategories,
    expenseCategories,
    incomeCategories,
    isLoading,
    isSaving,
    errorMessage,
    successMessage,

    // Methods
    fetchCategories,
    createCategory,
    updateCategory,
    deleteCategory,
    restoreCategory,
    resetToMaster,
    updateDisplayOrders,
    clearError,
    clearSuccess,
  };
}
