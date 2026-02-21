<script setup lang="ts">
import { computed } from "vue";
import type { CreateTransactionItem } from "../../types/transaction";
import type { ItemCategoryDto } from "../../types/itemCategory";
import { TransactionType } from "../../types/transaction";
import BaseText from "../atoms/BaseText.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseInputNumber from "../atoms/BaseInputNumber.vue";
import BaseSelect from "../atoms/BaseSelect.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseCard from "../atoms/BaseCard.vue";

interface Props {
  items: CreateTransactionItem[];
  itemCategories: ItemCategoryDto[];
  transactionType: TransactionType;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  "update:items": [value: CreateTransactionItem[]];
  add: [];
  remove: [index: number];
}>();

// カテゴリオプションを動的生成
const itemCategoryOptions = computed(() =>
  props.itemCategories.map((cat) => ({
    value: cat.id,
    label: cat.name,
  })),
);

const updateItem = (
  index: number,
  field: keyof CreateTransactionItem,
  value: any,
) => {
  const newItems = [...props.items];
  const currentItem = newItems[index];

  if (!currentItem) return;

  newItems[index] = {
    ...currentItem,
    [field]: value,
  };

  // カテゴリ更新時は type に応じて振り分け
  if (field === "userItemCategoryId" || field === "userIncomeItemCategoryId") {
    if (props.transactionType === TransactionType.Income) {
      newItems[index].userIncomeItemCategoryId = value;
      newItems[index].userItemCategoryId = null;
    } else {
      newItems[index].userItemCategoryId = value;
      newItems[index].userIncomeItemCategoryId = null;
    }
  }

  if (field === "quantity" || field === "unitPrice") {
    const quantity = field === "quantity" ? value : newItems[index].quantity;
    const unitPrice = field === "unitPrice" ? value : newItems[index].unitPrice;

    if (quantity && unitPrice) {
      newItems[index].amount = quantity * unitPrice;
    }
  }

  emit("update:items", newItems);
};

const itemTotal = computed(() => {
  return props.items.reduce((sum, item) => sum + item.amount, 0);
});
</script>

<template>
  <div class="space-y-3 md:space-y-4">
    <div class="flex items-center justify-between">
      <BaseText variant="h3">明細</BaseText>
    </div>

    <div
      v-if="items.length === 0"
      class="text-center py-6 md:py-8 border-2 border-dashed border-gray-300 rounded-lg"
    >
      <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
        <span class="md:hidden">明細を1つ以上追加してください</span>
        <span class="hidden md:inline">明細がありません。追加ボタンから明細を追加できます。</span>
      </BaseText>
    </div>

    <div v-else class="space-y-2 md:space-y-3">
      <BaseCard v-for="(item, index) in items" :key="index" padding="sm" class="p-2.5 md:p-3">
        <div class="space-y-2 md:space-y-3">
          <div class="flex items-start justify-between gap-2">
            <div class="flex-1 min-w-0">
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >商品名</BaseText
              >
              <BaseInput
                :model-value="item.name"
                @update:model-value="
                  updateItem(index, 'name', $event as string)
                "
                type="text"
                placeholder="商品名"
                size="sm"
              />
            </div>
            <button
              @click="emit('remove', index)"
              class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors flex-shrink-0"
            >
              <BaseIcon name="trash" size="sm" />
            </button>
          </div>

          <div class="grid grid-cols-3 gap-1.5 md:gap-2">
            <div>
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >数量</BaseText
              >
              <BaseInput
                :model-value="String(item.quantity)"
                @update:model-value="
                  updateItem(
                    index,
                    'quantity',
                    parseFloat($event as string) || 1,
                  )
                "
                type="text"
                placeholder="1"
                size="sm"
              />
            </div>
            <div>
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >単価</BaseText
              >
              <BaseInputNumber
                :model-value="item.unitPrice"
                @update:model-value="
                  updateItem(index, 'unitPrice', $event as number | null)
                "
                size="sm"
              />
            </div>
            <div>
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >金額</BaseText
              >
              <BaseInputNumber
                :model-value="item.amount"
                @update:model-value="
                  updateItem(index, 'amount', ($event as number) || 0)
                "
                size="sm"
              />
            </div>
          </div>

          <div>
            <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
              >カテゴリ</BaseText
            >
            <BaseSelect
              :model-value="
                transactionType === TransactionType.Income
                  ? item.userIncomeItemCategoryId || ''
                  : item.userItemCategoryId || ''
              "
              @update:model-value="
                updateItem(
                  index,
                  transactionType === TransactionType.Income
                    ? 'userIncomeItemCategoryId'
                    : 'userItemCategoryId',
                  $event ? ($event as string) : null,
                )
              "
              :options="itemCategoryOptions"
              placeholder="選択してください"
              size="sm"
            />
          </div>
        </div>
      </BaseCard>

      <div class="flex justify-end pt-2 border-t border-gray-200">
        <BaseText variant="body" weight="bold" class="text-sm md:text-base">
          小計: {{ itemTotal.toLocaleString() }}円
        </BaseText>
      </div>
    </div>

    <div class="pt-2 md:pt-3">
      <BaseButton
        variant="outline"
        size="sm"
        @click="emit('add')"
        class="w-full"
      >
        <span class="flex items-center justify-center gap-1.5">
          <BaseIcon name="plus" size="sm" />
          <span class="text-sm md:text-base">明細を追加</span>
        </span>
      </BaseButton>
    </div>
  </div>
</template>
