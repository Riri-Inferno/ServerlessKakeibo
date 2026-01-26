import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import {
  authRepository,
  type CurrentUser,
} from "../repositories/authRepository";
import { settingsRepository } from "../repositories/settingsRepository";
import { useAuthStore } from "../stores/authStore";

/**
 * useAuth - 認証処理の Composable
 */
export function useAuth() {
  const router = useRouter();
  const authStore = useAuthStore();

  const isLoading = ref(false);
  const errorMessage = ref("");

  // ストアから取得
  const isAuthenticated = computed(() => authStore.isAuthenticated);
  const user = computed(() => authStore.user);
  const accessToken = computed(() => authStore.accessToken);
  const effectiveDisplayName = computed(() => authStore.effectiveDisplayName);

  /**
   * ログアウト
   */
  const logout = () => {
    authStore.logout();
    router.push({ name: "login" });
  };

  /**
   * 現在のユーザー情報を取得（/api/Auth/me）
   */
  const fetchCurrentUser = async (): Promise<CurrentUser> => {
    if (!authStore.accessToken) {
      throw new Error("認証されていません");
    }

    return await authRepository.getCurrentUser();
  };

  // 設定を取得してストアに保存
  const fetchSettings = async () => {
    try {
      const settings = await settingsRepository.getUserSettings();
      authStore.setSettings(settings);
    } catch (error) {
      console.error("設定取得エラー:", error);
      // エラーでも処理は継続
    }
  };

  return {
    // State
    isLoading,
    errorMessage,
    isAuthenticated,
    user,
    accessToken,
    effectiveDisplayName,

    // Actions
    logout,
    fetchCurrentUser,
    fetchSettings,
  };
}
