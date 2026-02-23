<script setup lang="ts">
import { computed } from "vue";
import { useAuth } from "../../composables/useAuth";
import { isDemoMode as checkDemoMode } from "../../utils/env";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseCard from "../atoms/BaseCard.vue";

const emit = defineEmits<{
  toggleSidebar: [];
}>();

const { user, effectiveDisplayName, logout } = useAuth();

// デモモード判定
const isDemoMode = computed(() => checkDemoMode());
</script>

<template>
  <header class="bg-white shadow-sm border-b border-gray-200">
    <div class="flex items-center justify-between px-4 py-3 md:px-6 md:py-4">
      <div class="flex items-center gap-2 md:gap-4">
        <button
          @click="emit('toggleSidebar')"
          class="p-2 hover:bg-gray-100 rounded-lg md:hidden"
        >
          <BaseIcon name="bars-3" size="md" />
        </button>

        <BaseText variant="h2" class="hidden md:block"> 家計簿アプリ </BaseText>
        <BaseText variant="h3" class="md:hidden"> 家計簿 </BaseText>

        <!-- デモモードバッジ -->
        <BaseCard
          v-if="isDemoMode"
          padding="sm"
          class="bg-yellow-50 border-2 border-yellow-400"
        >
          <div class="flex items-center gap-1">
            <BaseIcon
              name="exclamation-triangle"
              size="sm"
              class="text-yellow-700"
            />
            <BaseText
              variant="caption"
              class="text-yellow-700 font-bold hidden sm:inline"
            >
              DEMO
            </BaseText>
          </div>
        </BaseCard>
      </div>

      <div class="flex items-center gap-2 md:gap-4">
        <BaseText variant="caption" color="gray" class="hidden sm:block">
          {{ effectiveDisplayName }}
        </BaseText>

        <img
          v-if="user?.pictureUrl"
          :src="user.pictureUrl"
          :alt="effectiveDisplayName"
          class="w-8 h-8 md:w-10 md:h-10 rounded-full"
        />

        <BaseButton
          variant="text"
          size="sm"
          @click="logout"
          class="hidden md:block"
        >
          ログアウト
        </BaseButton>

        <button
          @click="logout"
          class="p-2 hover:bg-gray-100 rounded-lg md:hidden"
        >
          <BaseIcon name="x" size="sm" />
        </button>
      </div>
    </div>
  </header>
</template>
