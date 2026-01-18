import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import {
  authRepository,
  type CurrentUser,
} from "../repositories/authRepository";
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

  return {
    // State
    isLoading,
    errorMessage,
    isAuthenticated,
    user,
    accessToken,

    // Actions
    logout,
    fetchCurrentUser,
  };
}
