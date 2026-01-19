<script setup lang="ts">
import { computed, ref, watch } from "vue";
import { useTransactionForm } from "../../composables/useTransactionForm";
import { useReceiptOcr } from "../../composables/useReceiptOcr";
import BaseModal from "../atoms/BaseModal.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import TransactionFormFields from "../molecules/TransactionFormFields.vue";
import TransactionItemsList from "../molecules/TransactionItemsList.vue";
import TransactionTaxesList from "../molecules/TransactionTaxesList.vue";
import ReceiptUploadArea from "../molecules/ReceiptUploadArea.vue";

interface Props {
  isOpen: boolean;
  mode: "manual" | "receipt";
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
  success: [];
}>();

const {
  type,
  transactionDate,
  amountTotal,
  payee,
  category,
  paymentMethod,
  notes,
  items,
  taxes,
  isLoading: isSubmitting,
  errorMessage: submitError,
  calculatedTotal,
  isAutoCalculate,
  setFromOcrResult,
  resetForm,
  submitTransaction,
  addItem,
  removeItem,
  addTax,
  removeTax,
} = useTransactionForm();

const {
  isLoading: isOcrLoading,
  errorMessage: ocrError,
  parseReceipt,
  reset: resetOcr,
} = useReceiptOcr();

const isOcrMode = computed(() => props.mode === "receipt");
const isOcrCompleted = ref(false);

const handleReceiptUpload = async (file: File) => {
  const result = await parseReceipt(file);
  if (result) {
    setFromOcrResult(result);
    isOcrCompleted.value = true;
  }
};

const handleSubmit = async () => {
  const success = await submitTransaction();
  if (success) {
    emit("success");
    handleClose();
  }
};

const handleClose = () => {
  resetForm();
  resetOcr();
  isOcrCompleted.value = false;
  emit("close");
};

watch(
  () => props.isOpen,
  (newValue) => {
    if (!newValue) {
      resetForm();
      resetOcr();
      isOcrCompleted.value = false;
    }
  }
);
</script>

<template>
  <BaseModal
    :is-open="isOpen"
    :title="isOcrMode ? 'レシート読み取り' : '手動入力'"
    @close="handleClose"
  >
    <div class="space-y-6">
      <div v-if="isOcrMode && !isOcrCompleted">
        <ReceiptUploadArea @upload="handleReceiptUpload" />

        <div v-if="isOcrLoading" class="text-center py-8">
          <BaseText variant="body" color="gray">解析中...</BaseText>
        </div>

        <div
          v-if="ocrError"
          class="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg"
        >
          <BaseText variant="caption">{{ ocrError }}</BaseText>
        </div>
      </div>

      <div v-if="!isOcrMode || isOcrCompleted" class="space-y-6">
        <TransactionFormFields
          :type="type"
          :transaction-date="transactionDate"
          :amount-total="amountTotal"
          :payee="payee"
          :category="category"
          :payment-method="paymentMethod"
          :notes="notes"
          :calculated-total="calculatedTotal"
          :is-auto-calculate="isAutoCalculate"
          @update:type="type = $event"
          @update:transaction-date="transactionDate = $event"
          @update:amount-total="amountTotal = $event"
          @update:payee="payee = $event"
          @update:category="category = $event"
          @update:payment-method="paymentMethod = $event"
          @update:notes="notes = $event"
          @update:is-auto-calculate="isAutoCalculate = $event"
        />

        <TransactionItemsList
          :items="items"
          @update:items="items = $event"
          @add="addItem"
          @remove="removeItem"
        />

        <TransactionTaxesList
          :taxes="taxes"
          @update:taxes="taxes = $event"
          @add="addTax"
          @remove="removeTax"
        />

        <div
          v-if="submitError"
          class="p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg"
        >
          <BaseText variant="caption">{{ submitError }}</BaseText>
        </div>

        <div class="flex gap-2">
          <BaseButton variant="outline" @click="handleClose" class="flex-1">
            キャンセル
          </BaseButton>
          <BaseButton
            variant="primary"
            :disabled="isSubmitting"
            @click="handleSubmit"
            class="flex-1"
          >
            <span v-if="isSubmitting">登録中...</span>
            <span v-else>登録</span>
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
