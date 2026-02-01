import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { authRepository } from "../repositories/authRepository";
import type { LoginResult } from "../types/auth";
import type { UserSettings } from "../types/settings";

/**
 * ユーザー情報の型
 */
export interface User {
  userId: string;
  displayName: string;
  pictureUrl: string;
}

/**
 * 認証ストア
 */
export const useAuthStore = defineStore("auth", () => {
  // State
  const accessToken = ref<string | null>(null);
  const refreshToken = ref<string | null>(null);
  const user = ref<User | null>(null);
  const settings = ref<UserSettings | null>(null);

  // Getters
  const isAuthenticated = computed(() => !!accessToken.value);

  // 表示名(Override考慮)
  const effectiveDisplayName = computed(() => {
    if (!user.value) return "";
    return settings.value?.displayNameOverride || user.value.displayName;
  });

  /**
   * 初期化：localStorage からデータを復元
   */
  const initialize = () => {
    const savedAccessToken = localStorage.getItem("accessToken");
    const savedRefreshToken = localStorage.getItem("refreshToken");
    const savedUser = localStorage.getItem("user");
    const savedSettings = localStorage.getItem("settings");

    if (savedAccessToken) accessToken.value = savedAccessToken;
    if (savedRefreshToken) refreshToken.value = savedRefreshToken;
    if (savedUser) user.value = JSON.parse(savedUser);
    if (savedSettings) settings.value = JSON.parse(savedSettings);
  };

  /**
   * ログイン成功時:トークンとユーザー情報を保存
   */
  const setAuthData = (data: LoginResult) => {
    accessToken.value = data.accessToken;
    refreshToken.value = data.refreshToken;
    user.value = {
      userId: data.userId,
      displayName: data.displayName,
      pictureUrl: data.pictureUrl,
    };

    // localStorage にも保存（リロード対策）
    localStorage.setItem("accessToken", data.accessToken);
    localStorage.setItem("refreshToken", data.refreshToken);
    localStorage.setItem("user", JSON.stringify(user.value));
  };

  // 設定を保存
  const setSettings = (newSettings: UserSettings) => {
    settings.value = newSettings;
    localStorage.setItem("settings", JSON.stringify(newSettings));
  };

  /**
   * ログアウト
   */
  const logout = () => {
    accessToken.value = null;
    refreshToken.value = null;
    user.value = null;
    settings.value = null;

    localStorage.removeItem("accessToken");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("user");
    localStorage.removeItem("settings");
  };

  /**
   * トークンを更新
   */
  const refreshAccessToken = async () => {
    if (!refreshToken.value) {
      throw new Error("リフレッシュトークンがありません");
    }

    try {
      const data = await authRepository.refreshToken(refreshToken.value);
      setAuthData(data);
      return data.accessToken;
    } catch (error) {
      // リフレッシュ失敗 → ログアウト
      logout();
      throw error;
    }
  };

  return {
    // State
    accessToken,
    refreshToken,
    user,
    settings,

    // Getters
    isAuthenticated,
    effectiveDisplayName,

    // Actions
    initialize,
    setAuthData,
    setSettings,
    logout,
    refreshAccessToken,
  };
});
