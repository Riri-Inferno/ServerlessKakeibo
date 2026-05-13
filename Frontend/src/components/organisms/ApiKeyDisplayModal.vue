<script setup lang="ts">
import { ref, watch } from "vue";
import BaseModal from "../atoms/BaseModal.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseCard from "../atoms/BaseCard.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import type { CreateApiKeyResult } from "../../types/apiKey";

interface Props {
  isOpen: boolean;
  result: CreateApiKeyResult | null;
}

const props = defineProps<Props>();

const emit = defineEmits<{
  close: [];
}>();

const isCopied = ref(false);
const copyError = ref<string | null>(null);

watch(
  () => props.isOpen,
  (isOpen) => {
    if (isOpen) {
      isCopied.value = false;
      copyError.value = null;
    }
  },
);

const handleCopy = async () => {
  if (!props.result) return;

  copyError.value = null;

  try {
    await navigator.clipboard.writeText(props.result.key);
    isCopied.value = true;
    // 数秒後にラベルを戻す
    setTimeout(() => {
      isCopied.value = false;
    }, 2000);
  } catch {
    copyError.value =
      "クリップボードへのコピーに失敗しました。手動で選択してコピーしてください。";
  }
};

const handleClose = () => {
  emit("close");
};
</script>

<template>
  <BaseModal
    :is-open="isOpen"
    title="APIキーが発行されました"
    :close-on-click-outside="false"
    :close-on-escape="false"
    @close="handleClose"
  >
    <div v-if="result" class="space-y-4 md:space-y-5">
      <BaseCard padding="sm" class="bg-amber-50 border-2 border-amber-300">
        <div class="flex items-start gap-3">
          <BaseIcon
            name="warning"
            size="md"
            class="text-amber-600 flex-shrink-0 mt-0.5"
            variant="solid"
          />
          <div class="space-y-1">
            <BaseText variant="body" weight="bold" class="text-amber-900">
              この画面を閉じると二度と表示できません
            </BaseText>
            <BaseText variant="caption" class="text-amber-800">
              下記のキーを今すぐコピーして安全な場所に保管してください。
            </BaseText>
          </div>
        </div>
      </BaseCard>

      <div>
        <BaseText variant="caption" color="gray" class="mb-1">ラベル</BaseText>
        <BaseText variant="body" class="text-sm md:text-base">
          {{ result.name }}
        </BaseText>
      </div>

      <div>
        <BaseText variant="caption" color="gray" class="mb-1">APIキー</BaseText>
        <div
          class="bg-gray-900 text-green-300 font-mono text-xs md:text-sm p-3 rounded-lg break-all select-all"
        >
          {{ result.key }}
        </div>
        <div class="mt-2 flex items-center gap-3">
          <BaseButton
            variant="primary"
            size="sm"
            @click="handleCopy"
          >
            <span class="flex items-center gap-1">
              <BaseIcon
                :name="isCopied ? 'check' : 'clipboard-list'"
                size="sm"
              />
              <span>{{ isCopied ? "コピー済み" : "コピー" }}</span>
            </span>
          </BaseButton>
          <BaseText
            v-if="copyError"
            variant="caption"
            class="text-red-600"
          >
            {{ copyError }}
          </BaseText>
        </div>
      </div>

      <div class="grid grid-cols-2 gap-3 pt-2 border-t border-gray-200">
        <div>
          <BaseText variant="caption" color="gray" class="mb-1">スコープ</BaseText>
          <BaseText variant="body" class="text-sm">
            {{ result.scopes.join(", ") }}
          </BaseText>
        </div>
        <div>
          <BaseText variant="caption" color="gray" class="mb-1">有効期限</BaseText>
          <BaseText variant="body" class="text-sm">
            {{
              result.expiresAt
                ? new Date(result.expiresAt).toLocaleDateString("ja-JP")
                : "無期限"
            }}
          </BaseText>
        </div>
      </div>
    </div>

    <template #footer>
      <BaseButton variant="primary" @click="handleClose" class="w-full">
        保管しました（閉じる）
      </BaseButton>
    </template>
  </BaseModal>
</template>
