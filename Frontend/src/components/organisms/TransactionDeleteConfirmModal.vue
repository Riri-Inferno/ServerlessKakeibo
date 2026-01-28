<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { useSettings } from "../../composables/useSettings";
import BaseModal from "../atoms/BaseModal.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseCard from "../atoms/BaseCard.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import BaseInput from "../atoms/BaseInput.vue";

interface Props {
  isOpen: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
}>();

const {
  isLoading,
  isDeleting,
  errorMessage,
  deleteResult,
  deleteTargetSummary,
  fetchDeleteTargetSummary,
  deleteAllTransactions,
  clearDeleteResult,
  clearError,
} = useSettings();

type ModalState = "confirm" | "loading" | "success";
const modalState = ref<ModalState>("confirm");
const confirmText = ref("");
const REQUIRED_CONFIRM_TEXT = "DELETE";

const isConfirmTextValid = computed(
  () => confirmText.value === REQUIRED_CONFIRM_TEXT,
);

const modalTitle = computed(() => {
  switch (modalState.value) {
    case "confirm":
      return "全データの削除";
    case "loading":
      return "削除中...";
    case "success":
      return "削除完了";
    default:
      return "全データの削除";
  }
});

const formatDate = (dateStr: string | null) => {
  if (!dateStr) return "-";
  return new Date(dateStr).toLocaleDateString("ja-JP");
};

const handleDelete = async () => {
  if (!isConfirmTextValid.value) return;

  modalState.value = "loading";

  try {
    await deleteAllTransactions();
    modalState.value = "success";
  } catch (error) {
    modalState.value = "confirm";
  }
};

const handleClose = () => {
  clearDeleteResult();
  clearError();
  modalState.value = "confirm";
  confirmText.value = "";
  emit("close");
};

watch(
  () => props.isOpen,
  async (newValue) => {
    if (newValue) {
      // モーダルを開いた時に削除対象のサマリーを取得
      modalState.value = "confirm";
      confirmText.value = "";
      clearDeleteResult();
      clearError();
      await fetchDeleteTargetSummary();
    }
  },
);
</script>

<template>
  <BaseModal :is-open="isOpen" :title="modalTitle" @close="handleClose">
    <!-- 確認画面 -->
    <div v-if="modalState === 'confirm'" class="space-y-6">
      <!-- 警告メッセージ -->
      <BaseCard padding="sm" class="bg-red-50 border-2 border-red-300">
        <div class="flex items-start gap-3">
          <BaseIcon
            name="warning"
            size="lg"
            class="text-red-600 flex-shrink-0 mt-0.5"
            variant="solid"
          />
          <div class="space-y-2">
            <BaseText variant="body" weight="bold" class="text-red-900">
              この操作は取り消せません
            </BaseText>
            <BaseText variant="caption" class="text-red-800">
              実行すると、すべての取引データが完全に削除されます。
            </BaseText>
          </div>
        </div>
      </BaseCard>

      <!-- データエクスポート推奨 -->
      <BaseCard padding="sm" class="bg-blue-50 border border-blue-200">
        <div class="flex items-start gap-2">
          <BaseIcon
            name="info"
            size="sm"
            class="text-blue-600 flex-shrink-0 mt-0.5"
            variant="solid"
          />
          <div>
            <BaseText variant="caption" class="text-blue-800">
              削除前にデータをエクスポートすることを強く推奨します。
            </BaseText>
          </div>
        </div>
      </BaseCard>

      <!-- 削除対象データのサマリー -->
      <div v-if="deleteTargetSummary">
        <BaseText variant="body" weight="bold" class="mb-3">
          削除対象データ
        </BaseText>

        <BaseCard
          v-if="deleteTargetSummary.totalCount > 0"
          padding="sm"
          class="space-y-3"
        >
          <div class="flex items-start gap-2">
            <BaseIcon name="calendar" size="sm" class="text-gray-500 mt-0.5" />
            <div class="flex-1">
              <BaseText variant="caption" color="gray">期間</BaseText>
              <BaseText variant="body" weight="medium">
                {{ formatDate(deleteTargetSummary.oldestDate) }}
                〜
                {{ formatDate(deleteTargetSummary.latestDate) }}
              </BaseText>
            </div>
          </div>

          <div class="flex items-start gap-2">
            <BaseIcon
              name="clipboard-list"
              size="sm"
              class="text-gray-500 mt-0.5"
            />
            <div class="flex-1">
              <BaseText variant="caption" color="gray">取引件数</BaseText>
              <BaseText variant="body" weight="bold" class="text-red-600">
                {{ deleteTargetSummary.totalCount }}件の取引データを削除します
              </BaseText>
            </div>
          </div>
        </BaseCard>

        <BaseCard v-else padding="sm" class="bg-gray-50">
          <BaseText variant="body" color="gray" class="text-center">
            削除対象のデータがありません
          </BaseText>
        </BaseCard>
      </div>

      <!-- ローディング中（サマリー取得） -->
      <div v-else-if="isLoading" class="flex justify-center items-center py-8">
        <BaseSpinner
          icon="refresh"
          size="md"
          color="primary"
          label="データ確認中"
        />
      </div>

      <!-- 確認テキスト入力 -->
      <div
        v-if="
          deleteTargetSummary &&
          deleteTargetSummary.totalCount > 0 &&
          !isLoading
        "
        class="pt-4 border-t border-gray-200"
      >
        <BaseText variant="body" weight="bold" class="mb-2">
          確認のため、以下を入力してください
        </BaseText>
        <BaseText variant="caption" color="gray" class="mb-3">
          <code class="bg-gray-100 px-2 py-1 rounded font-mono text-red-600">{{
            REQUIRED_CONFIRM_TEXT
          }}</code>
          と入力してください
        </BaseText>
        <BaseInput
          v-model="confirmText"
          type="text"
          placeholder="DELETE"
          :disabled="isDeleting"
          :error="confirmText.length > 0 && !isConfirmTextValid"
        />
        <BaseText
          v-if="confirmText.length > 0 && !isConfirmTextValid"
          variant="caption"
          class="text-red-600 mt-1"
        >
          入力が正しくありません
        </BaseText>
      </div>

      <!-- エラーメッセージ -->
      <div
        v-if="errorMessage"
        class="p-3 bg-red-100 border border-red-400 rounded-lg"
      >
        <div class="flex items-start gap-2">
          <BaseIcon
            name="warning"
            size="sm"
            class="text-red-700 mt-0.5"
            variant="solid"
          />
          <BaseText variant="caption" class="text-red-700">
            {{ errorMessage }}
          </BaseText>
        </div>
      </div>
    </div>

    <!-- ローディング画面 -->
    <div v-else-if="modalState === 'loading'" class="text-center py-12">
      <BaseSpinner
        icon="settings"
        size="xl"
        color="primary"
        label="削除処理中"
        class="mb-4"
      />
      <BaseText variant="body" weight="bold" class="mb-2">
        削除処理中です...
      </BaseText>
      <BaseText variant="caption" color="gray">
        しばらくお待ちください
      </BaseText>
    </div>

    <!-- 完了画面 -->
    <div v-else-if="modalState === 'success' && deleteResult" class="space-y-6">
      <div class="text-center py-4">
        <BaseIcon
          name="check-circle"
          size="xl"
          class="mx-auto mb-4 text-green-500"
        />
        <BaseText variant="h3" class="mb-2">削除完了</BaseText>
      </div>

      <BaseCard padding="sm" class="space-y-3">
        <div class="flex items-start gap-2">
          <BaseIcon
            name="clipboard-list"
            size="sm"
            class="text-gray-500 mt-0.5"
          />
          <div class="flex-1">
            <BaseText variant="caption" color="gray">削除された取引</BaseText>
            <BaseText variant="body" weight="bold">
              {{ deleteResult.deletedTransactionCount }}件
            </BaseText>
          </div>
        </div>

        <div class="flex items-start gap-2">
          <BaseIcon name="document" size="sm" class="text-gray-500 mt-0.5" />
          <div class="flex-1">
            <BaseText variant="caption" color="gray">削除された明細</BaseText>
            <BaseText variant="body">
              {{ deleteResult.deletedTransactionItemCount }}件
            </BaseText>
          </div>
        </div>

        <div class="flex items-start gap-2">
          <BaseIcon name="photo" size="sm" class="text-gray-500 mt-0.5" />
          <div class="flex-1">
            <BaseText variant="caption" color="gray">削除された画像</BaseText>
            <BaseText variant="body">
              {{ deleteResult.deletedImageCount }}件
            </BaseText>
          </div>
        </div>

        <div
          v-if="deleteResult.failedImageCount > 0"
          class="pt-3 border-t border-gray-200"
        >
          <div class="flex items-start gap-2">
            <BaseIcon
              name="warning"
              size="sm"
              class="text-amber-600 mt-0.5"
              variant="solid"
            />
            <div class="flex-1">
              <BaseText variant="body" class="text-amber-800">
                {{ deleteResult.failedImageCount }}件の画像削除に失敗しました
              </BaseText>
              <BaseText variant="caption" color="gray" class="mt-1">
                データベースからは削除されています
              </BaseText>
            </div>
          </div>
        </div>
      </BaseCard>
    </div>

    <template #footer>
      <!-- 確認画面のフッター -->
      <div v-if="modalState === 'confirm'" class="flex gap-3">
        <BaseButton variant="outline" @click="handleClose" class="flex-1">
          キャンセル
        </BaseButton>
        <BaseButton
          variant="danger"
          :disabled="
            !isConfirmTextValid ||
            isDeleting ||
            isLoading ||
            !deleteTargetSummary ||
            deleteTargetSummary.totalCount === 0
          "
          @click="handleDelete"
          class="flex-1"
        >
          <span class="flex items-center justify-center gap-2">
            <BaseIcon name="trash" size="sm" />
            <span>完全に削除</span>
          </span>
        </BaseButton>
      </div>

      <!-- ローディング中はフッター非表示 -->
      <div v-else-if="modalState === 'loading'">
        <!-- 空 -->
      </div>

      <!-- 完了画面のフッター -->
      <div v-else-if="modalState === 'success'">
        <BaseButton variant="primary" @click="handleClose" class="w-full">
          閉じる
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>
