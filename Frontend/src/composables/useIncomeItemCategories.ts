import { ref, computed } from "vue";
import { incomeItemCategoryRepository } from "../repositories/incomeItemCategoryRepository";
import type {
  IncomeItemCategoryDto,
  CreateIncomeItemCategoryRequest,
  UpdateIncomeItemCategoryRequest,
} from "../types/incomeItemCategory";

export function useIncomeItemCategories() {
  const categories = ref<IncomeItemCategoryDto[]>([]);
  const isLoading = ref(false);
  const isSaving = ref(false);
  const errorMessage = ref("");
  const successMessage = ref("");

  /**
   * 表示用カテゴリ（非表示を除外、displayOrderでソート）
   */
  const visibleCategories = computed(() =>
    categories.value
      .filter((c) => !c.isHidden)
      .sort((a, b) => a.displayOrder - b.displayOrder),
  );

  /**
   * 非表示カテゴリ（復元用）
   */
  const hiddenCategories = computed(() =>
    categories.value.filter((c) => c.isHidden),
  );

  /**
   * カテゴリ一覧を取得
   */
  const fetchCategories = async (includeHidden = false) => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      const result =
        await incomeItemCategoryRepository.getCategories(includeHidden);
      categories.value = result.categories;
    } catch (error) {
      console.error("給与項目カテゴリ取得エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "給与項目カテゴリの取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * カスタムカテゴリを作成
   */
  const createCategory = async (request: CreateIncomeItemCategoryRequest) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result = await incomeItemCategoryRepository.createCategory(request);
      successMessage.value = result.message;
      await fetchCategories(false);
    } catch (error) {
      console.error("給与項目カテゴリ作成エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "給与項目カテゴリの作成に失敗しました";
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
    request: UpdateIncomeItemCategoryRequest,
  ) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      const result = await incomeItemCategoryRepository.updateCategory(
        id,
        request,
      );
      successMessage.value = result.message;
      await fetchCategories(false);
    } catch (error) {
      console.error("給与項目カテゴリ更新エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "給与項目カテゴリの更新に失敗しました";
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
      const result = await incomeItemCategoryRepository.deleteCategory(id);
      successMessage.value = result.message;
      await fetchCategories(false);
    } catch (error) {
      console.error("給与項目カテゴリ削除エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "給与項目カテゴリの削除に失敗しました";
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
      const result = await incomeItemCategoryRepository.restoreCategory(id);
      successMessage.value = result.message;
      await fetchCategories(false);
    } catch (error) {
      console.error("給与項目カテゴリ復元エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "給与項目カテゴリの復元に失敗しました";
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
      const result = await incomeItemCategoryRepository.resetToMaster();
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
    reorderedCategories: IncomeItemCategoryDto[],
  ) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      // 一括更新APIを呼び出し
      const result = await incomeItemCategoryRepository.updateCategoryOrders(
        reorderedCategories.map((category, index) => ({
          id: category.id,
          displayOrder: index + 1,
        })),
      );

      // レスポンスで直接更新
      categories.value = result.categories;
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
