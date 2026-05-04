import axios, { type InternalAxiosRequestConfig } from "axios";
import { useAuthStore } from "../stores/authStore";
import { getApiBaseUrl } from "../utils/env";

const apiClient = axios.create({
  baseURL: getApiBaseUrl(),
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
