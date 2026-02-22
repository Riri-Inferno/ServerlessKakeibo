<script setup lang="ts">
import { ref, onMounted } from "vue";
import { googleSdkLoaded, type CallbackTypes } from "vue3-google-login";
import { useRouter } from "vue-router";
import { useAuth } from "../composables/useAuth";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import logo from "../assets/icons/logo.svg?url";

type CredentialResponse = CallbackTypes.CredentialPopupResponse;

const router = useRouter();
const { loginWithGoogle, startGitHubLogin, isLoading, errorMessage } =
  useAuth();
const googleButtonContainer = ref<HTMLDivElement>();

const handleCredentialResponse = async (response: CredentialResponse) => {
  try {
    const idToken = response.credential;
    if (!idToken) throw new Error("Google ID Token を取得できませんでした");

    await loginWithGoogle(idToken);
    router.push({ name: "dashboard" });
  } catch (error) {
    console.error("Google ログインエラー:", error);
  }
};

const handleGitHubLogin = () => {
  try {
    startGitHubLogin();
  } catch (error) {
    console.error("GitHub ログインエラー:", error);
  }
};

const googleClientId = 
  (window as any).ENV?.GOOGLE_CLIENT_ID || 
  import.meta.env.VITE_GOOGLE_CLIENT_ID || 
  '';

onMounted(() => {
  googleSdkLoaded((google) => {
    if (!googleButtonContainer.value) return;

    google.accounts.id.initialize({
      client_id: googleClientId,
      callback: handleCredentialResponse,
      use_fedcm_for_prompt: false, // FedCM を無効化
    });

    google.accounts.id.renderButton(googleButtonContainer.value, {
      type: "standard",
      theme: "outline",
      size: "large",
      text: "signin_with",
      shape: "rectangular",
      logo_alignment: "center",
      locale: "ja",
      width: googleButtonContainer.value.offsetWidth.toString(),
    });
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

      <!-- ログインボタン -->
      <div v-show="!isLoading" class="space-y-3">
        <!-- Google ログインボタン（公式SDK） -->
        <div ref="googleButtonContainer" class="w-full"></div>

        <!-- GitHub ログインボタン -->
        <button
          @click="handleGitHubLogin"
          :disabled="isLoading"
          class="w-full h-10 flex items-center justify-center gap-3 bg-white border border-[#dadce0] rounded text-[#3c4043] text-sm font-medium hover:bg-[#f8f9fa] hover:shadow-md active:bg-[#f1f3f4] disabled:opacity-50 disabled:cursor-not-allowed transition-all duration-200"
          :style="{
            fontFamily: `'Google Sans', 'Roboto', 'Noto Sans JP', sans-serif`,
          }"
        >
          <!-- GitHub ロゴ -->
          <svg
            class="w-[18px] h-[18px] flex-shrink-0"
            fill="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              d="M12 0C5.37 0 0 5.37 0 12c0 5.31 3.435 9.795 8.205 11.385.6.105.825-.255.825-.57 0-.285-.015-1.23-.015-2.235-3.015.555-3.795-.735-4.035-1.41-.135-.345-.72-1.41-1.23-1.695-.42-.225-1.02-.78-.015-.795.945-.015 1.62.87 1.845 1.23 1.08 1.815 2.805 1.305 3.495.99.105-.78.42-1.305.765-1.605-2.67-.3-5.46-1.335-5.46-5.925 0-1.305.465-2.385 1.23-3.225-.12-.3-.54-1.53.12-3.18 0 0 1.005-.315 3.3 1.23.96-.27 1.98-.405 3-.405s2.04.135 3 .405c2.295-1.56 3.3-1.23 3.3-1.23.66 1.65.24 2.88.12 3.18.765.84 1.23 1.905 1.23 3.225 0 4.605-2.805 5.625-5.475 5.925.435.375.81 1.095.81 2.22 0 1.605-.015 2.895-.015 3.3 0 .315.225.69.825.57A12.02 12.02 0 0024 12c0-6.63-5.37-12-12-12z"
            />
          </svg>

          <!-- テキスト -->
          <span>GitHub でログイン</span>
        </button>
      </div>

      <!-- フッター -->
      <div class="mt-8 text-center">
        <BaseText variant="caption" color="gray">
          ログインすることで、利用規約とプライバシーポリシーに同意したものとみなされます（準備中）
        </BaseText>
      </div>
    </BaseCard>
  </div>
</template>
