<script setup lang="ts">
import { ref, computed, watch } from "vue";
import BaseModal from "../atoms/BaseModal.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseCard from "../atoms/BaseCard.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseInput from "../atoms/BaseInput.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import { apiKeyRepository } from "../../repositories/apiKeyRepository";
import type { CreateApiKeyResult } from "../../types/apiKey";

interface Props {
  isOpen: boolean;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
  created: [result: CreateApiKeyResult];
}>();

const name = ref("");
const expiresAtDate = ref("");
const noExpiry = ref(false);
const isSubmitting = ref(false);
const errorMessage = ref<string | null>(null);

const isNameValid = computed(
  () => name.value.trim().length > 0 && name.value.trim().length <= 100,
);

const isExpiryValid = computed(() => {
  if (noExpiry.value) return true;
  if (!expiresAtDate.value) return false;

  // 入力された日付の終端（その日の 23:59:59）を有効期限とみなす
  const expiry = new Date(`${expiresAtDate.value}T23:59:59`);
  return expiry.getTime() > Date.now();
});

const canSubmit = computed(
  () => isNameValid.value && isExpiryValid.value && !isSubmitting.value,
);

// 既定値: 今日から 90 日後
const defaultExpiresAtDate = () => {
  const d = new Date();
  d.setDate(d.getDate() + 90);
  return d.toISOString().slice(0, 10);
};

const todayDate = () => new Date().toISOString().slice(0, 10);

const resetForm = () => {
  name.value = "";
  expiresAtDate.value = defaultExpiresAtDate();
  noExpiry.value = false;
  isSubmitting.value = false;
  errorMessage.value = null;
};

watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) resetForm();
  },
);

const handleSubmit = async () => {
  if (!canSubmit.value) return;

  errorMessage.value = null;
  isSubmitting.value = true;

  // 終端の 23:59:59Z を ISO 文字列として送る（無期限なら null）
  const expiresAtIso = noExpiry.value
    ? null
    : new Date(`${expiresAtDate.value}T23:59:59Z`).toISOString();

  try {
    const result = await apiKeyRepository.create({
      name: name.value.trim(),
      scopes: ["read"],
      expiresAt: expiresAtIso,
    });
    emit("created", result);
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error ? e.message : "APIキーの発行に失敗しました";
  } finally {
    isSubmitting.value = false;
  }
};

const handleClose = () => {
  if (isSubmitting.value) return;
  emit("close");
};
</script>

<template>
  <BaseModal :is-open="isOpen" title="APIキーを発行" @close="handleClose">
    <div class="space-y-4 md:space-y-5">
      <BaseCard padding="sm" class="bg-blue-50 border border-blue-200">
        <div class="flex items-start gap-2">
          <BaseIcon
            name="info"
            size="sm"
            class="text-blue-600 flex-shrink-0 mt-0.5"
            variant="solid"
          />
          <BaseText variant="caption" class="text-blue-800">
            発行されたAPIキーは <strong>発行直後の画面でのみ</strong> 表示されます。後から再表示はできません。
          </BaseText>
        </div>
      </BaseCard>

      <div>
        <BaseText variant="caption" color="gray" class="mb-1">
          ラベル <span class="text-red-600">*</span>
        </BaseText>
        <BaseInput
          v-model="name"
          type="text"
          placeholder="例: MCP - 自宅PC"
          :disabled="isSubmitting"
          :error="name.length > 0 && !isNameValid"
        />
        <BaseText variant="caption" color="gray" class="mt-1 text-xs">
          自分が識別するための名前（最大100文字）
        </BaseText>
      </div>

      <div>
        <BaseText variant="caption" color="gray" class="mb-1">スコープ</BaseText>
        <div
          class="flex items-center gap-2 px-3 py-2 bg-gray-50 border border-gray-200 rounded-lg"
        >
          <BaseIcon name="tag" size="sm" class="text-gray-500" />
          <BaseText variant="body" class="text-sm">read（読み取り）</BaseText>
        </div>
        <BaseText variant="caption" color="gray" class="mt-1 text-xs">
          現在は read のみ発行可能
        </BaseText>
      </div>

      <div>
        <BaseText variant="caption" color="gray" class="mb-1">有効期限</BaseText>
        <BaseInput
          v-model="expiresAtDate"
          type="date"
          :min="todayDate()"
          :disabled="noExpiry || isSubmitting"
          :error="!noExpiry && expiresAtDate.length > 0 && !isExpiryValid"
        />
        <label class="flex items-center gap-2 mt-2 cursor-pointer">
          <input
            v-model="noExpiry"
            type="checkbox"
            :disabled="isSubmitting"
            class="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
          />
          <BaseText variant="caption" class="text-sm">
            無期限にする（自己責任）
          </BaseText>
        </label>
      </div>

      <div
        v-if="errorMessage"
        class="p-3 bg-red-100 border border-red-400 rounded-lg"
      >
        <div class="flex items-start gap-2">
          <BaseIcon
            name="warning"
            size="sm"
            class="text-red-700 mt-0.5"
            variant="solid"
          />
          <BaseText variant="caption" class="text-red-700">
            {{ errorMessage }}
          </BaseText>
        </div>
      </div>
    </div>

    <template #footer>
      <div class="flex gap-3">
        <BaseButton
          variant="outline"
          @click="handleClose"
          :disabled="isSubmitting"
          class="flex-1"
        >
          キャンセル
        </BaseButton>
        <BaseButton
          variant="primary"
          :disabled="!canSubmit"
          @click="handleSubmit"
          class="flex-1"
        >
          <span class="flex items-center justify-center gap-2">
            <BaseSpinner
              v-if="isSubmitting"
              icon="refresh"
              size="sm"
              color="white"
            />
            <BaseIcon v-else name="plus" size="sm" />
            <span>{{ isSubmitting ? "発行中..." : "発行" }}</span>
          </span>
        </BaseButton>
      </div>
    </template>
  </BaseModal>
</template>
