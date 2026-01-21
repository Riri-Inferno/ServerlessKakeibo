<script setup lang="ts">
import { watch } from "vue";
import { useTransactionDetail } from "../../composables/useTransactionDetail";
import {
  TransactionType,
  CategoryLabels,
  TaxInclusionTypeLabels,
} from "../../types/transaction";
import BaseModal from "../atoms/BaseModal.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseBadge from "../atoms/BaseBadge.vue";
import BaseCard from "../atoms/BaseCard.vue";

interface Props {
  transactionId: string;
  isOpen: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
}>();

const { transaction, isLoading, errorMessage, fetchDetail } =
  useTransactionDetail();

watch(
  () => props.isOpen,
  (newValue) => {
    if (newValue && props.transactionId) {
      fetchDetail(props.transactionId);
    }
  },
  { immediate: true },
);

const formatDate = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleDateString("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
  });
};

const formatDateTime = (dateString: string) => {
  const date = new Date(dateString);
  return date.toLocaleString("ja-JP");
};
</script>

<template>
  <BaseModal :is-open="isOpen" title="取引詳細" @close="emit('close')">
    <div v-if="isLoading" class="text-center py-8">
      <BaseText variant="body" color="gray">読み込み中...</BaseText>
    </div>

    <div v-else-if="errorMessage" class="text-center py-8">
      <BaseText variant="body" color="danger">{{ errorMessage }}</BaseText>
    </div>

    <div v-else-if="transaction" class="space-y-6">
      <!-- バッジ -->
      <div class="flex items-center gap-2">
        <BaseBadge
          :color="
            transaction.type === TransactionType.Income ? 'success' : 'danger'
          "
        >
          {{ transaction.type === TransactionType.Income ? "収入" : "支出" }}
        </BaseBadge>
        <BaseBadge color="gray">
          {{ CategoryLabels[transaction.category] || transaction.category }}
        </BaseBadge>
        <!-- 税区分バッジ -->
        <BaseBadge v-if="transaction.taxInclusionType" color="info">
          {{ TaxInclusionTypeLabels[transaction.taxInclusionType] }}
        </BaseBadge>
      </div>

      <!-- 合計金額 -->
      <div>
        <BaseText variant="caption" color="gray" class="mb-1"
          >合計金額</BaseText
        >
        <BaseText
          variant="amount"
          :color="
            transaction.type === TransactionType.Income ? 'success' : 'danger'
          "
        >
          {{ transaction.type === TransactionType.Income ? "+" : "-" }}
          {{ transaction.amountTotal.toLocaleString() }}円
        </BaseText>
      </div>

      <!-- 支払先 -->
      <div>
        <BaseText variant="caption" color="gray" class="mb-1">
          {{
            transaction.type === TransactionType.Income ? "支払元" : "支払先"
          }}
        </BaseText>
        <BaseText variant="body" weight="bold">{{
          transaction.payee
        }}</BaseText>
      </div>

      <!-- 取引日 -->
      <div>
        <BaseText variant="caption" color="gray" class="mb-1">取引日</BaseText>
        <BaseText variant="body">{{
          formatDate(transaction.transactionDate)
        }}</BaseText>
      </div>

      <!-- 支払方法 -->
      <div v-if="transaction.paymentMethod">
        <BaseText variant="caption" color="gray" class="mb-1"
          >支払方法</BaseText
        >
        <BaseText variant="body">{{ transaction.paymentMethod }}</BaseText>
      </div>

      <!-- メモ -->
      <div v-if="transaction.notes">
        <BaseText variant="caption" color="gray" class="mb-1">メモ</BaseText>
        <BaseText variant="body" class="whitespace-pre-wrap">{{
          transaction.notes
        }}</BaseText>
      </div>

      <!-- 明細 -->
      <div v-if="transaction.items && transaction.items.length > 0">
        <BaseText variant="h3" class="mb-3">明細</BaseText>
        <div class="space-y-2">
          <BaseCard
            v-for="item in transaction.items"
            :key="item.id"
            padding="sm"
          >
            <div class="flex justify-between items-center">
              <div>
                <BaseText variant="body" weight="medium">{{
                  item.name
                }}</BaseText>
                <BaseText variant="caption" color="gray">
                  {{ item.quantity }}個 ×
                  {{ item.unitPrice.toLocaleString() }}円
                </BaseText>
              </div>
              <BaseText variant="body" weight="bold">
                {{ item.amount.toLocaleString() }}円
              </BaseText>
            </div>
          </BaseCard>
        </div>
      </div>

      <!-- 税・控除 -->
      <div v-if="transaction.taxes && transaction.taxes.length > 0">
        <BaseText variant="h3" class="mb-3">税・控除</BaseText>
        <div class="space-y-2">
          <BaseCard v-for="tax in transaction.taxes" :key="tax.id" padding="sm">
            <div class="flex justify-between items-center">
              <div>
                <BaseText variant="body" weight="medium">{{
                  tax.taxType
                }}</BaseText>
                <BaseText variant="caption" color="gray">
                  対象額: {{ tax.taxableAmount.toLocaleString() }}円
                </BaseText>
              </div>
              <BaseText variant="body" weight="bold" color="danger">
                -{{ tax.taxAmount.toLocaleString() }}円
              </BaseText>
            </div>
          </BaseCard>
        </div>
      </div>

      <!-- 作成・更新日時 -->
      <div class="pt-4 border-t border-gray-200">
        <div class="grid grid-cols-2 gap-4 text-sm">
          <div>
            <BaseText variant="caption" color="gray" class="mb-1"
              >作成日時</BaseText
            >
            <BaseText variant="caption">{{
              formatDateTime(transaction.createdAt)
            }}</BaseText>
          </div>
          <div>
            <BaseText variant="caption" color="gray" class="mb-1"
              >更新日時</BaseText
            >
            <BaseText variant="caption">{{
              formatDateTime(transaction.updatedAt)
            }}</BaseText>
          </div>
        </div>
      </div>
    </div>
  </BaseModal>
</template>
