<script setup lang="ts">
import { ref, onMounted } from "vue";
import BaseCard from "../atoms/BaseCard.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import { oauthRepository } from "../../repositories/oauthRepository";
import type { OAuthConnection } from "../../types/oauth";

/**
 * OAuth で連携中のアプリ（Claude など）の一覧と解除。
 * 手動で発行した API キーとは別に表示する
 */
const connections = ref<OAuthConnection[]>([]);
const isLoading = ref(false);
const errorMessage = ref<string | null>(null);
const revokingId = ref<string | null>(null);

const fetchConnections = async () => {
  isLoading.value = true;
  errorMessage.value = null;
  try {
    connections.value = await oauthRepository.listConnections();
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error ? e.message : "連携中のアプリの取得に失敗しました";
  } finally {
    isLoading.value = false;
  }
};

onMounted(fetchConnections);

const handleRevoke = async (connection: OAuthConnection) => {
  const message =
    `「${connection.clientName}」との連携を解除しますか?\n\n` +
    `解除すると、このアプリから家計簿を使えなくなります。使うには、アプリからもう一度接続して許可してください。`;
  if (!confirm(message)) return;

  revokingId.value = connection.id;
  errorMessage.value = null;
  try {
    await oauthRepository.revokeConnection(connection.id);
    await fetchConnections();
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error ? e.message : "連携の解除に失敗しました";
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

const scopeLabel = (scopes: string[]) =>
  scopes.includes("write") ? "読み取り・書き込み" : "読み取りのみ";
</script>

<template>
  <BaseCard class="p-4 md:p-6">
    <div class="space-y-3 md:space-y-4">
      <div class="flex items-center gap-2 mb-3 md:mb-4">
        <BaseIcon name="tag" size="md" class="text-gray-500" />
        <BaseText variant="h3">連携中のアプリ</BaseText>
      </div>

      <BaseText
        variant="caption"
        color="gray"
        class="text-xs md:text-sm text-center"
      >
        Claude などのアプリに、ログインして許可した連携の一覧。
        心当たりのない連携は解除してください。
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

      <div v-if="isLoading" class="flex items-center justify-center py-6">
        <BaseSpinner
          icon="refresh"
          size="md"
          color="primary"
          label="読み込み中"
        />
      </div>

      <div v-else-if="connections.length === 0" class="py-6 text-center">
        <BaseText variant="body" color="gray" class="text-sm">
          連携中のアプリはありません
        </BaseText>
      </div>

      <ul v-else class="space-y-2 md:space-y-3">
        <li
          v-for="connection in connections"
          :key="connection.id"
          class="border border-gray-200 rounded-lg p-3 md:p-4"
        >
          <div
            class="flex flex-col md:flex-row md:items-center md:justify-between gap-2 md:gap-3"
          >
            <div class="flex-1 min-w-0">
              <BaseText
                variant="body"
                weight="medium"
                class="text-sm md:text-base truncate mb-1"
              >
                {{ connection.clientName }}
              </BaseText>
              <div
                class="grid grid-cols-1 md:grid-cols-3 gap-1 md:gap-2 text-xs md:text-sm text-gray-600"
              >
                <div class="flex items-center gap-1">
                  <BaseIcon name="calendar" size="sm" class="text-gray-400" />
                  <span>許可: {{ formatDateTime(connection.createdAt) ?? "-" }}</span>
                </div>
                <div class="flex items-center gap-1">
                  <BaseIcon
                    name="clipboard-list"
                    size="sm"
                    class="text-gray-400"
                  />
                  <span>
                    最終使用:
                    {{ formatDateTime(connection.lastUsedAt) ?? "未使用" }}
                  </span>
                </div>
                <div class="flex items-center gap-1">
                  <BaseIcon name="tag" size="sm" class="text-gray-400" />
                  <span>{{ scopeLabel(connection.scopes) }}</span>
                </div>
              </div>
            </div>

            <BaseButton
              variant="outline"
              size="sm"
              :disabled="revokingId !== null"
              class="flex-shrink-0 self-start md:self-center"
              @click="handleRevoke(connection)"
            >
              <span
                class="flex items-center gap-1 text-red-600 text-xs md:text-sm"
              >
                <BaseSpinner
                  v-if="revokingId === connection.id"
                  icon="refresh"
                  size="sm"
                  color="danger"
                />
                <BaseIcon v-else name="trash" size="sm" />
                <span>{{ revokingId === connection.id ? "解除中..." : "解除" }}</span>
              </span>
            </BaseButton>
          </div>
        </li>
      </ul>
    </div>
  </BaseCard>
</template>
