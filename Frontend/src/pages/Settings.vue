<script setup lang="ts">
import { onMounted, ref, computed } from "vue";
import { useRouter } from "vue-router";
import { useAuth } from "../composables/useAuth";
import { useSettings } from "../composables/useSettings";
import DefaultLayout from "../layouts/DefaultLayout.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";
import BaseInput from "../components/atoms/BaseInput.vue";
import BaseSelect from "../components/atoms/BaseSelect.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";
import TransactionExportModal from "../components/organisms/TransactionExportModal.vue";
import TransactionDeleteConfirmModal from "../components/organisms/TransactionDeleteConfirmModal.vue";
import CategoryManagementSection from "../components/organisms/CategoryManagementSection.vue";

// 環境変数からバージョン情報を取得
const appVersion = import.meta.env.VITE_APP_VERSION || "不明";
const buildDate = import.meta.env.VITE_BUILD_DATE || "不明";

const router = useRouter();
const { logout } = useAuth();
const {
  settings,
  isLoading,
  isSaving,
  errorMessage,
  successMessage,
  fetchSettings,
  updateSettings,
  resetDisplayNameToGoogle,
  getClosingDayOptions,
  clearError,
  clearSuccess,
} = useSettings();

// フォーム用のローカルステート
const formData = ref({
  displayNameOverride: "",
  closingDay: "" as string | number,
  timeZone: "Asia/Tokyo",
  currencyCode: "JPY",
});

// モーダル用の状態
const isExportModalOpen = ref(false);
const isDeleteModalOpen = ref(false);
const exportFilters = ref({});

// フォームが変更されたかどうか
const hasChanges = computed(() => {
  if (!settings.value) return false;

  const currentClosingDay = settings.value.closingDay ?? "";

  return (
    formData.value.displayNameOverride !==
      (settings.value.displayNameOverride || "") ||
    formData.value.closingDay !== currentClosingDay ||
    formData.value.timeZone !== settings.value.timeZone ||
    formData.value.currencyCode !== settings.value.currencyCode
  );
});

// 締め日オプション
const closingDayOptions = getClosingDayOptions();

// 初期化
onMounted(async () => {
  await fetchSettings();
  if (settings.value) {
    formData.value = {
      displayNameOverride: settings.value.displayNameOverride || "",
      closingDay: settings.value.closingDay ?? "",
      timeZone: settings.value.timeZone,
      currencyCode: settings.value.currencyCode,
    };
  }
});

// 保存処理
const handleSave = async () => {
  clearError();
  clearSuccess();

  try {
    await updateSettings({
      ...formData.value,
      closingDay:
        formData.value.closingDay === ""
          ? null
          : Number(formData.value.closingDay),
    });

    const { fetchSettings } = useAuth();
    await fetchSettings();
  } catch (error) {
  }
};

// 認証元の情報に戻す
const handleResetToGoogle = async () => {
  if (confirm("表示名をGoogleアカウントの情報に戻しますか?")) {
    clearError();
    clearSuccess();

    try {
      await resetDisplayNameToGoogle();
      formData.value.displayNameOverride = "";
      const { fetchSettings } = useAuth();
      await fetchSettings();
    } catch (error) {
      // エラーはcomposableで処理済み
    }
  }
};

// ログアウト処理
const handleLogout = async () => {
  if (confirm("ログアウトしますか？")) {
    await logout();
    router.push("/");
  }
};

// エクスポートモーダルを開く
const handleExportAllData = () => {
  exportFilters.value = {};
  isExportModalOpen.value = true;
};

// エクスポートモーダルを閉じる
const closeExportModal = () => {
  isExportModalOpen.value = false;
};

// 削除モーダルを開く
const handleDeleteAllData = () => {
  isDeleteModalOpen.value = true;
};

// 削除モーダルを閉じる
const closeDeleteModal = () => {
  isDeleteModalOpen.value = false;
};
</script>

<template>
  <DefaultLayout>
    <div class="max-w-4xl mx-auto space-y-4 md:space-y-6">
      <div class="mb-6 md:mb-8">
        <BaseText variant="h1" class="mb-1 md:mb-2 text-xl md:text-2xl lg:text-3xl">設定</BaseText>
        <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
          アカウント情報と各種設定
        </BaseText>
      </div>

      <div
        v-if="isLoading"
        class="flex-1 flex items-center justify-center py-8 md:py-12"
      >
        <div class="text-center">
          <BaseSpinner
            icon="refresh"
            size="lg"
            color="primary"
            label="設定を読み込み中"
            class="mb-2"
          />
          <BaseText variant="body" color="gray">読み込み中...</BaseText>
        </div>
      </div>

      <div v-else-if="errorMessage && !settings" class="text-center py-8 md:py-12">
        <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
      </div>

      <template v-else-if="settings">
        <div
          v-if="successMessage"
          class="bg-green-50 border border-green-200 rounded-lg p-3 md:p-4 mb-4 md:mb-6"
        >
          <div class="flex items-center gap-2">
            <BaseIcon name="check" size="sm" class="text-green-600" />
            <BaseText variant="body" class="text-green-800 text-sm md:text-base">
              {{ successMessage }}
            </BaseText>
          </div>
        </div>

        <div
          v-if="errorMessage"
          class="bg-red-50 border border-red-200 rounded-lg p-3 md:p-4 mb-4 md:mb-6"
        >
          <div class="flex items-center gap-2">
            <BaseIcon name="alert" size="sm" class="text-red-600" />
            <BaseText variant="body" class="text-red-800 text-sm md:text-base">
              {{ errorMessage }}
            </BaseText>
          </div>
        </div>

        <BaseCard class="p-4 md:p-6">
          <div class="space-y-3 md:space-y-4">
            <div class="flex items-center gap-2 mb-3 md:mb-4">
              <BaseIcon name="user" size="md" class="text-gray-500" />
              <BaseText variant="h3">アカウント情報</BaseText>
            </div>

            <div class="space-y-3 md:space-y-4">
              <div>
                <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
                  メールアドレス
                </BaseText>
                <BaseText variant="body" class="text-sm md:text-base">
                  {{ settings.email || "未設定" }}
                </BaseText>
              </div>

              <div>
                <div class="flex items-center justify-between mb-1">
                  <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
                    ユーザー名
                  </BaseText>
                  <BaseButton
                    v-if="settings.displayNameOverride"
                    variant="text"
                    size="sm"
                    @click="handleResetToGoogle"
                    :disabled="isSaving"
                  >
                    <span class="flex items-center gap-1 text-blue-600">
                      <BaseIcon name="refresh" size="sm" />
                      <span class="text-xs">認証情報に戻す</span>
                    </span>
                  </BaseButton>
                </div>
                <BaseInput
                  v-model="formData.displayNameOverride"
                  :placeholder="settings.displayName"
                  :disabled="isSaving"
                />
                <BaseText variant="caption" color="gray" class="mt-1 text-xs">
                  空欄の場合は認証アカウントの名前が使用されます
                </BaseText>
              </div>

              <div>
                <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
                  締め日
                </BaseText>
                <BaseSelect
                  v-model="formData.closingDay"
                  :options="closingDayOptions"
                  :disabled="isSaving"
                  hide-placeholder
                />
                <BaseText variant="caption" color="gray" class="mt-1 text-xs">
                  家計簿の集計期間の区切り日
                </BaseText>
              </div>

              <div>
                <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
                  タイムゾーン
                </BaseText>
                <BaseInput
                  v-model="formData.timeZone"
                  :disabled="isSaving"
                  readonly
                />
              </div>

              <div>
                <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
                  通貨
                </BaseText>
                <BaseInput
                  v-model="formData.currencyCode"
                  :disabled="isSaving"
                  readonly
                />
              </div>
            </div>

            <div class="pt-3 md:pt-4 border-t border-gray-200">
              <BaseButton
                variant="primary"
                @click="handleSave"
                :disabled="!hasChanges || isSaving"
                class="w-full"
              >
                <span class="flex items-center justify-center gap-2">
                  <BaseSpinner
                    v-if="isSaving"
                    icon="refresh"
                    size="sm"
                    color="white"
                  />
                  <BaseIcon v-else name="check" size="sm" />
                  <span class="text-sm md:text-base">{{ isSaving ? "保存中..." : "変更を保存" }}</span>
                </span>
              </BaseButton>
            </div>

            <div class="pt-2">
              <BaseButton
                variant="outline"
                @click="handleLogout"
                :disabled="isSaving"
                class="w-full"
              >
                <span class="flex items-center justify-center gap-2">
                  <BaseIcon name="arrow-right" size="sm" />
                  <span class="text-sm md:text-base">ログアウト</span>
                </span>
              </BaseButton>
            </div>
          </div>
        </BaseCard>

        <CategoryManagementSection />

        <BaseCard class="p-4 md:p-6">
          <div class="space-y-3 md:space-y-4">
            <div class="flex items-center gap-2 mb-3 md:mb-4">
              <BaseIcon name="folder" size="md" class="text-gray-500" />
              <BaseText variant="h3">データ管理</BaseText>
            </div>

            <div class="space-y-2 md:space-y-3">
              <div class="py-2 md:py-3 border-b border-gray-100">
                <div
                  class="flex flex-col md:flex-row md:items-center md:justify-between gap-2 md:gap-3"
                >
                  <div class="flex-1 min-w-0">
                    <BaseText variant="body" weight="medium" class="block text-sm md:text-base">
                      取引データのエクスポート
                    </BaseText>

                    <BaseText variant="caption" color="gray" class="block mb-2 text-xs md:text-sm">
                      すべての取引データをCSVファイルとしてダウンロード
                    </BaseText>

                    <button
                      @click="router.push('/transactions')"
                      class="block text-left text-xs md:text-sm text-blue-600 hover:text-blue-700 transition-colors mt-1"
                    >
                      <div class="flex items-center gap-1">
                        <BaseIcon name="arrow-right" size="sm" />
                        <span class="underline"
                          >詳細な条件でエクスポートする場合は取引一覧へ</span
                        >
                      </div>
                    </button>
                  </div>

                  <BaseButton
                    variant="outline"
                    size="sm"
                    @click="handleExportAllData"
                    class="flex-shrink-0 self-start md:self-center"
                  >
                    <span class="flex items-center gap-1 text-xs md:text-sm">
                      <BaseIcon name="download" size="sm" />
                      <span>エクスポート</span>
                    </span>
                  </BaseButton>
                </div>
              </div>

              <div class="py-2 md:py-3">
                <div
                  class="flex flex-col md:flex-row md:items-center md:justify-between gap-2 md:gap-3"
                >
                  <div class="flex-1">
                    <BaseText variant="body" weight="medium" class="mb-1 text-sm md:text-base">
                      全データの削除
                    </BaseText>
                    <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
                      すべての取引データを削除します
                    </BaseText>
                  </div>
                  <BaseButton
                    variant="outline"
                    size="sm"
                    @click="handleDeleteAllData"
                    class="flex-shrink-0 self-start md:self-center"
                  >
                    <span class="flex items-center gap-1 text-red-600 text-xs md:text-sm">
                      <BaseIcon name="trash" size="sm" />
                      <span>削除</span>
                    </span>
                  </BaseButton>
                </div>
              </div>
            </div>
          </div>
        </BaseCard>

        <BaseCard class="p-4 md:p-6">
          <div class="space-y-3 md:space-y-4">
            <div class="flex items-center gap-2 mb-3 md:mb-4">
              <BaseIcon name="info" size="md" class="text-gray-500" />
              <BaseText variant="h3">アプリ情報</BaseText>
            </div>

            <div class="space-y-2 md:space-y-3">
              <div>
                <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
                  バージョン
                </BaseText>
                <BaseText variant="body" class="text-sm md:text-base">{{ appVersion }}</BaseText>
              </div>

              <div>
                <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
                  ビルド
                </BaseText>
                <BaseText variant="body" class="text-sm md:text-base">{{ buildDate }}</BaseText>
              </div>
            </div>
          </div>
        </BaseCard>
      </template>
    </div>

    <TransactionExportModal
      :is-open="isExportModalOpen"
      :filters="exportFilters"
      @close="closeExportModal"
    />

    <TransactionDeleteConfirmModal
      :is-open="isDeleteModalOpen"
      @close="closeDeleteModal"
    />
  </DefaultLayout>
</template>
