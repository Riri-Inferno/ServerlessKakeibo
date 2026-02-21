<script setup lang="ts">
import { computed } from "vue";
import type { CreateTaxDetail } from "../../types/transaction";
import BaseText from "../atoms/BaseText.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseInputNumber from "../atoms/BaseInputNumber.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseCard from "../atoms/BaseCard.vue";

interface Props {
  taxes: CreateTaxDetail[];
}

const props = defineProps<Props>();

const emit = defineEmits<{
  "update:taxes": [value: CreateTaxDetail[]];
  add: [];
  remove: [index: number];
}>();

const updateTax = (index: number, field: keyof CreateTaxDetail, value: any) => {
  const newTaxes = [...props.taxes];
  const currentTax = newTaxes[index];

  if (!currentTax) return;

  newTaxes[index] = {
    taxRate: field === "taxRate" ? value : currentTax.taxRate,
    taxAmount: field === "taxAmount" ? value : currentTax.taxAmount,
    taxableAmount: field === "taxableAmount" ? value : currentTax.taxableAmount,
    taxType: field === "taxType" ? value : currentTax.taxType,
  };

  emit("update:taxes", newTaxes);
};

const taxTotal = computed(() => {
  return props.taxes.reduce((sum, tax) => sum + (tax.taxAmount || 0), 0);
});
</script>

<template>
  <div class="space-y-3 md:space-y-4">
    <div class="flex items-center justify-between">
      <BaseText variant="h3">税情報</BaseText>
    </div>

    <div
      v-if="taxes.length === 0"
      class="text-center py-6 md:py-8 border-2 border-dashed border-gray-300 rounded-lg"
    >
      <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
        <span class="md:hidden">税情報はオプションです</span>
        <span class="hidden md:inline">税情報がありません。</span>
      </BaseText>
    </div>

    <div v-else class="space-y-2 md:space-y-3">
      <BaseCard v-for="(tax, index) in taxes" :key="index" padding="sm" class="p-2.5 md:p-3">
        <div class="space-y-2 md:space-y-3">
          <div class="flex items-start justify-between gap-2">
            <div class="flex-1 min-w-0">
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >税種別</BaseText
              >
              <BaseInput
                :model-value="tax.taxType || '消費税'"
                @update:model-value="
                  updateTax(index, 'taxType', $event as string)
                "
                type="text"
                placeholder="消費税"
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
                >税率(%)</BaseText
              >
              <BaseInput
                :model-value="tax.taxRate !== null ? String(tax.taxRate) : ''"
                @update:model-value="
                  updateTax(
                    index,
                    'taxRate',
                    $event ? parseInt($event as string) : null,
                  )
                "
                type="text"
                placeholder="10"
                size="sm"
              />
            </div>
            <div>
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >税額</BaseText
              >
              <BaseInputNumber
                :model-value="tax.taxAmount"
                @update:model-value="
                  updateTax(index, 'taxAmount', $event as number | null)
                "
                size="sm"
              />
            </div>
            <div>
              <BaseText variant="caption" color="gray" class="mb-1 text-xs md:text-sm"
                >対象額</BaseText
              >
              <BaseInputNumber
                :model-value="tax.taxableAmount"
                @update:model-value="
                  updateTax(index, 'taxableAmount', $event as number | null)
                "
                size="sm"
              />
            </div>
          </div>
        </div>
      </BaseCard>

      <div class="flex justify-end pt-2 border-t border-gray-200">
        <BaseText variant="body" weight="bold" class="text-sm md:text-base">
          税合計: {{ taxTotal.toLocaleString() }}円
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
          <span class="text-sm md:text-base">税情報を追加</span>
        </span>
      </BaseButton>
    </div>
  </div>
</template>
