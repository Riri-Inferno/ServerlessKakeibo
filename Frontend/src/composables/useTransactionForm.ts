import { ref, computed, watch } from "vue";
import { transactionRepository } from "../repositories/transactionRepository";
import type {
  CreateTransactionRequest,
  CreateTransactionItem,
  CreateTaxDetail,
  TaxInclusionType,
  ShopDetails,
  TransactionDetail,
  UpdateTransactionRequest,
} from "../types/transaction";
import type {
  ReceiptParseResult,
  AmountValidationResult,
} from "../types/receipt";
import type { TransactionCategoryDto } from "../types/transactionCategory";
import type { ItemCategoryDto } from "../types/itemCategory";
import { TransactionType } from "../types/transaction";

export function useTransactionForm() {
  const isLoading = ref(false);
  const errorMessage = ref("");

  const type = ref<TransactionType>(TransactionType.Expense);
  const transactionDate = ref("");
  const amountTotal = ref<number | null>(null);
  const payee = ref("");
  const payer = ref("");
  const category = ref<string | null>(null); // Guid（UserTransactionCategoryId）を保持
  const paymentMethod = ref("");
  const notes = ref("");
  const taxInclusionType = ref<TaxInclusionType | undefined>(undefined);
  const items = ref<CreateTransactionItem[]>([]);
  const taxes = ref<CreateTaxDetail[]>([]);
  const shopDetails = ref<ShopDetails | null>(null);

  /**
   * 税区分に応じた合計金額を計算
   */
  const calculatedTotal = computed(() => {
    const itemsSum = items.value.reduce((sum, item) => sum + item.amount, 0);
    const taxesSum = taxes.value.reduce(
      (sum, tax) => sum + (tax.taxAmount || 0),
      0,
    );

    // 税区分に応じて計算方法を変える
    if (
      taxInclusionType.value === "Inclusive" ||
      taxInclusionType.value === "NoTax"
    ) {
      // 内税または非課税: items のみ
      return itemsSum;
    } else {
      // 外税または不明: items + taxes（従来の計算）
      return itemsSum + taxesSum;
    }
  });

  const isAutoCalculate = ref(false);

  // 明細や税情報が変更された時に自動計算
  watch(
    [items, taxes, taxInclusionType],
    () => {
      if (isAutoCalculate.value) {
        amountTotal.value = calculatedTotal.value;
      }
    },
    { deep: true },
  );

  // 自動計算チェックを入れた瞬間に上書き
  watch(isAutoCalculate, (newValue) => {
    if (newValue) {
      amountTotal.value = calculatedTotal.value;
    }
  });

  /**
   * OCR結果からフォームにセット
   * @param result OCR解析結果
   * @param transactionCategories 取引カテゴリ一覧
   * @param itemCategories 商品カテゴリ一覧
   */
  const setFromOcrResult = (
    result: ReceiptParseResult,
    transactionCategories: TransactionCategoryDto[],
    itemCategories: ItemCategoryDto[],
  ) => {
    const normalized = result.normalized;

    type.value = TransactionType.Expense;

    if (normalized.transactionDate) {
      const date = new Date(normalized.transactionDate);
      transactionDate.value = date.toISOString().split("T")[0] || "";
    } else {
      transactionDate.value = "";
    }

    amountTotal.value = normalized.amountTotal || null;
    payer.value = "";
    payee.value = normalized.payee || "";
    paymentMethod.value = (normalized.paymentMethod as string) || "";

    // CategoryCode → UserTransactionCategoryId 変換
    if (normalized.categoryCode) {
      const foundCategory = transactionCategories.find(
        (c) => c.code === normalized.categoryCode,
      );
      category.value = foundCategory?.id || null;
    } else {
      category.value = null;
    }

    // 先に税区分を設定
    if (normalized.amountValidation) {
      taxInclusionType.value = determineTaxInclusionType(
        normalized.amountValidation,
      );
    } else {
      taxInclusionType.value = undefined;
    }

    // Items の変換（CategoryCode → userItemCategoryId）
    items.value = (normalized.items || []).map((item) => {
      let userItemCategoryId: string | null = null;

      if (item.categoryCode) {
        const foundItemCategory = itemCategories.find(
          (c) => c.code === item.categoryCode,
        );
        userItemCategoryId = foundItemCategory?.id || null;
      }

      return {
        name: item.name || "",
        quantity: item.quantity,
        unitPrice: item.unitPrice,
        amount: item.amount,
        userItemCategoryId,
        userIncomeItemCategoryId: null,
      };
    });

    taxes.value = normalized.taxes || [];
    shopDetails.value = normalized.shopDetails || null;
  };

  /**
   * 金額整合性から税区分を判定
   */
  const determineTaxInclusionType = (
    validation: AmountValidationResult,
  ): TaxInclusionType | undefined => {
    // 税額がゼロまたはnull → 非課税
    if (!validation.taxTotal || validation.taxTotal === 0) {
      return "NoTax";
    }

    // 外税として一致（items + tax = total）
    if (validation.matchesAsExclusiveTax && !validation.matchesAsInclusiveTax) {
      return "Exclusive";
    }

    // 内税として一致（items = total）
    if (validation.matchesAsInclusiveTax && !validation.matchesAsExclusiveTax) {
      return "Inclusive";
    }

    // どちらでもない → 不明
    return "Unknown";
  };

  /**
   * 既存の取引データからフォームに値を設定
   */
  const setFromExistingTransaction = (transaction: TransactionDetail) => {
    type.value = transaction.type;

    const date = new Date(transaction.transactionDate);
    transactionDate.value = date.toISOString().split("T")[0] ?? "";

    amountTotal.value = transaction.amountTotal;
    payer.value = transaction.payer || "";
    payee.value = transaction.payee || "";

    // カスタムカテゴリIDを取得
    category.value = transaction.userTransactionCategory?.id || null;

    paymentMethod.value = transaction.paymentMethod || "";
    notes.value = transaction.notes || "";
    taxInclusionType.value = transaction.taxInclusionType;

    // 明細のカテゴリIDを復元
    items.value = transaction.items.map((item) => ({
      id: item.id,
      name: item.name,
      quantity: item.quantity,
      unitPrice: item.unitPrice,
      amount: item.amount,
      userItemCategoryId: item.userItemCategoryId || null,
      userIncomeItemCategoryId: item.userIncomeItemCategoryId || null,
    }));
    // 税情報を復元（idを保持）
    taxes.value = transaction.taxes.map((tax) => ({
      id: tax.id,
      taxRate: tax.taxRate,
      taxAmount: tax.taxAmount,
      taxableAmount: tax.taxableAmount,
      taxType: tax.taxType,
    }));

    // 店舗詳細を復元
    shopDetails.value = transaction.shopDetails;
  };

  /**
   * 取引を更新
   */
  const updateTransaction = async (id: string): Promise<boolean> => {
    if (!validateForm(true)) {
      return false;
    }

    isLoading.value = true;
    errorMessage.value = "";

    try {
      const request: UpdateTransactionRequest = {
        transactionDate: transactionDate.value,
        currency: "JPY",
        payer: payer.value || undefined,
        payee: payee.value || undefined,
        paymentMethod: paymentMethod.value || undefined,
        userTransactionCategoryId: category.value || undefined,
        notes: notes.value || undefined,
        taxInclusionType: taxInclusionType.value,
        // items の振り分け
        items:
          items.value.length > 0
            ? items.value.map((item) => ({
                ...item,
                // type による振り分け
                userItemCategoryId:
                  type.value === TransactionType.Expense
                    ? item.userItemCategoryId
                    : null,
                userIncomeItemCategoryId:
                  type.value === TransactionType.Income
                    ? item.userIncomeItemCategoryId
                    : null,
              }))
            : undefined,
        taxes: taxes.value.length > 0 ? taxes.value : undefined,
        shopDetails: shopDetails.value || undefined,
      };

      await transactionRepository.update(id, request);
      return true;
    } catch (error) {
      console.error("取引更新エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引の更新に失敗しました";
      return false;
    } finally {
      isLoading.value = false;
    }
  };

  const resetForm = () => {
    type.value = TransactionType.Expense;
    transactionDate.value = "";
    amountTotal.value = null;
    payer.value = "";
    payee.value = "";
    category.value = null;
    paymentMethod.value = "";
    notes.value = "";
    taxInclusionType.value = undefined;
    items.value = [];
    taxes.value = [];
    shopDetails.value = null;
    errorMessage.value = "";
    isAutoCalculate.value = false;
  };

  const validateForm = (isUpdate: boolean = false): boolean => {
    if (!transactionDate.value) {
      errorMessage.value = "取引日は必須です";
      return false;
    }

    // 新規作成時のみ金額チェック（更新時はサーバー側で計算）
    if (!isUpdate) {
      if (!amountTotal.value || amountTotal.value <= 0) {
        errorMessage.value = "金額は0より大きい値を入力してください";
        return false;
      }

      if (items.value.length > 0 && !isAutoCalculate.value) {
        const diff = Math.abs(calculatedTotal.value - amountTotal.value);

        if (diff > 1) {
          // 税区分に応じたメッセージ
          const taxTypeLabel =
            taxInclusionType.value === "Inclusive"
              ? "内税"
              : taxInclusionType.value === "Exclusive"
                ? "外税"
                : taxInclusionType.value === "NoTax"
                  ? "非課税"
                  : "不明";

          errorMessage.value =
            `明細合計（${calculatedTotal.value}円）が入力金額（${amountTotal.value}円）と一致しません。` +
            `\n税区分: ${taxTypeLabel}\n` +
            `自動計算をオンにするか、税区分を確認してください。`;
          return false;
        }
      }
    }

    errorMessage.value = "";
    return true;
  };

  const submitTransaction = async (): Promise<{
    success: boolean;
    transactionId?: string;
  }> => {
    if (!validateForm(false)) {
      return { success: false };
    }

    isLoading.value = true;
    errorMessage.value = "";

    try {
      const request: CreateTransactionRequest = {
        type: type.value,
        transactionDate: transactionDate.value,
        amountTotal: amountTotal.value!,
        currency: "JPY",
        payer: payer.value || undefined,
        payee: payee.value || undefined,
        paymentMethod: paymentMethod.value || undefined,
        userTransactionCategoryId: category.value || undefined,
        notes: notes.value || undefined,
        taxInclusionType: taxInclusionType.value,
        // items の振り分け
        items:
          items.value.length > 0
            ? items.value.map((item) => ({
                ...item,
                // type による振り分け
                userItemCategoryId:
                  type.value === TransactionType.Expense
                    ? item.userItemCategoryId
                    : null,
                userIncomeItemCategoryId:
                  type.value === TransactionType.Income
                    ? item.userIncomeItemCategoryId
                    : null,
              }))
            : undefined,
        taxes: taxes.value.length > 0 ? taxes.value : undefined,
        shopDetails: shopDetails.value || undefined,
      };

      const result = await transactionRepository.create(request);

      return { success: true, transactionId: result.transactionId };
    } catch (error) {
      console.error("取引登録エラー:", error);
      errorMessage.value =
        error instanceof Error ? error.message : "取引の登録に失敗しました";
      return { success: false };
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
      userItemCategoryId: null,
      userIncomeItemCategoryId: null,
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
    payer,
    payee,
    category,
    paymentMethod,
    notes,
    taxInclusionType,
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
    setFromExistingTransaction,
    updateTransaction,
  };
}
