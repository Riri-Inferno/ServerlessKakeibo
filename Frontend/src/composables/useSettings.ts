import { ref } from "vue";
import { settingsRepository } from "../repositories/settingsRepository";
import type {
  UserSettings,
  UpdateUserSettingsRequest,
  DeleteAllTransactionsResult,
} from "../types/settings";

export function useSettings() {
  const settings = ref<UserSettings | null>(null);
  const isLoading = ref(false);
  const isSaving = ref(false);
  const isDeleting = ref(false);
  const errorMessage = ref("");
  const successMessage = ref("");
  const deleteResult = ref<DeleteAllTransactionsResult | null>(null);

  /**
   * ユーザー設定を取得
   */
  const fetchSettings = async () => {
    isLoading.value = true;
    errorMessage.value = "";

    try {
      settings.value = await settingsRepository.getUserSettings();
    } catch (error) {
      console.error("設定取得エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "設定の取得に失敗しました";
    } finally {
      isLoading.value = false;
    }
  };

  /**
   * ユーザー設定を更新
   */
  const updateSettings = async (request: UpdateUserSettingsRequest) => {
    isSaving.value = true;
    errorMessage.value = "";
    successMessage.value = "";

    try {
      settings.value = await settingsRepository.updateUserSettings(request);
      successMessage.value = "設定を保存しました";
    } catch (error) {
      console.error("設定更新エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "設定の更新に失敗しました";
      throw error;
    } finally {
      isSaving.value = false;
    }
  };

  /**
   * 全取引データを削除
   */
  const deleteAllTransactions = async () => {
    isDeleting.value = true;
    errorMessage.value = "";
    successMessage.value = "";
    deleteResult.value = null;

    try {
      deleteResult.value = await settingsRepository.deleteAllTransactions();

      successMessage.value = `${deleteResult.value.deletedTransactionCount}件の取引を削除しました`;

      return deleteResult.value;
    } catch (error) {
      console.error("取引削除エラー:", error);
      errorMessage.value =
        error instanceof Error
          ? error.message
          : "取引データの削除に失敗しました";
      throw error;
    } finally {
      isDeleting.value = false;
    }
  };

  /**
   * 表示名をGoogle情報に戻す
   */
  const resetDisplayNameToGoogle = async () => {
    await updateSettings({ displayNameOverride: "" });
  };

  /**
   * 締め日オプションを取得(1-31 + 月末)
   */
  const getClosingDayOptions = () => {
    const options: Array<{ value: string | number; label: string }> = [
      { value: "", label: "月末締め" },
    ];

    for (let i = 1; i <= 31; i++) {
      options.push({ value: i, label: `${i}日` });
    }

    return options;
  };
  /**
   * 締め日の表示ラベルを取得
   */
  const getClosingDayLabel = (closingDay: number | null): string => {
    return closingDay === null ? "月末" : `${closingDay}日`;
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
    settings,
    isLoading,
    isSaving,
    errorMessage,
    successMessage,

    // Methods
    fetchSettings,
    updateSettings,
    resetDisplayNameToGoogle,
    getClosingDayOptions,
    getClosingDayLabel,
    clearError,
    clearSuccess,
    deleteAllTransactions,
  };
}
