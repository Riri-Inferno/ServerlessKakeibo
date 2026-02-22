import axios, { type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "../stores/authStore";

// window.ENVを優先、フォールバックでimport.meta.env
const API_BASE_URL = 
  (window as any).ENV?.API_BASE_URL ||        // Docker環境
  import.meta.env.VITE_API_BASE_URL ||         // ローカル開発
  'http://localhost:8080';                     // デフォルト

// デバッグ用ログ
console.log('API Base URL:', API_BASE_URL);

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
});

apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const authStore = useAuthStore();

    if (authStore.accessToken) {
      config.headers.Authorization = `Bearer ${authStore.accessToken}`;
    }

    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

apiClient.interceptors.response.use(
  (response) => {
    return response;
  },
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      originalRequest._retry = true;

      try {
        const authStore = useAuthStore();

        console.log("トークンをリフレッシュ中...");
        await authStore.refreshAccessToken();

        originalRequest.headers.Authorization = `Bearer ${authStore.accessToken}`;
        return apiClient(originalRequest);
      } catch (refreshError) {
        console.error("リフレッシュトークンが無効です。ログアウトします。");
        const authStore = useAuthStore();
        authStore.logout();

        if (typeof window !== "undefined") {
          window.location.href = "/";
        }

        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default apiClient;
