<script setup lang="ts">
import { watch, computed, ref, onMounted } from "vue";
import { useTransactionForm } from "../../composables/useTransactionForm";
import { useTransactionDetail } from "../../composables/useTransactionDetail";
import { useReceiptOcr } from "../../composables/useReceiptOcr";
import { useTransactionCategories } from "../../composables/useTransactionCategories";
import { useItemCategories } from "../../composables/useItemCategories";
import BaseModal from "../atoms/BaseModal.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import LabeledCheckbox from "../molecules/LabeledCheckbox.vue";
import TransactionFormFields from "../molecules/TransactionFormFields.vue";
import TransactionItemsList from "../molecules/TransactionItemsList.vue";
import TransactionTaxesList from "../molecules/TransactionTaxesList.vue";
import ReceiptUploadArea from "../molecules/ReceiptUploadArea.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import { useIncomeItemCategories } from "../../composables/useIncomeItemCategories";

interface Props {
  isOpen: boolean;
  mode: "manual" | "receipt";
  transactionId?: string;
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
  payer,
  payee,
  category,
  paymentMethod,
  notes,
  taxInclusionType,
  items,
  taxes,
  isLoading: isSubmitting,
  errorMessage: submitError,
  calculatedTotal,
  isAutoCalculate,
  setFromOcrResult,
  setFromExistingTransaction,
  resetForm,
  submitTransaction,
  updateTransaction,
  addItem,
  removeItem,
  addTax,
  removeTax,
} = useTransactionForm();

const {
  isLoading: isOcrLoading,
  errorMessage: ocrError,
  shouldSaveImage,
  uploadedFile,
  parseReceipt,
  reset: resetOcr,
} = useReceiptOcr();

const {
  transaction: existingTransaction,
  fetchDetail,
  attachReceipt,
} = useTransactionDetail();

// カテゴリ管理
const {
  categories: transactionCategories,
  expenseCategories,
  incomeCategories,
  isLoading: isCategoriesLoading,
  fetchCategories: fetchTransactionCategories,
} = useTransactionCategories();

const {
  categories: itemCategories,
  isLoading: isItemCategoriesLoading,
  fetchCategories: fetchItemCategories,
} = useItemCategories();

const {
  categories: incomeItemCategories,
  isLoading: isIncomeItemCategoriesLoading,
  fetchCategories: fetchIncomeItemCategories,
} = useIncomeItemCategories();

// 取引種別に応じた明細カテゴリ
const availableItemCategories = computed(() => {
  if (type.value === "Income") {
    return incomeItemCategories.value;
  }
  return itemCategories.value;
});

const isEditMode = computed(() => !!props.transactionId);
const modalTitle = computed(() => {
  if (isEditMode.value) return "取引を編集";
  return props.mode === "receipt" ? "レシート読み取り" : "手動入力";
});

// モーダルが開いたらカテゴリを取得
onMounted(async () => {
  await Promise.all([
    fetchTransactionCategories(false),
    fetchItemCategories(false),
    fetchIncomeItemCategories(false),
  ]);
});

// 編集モード時に既存データを読み込む
watch(
  () => props.isOpen,
  async (newValue) => {
    if (newValue && props.transactionId) {
      await fetchDetail(props.transactionId);
      if (existingTransaction.value) {
        setFromExistingTransaction(existingTransaction.value);
      }
    } else if (!newValue) {
      resetForm();
      resetOcr();
      isOcrCompleted.value = false;
    }
  },
);

const isOcrMode = computed(() => props.mode === "receipt" && !isEditMode.value);
const isOcrCompleted = ref(false);
const isTypeReadonly = computed(() => isOcrMode.value || isEditMode.value);

const handleReceiptUpload = async (file: File) => {
  const result = await parseReceipt(file);
  if (result) {
    // カテゴリリストを渡して変換
    setFromOcrResult(result, transactionCategories.value, itemCategories.value);
    isOcrCompleted.value = true;
  }
};

const handleSubmit = async () => {
  let success = false;
  let createdTransactionId: string | undefined;

  if (isEditMode.value && props.transactionId) {
    // 更新の場合
    const result = await updateTransaction(props.transactionId);
    success = result !== null;
  } else {
    // 新規作成の場合
    const result = await submitTransaction();
    success = result.success;
    createdTransactionId = result.transactionId;

    // レシートモード & 画像保存ON & ファイルあり
    if (
      success &&
      createdTransactionId &&
      isOcrMode.value &&
      shouldSaveImage.value &&
      uploadedFile.value
    ) {
      try {
        await attachReceipt(createdTransactionId, uploadedFile.value);
        console.log("レシート画像を添付しました");
      } catch (error) {
        console.error("レシート画像の添付に失敗しました:", error);
        // 画像添付失敗でも取引は作成済みなので、エラーは表示するが処理は続行
      }
    }
  }

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

// 取引種別に応じたカテゴリを返す
const availableCategories = computed(() => {
  if (type.value === "Income") {
    return incomeCategories.value;
  }
  return expenseCategories.value;
});
</script>

<template>
  <BaseModal :is-open="isOpen" :title="modalTitle" @close="handleClose">
    <div class="space-y-6">
      <!-- OCRアップロード（編集モードでは非表示） -->
      <div v-if="isOcrMode && !isOcrCompleted">
        <ReceiptUploadArea @upload="handleReceiptUpload" />

        <!-- OCR解析中の表示 -->
        <div v-if="isOcrLoading" class="text-center py-8">
          <BaseSpinner
            icon="settings"
            size="lg"
            color="primary"
            label="レシート解析中"
            class="mb-2"
          />
          <BaseText variant="body" color="gray">解析中...</BaseText>
        </div>

        <div
          v-if="ocrError"
          class="mt-4 p-3 bg-red-100 border border-red-400 text-red-700 rounded-lg"
        >
          <BaseText variant="caption">{{ ocrError }}</BaseText>
        </div>
      </div>

      <!-- フォーム（手動入力 or OCR完了後 or 編集モード） -->
      <div v-if="!isOcrMode || isOcrCompleted || isEditMode" class="space-y-6">
        <!-- カテゴリ読み込み中 -->
        <div
          v-if="
            isCategoriesLoading ||
            isItemCategoriesLoading ||
            isIncomeItemCategoriesLoading
          "
          class="text-center py-4"
        >
          <BaseSpinner
            icon="settings"
            size="md"
            color="primary"
            label="カテゴリ読み込み中"
          />
        </div>

        <!-- フォーム本体 -->
        <div v-else>
          <TransactionFormFields
            :type="type"
            :transaction-date="transactionDate"
            :amount-total="amountTotal"
            :payer="payer"
            :payee="payee"
            :category="category"
            :payment-method="paymentMethod"
            :notes="notes"
            :tax-inclusion-type="taxInclusionType"
            :calculated-total="calculatedTotal"
            :is-auto-calculate="isAutoCalculate"
            :is-type-readonly="isTypeReadonly"
            :transaction-categories="availableCategories"
            @update:type="type = $event"
            @update:transaction-date="transactionDate = $event"
            @update:amount-total="amountTotal = $event"
            @update:payer="payer = $event"
            @update:payee="payee = $event"
            @update:category="category = $event"
            @update:payment-method="paymentMethod = $event"
            @update:notes="notes = $event"
            @update:tax-inclusion-type="taxInclusionType = $event"
            @update:is-auto-calculate="isAutoCalculate = $event"
          />

          <TransactionItemsList
            :items="items"
            :item-categories="availableItemCategories"
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
        </div>

        <!-- 画像保存オプション（レシートモード & 新規作成のみ） -->
        <div
          v-if="isOcrMode && !isEditMode"
          class="pt-4 border-t border-gray-200"
        >
          <LabeledCheckbox
            v-model="shouldSaveImage"
            label="レシート画像をサーバーに保存する"
            description="後から取引詳細で画像を確認できます（作成後7日以内であれば後から添付も可能）"
          />
        </div>

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
            :disabled="
              isSubmitting || isCategoriesLoading || isItemCategoriesLoading
            "
            @click="handleSubmit"
            class="flex-1"
          >
            <!-- ボタン内スピナー -->
            <span class="flex items-center justify-center gap-2">
              <BaseSpinner
                v-if="isSubmitting"
                icon="settings"
                size="sm"
                color="gray"
              />
              <span v-if="isSubmitting">
                {{ isEditMode ? "更新中..." : "登録中..." }}
              </span>
              <span v-else>
                {{ isEditMode ? "更新" : "登録" }}
              </span>
            </span>
          </BaseButton>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
