<script setup lang="ts">
// import { ref } from "vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseText from "../components/atoms/BaseText.vue";
import googleIcon from "../assets/icons/google.svg?url";
import logo from "../assets/icons/logo.svg?url";

// TODO: 後で実装する Google OAuth の URL
const handleGoogleLogin = () => {
  console.log("Google ログインボタンがクリックされました");
  // ここで Google OAuth にリダイレクト
  // window.location.href = 'バックエンドのGoogle認証エンドポイント'
};
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

      <!-- Google ログインボタン -->
      <BaseButton
        variant="outline"
        size="lg"
        full-width
        @click="handleGoogleLogin"
      >
        <img :src="googleIcon" alt="Google" class="w-6 h-6 inline mr-3" />
        <span>Google でログイン</span>
      </BaseButton>

      <!-- フッター -->
      <div class="mt-8 text-center">
        <BaseText variant="caption" color="gray">
          ログインすることで、利用規約とプライバシーポリシーに同意したものとみなされます
        </BaseText>
      </div>
    </BaseCard>
  </div>
</template>
