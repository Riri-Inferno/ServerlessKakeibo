<script setup lang="ts">
import { ref } from "vue";
import { useRouter } from "vue-router";
import { useAuth } from "../composables/useAuth";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";

const router = useRouter();
const { user, logout } = useAuth();

// ログアウト処理
const handleLogout = async () => {
  if (confirm("ログアウトしますか？")) {
    await logout();
    router.push("/login");
  }
};

// 将来的な機能（現在は未実装）
const handleExportAllData = () => {
  // エクスポートページに遷移
  router.push("/transactions");
  alert("取引一覧画面からエクスポートできます");
};

const handleDeleteAllData = () => {
  alert("この機能は準備中です");
};
</script>

<template>
  <DefaultLayout>
    <div class="max-w-4xl mx-auto space-y-6">
      <!-- ヘッダー -->
      <div class="mb-8">
        <BaseText variant="h1" class="mb-2">設定</BaseText>
        <BaseText variant="caption" color="gray">
          アカウント情報と各種設定
        </BaseText>
      </div>

      <!-- アカウント情報 -->
      <BaseCard>
        <div class="space-y-4">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="user" size="md" class="text-gray-500" />
            <BaseText variant="h3">アカウント情報</BaseText>
          </div>

          <div class="space-y-3">
            <div>
              <BaseText variant="caption" color="gray" class="mb-1">
                メールアドレス
              </BaseText>
              <BaseText variant="body">
                <!-- {{ user?.email || "未設定" }} -->
                misettei.com
              </BaseText>
            </div>

            <div>
              <BaseText variant="caption" color="gray" class="mb-1">
                ユーザー名
              </BaseText>
              <BaseText variant="body">
                <!-- {{ user?.name || "未設定" }} -->
                未設定 太郎
              </BaseText>
            </div>
          </div>

          <div class="pt-4 border-t border-gray-200">
            <BaseButton variant="outline" @click="handleLogout" class="w-full">
              <span class="flex items-center justify-center gap-2">
                <BaseIcon name="arrow-right" size="sm" />
                <span>ログアウト</span>
              </span>
            </BaseButton>
          </div>
        </div>
      </BaseCard>

      <!-- データ管理 -->
      <BaseCard>
        <div class="space-y-4">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="folder" size="md" class="text-gray-500" />
            <BaseText variant="h3">データ管理</BaseText>
          </div>

          <div class="space-y-3">
            <!-- エクスポート -->
            <div
              class="flex items-center justify-between py-3 border-b border-gray-100"
            >
              <div>
                <BaseText variant="body" weight="medium" class="mb-1">
                  取引データのエクスポート
                </BaseText>
                <BaseText variant="caption" color="gray">
                  CSVファイルとしてダウンロード
                </BaseText>
              </div>
              <BaseButton
                variant="outline"
                size="sm"
                @click="handleExportAllData"
              >
                <span class="flex items-center gap-1">
                  <BaseIcon name="download" size="sm" />
                  <span>エクスポート</span>
                </span>
              </BaseButton>
            </div>

            <!-- データ削除（将来実装） -->
            <div class="flex items-center justify-between py-3">
              <div>
                <BaseText variant="body" weight="medium" class="mb-1">
                  全データの削除
                </BaseText>
                <BaseText variant="caption" color="gray">
                  すべての取引データを削除します（準備中）
                </BaseText>
              </div>
              <BaseButton
                variant="outline"
                size="sm"
                :disabled="true"
                @click="handleDeleteAllData"
              >
                <span class="flex items-center gap-1">
                  <BaseIcon name="trash" size="sm" />
                  <span>削除</span>
                </span>
              </BaseButton>
            </div>
          </div>
        </div>
      </BaseCard>

      <!-- アプリ情報 -->
      <BaseCard>
        <div class="space-y-4">
          <div class="flex items-center gap-2 mb-4">
            <BaseIcon name="info" size="md" class="text-gray-500" />
            <BaseText variant="h3">アプリ情報</BaseText>
          </div>

          <div class="space-y-3">
            <div>
              <BaseText variant="caption" color="gray" class="mb-1">
                バージョン
              </BaseText>
              <BaseText variant="body">1.0.0</BaseText>
            </div>

            <div>
              <BaseText variant="caption" color="gray" class="mb-1">
                ビルド
              </BaseText>
              <BaseText variant="body">2026.01.25</BaseText>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>
  </DefaultLayout>
</template>
