<script setup lang="ts">
import { ref, onMounted } from "vue";
import { googleSdkLoaded, type CallbackTypes } from "vue3-google-login";
import { authRepository } from "../repositories/authRepository";
import { useAuthStore } from "../stores/authStore";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import logo from "../assets/icons/logo.svg?url";

type CredentialResponse = CallbackTypes.CredentialPopupResponse;

const authStore = useAuthStore();
const isLoading = ref(false);
const errorMessage = ref("");
const googleButtonContainer = ref<HTMLDivElement>();

// Google ログイン成功時のコールバック
const handleCredentialResponse = async (response: CredentialResponse) => {
  isLoading.value = true;
  errorMessage.value = "";

  try {
    const idToken = response.credential;

    if (!idToken) {
      throw new Error("Google ID Token を取得できませんでした");
    }

    console.log("Google ID Token を取得しました");

    // バックエンドに ID Token を送信
    console.log("バックエンドに認証リクエスト...");
    const userData = await authRepository.loginWithGoogle(idToken);

    console.log("ログイン成功:", userData);

    // Pinia ストアに保存
    authStore.setAuthData(userData);

    alert(`ログイン成功！\nようこそ、${userData.displayName}さん`);

    // ダッシュボードにリダイレクト（後で実装）
    // router.push({ name: "dashboard" });
  } catch (error) {
    console.error("ログインエラー:", error);

    if (error instanceof Error) {
      errorMessage.value = error.message;
    } else {
      errorMessage.value = "ログインに失敗しました。もう一度お試しください。";
    }
  } finally {
    isLoading.value = false;
  }
};

// Google SDK 読み込み後にボタンをレンダリング
onMounted(() => {
  googleSdkLoaded((google) => {
    if (!googleButtonContainer.value) return;

    // Google Identity Services を初期化
    google.accounts.id.initialize({
      client_id: import.meta.env.VITE_GOOGLE_CLIENT_ID,
      callback: handleCredentialResponse,
      use_fedcm_for_prompt: false, // FedCM を無効化
    });

    // ボタンをレンダリング
    google.accounts.id.renderButton(googleButtonContainer.value, {
      theme: "outline",
      size: "large",
      text: "signin_with",
      shape: "rectangular",
      locale: "ja",
      width: "400",
    });

    console.log("Google ログインボタンをレンダリングしました");
  });
});
</script>

<template>
  <div
    class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
  >
    <BaseCard padding="lg" class="w-full max-w-md">
      <!-- ロゴ -->
      <div class="text-center mb-8">
        <img :src="logo" alt="家計簿アプリ" class="w-60 h-16 mx-auto mb-4" />
        <BaseText variant="h1" class="mb-2">家計簿アプリ(仮)</BaseText>
        <BaseText variant="body" color="gray">Serverless Kakeibo</BaseText>
      </div>

      <!-- エラーメッセージ -->
      <div
        v-if="errorMessage"
        class="mb-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg"
      >
        <BaseText variant="caption">{{ errorMessage }}</BaseText>
      </div>

      <!-- ローディング表示 -->
      <div v-if="isLoading" class="text-center py-4">
        <BaseText variant="body">ログイン中...</BaseText>
      </div>

      <!-- Google ログインボタン（公式） -->
      <div
        v-show="!isLoading"
        ref="googleButtonContainer"
        class="flex justify-center"
      ></div>

      <!-- フッター -->
      <div class="mt-8 text-center">
        <BaseText variant="caption" color="gray">
          ログインすることで、利用規約とプライバシーポリシーに同意したものとみなされます
        </BaseText>
      </div>
    </BaseCard>
  </div>
</template>
