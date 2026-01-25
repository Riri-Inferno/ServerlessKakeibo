<script setup lang="ts">
import { watch, ref, computed } from "vue";
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
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import { compressImage } from "../../utils/imageCompression";

interface Props {
  transactionId: string;
  isOpen: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
  edit: [];
  delete: [];
}>();

const {
  transaction,
  receiptImageUrl,
  isLoading,
  isLoadingImage,
  errorMessage,
  fetchDetail,
  fetchReceiptImageUrl,
  attachReceipt,
} = useTransactionDetail();

const isAttaching = ref(false);
const showFullImage = ref(false);
const fileInput = ref<HTMLInputElement>();
const isCompressing = ref(false);

watch(
  () => props.isOpen,
  async (newValue) => {
    if (newValue && props.transactionId) {
      await fetchDetail(props.transactionId);

      // 画像がある場合は署名付きURLを取得
      if (transaction.value?.sourceUrl) {
        await fetchReceiptImageUrl(props.transactionId);
      }
    }
  },
  { immediate: true },
);

/**
 * 添付可能かどうか（作成から7日以内 & 画像未添付）
 */
const canAttachReceipt = computed(() => {
  if (!transaction.value) return false;
  if (transaction.value.sourceUrl) return false;

  const createdAt = new Date(transaction.value.createdAt);
  const now = new Date();
  const diffDays =
    (now.getTime() - createdAt.getTime()) / (1000 * 60 * 60 * 24);

  return diffDays <= 7;
});

/**
 * レシート画像を添付
 */
const handleAttachReceipt = async (file: File) => {
  if (!props.transactionId) return;

  isAttaching.value = true;
  const success = await attachReceipt(props.transactionId, file);
  isAttaching.value = false;

  if (success) {
    // 添付成功後、署名付きURLを取得
    await fetchReceiptImageUrl(props.transactionId);
  }
};

/**
 * ファイル選択ダイアログを開く
 */
const openFileDialog = () => {
  fileInput.value?.click();
};

/**
 * ファイル選択時の処理
 */
const handleFileSelect = async (event: Event) => {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (file) {
    isCompressing.value = true;
    const compressedFile = await compressImage(file, {
      maxSizeMB: 0.5,
      maxWidthOrHeight: 1920,
    });
    isCompressing.value = false;

    handleAttachReceipt(compressedFile);
  }
};

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
const handleEdit = () => {
  emit("edit");
};

const handleDelete = () => {
  emit("delete");
};
</script>

<template>
  <BaseModal :is-open="isOpen" title="取引詳細" @close="emit('close')">
    <div v-if="isLoading" class="text-center py-8">
      <BaseSpinner
        icon="refresh"
        size="lg"
        color="primary"
        label="取引詳細を読み込み中"
        class="mb-2"
      />
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

      <!-- レシート画像セクション -->
      <div v-if="transaction.sourceUrl || canAttachReceipt">
        <BaseText variant="h3" class="mb-3">レシート画像</BaseText>

        <!-- 画像がある場合 -->
        <div v-if="receiptImageUrl">
          <BaseCard padding="sm" class="relative">
            <div v-if="isLoadingImage" class="text-center py-8">
              <BaseText variant="caption" color="gray">読み込み中...</BaseText>
            </div>
            <div v-else>
              <img
                :src="receiptImageUrl"
                alt="レシート画像"
                class="w-full rounded-lg cursor-pointer hover:opacity-90 transition-opacity"
                @click="showFullImage = true"
              />
              <BaseText variant="caption" color="gray" class="mt-2 block">
                添付日: {{ formatDate(transaction.receiptAttachedAt!) }}
              </BaseText>
            </div>
          </BaseCard>
        </div>

        <!-- 圧縮中の表示 -->
        <div v-else-if="isCompressing || isAttaching">
          <BaseCard padding="sm">
            <div class="text-center py-8">
              <BaseIcon
                name="plus"
                size="xl"
                class="mx-auto mb-2 text-gray-400"
              />
              <BaseText variant="body" color="gray" class="mb-2">
                {{ isCompressing ? "画像を圧縮中..." : "アップロード中..." }}
              </BaseText>
              <BaseText variant="caption" color="gray">
                しばらくお待ちください
              </BaseText>
            </div>
          </BaseCard>
        </div>

        <!-- 画像がない & 添付可能 -->
        <div v-else-if="canAttachReceipt">
          <BaseCard padding="sm">
            <div
              class="text-center py-8 cursor-pointer hover:bg-gray-50 transition-colors rounded-lg"
              @click="openFileDialog"
            >
              <BaseIcon
                name="plus"
                size="xl"
                class="mx-auto mb-2 text-gray-400"
              />
              <BaseText variant="body" color="gray" class="mb-2">
                レシート画像を添付
              </BaseText>
              <BaseText variant="caption" color="gray">
                クリックして画像を選択（自動で圧縮されます）
              </BaseText>
            </div>
            <div class="flex items-center justify-center gap-1.5 mt-2">
              <BaseIcon name="warning" size="sm" class="text-yellow-600" />
              <BaseText variant="caption" color="gray">
                一度添付すると変更できません（作成から7日以内のみ可能）
              </BaseText>
            </div>
          </BaseCard>

          <input
            ref="fileInput"
            type="file"
            accept="image/*"
            class="hidden"
            @change="handleFileSelect"
          />
        </div>

        <!-- 期限切れ -->
        <div v-else>
          <BaseCard padding="sm">
            <div class="text-center py-4">
              <BaseText variant="body" color="gray">
                添付期限が過ぎています（作成から7日以内のみ添付可能）
              </BaseText>
            </div>
          </BaseCard>
        </div>
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
                  {{ item.quantity }}個
                  <span v-if="item.unitPrice !== null">
                    × {{ item.unitPrice.toLocaleString() }}円
                  </span>
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
                  <span v-if="tax.taxableAmount !== null">
                    対象額: {{ tax.taxableAmount.toLocaleString() }}円 ・
                  </span>
                  税率: {{ tax.taxRate }}%
                </BaseText>
              </div>
              <BaseText variant="body" weight="bold" color="danger">
                {{ tax.taxAmount.toLocaleString() }}円
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
    <template #footer>
      <div class="flex gap-3">
        <BaseButton variant="outline" @click="handleDelete" class="flex-1">
          削除
        </BaseButton>
        <BaseButton variant="primary" @click="handleEdit" class="flex-1">
          編集
        </BaseButton>
      </div>
    </template>

    <!-- 画像フルスクリーン表示モーダル -->
    <BaseModal
      :is-open="showFullImage"
      title="レシート画像"
      @close="showFullImage = false"
    >
      <img
        v-if="receiptImageUrl"
        :src="receiptImageUrl"
        alt="レシート画像（拡大）"
        class="w-full"
      />
    </BaseModal>
  </BaseModal>
</template>
