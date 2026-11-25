<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useRoute } from "vue-router";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";
import BaseSpinner from "../components/atoms/BaseSpinner.vue";
import AppLogo from "../components/atoms/AppLogo.vue";
import { useAuthStore } from "../stores/authStore";
import { oauthRepository } from "../repositories/oauthRepository";
import type { OAuthAuthorizeInfo } from "../types/oauth";

/**
 * OAuth の承認画面。
 * backend の /oauth/authorize が、認可リクエストのパラメータを付けてここへ転送してくる。
 * 許可したら承認チケットを受け取り、パラメータとチケットを付けて /oauth/authorize に戻る。
 * 設計: Document/Architecture/oauth-authorization-server.md
 */
const route = useRoute();
const authStore = useAuthStore();

const info = ref<OAuthAuthorizeInfo | null>(null);
const isLoading = ref(true);
const isSubmitting = ref(false);
const errorMessage = ref<string | null>(null);
const allowWrite = ref(false);

/** 認可リクエストのパラメータ（そのまま /oauth/authorize に戻す） */
const query = computed<Record<string, string>>(() => {
  const result: Record<string, string> = {};
  for (const [key, value] of Object.entries(route.query)) {
    if (typeof value === "string") result[key] = value;
  }
  return result;
});

const clientId = computed(() => query.value.client_id ?? "");
const redirectUri = computed(() => query.value.redirect_uri ?? "");
const codeChallenge = computed(() => query.value.code_challenge ?? "");

const canRequestWrite = computed(
  () => info.value?.requestedScopes.includes("write") ?? false,
);

const displayName = computed(() => authStore.effectiveDisplayName);

onMounted(async () => {
  if (!clientId.value || !redirectUri.value || !codeChallenge.value) {
    errorMessage.value =
      "連携のリクエストが正しくありません。接続しようとしたアプリから、もう一度やり直してください。";
    isLoading.value = false;
    return;
  }

  try {
    info.value = await oauthRepository.getAuthorizeInfo(
      clientId.value,
      redirectUri.value,
      query.value.scope ?? null,
    );
    // アプリが書き込みを求めている場合は、既定で書き込みも許可する（チェックを外せば読み取りだけになる）
    allowWrite.value = canRequestWrite.value;
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error
        ? e.message
        : "連携するアプリの情報を取得できませんでした";
  } finally {
    isLoading.value = false;
  }
});

const handleAllow = async () => {
  if (!info.value) return;
  isSubmitting.value = true;
  errorMessage.value = null;

  try {
    const consentTicket = await oauthRepository.consent({
      clientId: clientId.value,
      redirectUri: redirectUri.value,
      codeChallenge: codeChallenge.value,
      scopes: allowWrite.value ? ["read", "write"] : ["read"],
    });
    window.location.href = oauthRepository.buildAuthorizeUrl(query.value, {
      consentTicket,
    });
  } catch (e: unknown) {
    errorMessage.value =
      e instanceof Error ? e.message : "連携の許可に失敗しました";
    isSubmitting.value = false;
  }
};

const handleDeny = () => {
  isSubmitting.value = true;
  window.location.href = oauthRepository.buildAuthorizeUrl(query.value, {
    denied: true,
  });
};
</script>

<template>
  <div
    class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4"
  >
    <BaseCard padding="lg" class="w-full max-w-md">
      <div class="flex flex-col items-center mb-6">
        <AppLogo variant="mark" size="xl" class="mb-3" />
        <BaseText variant="h3" class="text-center">アプリとの連携</BaseText>
      </div>

      <div v-if="isLoading" class="flex items-center justify-center py-8">
        <BaseSpinner
          icon="refresh"
          size="md"
          color="primary"
          label="読み込み中"
        />
      </div>

      <div
        v-else-if="!info"
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

      <div v-else class="space-y-5">
        <BaseText variant="body" class="text-center">
          <span class="font-bold">{{ info.clientName }}</span>
          が、あなたの SELENE の家計簿へのアクセスを求めています。
        </BaseText>

        <div class="p-3 bg-gray-50 border border-gray-200 rounded-lg space-y-1">
          <BaseText variant="caption" color="gray">
            許可すると、次のアドレスに戻ります
          </BaseText>
          <div class="font-mono text-sm font-bold break-all">
            {{ info.redirectHost }}
          </div>
          <BaseText variant="caption" color="gray">
            心当たりのないアドレスなら、許可しないでください。
          </BaseText>
        </div>

        <div class="space-y-2">
          <BaseText variant="body" weight="medium">許可する内容</BaseText>
          <label class="flex items-start gap-2 opacity-80">
            <input type="checkbox" checked disabled class="mt-1" />
            <span class="text-sm">
              家計簿を読み取る（取引、カテゴリ、集計）
            </span>
          </label>
          <label
            class="flex items-start gap-2"
            :class="{ 'opacity-50': !canRequestWrite }"
          >
            <input
              v-model="allowWrite"
              type="checkbox"
              :disabled="!canRequestWrite"
              class="mt-1"
            />
            <span class="text-sm">
              家計簿に書き込む（取引やカテゴリの登録・更新・削除）
            </span>
          </label>
          <div
            v-if="allowWrite"
            class="p-2 bg-yellow-50 border border-yellow-300 rounded text-xs text-yellow-800"
          >
            書き込みを許可すると、アプリが取引を削除することもできます。
          </div>
        </div>

        <BaseText variant="caption" color="gray" class="block">
          ログイン中: {{ displayName }}。連携は「設定」の「連携中のアプリ」からいつでも解除できます。
        </BaseText>

        <div
          v-if="errorMessage"
          class="p-3 bg-red-50 border border-red-200 rounded-lg"
        >
          <BaseText variant="caption" class="text-red-800">
            {{ errorMessage }}
          </BaseText>
        </div>

        <div class="flex gap-3">
          <BaseButton
            variant="outline"
            class="flex-1"
            :disabled="isSubmitting"
            @click="handleDeny"
          >
            許可しない
          </BaseButton>
          <BaseButton
            variant="primary"
            class="flex-1"
            :disabled="isSubmitting"
            @click="handleAllow"
          >
            {{ isSubmitting ? "処理中..." : "許可する" }}
          </BaseButton>
        </div>
      </div>
    </BaseCard>
  </div>
</template>
