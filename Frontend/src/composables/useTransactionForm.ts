import { ref, computed, watch } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type {
  CreateTransactionRequest,
  CreateTransactionItem,
  CreateTaxDetail,
  ShopDetails,
} from "../types/transaction";
import type { ReceiptParseResult } from "../types/receipt";
import { TransactionType, TransactionCategory } from "../types/transaction";

export function useTransactionForm() {
  const isLoading = ref(false);
  const errorMessage = ref("");

  const type = ref<TransactionType>(TransactionType.Expense);
  const transactionDate = ref("");
  const amountTotal = ref<number | null>(null);
  const payee = ref("");
  const category = ref<TransactionCategory>(TransactionCategory.Uncategorized);
  const paymentMethod = ref("");
  const notes = ref("");
  const items = ref<CreateTransactionItem[]>([]);
  const taxes = ref<CreateTaxDetail[]>([]);
  const shopDetails = ref<ShopDetails | null>(null);

  const calculatedTotal = computed(() => {
    const itemsSum = items.value.reduce((sum, item) => sum + item.amount, 0);
    const taxesSum = taxes.value.reduce(
      (sum, tax) => sum + (tax.taxAmount || 0),
      0
    );
    return itemsSum + taxesSum;
  });

  const isAutoCalculate = ref(false);

  watch(
    [items, taxes],
    () => {
      if (isAutoCalculate.value) {
        amountTotal.value = calculatedTotal.value;
      }
    },
    { deep: true }
  );

  const setFromOcrResult = (result: ReceiptParseResult) => {
    const normalized = result.normalized;

    type.value = TransactionType.Expense;
    transactionDate.value = normalized.transactionDate || "";
    amountTotal.value = normalized.amountTotal || null;
    payee.value = normalized.payee || "";
    category.value =
      (normalized.category as TransactionCategory) ||
      TransactionCategory.Uncategorized;
    paymentMethod.value = normalized.paymentMethod || "";
    items.value = normalized.items || [];
    taxes.value = normalized.taxes || [];
    shopDetails.value = normalized.shopDetails || null;
  };

  const resetForm = () => {
    type.value = TransactionType.Expense;
    transactionDate.value = "";
    amountTotal.value = null;
    payee.value = "";
    category.value = TransactionCategory.Uncategorized;
    paymentMethod.value = "";
    notes.value = "";
    items.value = [];
    taxes.value = [];
    shopDetails.value = null;
    errorMessage.value = "";
    isAutoCalculate.value = false;
  };

  const validateForm = (): boolean => {
    if (!transactionDate.value) {
      errorMessage.value = "取引日は必須です";
      return false;
    }

    if (!amountTotal.value || amountTotal.value <= 0) {
      errorMessage.value = "金額は0より大きい値を入力してください";
      return false;
    }

    if (items.value.length > 0) {
      const diff = Math.abs(calculatedTotal.value - amountTotal.value);
      if (diff > 1) {
        errorMessage.value = `明細と税の合計（${calculatedTotal.value}円）が、入力金額（${amountTotal.value}円）と一致しません`;
        return false;
      }
    }

    errorMessage.value = "";
    return true;
  };

  const submitTransaction = async (): Promise<boolean> => {
    if (!validateForm()) {
      return false;
    }

    isLoading.value = true;
    errorMessage.value = "";

    try {
      const request: CreateTransactionRequest = {
        type: type.value,
        transactionDate: transactionDate.value,
        amountTotal: amountTotal.value!,
        currency: "JPY",
        payee: payee.value || undefined,
        paymentMethod: paymentMethod.value || undefined,
        category: category.value,
        notes: notes.value || undefined,
        items: items.value.length > 0 ? items.value : undefined,
        taxes: taxes.value.length > 0 ? taxes.value : undefined,
        shopDetails: shopDetails.value || undefined,
      };

      await transactionRepository.create(request);
      return true;
    } catch (error) {
      console.error("取引登録エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引の登録に失敗しました";
      return false;
    } finally {
      isLoading.value = false;
    }
  };

  const addItem = () => {
    items.value.push({
      name: "",
      quantity: 1,
      unitPrice: null,
      amount: 0,
      category: "Uncategorized",
    });
  };

  const removeItem = (index: number) => {
    items.value.splice(index, 1);
  };

  const addTax = () => {
    taxes.value.push({
      taxRate: 10,
      taxAmount: null,
      taxableAmount: null,
      taxType: "消費税",
    });
  };

  const removeTax = (index: number) => {
    taxes.value.splice(index, 1);
  };

  return {
    type,
    transactionDate,
    amountTotal,
    payee,
    category,
    paymentMethod,
    notes,
    items,
    taxes,
    shopDetails,
    isLoading,
    errorMessage,
    calculatedTotal,
    isAutoCalculate,
    setFromOcrResult,
    resetForm,
    validateForm,
    submitTransaction,
    addItem,
    removeItem,
    addTax,
    removeTax,
  };
}
