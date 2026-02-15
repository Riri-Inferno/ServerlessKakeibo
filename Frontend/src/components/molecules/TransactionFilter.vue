<script setup lang="ts">
import { ref, computed } from "vue";
import { TransactionType } from "../../types/transaction";
import type { GetTransactionsRequest } from "../../types/transaction";
import { useTransactionCategories } from "../../composables/useTransactionCategories";
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

// カスタムカテゴリ取得
const { expenseCategories, incomeCategories, fetchCategories } =
  useTransactionCategories();

// 初回読み込み
fetchCategories(false);

const filters = ref<GetTransactionsRequest>({
  startDate: undefined,
  endDate: undefined,
  userTransactionCategoryId: undefined,
  payer: undefined,
  payee: undefined,
  minAmount: undefined,
  maxAmount: undefined,
  type: undefined,
});

const typeOptions = [
  // { value: "", label: "すべて" },
  { value: TransactionType.Income, label: "収入" },
  { value: TransactionType.Expense, label: "支出" },
];

// type に応じてカテゴリオプションを切り替え
const categoryOptions = computed(() => {
  if (filters.value.type === TransactionType.Income) {
    return incomeCategories.value.map((cat) => ({
      value: cat.id,
      label: cat.name,
    }));
  } else if (filters.value.type === TransactionType.Expense) {
    return expenseCategories.value.map((cat) => ({
      value: cat.id,
      label: cat.name,
    }));
  }

  // type が未選択の場合は両方を表示
  return [
    ...expenseCategories.value.map((cat) => ({
      value: cat.id,
      label: `${cat.name}（支出）`,
    })),
    ...incomeCategories.value.map((cat) => ({
      value: cat.id,
      label: `${cat.name}（収入）`,
    })),
  ];
});

// type に応じたラベル
const payerPayeeLabel = computed(() => {
  if (filters.value.type === TransactionType.Income) {
    return "支払元（勤務先など）";
  } else if (filters.value.type === TransactionType.Expense) {
    return "支払先（店舗名など）";
  }
  return "支払元/支払先";
});

const payerPayeePlaceholder = computed(() => {
  if (filters.value.type === TransactionType.Income) {
    return "会社名など";
  } else if (filters.value.type === TransactionType.Expense) {
    return "店舗名など";
  }
  return "会社名や店舗名など";
});

const handleSearch = () => {
  const searchParams: GetTransactionsRequest = {};

  if (filters.value.startDate) searchParams.startDate = filters.value.startDate;
  if (filters.value.endDate) searchParams.endDate = filters.value.endDate;

  // カスタムカテゴリID
  if (filters.value.userTransactionCategoryId) {
    searchParams.userTransactionCategoryId =
      filters.value.userTransactionCategoryId;
  }

  // type に応じて payer/payee を振り分け
  if (filters.value.type === TransactionType.Income && filters.value.payer) {
    searchParams.payer = filters.value.payer;
  } else if (
    filters.value.type === TransactionType.Expense &&
    filters.value.payee
  ) {
    searchParams.payee = filters.value.payee;
  } else if (!filters.value.type) {
    // type 未選択の場合は両方検索
    if (filters.value.payer) searchParams.payer = filters.value.payer;
    if (filters.value.payee) searchParams.payee = filters.value.payee;
  }

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
    userTransactionCategoryId: undefined,
    payer: undefined,
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

        <!-- 種別・カテゴリの順序を入れ替え（種別を先に） -->
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
              v-model="filters.userTransactionCategoryId"
              :options="categoryOptions"
              placeholder="すべて"
              size="md"
            />
          </div>
        </div>

        <!-- 支払元/支払先を type に応じて表示 -->
        <div>
          <BaseText variant="caption" color="gray" class="mb-1">
            {{ payerPayeeLabel }}
          </BaseText>
          <BaseInput
            v-if="filters.type === TransactionType.Income"
            v-model="filters.payer"
            type="text"
            :placeholder="payerPayeePlaceholder"
            size="md"
          />
          <BaseInput
            v-else-if="filters.type === TransactionType.Expense"
            v-model="filters.payee"
            type="text"
            :placeholder="payerPayeePlaceholder"
            size="md"
          />
          <BaseInput
            v-else
            v-model="filters.payee"
            type="text"
            :placeholder="payerPayeePlaceholder"
            size="md"
          />
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
