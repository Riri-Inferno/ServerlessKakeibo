<script setup lang="ts">
import { ref, watch, computed } from "vue";
import BaseModal from "../atoms/BaseModal.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import type { TransactionCategoryDto } from "../../types/transactionCategory";
import type { ItemCategoryDto } from "../../types/itemCategory";
import type { IncomeItemCategoryDto } from "../../types/incomeItemCategory";

type CategoryDto =
  | TransactionCategoryDto
  | ItemCategoryDto
  | IncomeItemCategoryDto;

type CategoryType = "transaction" | "item" | "incomeItem";

interface Props {
  isOpen: boolean;
  mode: "create" | "edit";
  categoryType: CategoryType;
  initialData?: CategoryDto | null;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
  save: [data: any];
}>();

// プリセット色
const presetColors = [
  "#FF5733", // 赤
  "#FF8C33", // オレンジ
  "#FFC107", // 黄色
  "#4CAF50", // 緑
  "#2196F3", // 青
  "#9C27B0", // 紫
  "#E91E63", // ピンク
  "#795548", // 茶色
  "#607D8B", // グレー
  "#000000", // 黒
];

// フォームデータ
const formData = ref({
  name: "",
  colorCode: presetColors[0],
  isIncome: false,
});

const isSaving = ref(false);
const validationError = ref("");

// モーダルタイトル
const modalTitle = computed(() => {
  const typeLabel = {
    transaction: "取引カテゴリ",
    item: "商品カテゴリ",
    incomeItem: "給与項目",
  }[props.categoryType];

  return props.mode === "create" ? `${typeLabel}を追加` : `${typeLabel}を編集`;
});

// 取引カテゴリかどうか
const isTransactionCategory = computed(
  () => props.categoryType === "transaction",
);

// 収入/支出選択を表示するか（取引カテゴリの新規作成時のみ）
const shouldShowIncomeToggle = computed(
  () => isTransactionCategory.value && props.mode === "create",
);

// 初期データの監視
watch(
  () => props.initialData,
  (newData) => {
    if (props.mode === "edit" && newData) {
      formData.value = {
        name: newData.name ?? "",
        colorCode: newData.colorCode ?? presetColors[0],
        isIncome: "isIncome" in newData ? newData.isIncome : false,
      };
    } else {
      // 作成モードの場合はリセット
      formData.value = {
        name: "",
        colorCode: presetColors[0],
        isIncome: false,
      };
    }
    validationError.value = "";
  },
  { immediate: true },
);

// バリデーション
const validate = (): boolean => {
  if (!formData.value.name.trim()) {
    validationError.value = "カテゴリ名を入力してください";
    return false;
  }
  if (formData.value.name.length > 100) {
    validationError.value = "カテゴリ名は100文字以内で入力してください";
    return false;
  }
  if (
    !/^#[0-9A-Fa-f]{6}$/.test(
      formData.value.colorCode ?? presetColors[8] ?? "#607D8B",
    )
  ) {
    validationError.value = "表示色が不正です";
    return false;
  }
  validationError.value = "";
  return true;
};

// 保存処理
const handleSave = async () => {
  if (!validate()) return;

  isSaving.value = true;

  try {
    emit("save", { ...formData.value });
    emit("close");
  } catch (error) {
    // エラーは親で処理される
  } finally {
    isSaving.value = false;
  }
};

// モーダルを閉じる
const handleClose = () => {
  if (!isSaving.value) {
    emit("close");
  }
};
</script>

<template>
  <BaseModal :is-open="isOpen" @close="handleClose">
    <div class="space-y-6">
      <!-- ヘッダー -->
      <div class="flex items-center justify-between">
        <BaseText variant="h2">{{ modalTitle }}</BaseText>
        <button
          @click="handleClose"
          :disabled="isSaving"
          class="text-gray-400 hover:text-gray-600 transition-colors"
        >
          <BaseIcon name="x-mark" size="md" />
        </button>
      </div>

      <!-- エラーメッセージ -->
      <div
        v-if="validationError"
        class="bg-red-50 border border-red-200 rounded-lg p-3"
      >
        <div class="flex items-center gap-2">
          <BaseIcon
            name="exclamation-triangle"
            size="sm"
            class="text-red-600"
          />
          <BaseText variant="body" class="text-red-800">
            {{ validationError }}
          </BaseText>
        </div>
      </div>

      <!-- フォーム -->
      <div class="space-y-4">
        <!-- カテゴリ名 -->
        <div>
          <BaseText variant="caption" color="gray" class="mb-2">
            カテゴリ名
          </BaseText>
          <BaseInput
            v-model="formData.name"
            placeholder="例: 食材費"
            :disabled="isSaving"
          />
        </div>

        <!-- 収入/支出選択（取引カテゴリの新規作成時のみ） -->
        <div v-if="shouldShowIncomeToggle">
          <BaseText variant="caption" color="gray" class="mb-2">
            種別
          </BaseText>
          <div class="flex gap-2">
            <button
              @click="formData.isIncome = false"
              :disabled="isSaving"
              class="flex-1 py-2 px-4 rounded-lg border transition-colors"
              :class="
                !formData.isIncome
                  ? 'border-blue-500 bg-blue-50 text-blue-700'
                  : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
              "
            >
              支出
            </button>
            <button
              @click="formData.isIncome = true"
              :disabled="isSaving"
              class="flex-1 py-2 px-4 rounded-lg border transition-colors"
              :class="
                formData.isIncome
                  ? 'border-green-500 bg-green-50 text-green-700'
                  : 'border-gray-300 bg-white text-gray-700 hover:bg-gray-50'
              "
            >
              収入
            </button>
          </div>
        </div>

        <!-- 色選択 -->
        <div>
          <BaseText variant="caption" color="gray" class="mb-2">
            表示色
          </BaseText>
          <div class="grid grid-cols-5 sm:grid-cols-10 gap-2">
            <button
              v-for="color in presetColors"
              :key="color"
              @click="formData.colorCode = color"
              :disabled="isSaving"
              :style="{ backgroundColor: color }"
              class="w-10 h-10 rounded-lg border-2 transition-all hover:scale-110"
              :class="
                formData.colorCode === color
                  ? 'border-gray-900 shadow-lg'
                  : 'border-gray-300'
              "
            />
          </div>

          <!-- 選択中の色プレビュー -->
          <div class="flex items-center gap-2 mt-3">
            <BaseText variant="caption" color="gray">選択中:</BaseText>
            <div
              :style="{ backgroundColor: formData.colorCode }"
              class="w-8 h-8 rounded border border-gray-300"
            />
            <BaseText variant="caption" color="gray">
              {{ formData.colorCode }}
            </BaseText>
          </div>
        </div>
      </div>

      <!-- アクションボタン -->
      <div class="flex gap-3 pt-4 border-t border-gray-200">
        <BaseButton
          variant="outline"
          @click="handleClose"
          :disabled="isSaving"
          class="flex-1"
        >
          キャンセル
        </BaseButton>
        <BaseButton
          variant="primary"
          @click="handleSave"
          :disabled="isSaving"
          class="flex-1"
        >
          <span class="flex items-center justify-center gap-2">
            <BaseSpinner
              v-if="isSaving"
              icon="refresh"
              size="sm"
              color="white"
            />
            <BaseIcon v-else name="check" size="sm" />
            <span>{{ isSaving ? "保存中..." : "保存" }}</span>
          </span>
        </BaseButton>
      </div>
    </div>
  </BaseModal>
</template>
