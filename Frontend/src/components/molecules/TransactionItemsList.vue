<script setup lang="ts">
import { computed } from "vue";
import type { CreateTransactionItem } from "../../types/transaction";
import { ItemCategoryLabels } from "../../types/receipt";
import BaseText from "../atoms/BaseText.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseInputNumber from "../atoms/BaseInputNumber.vue";
import BaseSelect from "../atoms/BaseSelect.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseCard from "../atoms/BaseCard.vue";

interface Props {
  items: CreateTransactionItem[];
}

const props = defineProps<Props>();

const emit = defineEmits<{
  "update:items": [value: CreateTransactionItem[]];
  add: [];
  remove: [index: number];
}>();

const itemCategoryOptions = Object.entries(ItemCategoryLabels).map(
  ([key, label]) => ({
    value: key,
    label,
  })
);

const updateItem = (
  index: number,
  field: keyof CreateTransactionItem,
  value: any
) => {
  const newItems = [...props.items];
  const currentItem = newItems[index];

  if (!currentItem) return;

  newItems[index] = {
    name: field === "name" ? value : currentItem.name,
    quantity: field === "quantity" ? value : currentItem.quantity,
    unitPrice: field === "unitPrice" ? value : currentItem.unitPrice,
    amount: field === "amount" ? value : currentItem.amount,
    category: field === "category" ? value : currentItem.category,
  };

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
  <div class="space-y-4">
    <div class="flex items-center justify-between">
      <BaseText variant="h3">明細</BaseText>
      <BaseButton variant="outline" size="sm" @click="emit('add')">
        <BaseIcon name="plus" size="sm" class="mr-1" />
        追加
      </BaseButton>
    </div>

    <div
      v-if="items.length === 0"
      class="text-center py-8 border-2 border-dashed border-gray-300 rounded-lg"
    >
      <BaseText variant="caption" color="gray">
        明細がありません。追加ボタンから明細を追加できます。
      </BaseText>
    </div>

    <div v-else class="space-y-3">
      <BaseCard v-for="(item, index) in items" :key="index" padding="sm">
        <div class="space-y-3">
          <div class="flex items-start justify-between gap-2">
            <div class="flex-1">
              <BaseText variant="caption" color="gray" class="mb-1"
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
              class="p-2 text-red-600 hover:bg-red-50 rounded-lg transition-colors"
            >
              <BaseIcon name="trash" size="sm" />
            </button>
          </div>

          <div class="grid grid-cols-3 gap-2">
            <div>
              <BaseText variant="caption" color="gray" class="mb-1"
                >数量</BaseText
              >
              <BaseInput
                :model-value="String(item.quantity)"
                @update:model-value="
                  updateItem(
                    index,
                    'quantity',
                    parseFloat($event as string) || 1
                  )
                "
                type="text"
                placeholder="1"
                size="sm"
              />
            </div>
            <div>
              <BaseText variant="caption" color="gray" class="mb-1"
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
              <BaseText variant="caption" color="gray" class="mb-1"
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
            <BaseText variant="caption" color="gray" class="mb-1"
              >カテゴリ</BaseText
            >
            <BaseSelect
              :model-value="item.category"
              @update:model-value="
                updateItem(index, 'category', $event as string)
              "
              :options="itemCategoryOptions"
              size="sm"
            />
          </div>
        </div>
      </BaseCard>

      <div class="flex justify-end pt-2 border-t border-gray-200">
        <BaseText variant="body" weight="bold">
          小計: {{ itemTotal.toLocaleString() }}円
        </BaseText>
      </div>
    </div>
  </div>
</template>
