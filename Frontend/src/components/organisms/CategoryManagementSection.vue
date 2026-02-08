<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import draggable from "vuedraggable";
import BaseCard from "../atoms/BaseCard.vue";
import BaseText from "../atoms/BaseText.vue";
import BaseButton from "../atoms/BaseButton.vue";
import BaseIcon from "../atoms/BaseIcon.vue";
import BaseSpinner from "../atoms/BaseSpinner.vue";
import CategoryListItem from "../molecules/CategoryListItem.vue";
import CategoryFormModal from "../organisms/CategoryFormModal.vue";
import { useTransactionCategories } from "../../composables/useTransactionCategories";
import { useItemCategories } from "../../composables/useItemCategories";
import { useIncomeItemCategories } from "../../composables/useIncomeItemCategories";
import type { TransactionCategoryDto } from "../../types/transactionCategory";
import type { ItemCategoryDto } from "../../types/itemCategory";
import type { IncomeItemCategoryDto } from "../../types/incomeItemCategory";

type CategoryType = "transaction" | "item" | "incomeItem";
type CategoryDto =
  | TransactionCategoryDto
  | ItemCategoryDto
  | IncomeItemCategoryDto;

// 各カテゴリのcomposable
const transactionCategories = useTransactionCategories();
const itemCategories = useItemCategories();
const incomeItemCategories = useIncomeItemCategories();

// タブ管理
const activeTab = ref<CategoryType>("transaction");

// モーダル管理
const isModalOpen = ref(false);
const modalMode = ref<"create" | "edit">("create");
const editingCategory = ref<CategoryDto | null>(null);

// 削除済みカテゴリ表示フラグ
const showHiddenCategories = ref(false);

// アクティブなcomposableを取得
const activeComposable = computed(() => {
  switch (activeTab.value) {
    case "transaction":
      return transactionCategories;
    case "item":
      return itemCategories;
    case "incomeItem":
      return incomeItemCategories;
  }
});

// 表示用カテゴリリスト
const displayCategories = computed({
  get: () => activeComposable.value.visibleCategories.value,
  set: () => {
    // vuedraggableの双方向バインディング用
    // 実際の更新はドラッグ終了時に行う
  },
});

// タブラベル
const tabLabels: Record<CategoryType, string> = {
  transaction: "取引カテゴリ",
  item: "商品カテゴリ",
  incomeItem: "給与項目",
};

// 初期化
onMounted(async () => {
  await Promise.all([
    transactionCategories.fetchCategories(),
    itemCategories.fetchCategories(),
    incomeItemCategories.fetchCategories(),
  ]);
});

// タブ切り替え
const handleTabChange = (type: CategoryType) => {
  activeTab.value = type;
  activeComposable.value.clearError();
  activeComposable.value.clearSuccess();
};

// 作成モーダルを開く
const openCreateModal = () => {
  modalMode.value = "create";
  editingCategory.value = null;
  isModalOpen.value = true;
};

// 編集モーダルを開く
const openEditModal = (id: string) => {
  const category = activeComposable.value.categories.value.find(
    (c) => c.id === id,
  );
  if (category) {
    modalMode.value = "edit";
    editingCategory.value = category;
    isModalOpen.value = true;
  }
};

// モーダルを閉じる
const closeModal = () => {
  isModalOpen.value = false;
  editingCategory.value = null;
};

// 保存処理
const handleSave = async (data: any) => {
  try {
    if (modalMode.value === "create") {
      await activeComposable.value.createCategory(data);
    } else if (editingCategory.value) {
      await activeComposable.value.updateCategory(editingCategory.value.id, {
        ...data,
        displayOrder: editingCategory.value.displayOrder,
      });
    }
    closeModal();
  } catch (error) {
    // エラーはcomposableで処理済み
  }
};

// 削除処理
const handleDelete = async (id: string) => {
  try {
    await activeComposable.value.deleteCategory(id);
  } catch (error) {
    // エラーはcomposableで処理済み
  }
};

// 復元処理
const handleRestore = async (id: string) => {
  try {
    await activeComposable.value.restoreCategory(id);
  } catch (error) {
    // エラーはcomposableで処理済み
  }
};

// マスタ設定に戻す
const handleResetToMaster = async () => {
  if (confirm("マスタ設定に戻しますか？\n（カスタムカテゴリは保持されます）")) {
    try {
      await activeComposable.value.resetToMaster();
    } catch (error) {
      // エラーはcomposableで処理済み
    }
  }
};

// ドラッグ&ドロップ終了時の処理
const handleDragEnd = async () => {
  try {
    await activeComposable.value.updateDisplayOrders(
      displayCategories.value as any,
    );
  } catch (error) {
    // エラーはcomposableで処理済み
  }
};

// 削除済みカテゴリの表示切り替え
const toggleHiddenCategories = () => {
  showHiddenCategories.value = !showHiddenCategories.value;
};
</script>

<template>
  <BaseCard>
    <div class="space-y-4">
      <!-- ヘッダー -->
      <div class="flex items-center gap-2">
        <BaseIcon name="tag" size="md" class="text-gray-500" />
        <BaseText variant="h3">カテゴリ管理</BaseText>
      </div>

      <!-- タブ -->
      <div class="flex gap-2 border-b border-gray-200">
        <button
          v-for="type in [
            'transaction',
            'item',
            'incomeItem',
          ] as CategoryType[]"
          :key="type"
          @click="handleTabChange(type)"
          class="px-4 py-2 font-medium transition-colors border-b-2"
          :class="
            activeTab === type
              ? 'border-blue-500 text-blue-600'
              : 'border-transparent text-gray-500 hover:text-gray-700'
          "
        >
          {{ tabLabels[type] }}
        </button>
      </div>

      <!-- 成功メッセージ -->
      <div
        v-if="activeComposable.successMessage.value"
        class="bg-green-50 border border-green-200 rounded-lg p-3"
      >
        <div class="flex items-center gap-2">
          <BaseIcon name="check-circle" size="sm" class="text-green-600" />
          <BaseText variant="body" class="text-green-800">
            {{ activeComposable.successMessage.value }}
          </BaseText>
        </div>
      </div>

      <!-- エラーメッセージ -->
      <div
        v-if="activeComposable.errorMessage.value"
        class="bg-red-50 border border-red-200 rounded-lg p-3"
      >
        <div class="flex items-center gap-2">
          <BaseIcon
            name="exclamation-triangle"
            size="sm"
            class="text-red-600"
          />
          <BaseText variant="body" class="text-red-800">
            {{ activeComposable.errorMessage.value }}
          </BaseText>
        </div>
      </div>

      <!-- ローディング -->
      <div
        v-if="activeComposable.isLoading.value"
        class="flex justify-center py-8"
      >
        <BaseSpinner
          icon="refresh"
          size="lg"
          color="primary"
          label="読み込み中"
        />
      </div>

      <!-- カテゴリリスト -->
      <div v-else class="space-y-4">
        <!-- ドラッグ可能リスト -->
        <draggable
          v-model="displayCategories"
          item-key="id"
          handle=".cursor-move"
          @end="handleDragEnd"
          class="space-y-2"
          tag="div"
        >
          <template #item="{ element }">
            <CategoryListItem
              :category="element"
              @edit="openEditModal"
              @delete="handleDelete"
            />
          </template>
        </draggable>

        <!-- カテゴリが0件の場合 -->
        <div
          v-if="displayCategories.length === 0"
          class="text-center py-8 text-gray-500"
        >
          <BaseIcon name="tag" size="lg" class="mx-auto mb-2 opacity-50" />
          <BaseText variant="body" color="gray">
            カテゴリがありません
          </BaseText>
        </div>

        <!-- アクションボタン -->
        <div class="flex gap-2 pt-4">
          <BaseButton
            variant="primary"
            @click="openCreateModal"
            :disabled="activeComposable.isSaving.value"
            class="flex-1"
          >
            <span class="flex items-center justify-center gap-2">
              <BaseIcon name="plus" size="sm" />
              <span>カテゴリを追加</span>
            </span>
          </BaseButton>

          <BaseButton
            variant="outline"
            @click="handleResetToMaster"
            :disabled="activeComposable.isSaving.value"
          >
            <span class="flex items-center gap-1">
              <BaseIcon name="arrow-path" size="sm" />
              <span class="hidden sm:inline">マスタに戻す</span>
            </span>
          </BaseButton>
        </div>

        <!-- 削除済みカテゴリ -->
        <div
          v-if="activeComposable.hiddenCategories.value.length > 0"
          class="pt-4 border-t border-gray-200"
        >
          <button
            @click="toggleHiddenCategories"
            class="flex items-center gap-2 text-gray-600 hover:text-gray-800 transition-colors"
          >
            <BaseIcon
              :name="showHiddenCategories ? 'chevron-up' : 'chevron-down'"
              size="sm"
            />
            <BaseText variant="body">
              削除済みカテゴリ ({{
                activeComposable.hiddenCategories.value.length
              }})
            </BaseText>
          </button>

          <!-- 削除済みカテゴリリスト -->
          <div v-if="showHiddenCategories" class="mt-3 space-y-2">
            <div
              v-for="category in activeComposable.hiddenCategories.value"
              :key="category.id"
              class="flex items-center gap-3 p-3 bg-gray-50 border border-gray-200 rounded-lg"
            >
              <div
                :style="{ backgroundColor: category.colorCode }"
                class="w-6 h-6 rounded-full border border-gray-300 opacity-50"
              />
              <BaseText variant="body" color="gray" class="flex-1">
                {{ category.name }}
              </BaseText>
              <BaseButton
                variant="text"
                size="sm"
                @click="handleRestore(category.id)"
              >
                <span class="flex items-center gap-1 text-blue-600">
                  <BaseIcon name="arrow-path" size="sm" />
                  <span>復元</span>
                </span>
              </BaseButton>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 作成・編集モーダル -->
    <CategoryFormModal
      :is-open="isModalOpen"
      :mode="modalMode"
      :category-type="activeTab"
      :initial-data="editingCategory"
      @save="handleSave"
      @close="closeModal"
    />
  </BaseCard>
</template>
