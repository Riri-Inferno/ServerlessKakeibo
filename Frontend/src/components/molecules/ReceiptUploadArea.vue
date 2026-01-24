<script setup lang="ts">
import { ref } from "vue";
import BaseText from "../atoms/BaseText.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import { compressImage } from "../../utils/imageCompression";

const emit = defineEmits<{
  upload: [file: File];
}>();

const fileInput = ref<HTMLInputElement>();
const isDragging = ref(false);
const previewUrl = ref<string | null>(null);
const isCompressing = ref(false);

const handleFileSelect = async (event: Event) => {
  const input = event.target as HTMLInputElement;
  const file = input.files?.[0];
  if (file) {
    await handleFile(file);
  }
};

const handleFile = async (file: File) => {
  if (!file.type.startsWith("image/")) {
    alert("画像ファイルを選択してください");
    return;
  }

  // 圧縮処理
  isCompressing.value = true;
  const compressedFile = await compressImage(file, {
    maxSizeMB: 0.5, // 500KB以下
    maxWidthOrHeight: 1920, // フルHD
  });
  isCompressing.value = false;

  // console.log("圧縮後のファイル:", compressedFile);
  // console.log("圧縮後のサイズ:", compressedFile.size, "bytes");
  // console.log("圧縮後の型:", compressedFile.constructor.name);

  // プレビュー表示
  const reader = new FileReader();
  reader.onload = (e) => {
    previewUrl.value = e.target?.result as string;
  };
  reader.readAsDataURL(compressedFile);

  emit("upload", compressedFile);
};

const handleDrop = async (event: DragEvent) => {
  isDragging.value = false;
  const file = event.dataTransfer?.files[0];
  if (file) {
    await handleFile(file);
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
        <BaseText variant="caption" color="gray">
          JPG、PNG形式に対応（自動で圧縮されます）
        </BaseText>
      </div>

      <!-- 圧縮中の表示 -->
      <div v-if="isCompressing" class="text-center mt-4">
        <BaseText variant="body" color="gray">画像を圧縮中...</BaseText>
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
