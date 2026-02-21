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
const { categories, fetchCategories } = useTransactionCategories();

// 初回読み込み
fetchCategories(true);

const filters = ref({
  startDate: undefined,
  endDate: undefined,
  userTransactionCategoryId: null as string | null,
  payer: undefined,
  payee: undefined,
  payerOrPayee: undefined,
  minAmount: undefined,
  maxAmount: undefined,
  type: null as TransactionType | null,
});

const typeOptions = [
  { value: TransactionType.Income, label: "収入" },
  { value: TransactionType.Expense, label: "支出" },
];

// type に応じてカテゴリオプションを切り替え
const categoryOptions = computed(() => {
  if (filters.value.type === TransactionType.Income) {
    return categories.value
      .filter((cat) => cat.isIncome)
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map((cat) => ({
        value: cat.id,
        label: cat.name,
      }));
  } else if (filters.value.type === TransactionType.Expense) {
    return categories.value
      .filter((cat) => !cat.isIncome)
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map((cat) => ({
        value: cat.id,
        label: cat.name,
      }));
  }

  // type 未選択: 両方表示
  return [
    ...categories.value
      .filter((cat) => !cat.isIncome)
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map((cat) => ({
        value: cat.id,
        label: `${cat.name}（支出）`,
      })),
    ...categories.value
      .filter((cat) => cat.isIncome)
      .sort((a, b) => a.displayOrder - b.displayOrder)
      .map((cat) => ({
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
  if (!filters.value.type && filters.value.payerOrPayee) {
    // type未選択: 両方検索
    searchParams.payer = filters.value.payerOrPayee;
    searchParams.payee = filters.value.payerOrPayee;
  } else if (
    filters.value.type === TransactionType.Income &&
    filters.value.payer
  ) {
    searchParams.payer = filters.value.payer;
  } else if (
    filters.value.type === TransactionType.Expense &&
    filters.value.payee
  ) {
    searchParams.payee = filters.value.payee;
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
    userTransactionCategoryId: null,
    payer: undefined,
    payee: undefined,
    payerOrPayee: undefined,
    minAmount: undefined,
    maxAmount: undefined,
    type: null,
  };
  emit("clear");
};

const toggleExpand = () => {
  isExpanded.value = !isExpanded.value;
};
</script>

<template>
  <BaseCard class="p-3 md:p-4 lg:p-6">
    <div class="space-y-3 md:space-y-4">
      <div class="flex items-center justify-between">
        <BaseText variant="h3" class="text-base md:text-lg">検索条件</BaseText>
        <button
          @click="toggleExpand"
          class="p-1.5 md:p-2 hover:bg-gray-100 rounded-lg transition-colors tap-transparent"
          aria-label="検索条件を展開"
        >
          <BaseIcon
            :name="isExpanded ? 'chevron-up' : 'chevron-down'"
            size="md"
          />
        </button>
      </div>

      <div v-show="isExpanded" class="space-y-3 md:space-y-4">
        <!-- 日付範囲 -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 md:gap-4">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
              開始日
            </BaseText>
            <BaseInput v-model="filters.startDate" type="date" size="md" />
          </div>

          <div>
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
              終了日
            </BaseText>
            <BaseInput v-model="filters.endDate" type="date" size="md" />
          </div>
        </div>

        <!-- 種別・カテゴリ -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 md:gap-4">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
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
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
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

        <!-- 支払元/支払先を type に応じて切り替え -->
        <div>
          <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
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
            v-model="filters.payerOrPayee"
            type="text"
            placeholder="会社名や店舗名など"
            size="md"
          />
        </div>

        <!-- 金額範囲 -->
        <div class="grid grid-cols-1 sm:grid-cols-2 gap-3 md:gap-4">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
              最小金額
            </BaseText>
            <BaseInputNumber
              v-model="filters.minAmount"
              placeholder="0"
              size="md"
            />
          </div>

          <div>
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm">
              最大金額
            </BaseText>
            <BaseInputNumber
              v-model="filters.maxAmount"
              placeholder="999999999"
              size="md"
            />
          </div>
        </div>

        <!-- ボタン -->
        <div class="flex flex-col sm:flex-row gap-2">
          <BaseButton
            variant="primary"
            class="w-full sm:flex-1"
            @click="handleSearch"
          >
            <span class="text-sm md:text-base">検索</span>
          </BaseButton>
          <BaseButton
            variant="outline"
            class="w-full sm:w-auto sm:flex-shrink-0"
            @click="handleClear"
          >
            <span class="text-sm md:text-base">クリア</span>
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseCard>
</template>
