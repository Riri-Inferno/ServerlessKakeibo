<script setup lang="ts">
import { ref } from "vue";
import BaseText from "../atoms/BaseText.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";

const emit = defineEmits<{
  upload: [file: File];
}>();

const fileInput = ref<HTMLInputElement>();
const isDragging = ref(false);
const previewUrl = ref<string | null>(null);

const handleFileSelect = (event: Event) => {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (file) {
    handleFile(file);
  }
};

const handleFile = (file: File) => {
  if (!file.type.startsWith("image/")) {
    alert("画像ファイルを選択してください");
    return;
  }

  const reader = new FileReader();
  reader.onload = (e) => {
    previewUrl.value = e.target?.result as string;
  };
  reader.readAsDataURL(file);

  emit("upload", file);
};

const handleDrop = (event: DragEvent) => {
  isDragging.value = false;
  const file = event.dataTransfer?.files[0];
  if (file) {
    handleFile(file);
  }
};

const handleDragOver = (event: DragEvent) => {
  event.preventDefault();
  isDragging.value = true;
};

const handleDragLeave = () => {
  isDragging.value = false;
};

const openFileDialog = () => {
  fileInput.value?.click();
};

const clearPreview = () => {
  previewUrl.value = null;
  if (fileInput.value) {
    fileInput.value.value = "";
  }
};
</script>

<template>
  <div class="space-y-4">
    <BaseText variant="h3">領収書をアップロード</BaseText>

    <div v-if="!previewUrl">
      <div
        @click="openFileDialog"
        @drop.prevent="handleDrop"
        @dragover.prevent="handleDragOver"
        @dragleave="handleDragLeave"
        class="border-2 border-dashed rounded-lg p-8 text-center cursor-pointer transition-colors"
        :class="{
          'border-blue-500 bg-blue-50': isDragging,
          'border-gray-300 hover:border-gray-400': !isDragging,
        }"
      >
        <BaseIcon name="plus" size="xl" class="mx-auto mb-4 text-gray-400" />
        <BaseText variant="body" color="gray" class="mb-2">
          クリックまたはドラッグ＆ドロップで画像をアップロード
        </BaseText>
        <BaseText variant="caption" color="gray"> JPG、PNG形式に対応 </BaseText>
      </div>

      <input
        ref="fileInput"
        type="file"
        accept="image/*"
        class="hidden"
        @change="handleFileSelect"
      />
    </div>

    <div v-else class="space-y-4">
      <div class="relative">
        <img
          :src="previewUrl"
          alt="アップロードされた領収書"
          class="w-full rounded-lg border border-gray-300"
        />
        <button
          @click="clearPreview"
          class="absolute top-2 right-2 p-2 bg-white rounded-full shadow-lg hover:bg-gray-100 transition-colors"
        >
          <BaseIcon name="x" size="sm" />
        </button>
      </div>

      <BaseButton variant="outline" full-width @click="openFileDialog">
        別の画像を選択
      </BaseButton>
    </div>
  </div>
</template>
