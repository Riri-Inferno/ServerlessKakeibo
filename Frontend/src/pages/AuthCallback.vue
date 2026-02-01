<script setup lang="ts">
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useAuth } from "../composables/useAuth";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";

const route = useRoute();
const router = useRouter();
const { loginWithGitHub } = useAuth();

const isLoading = ref(true);
const errorMessage = ref("");

onMounted(async () => {
  try {
    // URL パラメータから code と state を取得
    const code = route.query.code as string;
    const state = route.query.state as string;

    // GitHub からのエラーをチェック
    if (route.query.error) {
      const errorDesc =
        (route.query.error_description as string) || "GitHub認証に失敗しました";
      throw new Error(errorDesc);
    }

    // code の存在確認
    if (!code) {
      throw new Error("認証コードが取得できませんでした");
    }

    // Composable 経由でログイン（state 検証含む）
    await loginWithGitHub(code, state);

    // ダッシュボードにリダイレクト
    router.push({ name: "dashboard" });
  } catch (error) {
    console.error("GitHub 認証エラー:", error);

    errorMessage.value =
      error instanceof Error
        ? error.message
        : "GitHub 認証に失敗しました。もう一度お試しください。";

    isLoading.value = false;
  }
});
</script>

<template>
  <div
    class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
  >
    <BaseCard padding="lg" class="w-full max-w-md">
      <!-- ローディング -->
      <div v-if="isLoading" class="text-center py-8">
        <BaseSpinner size="lg" color="primary" class="mb-4" />
        <BaseText variant="h3" class="mb-2">GitHub 認証中...</BaseText>
        <BaseText variant="body" color="gray">
          しばらくお待ちください
        </BaseText>
      </div>

      <!-- エラー -->
      <div v-else class="text-center py-8">
        <div
          class="w-16 h-16 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-4"
        >
          <svg
            class="w-8 h-8 text-red-600"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M6 18L18 6M6 6l12 12"
            />
          </svg>
        </div>

        <BaseText variant="h3" class="mb-2">認証エラー</BaseText>
        <BaseText variant="body" color="gray" class="mb-6">
          {{ errorMessage }}
        </BaseText>

        <button
          @click="router.push({ name: 'login' })"
          class="px-6 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700 transition-colors"
        >
          ログイン画面に戻る
        </button>
      </div>
    </BaseCard>
  </div>
</template>
