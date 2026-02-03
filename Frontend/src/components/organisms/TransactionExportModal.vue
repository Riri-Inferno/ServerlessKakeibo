<script setup lang="ts">
import { ref, computed, watch } from "vue";
import { useTransactionExport } from "../../composables/useTransactionExport";
import { CategoryLabels } from "../../types/transaction";
import type { GetTransactionsRequest } from "../../types/transaction";
import BaseModal from "../atoms/BaseModal.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import LabeledCheckbox from "../molecules/LabeledCheckbox.vue";
import BaseCard from "../atoms/BaseCard.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";

interface Props {
  isOpen: boolean;
  filters: GetTransactionsRequest;
  totalCount?: number;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
}>();

const {
  isExporting,
  errorMessage,
  exportResult,
  exportTransactions,
  clearResult,
} = useTransactionExport();

const includeImages = ref(true);

type ModalState = "confirm" | "loading" | "success";
const modalState = ref<ModalState>("confirm");

const modalTitle = computed(() => {
  switch (modalState.value) {
    case "confirm":
      return "取引をエクスポート";
    case "loading":
      return "エクスポート中...";
    case "success":
      return "エクスポート完了";
    default:
      return "エクスポート";
  }
});

const formatDateRange = () => {
  const start = props.filters.startDate
    ? new Date(props.filters.startDate).toLocaleDateString("ja-JP")
    : "指定なし";
  const end = props.filters.endDate
    ? new Date(props.filters.endDate).toLocaleDateString("ja-JP")
    : "指定なし";
  return `${start} 〜 ${end}`;
};

const getCategoryLabel = () => {
  if (!props.filters.category) return "すべて";
  return CategoryLabels[props.filters.category] || props.filters.category;
};

const getTypeLabel = () => {
  if (!props.filters.type) return "すべて";
  return props.filters.type === "Income" ? "収入" : "支出";
};

const handleExport = async () => {
  modalState.value = "loading";

  const request = {
    ...props.filters,
    includeReceiptImages: includeImages.value,
  };

  const success = await exportTransactions(request);

  if (success) {
    modalState.value = "success";
  } else {
    modalState.value = "confirm";
  }
};

const handleClose = () => {
  clearResult();
  modalState.value = "confirm";
  emit("close");
};

watch(
  () => props.isOpen,
  (newValue) => {
    if (!newValue) {
      clearResult();
      modalState.value = "confirm";
    }
  },
);
</script>

<template>
  <BaseModal :is-open="isOpen" :title="modalTitle" @close="handleClose">
    <!-- 確認画面 -->
    <div v-if="modalState === 'confirm'" class="space-y-6">
      <div>
        <BaseText variant="body" class="mb-4">
          以下の条件でエクスポートします:
        </BaseText>

        <BaseCard padding="sm" class="space-y-3">
          <div class="flex items-start gap-2">
            <BaseIcon name="calendar" size="sm" class="text-gray-500 mt-0.5" />
            <div class="flex-1">
              <BaseText variant="caption" color="gray">期間</BaseText>
              <BaseText variant="body">{{ formatDateRange() }}</BaseText>
            </div>
          </div>

          <div class="flex items-start gap-2">
            <BaseIcon name="tag" size="sm" class="text-gray-500 mt-0.5" />
            <div class="flex-1">
              <BaseText variant="caption" color="gray">カテゴリ</BaseText>
              <BaseText variant="body">{{ getCategoryLabel() }}</BaseText>
            </div>
          </div>

          <div class="flex items-start gap-2">
            <BaseIcon name="banknotes" size="sm" class="text-gray-500 mt-0.5" />
            <div class="flex-1">
              <BaseText variant="caption" color="gray">取引種別</BaseText>
              <BaseText variant="body">{{ getTypeLabel() }}</BaseText>
            </div>
          </div>

          <div v-if="totalCount !== undefined" class="flex items-start gap-2">
            <BaseIcon
              name="clipboard-list"
              size="sm"
              class="text-gray-500 mt-0.5"
            />
            <div class="flex-1">
              <BaseText variant="caption" color="gray">取引件数</BaseText>
              <BaseText variant="body" weight="bold"
                >{{ totalCount }}件</BaseText
              >
            </div>
          </div>
        </BaseCard>
      </div>

      <div class="pt-4 border-t border-gray-200">
        <LabeledCheckbox
          v-model="includeImages"
          label="レシート画像を含める"
          :description="
            totalCount
              ? `画像が添付されている取引のみZipに同梱されます`
              : undefined
          "
        />
      </div>

      <BaseCard padding="sm" class="bg-yellow-50 border border-yellow-200">
        <div class="flex items-start gap-2 mb-2">
          <BaseIcon
            name="warning"
            size="sm"
            class="text-yellow-600"
            variant="solid"
          />
          <BaseText variant="caption" weight="bold" class="text-yellow-800">
            注意事項
          </BaseText>
        </div>
        <ul class="space-y-1 text-sm text-gray-600 ml-6">
          <li>• 画像を含める場合、処理に時間がかかることがあります</li>
          <li>• ファイルサイズは最大10MB程度になります</li>
          <li>• 現在のフィルタ条件が適用されます</li>
        </ul>
      </BaseCard>

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
        label="エクスポート処理中"
        class="mb-4"
      />
      <BaseText variant="body" weight="bold" class="mb-2">
        処理中です...
      </BaseText>
      <BaseText variant="caption" color="gray">
        しばらくお待ちください
      </BaseText>
    </div>

    <!-- 完了画面 -->
    <div v-else-if="modalState === 'success' && exportResult" class="space-y-6">
      <div class="text-center py-4">
        <BaseIcon
          name="check-circle"
          size="xl"
          class="mx-auto mb-4 text-green-500"
        />
        <BaseText variant="h3" class="mb-2">エクスポート完了！</BaseText>
      </div>

      <BaseCard padding="sm" class="space-y-3">
        <div class="flex items-start gap-2">
          <BaseIcon name="document" size="sm" class="text-gray-500 mt-0.5" />
          <div class="flex-1">
            <BaseText variant="caption" color="gray">ファイル名</BaseText>
            <BaseText variant="body" weight="bold">
              {{ exportResult.fileName }}
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
            <BaseText variant="caption" color="gray">エクスポート件数</BaseText>
            <BaseText variant="body">
              {{ exportResult.totalCount }}件の取引
            </BaseText>
          </div>
        </div>

        <div v-if="includeImages" class="flex items-start gap-2">
          <BaseIcon name="photo" size="sm" class="text-gray-500 mt-0.5" />
          <div class="flex-1">
            <BaseText variant="caption" color="gray">画像</BaseText>
            <BaseText variant="body">
              {{ exportResult.imagesIncludedCount }}件の画像を含めました
            </BaseText>
          </div>
        </div>

        <div
          v-if="exportResult.imagesFailedCount > 0"
          class="pt-3 border-t border-gray-200"
        >
          <div class="flex items-start gap-2">
            <BaseIcon
              name="warning"
              size="sm"
              class="text-red-500 mt-0.5"
              variant="solid"
            />
            <BaseText variant="body" class="text-red-600">
              {{ exportResult.imagesFailedCount }}件の画像を取得できませんでした
            </BaseText>
          </div>
        </div>
      </BaseCard>

      <BaseCard padding="sm" class="bg-blue-50 border border-blue-200">
        <div class="flex items-start gap-2">
          <BaseIcon name="info" size="sm" class="text-blue-600 mt-0.5" />
          <BaseText variant="caption" class="text-blue-800">
            ファイルはダウンロードフォルダに保存されました。
          </BaseText>
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
          variant="primary"
          :disabled="isExporting"
          @click="handleExport"
          class="flex-1"
        >
          <span class="flex items-center justify-center gap-2">
            <BaseIcon name="download" size="sm" />
            <span>エクスポート実行</span>
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
