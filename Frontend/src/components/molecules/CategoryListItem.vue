<script setup lang="ts">
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseButton from "../atoms/BaseButton.vue";
import type { TransactionCategoryDto } from "../../types/transactionCategory";
import type { ItemCategoryDto } from "../../types/itemCategory";
import type { IncomeItemCategoryDto } from "../../types/incomeItemCategory";

// 3種類のカテゴリすべてに対応
type CategoryDto =
  | TransactionCategoryDto
  | ItemCategoryDto
  | IncomeItemCategoryDto;

defineProps<{
  category: CategoryDto;
}>();

defineEmits<{
  edit: [id: string];
  delete: [id: string];
}>();
</script>

<template>
  <div
    class="flex items-center gap-3 p-3 bg-white border border-gray-200 rounded-lg hover:bg-gray-50 transition-colors"
  >
    <!-- ドラッグハンドル -->
    <div class="cursor-move text-gray-400 hover:text-gray-600">
      <BaseIcon name="bars-3" size="md" />
    </div>

    <!-- 色丸 -->
    <div
      :style="{ backgroundColor: category.colorCode }"
      class="w-6 h-6 rounded-full border border-gray-200 flex-shrink-0"
    />

    <!-- カテゴリ名 -->
    <div class="flex-1 min-w-0">
      <BaseText variant="body" weight="medium" class="truncate">
        {{ category.name }}
      </BaseText>
      <BaseText
        v-if="category.isCustom"
        variant="caption"
        color="gray"
        class="flex items-center gap-1"
      >
        <BaseIcon name="tag" size="sm" />
        <span>カスタム</span>
      </BaseText>
    </div>

    <!-- 収入/支出バッジ（TransactionCategoryのみ） -->
    <div
      v-if="'isIncome' in category"
      class="px-2 py-1 rounded text-xs font-medium"
      :class="
        category.isIncome
          ? 'bg-green-100 text-green-800'
          : 'bg-blue-100 text-blue-800'
      "
    >
      {{ category.isIncome ? "収入" : "支出" }}
    </div>

    <!-- アクションボタン -->
    <div class="flex items-center gap-2 flex-shrink-0">
      <BaseButton variant="text" size="sm" @click="$emit('edit', category.id)">
        <span class="flex items-center gap-1 text-blue-600">
          <BaseIcon name="pencil" size="sm" />
          <span class="hidden sm:inline">編集</span>
        </span>
      </BaseButton>

      <BaseButton
        variant="text"
        size="sm"
        @click="$emit('delete', category.id)"
      >
        <span class="flex items-center gap-1 text-red-600">
          <BaseIcon name="trash" size="sm" />
          <span class="hidden sm:inline">削除</span>
        </span>
      </BaseButton>
    </div>
  </div>
</template>
