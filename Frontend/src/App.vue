<template>
  <div
    class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
  >
    <div class="bg-white rounded-2xl shadow-2xl p-8 w-full max-w-md">
      <!-- ヘッダー -->
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold text-gray-800 mb-2">家計簿アプリ</h1>
        <p class="text-gray-600">Serverless Kakeibo</p>
      </div>

      <!-- ログインフォーム -->
      <div class="space-y-4">
        <!-- Googleログインボタン -->
        <button
          @click="handleGoogleLogin"
          class="w-full bg-white border-2 border-gray-300 hover:border-gray-400 text-gray-700 font-semibold py-3 px-4 rounded-lg flex items-center justify-center gap-3 transition-all hover:shadow-md"
        >
          <svg class="w-6 h-6" viewBox="0 0 24 24">
            <path
              fill="#4285F4"
              d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z"
            />
            <path
              fill="#34A853"
              d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z"
            />
            <path
              fill="#FBBC05"
              d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z"
            />
            <path
              fill="#EA4335"
              d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z"
            />
          </svg>
          <span>Google でログイン</span>
        </button>

        <!-- 区切り線 -->
        <div class="relative">
          <div class="absolute inset-0 flex items-center">
            <div class="w-full border-t border-gray-300"></div>
          </div>
          <div class="relative flex justify-center text-sm">
            <span class="px-4 bg-white text-gray-500">または</span>
          </div>
        </div>

        <!-- メールログイン（ハリボテ） -->
        <button
          disabled
          class="w-full bg-gray-100 text-gray-400 font-semibold py-3 px-4 rounded-lg cursor-not-allowed"
        >
          メールでログイン（準備中）
        </button>
      </div>

      <!-- フッター -->
      <div class="mt-8 text-center text-sm text-gray-500">
        <p>
          ログインすることで、利用規約とプライバシーポリシーに同意したものとみなされます
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const handleGoogleLogin = () => {
  console.log("Googleログインボタンがクリックされました");
  alert("Googleログイン処理（実装予定）");
  // TODO: 実際のGoogle OAuth処理を実装
};
</script>
