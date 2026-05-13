<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import BaseCard from "../atoms/BaseCard.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import CreateApiKeyModal from "./CreateApiKeyModal.vue";
import ApiKeyDisplayModal from "./ApiKeyDisplayModal.vue";
import { apiKeyRepository } from "../../repositories/apiKeyRepository";
import type { ApiKeyDto, CreateApiKeyResult } from "../../types/apiKey";

const keys = ref<ApiKeyDto[]>([]);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const revokingId = ref<string | null>(null);

const isCreateModalOpen = ref(false);
const isDisplayModalOpen = ref(false);
const lastCreated = ref<CreateApiKeyResult | null>(null);

const sortedKeys = computed(() =>
  [...keys.value].sort((a, b) => {
    // アクティブ → 失効済みの順
    const aActive = !a.revokedAt;
    const bActive = !b.revokedAt;
    if (aActive !== bActive) return aActive ? -1 : 1;
    return new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime();
  }),
);

const fetchKeys = async () => {
  isLoading.value = true;
  errorMessage.value = null;
  try {
    keys.value = await apiKeyRepository.list();
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error ? e.message : "APIキー一覧の取得に失敗しました";
  } finally {
    isLoading.value = false;
  }
};

onMounted(fetchKeys);

const openCreateModal = () => {
  isCreateModalOpen.value = true;
};

const closeCreateModal = () => {
  isCreateModalOpen.value = false;
};

const handleCreated = (result: CreateApiKeyResult) => {
  lastCreated.value = result;
  isCreateModalOpen.value = false;
  isDisplayModalOpen.value = true;
  // 一覧を更新
  fetchKeys();
};

const closeDisplayModal = () => {
  isDisplayModalOpen.value = false;
  lastCreated.value = null;
};

const handleRevoke = async (key: ApiKeyDto) => {
  if (key.revokedAt) return;

  const message =
    `「${key.name}」を失効しますか?\n\n` +
    `失効後、このキーを使う MCP サーバや外部ツールは動かなくなります。`;
  if (!confirm(message)) return;

  revokingId.value = key.id;
  errorMessage.value = null;

  try {
    await apiKeyRepository.revoke(key.id);
    await fetchKeys();
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error ? e.message : "APIキーの失効に失敗しました";
  } finally {
    revokingId.value = null;
  }
};

const formatDateTime = (iso: string | null) => {
  if (!iso) return null;
  return new Date(iso).toLocaleString("ja-JP", {
    year: "numeric",
    month: "2-digit",
    day: "2-digit",
    hour: "2-digit",
    minute: "2-digit",
  });
};

const formatDate = (iso: string | null) => {
  if (!iso) return null;
  return new Date(iso).toLocaleDateString("ja-JP");
};

const expiryLabel = (key: ApiKeyDto) => {
  if (!key.expiresAt) return "無期限";
  const d = formatDate(key.expiresAt);
  if (!d) return "無期限";
  const isExpired = new Date(key.expiresAt).getTime() < Date.now();
  return `${d}まで${isExpired ? "(期限切れ)" : ""}`;
};

const statusLabel = (key: ApiKeyDto): {
  text: string;
  color: string;
} => {
  if (key.revokedAt) return { text: "失効済み", color: "text-gray-500" };
  if (key.expiresAt && new Date(key.expiresAt).getTime() < Date.now())
    return { text: "期限切れ", color: "text-amber-600" };
  return { text: "有効", color: "text-green-600" };
};
</script>

<template>
  <BaseCard class="p-4 md:p-6">
    <div class="space-y-3 md:space-y-4">
      <div class="flex items-center justify-between mb-3 md:mb-4">
        <div class="flex items-center gap-2">
          <BaseIcon name="tag" size="md" class="text-gray-500" />
          <BaseText variant="h3">APIキー</BaseText>
        </div>
        <BaseButton variant="primary" size="sm" @click="openCreateModal">
          <span class="flex items-center gap-1 text-xs md:text-sm">
            <BaseIcon name="plus" size="sm" />
            <span>新規発行</span>
          </span>
        </BaseButton>
      </div>

      <BaseText variant="caption" color="gray" class="text-xs md:text-sm">
        MCPサーバや外部ツールから SELENE の API を叩くためのキー。
        漏洩した場合は速やかに失効してください。
      </BaseText>

      <div
        v-if="errorMessage"
        class="p-3 bg-red-50 border border-red-200 rounded-lg"
      >
        <div class="flex items-start gap-2">
          <BaseIcon
            name="warning"
            size="sm"
            class="text-red-600 mt-0.5"
            variant="solid"
          />
          <BaseText variant="caption" class="text-red-800">
            {{ errorMessage }}
          </BaseText>
        </div>
      </div>

      <div
        v-if="isLoading"
        class="flex items-center justify-center py-6"
      >
        <BaseSpinner
          icon="refresh"
          size="md"
          color="primary"
          label="読み込み中"
        />
      </div>

      <div
        v-else-if="sortedKeys.length === 0"
        class="py-6 text-center"
      >
        <BaseText variant="body" color="gray" class="text-sm">
          発行済みのAPIキーはありません
        </BaseText>
      </div>

      <ul v-else class="space-y-2 md:space-y-3">
        <li
          v-for="key in sortedKeys"
          :key="key.id"
          class="border border-gray-200 rounded-lg p-3 md:p-4"
          :class="{ 'bg-gray-50 opacity-70': key.revokedAt }"
        >
          <div
            class="flex flex-col md:flex-row md:items-center md:justify-between gap-2 md:gap-3"
          >
            <div class="flex-1 min-w-0">
              <div class="flex items-center gap-2 mb-1">
                <BaseText
                  variant="body"
                  weight="medium"
                  class="text-sm md:text-base truncate"
                >
                  {{ key.name }}
                </BaseText>
                <span
                  class="text-xs px-2 py-0.5 rounded-full border"
                  :class="statusLabel(key).color"
                >
                  {{ statusLabel(key).text }}
                </span>
              </div>
              <div
                class="font-mono text-xs text-gray-600 mb-1 md:mb-2"
              >
                {{ key.keyPrefix }}...
              </div>
              <div
                class="grid grid-cols-1 md:grid-cols-3 gap-1 md:gap-2 text-xs md:text-sm text-gray-600"
              >
                <div class="flex items-center gap-1">
                  <BaseIcon name="calendar" size="sm" class="text-gray-400" />
                  <span>{{ expiryLabel(key) }}</span>
                </div>
                <div class="flex items-center gap-1">
                  <BaseIcon
                    name="clipboard-list"
                    size="sm"
                    class="text-gray-400"
                  />
                  <span>
                    最終使用:
                    {{ formatDateTime(key.lastUsedAt) ?? "未使用" }}
                  </span>
                </div>
                <div class="flex items-center gap-1">
                  <span class="text-gray-400">scope:</span>
                  <span>{{ key.scopes.join(", ") || "(なし)" }}</span>
                </div>
              </div>
            </div>

            <BaseButton
              v-if="!key.revokedAt"
              variant="outline"
              size="sm"
              :disabled="revokingId !== null"
              @click="handleRevoke(key)"
              class="flex-shrink-0 self-start md:self-center"
            >
              <span class="flex items-center gap-1 text-red-600 text-xs md:text-sm">
                <BaseSpinner
                  v-if="revokingId === key.id"
                  icon="refresh"
                  size="sm"
                  color="danger"
                />
                <BaseIcon v-else name="trash" size="sm" />
                <span>{{ revokingId === key.id ? "失効中..." : "失効" }}</span>
              </span>
            </BaseButton>
          </div>
        </li>
      </ul>
    </div>
  </BaseCard>

  <CreateApiKeyModal
    :is-open="isCreateModalOpen"
    @close="closeCreateModal"
    @created="handleCreated"
  />

  <ApiKeyDisplayModal
    :is-open="isDisplayModalOpen"
    :result="lastCreated"
    @close="closeDisplayModal"
  />
</template>
