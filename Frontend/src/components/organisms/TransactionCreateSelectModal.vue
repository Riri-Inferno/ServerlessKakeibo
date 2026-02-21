<script setup lang="ts">
import BaseModal from "../atoms/BaseModal.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseIcon from "../atoms/BaseIcon.vue";

interface Props {
  isOpen: boolean;
}

defineProps<Props>();

const emit = defineEmits<{
  close: [];
  selectManual: [];
  selectReceipt: [];
}>();
</script>

<template>
  <BaseModal :is-open="isOpen" title="取引を登録" @close="emit('close')">
    <div class="space-y-3 md:space-y-4">
      <BaseText variant="body" color="gray" class="text-center text-sm md:text-base">
        登録方法を選択してください
      </BaseText>

      <div class="grid grid-cols-1 md:grid-cols-2 gap-3 md:gap-4">
        <div
          @click="emit('selectManual')"
          class="p-4 md:p-6 border-2 border-gray-300 hover:border-blue-500 hover:bg-blue-50 rounded-lg transition-all cursor-pointer"
        >
          <BaseIcon
            name="pencil"
            size="xl"
            class="mx-auto mb-2 md:mb-3 text-gray-600"
          />
          <BaseText variant="body" weight="bold" class="mb-1 md:mb-2 text-center text-sm md:text-base">
            手動入力
          </BaseText>
          <BaseText variant="caption" color="gray" class="text-center text-xs md:text-sm">
            金額や日付を直接入力
          </BaseText>
        </div>

        <div
          @click="emit('selectReceipt')"
          class="p-4 md:p-6 border-2 border-gray-300 hover:border-blue-500 hover:bg-blue-50 rounded-lg transition-all cursor-pointer"
        >
          <BaseIcon name="chart" size="xl" class="mx-auto mb-2 md:mb-3 text-gray-600" />
          <BaseText variant="body" weight="bold" class="mb-1 md:mb-2 text-center text-sm md:text-base">
            レシート読み取り
          </BaseText>
          <BaseText variant="caption" color="gray" class="text-center text-xs md:text-sm">
            画像から自動入力
          </BaseText>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
