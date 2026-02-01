<script setup lang="ts">
import { ref } from "vue";
import {
  TransactionType,
  //   TransactionCategory,
  CategoryLabels,
} from "../../types/transaction";
import type { GetTransactionsRequest } from "../../types/transaction";
import BaseCard from "../atoms/BaseCard.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseInputNumber from "../atoms/BaseInputNumber.vue";
import BaseSelect from "../atoms/BaseSelect.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseIcon from "../atoms/BaseIcon.vue";

const emit = defineEmits<{
  search: [filters: GetTransactionsRequest];
  clear: [];
}>();

const isExpanded = ref(false);

const filters = ref<GetTransactionsRequest>({
  startDate: undefined,
  endDate: undefined,
  category: undefined,
  payee: undefined,
  minAmount: undefined,
  maxAmount: undefined,
  type: undefined,
});

const typeOptions = [
  { value: TransactionType.Income, label: "収入" },
  { value: TransactionType.Expense, label: "支出" },
];

const categoryOptions = Object.entries(CategoryLabels).map(([key, label]) => ({
  value: key,
  label,
}));

const handleSearch = () => {
  const searchParams: GetTransactionsRequest = {};

  if (filters.value.startDate) searchParams.startDate = filters.value.startDate;
  if (filters.value.endDate) searchParams.endDate = filters.value.endDate;
  if (filters.value.category) searchParams.category = filters.value.category;
  if (filters.value.payee) searchParams.payee = filters.value.payee;
  if (
    filters.value.minAmount !== undefined &&
    filters.value.minAmount !== null
  ) {
    searchParams.minAmount = filters.value.minAmount;
  }
  if (
    filters.value.maxAmount !== undefined &&
    filters.value.maxAmount !== null
  ) {
    searchParams.maxAmount = filters.value.maxAmount;
  }
  if (filters.value.type) searchParams.type = filters.value.type;

  emit("search", searchParams);
};

const handleClear = () => {
  filters.value = {
    startDate: undefined,
    endDate: undefined,
    category: undefined,
    payee: undefined,
    minAmount: undefined,
    maxAmount: undefined,
    type: undefined,
  };
  emit("clear");
};

const toggleExpand = () => {
  isExpanded.value = !isExpanded.value;
};
</script>

<template>
  <BaseCard padding="md">
    <div class="space-y-4">
      <div class="flex items-center justify-between">
        <BaseText variant="h3">検索条件</BaseText>
        <button
          @click="toggleExpand"
          class="p-2 hover:bg-gray-100 rounded-lg transition-colors tap-transparent"
          aria-label="検索条件を展開"
        >
          <BaseIcon
            :name="isExpanded ? 'chevron-up' : 'chevron-down'"
            size="md"
          />
        </button>
      </div>

      <div v-show="isExpanded" class="space-y-4">
        <!-- 日付範囲：モバイルは縦並び、タブレット以上は横並び -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1">
              開始日
            </BaseText>
            <BaseInput v-model="filters.startDate" type="date" size="md" />
          </div>

          <div>
            <BaseText variant="caption" color="gray" class="mb-1">
              終了日
            </BaseText>
            <BaseInput v-model="filters.endDate" type="date" size="md" />
          </div>
        </div>

        <div>
          <BaseText variant="caption" color="gray" class="mb-1">
            支払先
          </BaseText>
          <BaseInput
            v-model="filters.payee"
            type="text"
            placeholder="店舗名など"
            size="md"
          />
        </div>

        <!-- 種別・カテゴリ：モバイルは縦並び、タブレット以上は横並び -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1">
              取引種別
            </BaseText>
            <BaseSelect
              v-model="filters.type"
              :options="typeOptions"
              placeholder="すべて"
              size="md"
            />
          </div>

          <div>
            <BaseText variant="caption" color="gray" class="mb-1">
              カテゴリ
            </BaseText>
            <BaseSelect
              v-model="filters.category"
              :options="categoryOptions"
              placeholder="すべて"
              size="md"
            />
          </div>
        </div>

        <!-- 金額範囲：モバイルは縦並び、タブレット以上は横並び -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1">
              最小金額
            </BaseText>
            <BaseInputNumber
              v-model="filters.minAmount"
              placeholder="0"
              size="md"
            />
          </div>

          <div>
            <BaseText variant="caption" color="gray" class="mb-1">
              最大金額
            </BaseText>
            <BaseInputNumber
              v-model="filters.maxAmount"
              placeholder="999999999"
              size="md"
            />
          </div>
        </div>

        <!-- ボタン：モバイルは縦並び、タブレット以上は横並び -->
        <div class="flex flex-col sm:flex-row gap-2">
          <BaseButton
            variant="primary"
            class="w-full sm:flex-1"
            @click="handleSearch"
          >
            検索
          </BaseButton>
          <BaseButton
            variant="outline"
            class="w-full sm:w-auto sm:flex-shrink-0"
            @click="handleClear"
          >
            クリア
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseCard>
</template>
