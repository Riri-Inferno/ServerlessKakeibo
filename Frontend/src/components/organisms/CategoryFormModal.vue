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

// カスタム色入力
const customColorInput = ref("");

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

// isOpenの変化を監視してフォームをリセット（タブ切り替え時の状態共有を防ぐ）
watch(
  () => props.isOpen,
  (newIsOpen) => {
    if (!newIsOpen) {
      // モーダルが閉じられた時はリセットしない（閉じるアニメーション中）
      return;
    }

    // モーダルが開かれた時にリセット
    if (props.mode === "edit" && props.initialData) {
      formData.value = {
        name: props.initialData.name ?? "",
        colorCode: props.initialData.colorCode ?? presetColors[0],
        isIncome:
          "isIncome" in props.initialData ? props.initialData.isIncome : false,
      };
      customColorInput.value = "";
    } else {
      // 作成モードの場合は完全リセット
      formData.value = {
        name: "",
        colorCode: presetColors[0],
        isIncome: false,
      };
      customColorInput.value = "";
    }
    validationError.value = "";
  },
  { immediate: true },
);

// カスタム色を適用
const applyCustomColor = () => {
  const color = customColorInput.value.trim();
  if (/^#[0-9A-Fa-f]{6}$/.test(color)) {
    formData.value.colorCode = color;
    validationError.value = "";
  } else {
    validationError.value = "カラーコードは #RRGGBB 形式で入力してください";
  }
};

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
      <div>
        <BaseText variant="h2">{{ modalTitle }}</BaseText>
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

          <!-- プリセット色 -->
          <div class="grid grid-cols-5 sm:grid-cols-10 gap-2 mb-3">
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

          <!-- カスタム色入力 -->
          <div class="space-y-2">
            <BaseText variant="caption" color="gray">
              カスタムカラー（#RRGGBB形式）
            </BaseText>
            <div class="flex gap-2">
              <BaseInput
                v-model="customColorInput"
                placeholder="#FF5733"
                :disabled="isSaving"
                class="flex-1"
              />
              <BaseButton
                variant="outline"
                size="sm"
                @click="applyCustomColor"
                :disabled="isSaving"
              >
                適用
              </BaseButton>
            </div>
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
